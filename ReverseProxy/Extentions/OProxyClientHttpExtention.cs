/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Authentication;
using System.IO;
using System.Threading;

using K2host.Core;
using K2host.Sockets.ReverseProxy.Mapping.Http;
using K2host.Sockets.Raw.Enums;
using K2host.Sockets.ReverseProxy.Interface;

using gl = K2host.Core.OHelpers;
using gs = K2host.Sockets.OHelpers;

namespace K2host.Sockets.ReverseProxy.Extentions
{

    public static class OProxyClientHttpExtention
    {

        /// <summary>
        /// This is accepted connection from the client and the data downloaded to process.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="data"></param>
        public static void OnClientConneced(this OProxyClientHttp e, MemoryStream data)
        {

            byte[] output = data.ToArray();
           
            data.Dispose();

            if (e.OnProcessRequest(output, output.Length, false, out byte[] newBuffer))
            {
                e.OnMapDestination(out IPEndPoint destinationEndPoint);
                e.OnConnectDestination(destinationEndPoint, newBuffer);
            }
            else
                if (!e.IsDisposed)
                e.Dispose();

        }
        
        /// <summary>
        /// This validates the request by building up a request object based on the data.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool OnProcessRequest(this OProxyClientHttp e, byte[] buffer, int read, bool fromRelay, out byte[] newBuffer) 
        {

            newBuffer = Array.Empty<byte>();

            //Lets build the request up and parse it (post will download all the request)
            e.WebRequest = new OProxyHttpRequest(buffer, read) { Method = OProxyHttpMethodType.NONE };

            if (e.WebRequest.RequestData.Length > 0) 
            {

                OProxyHttpRequestType request = OProxyHttpRequestType.HttpPage;

                //Build Header Values
                string[] lines = e.WebRequest.RequestData.Split("\r\n", StringSplitOptions.None);

                for (int j = 0; j < lines.Length; j++)
                {
                    if (j == 0) // First line is always type and path.
                    {
                        string[] parts = lines[j].Split(" ", StringSplitOptions.None);
                        e.WebRequest.Method       = gs.GetMethodType(parts[0]);
                        e.WebRequest.CompletePath = parts[1];
                        e.WebRequest.Protocol     = parts[2];

                        if (!string.IsNullOrEmpty(parts[1]))
                        {
                            string[] resourcePaths = parts[1].Fracture("/");
                            if (resourcePaths.Length > 0)
                                request = gs.IsStaticResource(resourcePaths[^1]) ? OProxyHttpRequestType.HttpStaticRequest : OProxyHttpRequestType.HttpPage;
                        }
                        else
                            throw new InvalidOperationException("Invalid Request : " + e.WebRequest.RequestData);

                        string[] subPaths   = parts[1].Fracture("?");
                        e.WebRequest.Path   = subPaths[0];
                        e.WebRequest.Paths  = subPaths[0].Fracture("/").ToList().Select(p => "/" + p).ToList();
                    }
                    else
                    {
                        if (lines[j] == string.Empty)
                            break;

                        if (lines[j].Contains(": "))
                        {
                            string[] pairs = lines[j].Fracture(": ");
                                    
                            try {
                                e.WebRequest.Requests.Add(pairs[0], pairs[1]);
                            } catch {
                                e.WebRequest.Requests.Add(pairs[0], string.Empty);
                            }

                            switch (pairs[0].ToLower())
                            {
                                case "referer":                         e.WebRequest.Referer                        = pairs[1]; break;
                                case "access-control-request-method":   e.WebRequest.AccessControlRequestMethod     = pairs[1]; break;
                                case "access-control-request-headers":  e.WebRequest.AccessControlRequestHeaders    = pairs[1]; break;
                                case "origin":                          e.WebRequest.Origin                         = pairs[1]; break;
                                case "sec-fetch-mode":                  e.WebRequest.SecFetchMode                   = pairs[1]; break;
                                case "via":                             e.WebRequest.Via                            = pairs[1]; break;
                                case "host":                            e.WebRequest.Host                           = pairs[1]; break;
                                case "content-type":                    e.WebRequest.ContentType                    = pairs[1]; break;
                                case "accept":                          e.WebRequest.Accept                         = pairs[1]; break;
                                case "connection":                      e.WebRequest.Connection                     = pairs[1].ToLower() == "close" ? OProxyHttpConnectionType.Close : OProxyHttpConnectionType.KeepAlive; break;
                                case "user-agent":                      e.WebRequest.UserAgent                      = pairs[1]; break;
                                case "content-length":                  e.WebRequest.ContentLength                  = Convert.ToInt32(pairs[1]); break;
                                default: break;
                            }

                        }

                    }
                }

                //At this point if there is no method on the request assum this is a join from a prev request
                if (e.WebRequest.Method == OProxyHttpMethodType.NONE)
                    return false;

                //Build QueryString Values                
                foreach (string p in gs.RemoveToken(e.WebRequest.CompletePath).Fracture("&"))
                {
                    string[] query = p.Fracture("=");
                    if (query.Length == 2)
                        try { e.WebRequest.Parms.Add(query[0], query[1]); } catch { }
                }

                //Check for Pre Flight Request for Cors Security from source
                if (!string.IsNullOrEmpty(e.WebRequest.SecFetchMode) && e.WebRequest.Method == OProxyHttpMethodType.OPTIONS)
                    request = OProxyHttpRequestType.PreFlight;

                //Create the url requested
                string prot = "http" + (e.WebRequest.Method == OProxyHttpMethodType.CONNECT ? "s" : string.Empty);
                e.WebRequest.Url = new Uri(prot + "://" + e.WebRequest.Host);

                //Remove port from the hostname www.tonyspc.com:446
                if (e.WebRequest.Host.Contains(":"))
                    e.WebRequest.Host = e.WebRequest.Host.Substring(0, e.WebRequest.Host.LastIndexOf(":"));

                //Get the rest of data from the stream if required if the request comes from the relay as the initial connection will already have the full request.
                if (fromRelay && e.WebRequest.ContentLength > e.WebRequest.Body.Length) 
                {
                    int a = 0; byte[] b = new byte[16384]; MemoryStream c = new();
                    if (((NetworkStream)e.ClientInnerStream).DataAvailable)
                        do {
                            a = e.ClientStream.Read(b, 0, b.Length);
                            c.Write(b, 0, a);
                            if (((NetworkStream)e.ClientInnerStream).DataAvailable)
                                Thread.Sleep(1);
                        } while (((NetworkStream)e.ClientInnerStream).DataAvailable);

                    e.WebRequest.Body = gl.CombineByteArrays(e.WebRequest.Body, c.ToArray());
                    c.SetLength(0);
                    c.Dispose();
                    c = null;
                }

                //If the body is of content type x-www-form-urlencoded
                if (e.WebRequest.ContentType.Contains("application/x-www-form-urlencoded"))
                    foreach (string p in Encoding.UTF8.GetString(e.WebRequest.Body).Fracture("&")) {
                        string[] query = p.Fracture("=");
                        if (query.Length == 2)
                            try { e.WebRequest.Parms.Add(query[0], query[1]); } catch { }
                    }

                //Support multipart/form-data parsing
                if (e.WebRequest.ContentType.Contains("multipart/form-data"))
                {

                    //The boundary setup
                    string boundaryString = e.WebRequest.ContentType.Fracture(";")[1].Fracture("=")[1];

                    //The boundary setup
                    byte[] boundaryMarker = Encoding.UTF8.GetBytes("\r\n" + "--" + boundaryString + "\r\n");
                    byte[] content = gl.CombineByteArrays(Encoding.UTF8.GetBytes("\r\n"), e.WebRequest.Body);

                    //Split the bounds by the bound marker.
                    byte[][] boundariesFound = content.Split(boundaryMarker);

                    //Create the boundary segments to the request list.
                    foreach (byte[] found in boundariesFound)
                        try
                        {
                            //Create the item and parse in each item.
                            IProxyHttpRequestBoundray boundary = new OProxyHttpRequestBoundray() { BoundrayId = boundaryString }.Build(found);

                            //Add the boundary to the  request.
                            e.WebRequest.Boundries.Add(boundary.Name, boundary);
                        }
                        catch (Exception) { }

                }

                e.WebRequest.Type = request;

            }
            else
                return !e.OnSendBadRequest("This maybe due to no header information in the request.");

            //Lets screen and filter.
            try
            {

                if (!gs.IsAddressAllowed(e, out string errorOutAddress))
                    return !e.OnSendBadRequest(errorOutAddress);

                foreach (var c in e.SelectedFilter.CheckList)
                    if (!c.Invoke(e, out string errorOut))
                        return !e.OnSendBadRequest(errorOut);
            }
            catch (Exception ex)
            {
                return !e.OnSendBadRequest(ex.ToString());
            }

            newBuffer = e.OnRebuildRequest();

            //If from relay socket the connection needs remapping.
            if (fromRelay)
            {
                try
                {
                    if (e.OnMapDestination(out IPEndPoint destinationEndPoint))
                        e.OnConnectDestination(destinationEndPoint, newBuffer, true); 
                }
                catch (Exception ex)
                {
                    return !e.OnSendBadRequest(ex.ToString() + " re-mapping: " + e.ClientSocket.RemoteEndPoint.ToString());
                }
            }

            return true;

        }

        /// <summary>
        /// This is manuipulate the response from the server before it gos back to the client.
        /// </summary>
        /// <param name="e"></param>
        public static bool OnProcessResponse(this OProxyClientHttp _, byte[] buffer, int read, bool fromRelay, out byte[] newBuffer)
        {
            
            newBuffer = buffer;

            if(!fromRelay)
                return true;

            OProxyHttpResponse r = new(newBuffer);

            if (r.Header != null && r.Header.Length > 0)
            {

                string[] lines = r.ResponseData.Split("\r\n", StringSplitOptions.None);
                for (int j = 0; j < lines.Length; j++)
                    if (j == 0)
                    {
                        string[] parts = lines[j].Split(" ", StringSplitOptions.None);
                        r.StatusCode = Convert.ToInt32(parts[1]);
                        r.StatusDescription = parts[2];
                    }
                    else
                    {
                        if (lines[j] == string.Empty)
                            break;

                        if (lines[j].Contains(": "))
                        {
                            string[] pairs = lines[j].Fracture(": ");
                            try { r.Headers.Add(pairs[0], pairs[1]); } catch { r.Headers.Add(pairs[0], string.Empty); }
                        }
                    }

                if (r.Headers.ContainsKey("Connection"))
                    r.Connection = r.Headers["Connection"].ToLower() == "keep-alive" ? OProxyHttpConnectionType.KeepAlive : OProxyHttpConnectionType.Close;

                if (r.Headers.ContainsKey("Content-Encoding"))
                    r.IsCompressed = r.Headers["Content-Encoding"].ToLower().Contains("gzip");

                r.Headers.Add("Proxy-Agent", "RevProxy Server V2");
                
                //We could screen and filter the responce but will slow down the relay. opto enable / disable this feature ?
                //try
                //{

                //    foreach (var c in e.SelectedFilter.CheckList)
                //        if (!c.Invoke(e, out string errorOut))
                //            return !e.OnSendBadRequest(errorOut);
                //}
                //catch (Exception ex)
                //{
                //    return !e.OnSendBadRequest(ex.ToString());
                //}

                newBuffer = gl.Combine(r.GetHeader(), r.Body);

            }
            else
            {
                if (buffer.Length != read)
                    newBuffer = buffer.BufferShrink(read);
            }

            r.Dispose();

            return true;

        }

        /// <summary>
        /// Used to remap a destination based on the proxy header required to remap.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool OnMapDestination(this OProxyClientHttp e, out IPEndPoint destinationEndPoint) 
        {

            destinationEndPoint = null;

            IProxyFailOver selectedDestination = e.SelectedFilter.CurrentFailover;

            if (selectedDestination == null)
                return !e.OnSendDeniedRequest("This web site or url is down at this current time.");

            //Let do a re-map on the proxy header required.
            if (e.WebRequest.Requests.ContainsKey("RevProx-Map-Service"))
                selectedDestination = e.SelectedFilter.MappedServices[e.WebRequest["RevProx-Map-Service"]];

            int AccessPort;

            if (e.WebRequest.Method == OProxyHttpMethodType.CONNECT || e.ServerSNI.IsValid)
                AccessPort = selectedDestination.MappedSSLPort; //HTTPS 
            else
                AccessPort = selectedDestination.MappedPort;    //HTTP 

            try
            {

                if (selectedDestination.MappedHost.Equals(IPAddress.Any))
                    selectedDestination.MappedHost = Dns.GetHostAddresses(e.WebRequest.Host)[0];

                destinationEndPoint = new IPEndPoint(selectedDestination.MappedHost, AccessPort);

                if (e.DestinationSocket != null)
                    if (((IPEndPoint)e.DestinationSocket.RemoteEndPoint).Address.Equals(destinationEndPoint.Address) && ((IPEndPoint)e.DestinationSocket.RemoteEndPoint).Port.Equals(destinationEndPoint.Port))
                        return false;
                    else
                    {
                        e.DestinationStream.Flush();
                        e.DestinationStream.Close();
                        e.DestinationStream.Dispose();
                        e.DestinationSocket.Disconnect(false);
                        e.DestinationSocket.Close();
                        e.DestinationSocket.Dispose();
                        e.DestinationSocket = null;
                    }

                e.DestinationSocket = new Socket(destinationEndPoint.AddressFamily, gs.GetSocketType(selectedDestination.Protocol), selectedDestination.Protocol);

                if (e.WebRequest.ContainsRequest("Proxy-Connection") && e.WebRequest["Proxy-Connection"].ToLower() == "keep-alive")
                    e.DestinationSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);

                return true;

            }
            catch (Exception ex)
            {
                return !e.OnSendBadRequest(ex.ToString() + " client: " + e.ClientSocket.RemoteEndPoint.ToString());
            }

        }

        /// <summary>
        /// Used to rebuild the header removing the proxy items or setting proxy agent to client.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static byte[] OnRebuildRequest(this OProxyClientHttp e) 
        {
            //Https Connect for proxy
            if (e.WebRequest.Method == OProxyHttpMethodType.CONNECT) 
            {
               
                var r = new OProxyHttpResponse()
                {
                    StatusCode          = 200,
                    StatusDescription   = "Connection established",
                    Connection          = OProxyHttpConnectionType.KeepAlive
                };
                r.Headers.Add("Proxy-Agent", "RevProxy Server V2");
                return r.GetData();

            } 
            else 
            {
                //Let do a urll re-write based on the proxy header required.
                if (e.WebRequest.ContainsRequest("RevProx-Url-Rewrite"))
                    e.WebRequest["Host"] = e.SelectedFilter.UrlReWrites[e.WebRequest["RevProx-Url-Rewrite"]];

                //Lets rebuild the header with its body and send to the destination connection.
                StringBuilder ret = new();

                ret.Append(e.WebRequest.Method.ToString() + " " + e.WebRequest.CompletePath + " " + e.WebRequest.Protocol);
                e.WebRequest.Requests.ForEach(kp => {
                    if (kp.Key.ToLower().StartsWith("proxy-") || kp.Key.ToLower().StartsWith("revprox-")) { } else
                        ret.Append("\r\n" + kp.Key + ": " + kp.Value);
                });
                ret.Append("\r\n\r\n");

                byte[] v = Encoding.UTF8.GetBytes(ret.ToString());

                //Lets now join the bod back on the header.
                if (e.WebRequest.Body.Length > 0)
                    v = gl.Combine(v, e.WebRequest.Body);

                return v;

            }

        }

        /// <summary>
        /// This will process the header of the http request and filter the traffic.
        /// </summary>
        /// <param name="e"></param>
        public static bool OnConnectDestination(this OProxyClientHttp e, IPEndPoint DestinationEndPoint, byte[] buffer, bool remapped = false)
        {

            try
            {

                e.DestinationSocket.BeginConnect(
                    DestinationEndPoint, 
                    new AsyncCallback(e.OnConnectedDestination), 
                    new object[] { 
                        e.DestinationSocket, 
                        remapped,
                        buffer
                    }
                );

                return true;

            }
            catch (Exception ex)
            {
                return !e.OnSendBadRequest(ex.ToString() + " client: " + e.ClientSocket.RemoteEndPoint.ToString());
            }

        }

        /// <summary>
        /// This will connect to the destination and send on the data from the client after processing.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ar"></param>
        public static void OnConnectedDestination(this OProxyClientHttp e, IAsyncResult ar)
        {
            try
            {

                object[] state = (object[])ar.AsyncState;

                Socket  destination = (Socket)state[0];
                bool    remapped    = (bool)state[1];
                byte[]  buffer      = (byte[])state[2];

                //If this is non-async then the ar will be null
                if (ar != null)
                    e.DestinationSocket.EndConnect(ar);

                if (e.WebRequest.Method == OProxyHttpMethodType.CONNECT)
                {
                    if (ar != null)
                        buffer.BufferWriteAsync(e.ClientStream, (4096 * 2), (obj) => { e.StreamStartRelay(); });
                }
                else
                {
                    //Make the stream to the destination socket.
                    e.DestinationInnerStream = new NetworkStream(e.DestinationSocket) { ReadTimeout = 5000, WriteTimeout = 5000 };
                    e.DestinationStream = e.DestinationInnerStream;

                    //If ssl then auth with cert and convert to ssl stream.
                    if (e.ServerSNI.IsValid)
                    {
                        e.DestinationStream = new SslStream(e.DestinationInnerStream, false);
                        ((SslStream)e.DestinationStream).AuthenticateAsClient(e.ServerSNI.ServerNameIndication, new X509Certificate2Collection { e.AssignedX509Certificate }, SslProtocols.Tls12 | SslProtocols.Tls13, false);
                    }

                    if (ar != null && !remapped)
                        buffer.BufferWriteAsync(e.DestinationStream, (4096 * 2), (obj) => { e.StreamStartRelay(); });

                }

            }
            catch(Exception)
            {
                if (!e.IsDisposed)
                    e.Dispose();
            }
        }
                
        /// <summary>
        /// This sends a response based on any errors or mismatched filters during processing.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="errorContext"></param>
        public static bool OnSendDeniedRequest(this OProxyClientHttp e, string errorContext = "")
        {
            try
            {

                byte[] buffer = new OProxyHttpResponse() {
                        StatusCode          = 400,
                        StatusDescription   = "Denied Request",
                        ContentType         = "text/html",
                        Connection          = OProxyHttpConnectionType.Close,
                        IsCompressed        = e.WebRequest.IsGZIPCommpressed(),
                    }
                    .Write(string.Format(((OProxyListenerHttp)e.Parent).DeniedRequestHtmlContent.Clone().ToString(), (errorContext.Length > 0 ? "<br /><br />" + errorContext : string.Empty)))
                    .GetData();

                buffer.BufferWriteAsync(e.ClientStream, (4096 * 2), o => {
                    
                    buffer.Dispose();

                    if (!e.IsDisposed)
                        e.Dispose();

                });

                return true;

            }
            catch
            {
                if (!e.IsDisposed)
                    e.Dispose();

                return false;

            }

        }

        /// <summary>
        /// This sends a response based on any errors or mismatched filters during processing.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="errorContext"></param>
        public static bool OnSendBadRequest(this OProxyClientHttp e, string errorContext = "")
        {

            try
            {

                byte[] buffer = new OProxyHttpResponse() {
                        StatusCode          = 400,
                        StatusDescription   = "Bad Request",
                        ContentType         = "text/html",
                        Connection          = OProxyHttpConnectionType.Close,
                        IsCompressed        = e.WebRequest.IsGZIPCommpressed(),
                    }
                    .Write(string.Format(((OProxyListenerHttp)e.Parent).BadRequestHtmlContent.Clone().ToString(), (errorContext.Length > 0 ? "<br /><br />" + errorContext : string.Empty)))
                    .GetData();

                buffer.BufferWriteAsync(e.ClientStream, (4096 * 2), o => {

                    buffer.Dispose();

                    if (!e.IsDisposed)
                        e.Dispose();

                });

                return true;

            }
            catch
            {
                if (!e.IsDisposed)
                    e.Dispose();
                return false;

            }

        }

    }

}
