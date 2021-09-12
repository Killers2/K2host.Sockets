/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.IO;
using System.Security.Cryptography.X509Certificates;

using SharpPcap.LibPcap;

using K2host.Core;
using K2host.Sockets.Delegates;
using K2host.Sockets.Raw.Enums;
using K2host.Sockets.ReverseProxy.Extentions;
using K2host.Sockets.ReverseProxy.Mapping.Http;
using K2host.Sockets.ReverseProxy.Interface;

using gl = K2host.Core.OHelpers;

namespace K2host.Sockets
{
   
    public static class OHelpers
    {

        #region Proxy Filtering

        public static bool IsAddressAllowed(OProxyClientHttp e, out string errorOut)
        {

            errorOut = string.Empty;

            //if there is an allow all paths filter
            e.SelectedFilter = e.HttpFilters.Where(f => f.UrlHostName == "*").FirstOrDefault();

            if (e.SelectedFilter != null)
                return true;

            //find the filter which allows the url path
            e.SelectedFilter = e.HttpFilters.Where(f => f.UrlHostName == e.WebRequest.Host).FirstOrDefault();

            if (e.SelectedFilter != null)
            {
                e.EnableRequestRelayCapture     = e.SelectedFilter.EnableRequestRelayCapture;
                e.EnableResponseRelayCapture    = e.SelectedFilter.EnableResponseRelayCapture;
                return true;
            }

            errorOut = "The url / web address(" + e.WebRequest.Host + ") has been denied, sorry for any inconvenience.";

            return false;

        }

        public static bool IsAuthenticated(OProxyClientHttp e, out string errorOut)
        {
            errorOut = string.Empty;

            if (e.SelectedFilter.ProxyAuthorizationBasic.Length <= 0)
                return true;

            if (e.WebRequest.ContainsRequest("revprox-authorization") && e.SelectedFilter.ProxyAuthorizationBasic.Where(a => a == e.WebRequest["revprox-authorization"]).Any())
                return true;

            errorOut = "The authentication failed for this host and has been denied, sorry for any inconvenience.";

            return false;

        }

        public static bool IsPathAllowed(OProxyClientHttp e, out string errorOut)
        {
            errorOut = string.Empty;

            if (!e.SelectedFilter.DeniedUrlPaths.Contains(e.WebRequest.Path == "/" ? "/*" : e.WebRequest.Path))
                return true;

            if (!e.WebRequest.Paths.Where(p => e.SelectedFilter.DeniedUrlPaths.Contains(p)).Any())
                return true;

            errorOut = "The requested path ( " + e.WebRequest.Path + " ) has been denied, sorry for any inconvenience.";

            return false;

        }

        public static bool IsAcceptContentAllowed(OProxyClientHttp e, out string errorOut)
        {
            errorOut = string.Empty;

            if (string.IsNullOrEmpty(e.WebRequest.Accept))
                return true;

            string[] accept = e.WebRequest.Accept
                .Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
           
            string denied = e.SelectedFilter.DeniedAcceptContentTypes.Where(d => accept.Contains(d)).FirstOrDefault();
           
            if (string.IsNullOrEmpty(denied))
                return true;

            errorOut = "The accept content type ( " + denied + " ) has been denied, sorry for any inconvenience.";

            return false;

        }

        public static bool IsAcceptEncodingAllowed(OProxyClientHttp e, out string errorOut)
        {
            errorOut = string.Empty;

            if (!e.WebRequest.ContainsRequest("accept-encoding"))
                return true;

            string[] encodings = e.WebRequest["accept-encoding"]
                .Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToArray();

            string denied = e.SelectedFilter.DeniedAcceptEncoding.Where(d => encodings.Contains(d)).FirstOrDefault();

            if (string.IsNullOrEmpty(denied))
                return true;

            errorOut = "The accept encoding type ( " + denied + " ) has been denied, sorry for any inconvenience.";

            return false;

        }

        public static bool IsLanguageAllowed(OProxyClientHttp e, out string errorOut)
        {
            errorOut = string.Empty;

            if (!e.WebRequest.ContainsRequest("accept-language"))
                return true;

            string[] languages = e.WebRequest["accept-language"]
                .Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToArray();
           
            string denied = e.SelectedFilter.DeniedLanguages.Where(d => languages.Contains(d)).FirstOrDefault();

            if (string.IsNullOrEmpty(denied))
                return true;

            errorOut = "The language ( " + denied + " ) has been denied, sorry for any inconvenience.";

            return false;

        }

        public static bool IsUserAgentAllowed(OProxyClientHttp e, out string errorOut)
        {
            errorOut = string.Empty;

            if (string.IsNullOrEmpty(e.WebRequest.UserAgent))
                return true;

            string denied = e.SelectedFilter.DeniedUserAgents.Where(d => d == e.WebRequest.UserAgent).FirstOrDefault();
            
            if (string.IsNullOrEmpty(denied))
                return true;

            errorOut = "The user agent ( " + denied + " ) has been denied, sorry for any inconvenience.";

            return false;

        }

        public static bool IsContentTypesAllowed(OProxyClientHttp e, out string errorOut)
        {
            errorOut = string.Empty;

            if (string.IsNullOrEmpty(e.WebRequest.ContentType))
                return true;

            string denied = e.SelectedFilter.DeniedContentTypes.Where(d => d == e.WebRequest.ContentType).FirstOrDefault();

            if (string.IsNullOrEmpty(denied))
                return true;

            errorOut = "The content type ( " + denied + " ) has been denied, sorry for any inconvenience.";

            return false;

        }

        public static bool IsContentLengthAllowed(OProxyClientHttp e, out string errorOut)
        {
            errorOut = string.Empty;

            if (e.SelectedFilter.MaxContentLength == -1)
                return true;

            if (Convert.ToInt64(e.WebRequest.ContentLength) < e.SelectedFilter.MaxContentLength)
                return true;

            errorOut = "The content length received has been denied, sorry for any inconvenience.";

            return false;

        }

        public static bool IsCorsSecFetchModeAllowed(OProxyClientHttp e, out string errorOut)
        {
            errorOut = string.Empty;

            if (string.IsNullOrEmpty(e.WebRequest.SecFetchMode))
                return true;

            string denied = e.SelectedFilter.DeniedCorsSecFetchMode.Where(d => d == e.WebRequest.ContentType).FirstOrDefault();

            if (string.IsNullOrEmpty(denied))
                return true;

            errorOut = "The cors sec-fetch-mode ( " + denied + " ) has been denied, sorry for any inconvenience.";

            return false;

        }

        public static bool IsCorsSecFetchSiteAllowed(OProxyClientHttp e, out string errorOut)
        {

            errorOut = string.Empty;

            if (!e.WebRequest.ContainsRequest("sec-fetch-site"))
                return true;

            string denied = e.SelectedFilter.DeniedCorsSecFetchSite.Where(d => d == e.WebRequest["sec-fetch-site"]).FirstOrDefault();

            if (string.IsNullOrEmpty(denied))
                return true;

            errorOut = "The cors sec-fetch-site ( " + denied + " ) has been denied, sorry for any inconvenience.";

            return false;

        }

        public static bool IsOriginAllowed(OProxyClientHttp e, out string errorOut)
        {
            errorOut = string.Empty;

            if (!e.WebRequest.ContainsRequest("origin"))
                return true;

            string denied = e.SelectedFilter.DeniedOrigins.Where(d => d == e.WebRequest["origin"]).FirstOrDefault();

            if (string.IsNullOrEmpty(denied))
                return true;

            errorOut = "The origin ( " + denied + " ) has been denied, sorry for any inconvenience.";

            return false;

        }

        public static bool IsRefererAllowed(OProxyClientHttp e, out string errorOut)
        {
            errorOut = string.Empty;

            if (string.IsNullOrEmpty(e.WebRequest.Referer))
                return true;

            string denied = e.SelectedFilter.DeniedReferers.Where(d => d == e.WebRequest.Referer).FirstOrDefault();

            if (string.IsNullOrEmpty(denied))
                return true;

            errorOut = "The referer ( " + denied + " ) has been denied, sorry for any inconvenience.";

            return false;

        }

        public static bool IsCustomAllowed(OProxyClientHttp e, out string errorOut)
        {
            
            errorOut        = string.Empty;
            string denied   = string.Empty;

            foreach (var d in e.SelectedFilter.DeniedCustom) 
            { 
                
                switch (d.SearchType) {
                    case OProxyFilterSearchType.KEY:
                        switch (d.Operator) {
                            case OProxyFilterSearchOperator.SPLIT_EQUAL:    denied = KEYSPLIT_EQUAL(e, d); break;
                            case OProxyFilterSearchOperator.EQUAL:          denied = KEYEQUAL(e, d); break;
                            case OProxyFilterSearchOperator.CONTAINS:       denied = KEYCONTAINS(e, d); break;
                            case OProxyFilterSearchOperator.STARTS_WITH:    denied = KEYSTARTS_WITH(e, d); break;
                            case OProxyFilterSearchOperator.ENDS_WITH:      denied = KEYENDS_WITH(e, d); break;
                            case OProxyFilterSearchOperator.REGEX:          denied = KEYREGEX(e, d); break;
                        }
                        break;
                    case OProxyFilterSearchType.VALUE:
                        switch (d.Operator) {
                            case OProxyFilterSearchOperator.SPLIT_EQUAL:    denied = VALSPLIT_EQUAL(e, d); break;
                            case OProxyFilterSearchOperator.EQUAL:          denied = VALEQUAL(e, d); break;
                            case OProxyFilterSearchOperator.CONTAINS:       denied = VALCONTAINS(e, d); break;
                            case OProxyFilterSearchOperator.STARTS_WITH:    denied = VALSTARTS_WITH(e, d); break;
                            case OProxyFilterSearchOperator.ENDS_WITH:      denied = VALENDS_WITH(e, d); break;
                            case OProxyFilterSearchOperator.REGEX:          denied = VALREGEX(e, d); break;
                        }
                        break;
                }

                if (!string.IsNullOrEmpty(denied))
                    break;
            }

            if (string.IsNullOrEmpty(denied))
                return true;

            errorOut = "The server filter found: ( " + denied + " ) which was denied, sorry for any inconvenience.";

            return false;

        }

        #region String Searches

        public static string KEYSPLIT_EQUAL(OProxyClientHttp client, IProxyFilterSearch search) {
            return search.Key.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray()
                    .Where(k => client.WebRequest.Requests.Keys.Contains(k))
                    .FirstOrDefault();
        }

        public static string KEYEQUAL(OProxyClientHttp client, IProxyFilterSearch search) {
            return client.WebRequest.Requests.Keys.Where(k => k == search.Key).FirstOrDefault();
        }

        public static string KEYCONTAINS (OProxyClientHttp client, IProxyFilterSearch search) {
            return client.WebRequest.Requests.Keys.Where(k => k.Contains(search.Key)).FirstOrDefault();
        }

        public static string KEYSTARTS_WITH(OProxyClientHttp client, IProxyFilterSearch search) {
            return client.WebRequest.Requests.Keys.Where(k => k.StartsWith(search.Key)).FirstOrDefault();
        }

        public static string KEYENDS_WITH(OProxyClientHttp client, IProxyFilterSearch search) {
            return client.WebRequest.Requests.Keys.Where(k => k.EndsWith(search.Key)).FirstOrDefault();
        }

        public static string KEYREGEX(OProxyClientHttp client, IProxyFilterSearch search) {
            Regex expression = new(search.Key, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            foreach (string key in client.WebRequest.Requests.Keys)
            {
                MatchCollection found = expression.Matches(client.WebRequest[key]);
                if (found.Count > 0)
                    return found[0].Value;
            }
            return string.Empty;
        }

        public static string VALSPLIT_EQUAL(OProxyClientHttp client, IProxyFilterSearch search) {

            if (client.WebRequest.ContainsRequest(search.Key))
                return string.Empty;

            return client.WebRequest[search.Key]
                .Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(k => k == search.Value)
                .FirstOrDefault();

        }

        public static string VALEQUAL(OProxyClientHttp client, IProxyFilterSearch search) {

            if (client.WebRequest.ContainsRequest(search.Key))
                return string.Empty;

            return client.WebRequest[search.Key] == search.Value ? client.WebRequest[search.Key] : string.Empty;

        }

        public static string VALCONTAINS(OProxyClientHttp client, IProxyFilterSearch search) {

            if (client.WebRequest.ContainsRequest(search.Key))
                return string.Empty;

            return client.WebRequest[search.Key].Contains(search.Value) ? client.WebRequest[search.Key] : string.Empty;
        }

        public static string VALSTARTS_WITH(OProxyClientHttp client, IProxyFilterSearch search) {

            if (client.WebRequest.ContainsRequest(search.Key))
                return string.Empty;

            return client.WebRequest[search.Key].StartsWith(search.Value) ? client.WebRequest[search.Key] : string.Empty;
        }

        public static string VALENDS_WITH(OProxyClientHttp client, IProxyFilterSearch search) {

            if (client.WebRequest.ContainsRequest(search.Key))
                return string.Empty;

            return client.WebRequest[search.Key].EndsWith(search.Value) ? client.WebRequest[search.Key] : string.Empty;

        }

        public static string VALREGEX(OProxyClientHttp client, IProxyFilterSearch search) {

            if (client.WebRequest.ContainsRequest(search.Key))
                return string.Empty;

            Regex expression = new(search.Value, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            MatchCollection found = expression.Matches(client.WebRequest[search.Key]);
            if (found.Count > 0)
                return found[0].Value;

            return string.Empty;

        }

        #endregion

        #endregion

        #region Network Methods and Functions

        public static void BufferWriteNonAsync(this Stream e, byte[] buffer, int writeSize)
        {
         
            int a; 

            byte[] b = new byte[writeSize]; 

            MemoryStream c = new(buffer);

            while ((a = c.Read(b, 0, b.Length)) > 0) 
            {

                e.Write(b, 0, a);

                e.Flush();

            }

            c.Close();

            c.Dispose();

            Array.Clear(b, 0, b.Length);

        }
        
        public static void BufferWriteNonAsync(this Stream e, byte[] buffer, int length, int writeSize)
        {

            int a;

            byte[] b = new byte[writeSize];

            MemoryStream c = new();
          
            c.Write(buffer, 0, length);

            while ((a = c.Read(b, 0, b.Length)) > 0)
            {

                e.Write(b, 0, a);

                e.Flush();

            }

            c.Close();

            c.Dispose();

            Array.Clear(b, 0, b.Length);

        }

        public static byte[] BufferReadNonAsync(this NetworkStream e, byte[] intialBuffer, int intialLength, int readSize)
        {

            int a; byte[] b = new byte[readSize]; MemoryStream c = new();

            c.Write(intialBuffer, 0, intialLength);

            if (e.DataAvailable)
                do {
                    a = e.Read(b, 0, b.Length);
                    c.Write(b, 0, a);
                } while (e.DataAvailable);

            e.Flush();

            byte[] output = c.ToArray();

            c.Close();

            c.Dispose();

            return output;

        }

        public static byte[] BufferReadNonAsync(this NetworkStream e, int readSize)
        {

            int a; byte[] b = new byte[readSize]; MemoryStream c = new();

            if (e.DataAvailable)
                do {
                    a = e.Read(b, 0, b.Length);
                    c.Write(b, 0, a);
                } while (e.DataAvailable);

            e.Flush();

            byte[] output = c.ToArray();

            c.Close();

            c.Dispose();

            return output;

        }

        public static byte[] BufferReadNonAsync(this Stream e, NetworkStream innerStream, byte[] intialBuffer, int intialLength, int readSize)
        {

            int a; byte[] b = new byte[readSize]; MemoryStream c = new();

            c.Write(intialBuffer, 0, intialLength);

            if (innerStream.DataAvailable)
                do
                {
                    a = e.Read(b, 0, b.Length);
                    c.Write(b, 0, a);
                } while (innerStream.DataAvailable);

            e.Flush();

            byte[] output = c.ToArray();

            c.Close();

            c.Dispose();

            return output;

        }

        public static byte[] BufferReadNonAsync(this Stream e, NetworkStream innerStream, int readSize)
        {

            int a; byte[] b = new byte[readSize]; MemoryStream c = new();

            if (innerStream.DataAvailable)
                do
                {
                    a = e.Read(b, 0, b.Length);
                    c.Write(b, 0, a);
                } while (innerStream.DataAvailable);

            e.Flush();

            byte[] output = c.ToArray();

            c.Close();

            c.Dispose();

            return output;

        }

        public static void BufferWriteAsync(this byte[] e, Stream stream, int chunkSize, OnWriteAsyncComplete callback)
        {

            if (chunkSize > e.Length)
                chunkSize = e.Length;

            stream.BeginWrite(e, 0, chunkSize, a => {

                lock (stream)
                    stream.EndWrite(a);

                stream.Flush();

                if ((e.Length - chunkSize) > 0)
                {

                    byte[] n = e.BufferShrink(chunkSize, (e.Length - chunkSize));

                    e.Dispose();

                    n.BufferWriteAsync(stream, chunkSize, callback);

                }
                else
                {
                    e.Dispose();

                    callback?.Invoke(stream);
                }

            }, stream);



        }

        public static void BufferReadHttpAsync(this Stream e, MemoryStream stream, int chunkSize, OnReadAsyncComplete callback)
        {

            byte[] b = new byte[chunkSize];

            e.BeginRead(b, 0, b.Length, o => {

                int read;

                lock (e)
                    read = e.EndRead(o);

                if (read > 0)  { 

                    e.Flush();

                    stream.Write(b, 0, read);

                    OProxyHttpRequest v = new(stream.ToArray());

                    b.Dispose();

                    if (v.Header != null && v.Header.Length > 0)  {

                        string k = v.RequestData.Fracture("\r\n")
                            .Where(c => c.ToLower().StartsWith("content-length"))
                            .FirstOrDefault();
                        
                        if (!string.IsNullOrEmpty(k))
                            if (v.Body.Length != Convert.ToInt64(k.Fracture(": ")[1]))
                                e.BufferReadHttpAsync(stream, chunkSize, callback);
                            else
                                callback?.Invoke(e, stream);
                        else
                            callback?.Invoke(e, stream);

                        v.Dispose();
                    }
                    else
                        callback?.Invoke(e, stream);
                }
                else
                    callback?.Invoke(e, stream);

            }, e);

        }

        public static byte[] BufferShrink(this byte[] e, int newLength) {
            byte[] a = new byte[newLength];
            Buffer.BlockCopy(e, 0, a, 0, a.Length);
            return a;
        }

        public static byte[] BufferShrink(this byte[] e, int fromOffSet, int newLength)
        {
            byte[] a = new byte[newLength];
            Buffer.BlockCopy(e, fromOffSet, a, 0, a.Length);
            return a;
        }

        public static byte[] CompressGZIP(byte[] data)
        {

            MemoryStream streamoutput = new();
            GZipStream gzip = new(streamoutput, CompressionMode.Compress, false);

            gzip.Write(data, 0, data.Length);
            gzip.Close();

            return streamoutput.ToArray();

        }

        public static OProxyHttpMethodType GetMethodType(string e)
        {

            var result = (e.ToLower()) switch
            {
                "get" => OProxyHttpMethodType.GET,
                "head" => OProxyHttpMethodType.HEAD,
                "post" => OProxyHttpMethodType.POST,
                "put" => OProxyHttpMethodType.PUT,
                "delete" => OProxyHttpMethodType.DELETE,
                "connect" => OProxyHttpMethodType.CONNECT,
                "options" => OProxyHttpMethodType.OPTIONS,
                "trace" => OProxyHttpMethodType.TRACE,
                "patch" => OProxyHttpMethodType.PATCH,
                _ => OProxyHttpMethodType.NONE,
            };
            return result;

        }

        public static bool IsStaticResource(string resource)
        {
            return Regex.IsMatch(resource, @"(.*?)\.(ico|css|gif|jpg|jpeg|png|js|xml|ttf)$");
        }

        public static string RemoveToken(string completeReq)
        {
            if (completeReq.Contains("__"))
                completeReq = completeReq.Split(new string[] { "__" }, StringSplitOptions.RemoveEmptyEntries)[0];

            string[] _pReq = completeReq.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries);

            if (_pReq.Length < 2)
                return string.Empty;
            else
                return _pReq[1];

        }

        public static X509Certificate2 GetCertificate(string[] certNames)
        {

            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);

            store.Open(OpenFlags.ReadWrite);

            foreach (string certName in certNames)
            {

                X509Certificate2[] existingCerts = store.Certificates.Find(X509FindType.FindBySubjectName, certName, false).OfType<X509Certificate2>().ToArray();

                if (existingCerts.Any())
                {
                    store.Close();
                    return existingCerts.Where(c => c.Subject.ToLower().Contains("cn=" + certName)).FirstOrDefault();
                }

            }

            return null;

        }

        public static IPAddress GetLocalExternalIp(this string Host)
        {
            try
            {
                IPHostEntry he = Dns.GetHostEntry(Host);
                for (int n = 0; n <= he.AddressList.Length; n++)
                {
                    if (IsRemoteIp(he.AddressList[n]))
                    {
                        return he.AddressList[n];
                    }
                }
                return he.AddressList[0];
            }
            catch
            {
                return IPAddress.Any;
            }
        }

        public static IPAddress GetLocalInternalIp()
        {
            IPHostEntry he = Dns.GetHostEntry(Dns.GetHostName());
            try
            {
                for (int n = 0; n <= he.AddressList.Length; n++)
                {
                    if (IsLocalIp(he.AddressList[n]))
                    {
                        if (!(he.AddressList[n].ToString() == "127.0.0.1"))
                            return he.AddressList[n];
                    }
                }
                return he.AddressList[0];
            }
            catch
            {
                return he.AddressList[0];
            }
        }

        public static bool IsRemoteIp(this IPAddress IP)
        {
            byte First = Convert.ToByte(Math.Floor(BitConverter.ToDouble(IP.GetAddressBytes(), 0) % 256));
            byte Second = Convert.ToByte(Math.Floor((BitConverter.ToDouble(IP.GetAddressBytes(), 0) % 65536) / 256));
            //Not 10.x.x.x And Not 172.16.x.x <-> 172.31.x.x And Not 192.168.x.x 
            //And Not Any And Not Loopback And Not Broadcast 
            return (First != 10) && (First != 172 || (Second < 16 || Second > 31)) && (First != 192 || Second != 168) && (!IP.Equals(IPAddress.Any)) && (!IP.Equals(IPAddress.Loopback)) && (!IP.Equals(IPAddress.Broadcast));
        }

        public static bool IsRemoteIp(this string IP)
        {
            string[] IPArray = Microsoft.VisualBasic.Strings.Split(IP, ".");
            byte First = Convert.ToByte(IPArray[0].ToString());
            byte Second = Convert.ToByte(IPArray[1].ToString());
            //Not 10.x.x.x And Not 172.16.x.x <-> 172.31.x.x And Not 192.168.x.x 
            //And Not Any And Not Loopback And Not Broadcast 
            return (First != 10) && (First != 172 || (Second < 16 || Second > 31)) && (First != 192 || Second != 168) && (!IP.Equals(IPAddress.Any)) && (!IP.Equals(IPAddress.Loopback)) && (!IP.Equals(IPAddress.Broadcast));
        }

        public static bool IsLocalIp(this string IP)
        {
            string[] IPArray = Microsoft.VisualBasic.Strings.Split(IP, ".");
            byte First = Convert.ToByte(IPArray[0].ToString());
            byte Second = Convert.ToByte(IPArray[1].ToString());
            //10.x.x.x Or 172.16.x.x <-> 172.31.x.x Or 192.168.x.x 
            return (First == 10) || (First == 172 && (Second >= 16 && Second <= 31)) || (First == 192 && Second == 168);
        }

        public static bool IsLocalIp(this IPAddress IP)
        {
            byte First = Convert.ToByte(Math.Floor(BitConverter.ToDouble(IP.GetAddressBytes(), 0) % 256));
            byte Second = Convert.ToByte(Math.Floor((BitConverter.ToDouble(IP.GetAddressBytes(), 0) % 65536) / 256));
            //byte Second = (byte)Math.Floor((IP.Address % 65536) / 256); 
            //10.x.x.x Or 172.16.x.x <-> 172.31.x.x Or 192.168.x.x 
            return (First == 10) || (First == 172 && (Second >= 16 && Second <= 31)) || (First == 192 && Second == 168);
        }

        public static long Bytes2Long(byte[] address)
        {
            long ipnum = 0;
            for (int i = 0; i < 4; ++i)
            {
                long y = address[i];
                if (y < 0)
                    y += 256;
                ipnum += y << ((3 - i) * 8);
            }
            return ipnum;
        }

        public static bool PortNumberInRange(int port)
        {
            if (port >= 0 & port <= 65535)
                return true;
            return false;
        }

        public static string GetIpAddress(this UnicastIPAddressInformationCollection ips, int IpVersion)
        {
            string retip = "invalid.";
            if (IpVersion == 4)
            {
                foreach (UnicastIPAddressInformation ip in ips)
                {
                    if (!ip.Address.ToString().Contains("::"))
                        retip = ip.Address.ToString();
                }
            }
            else if (IpVersion == 6)
            {
                foreach (UnicastIPAddressInformation ip in ips)
                {
                    if (ip.Address.ToString().Contains("::"))
                        retip = ip.Address.ToString();
                }
            }
            return retip;
        }

        public static string GetIpAddress(this IPAddressCollection ips, int IpVersion)
        {
            string retip = "invalid.";
            if (IpVersion == 4)
            {
                foreach (IPAddress ip in ips)
                {
                    if (!ip.ToString().Contains("::"))
                        retip = ip.ToString();
                }
            }
            else if (IpVersion == 6)
            {
                foreach (IPAddress ip in ips)
                {
                    if (ip.ToString().Contains("::"))
                        retip = ip.ToString();
                }
            }
            return retip;
        }

        public static string GetIpAddress(this IPAddress[] ips, int IpVersion)
        {
            string retip = "invalid.";
            if (IpVersion == 4)
            {
                foreach (IPAddress ip in ips)
                {
                    if (!ip.ToString().Contains("::"))
                        retip = ip.ToString();
                }
            }
            else if (IpVersion == 6)
            {
                foreach (IPAddress ip in ips)
                {
                    if (ip.ToString().Contains("::"))
                        retip = ip.ToString();
                }
            }
            return retip;
        }

        public static string GetIpAddress(this PcapAddress[] ips, int IpVersion)
        {
            string retip = "invalid.";
            if (IpVersion == 4)
            {
                foreach (PcapAddress ip in ips)
                {
                    if (ip.Addr.ipAddress != null)
                    {
                        if (!ip.Addr.ipAddress.ToString().Contains(":"))
                        {
                            retip = ip.Addr.ipAddress.ToString();
                            break;
                        }
                    }
                }
            }
            else if (IpVersion == 6)
            {
                foreach (PcapAddress ip in ips)
                {
                    if (ip.Addr.ipAddress != null)
                    {
                        if (ip.Addr.ipAddress.ToString().Contains(":"))
                        {
                            retip = ip.Addr.ipAddress.ToString();
                            break;
                        }
                    }
                }
            }
            return retip;
        }

        public static IPAddress GetIpAddress(this IPAddress[] ips, int IpVersion, bool overload)
        {

            string retip = "invalid.";
            bool temp = overload;

            if (temp)
                retip = "invalid.";

            if (IpVersion == 4)
            {
                foreach (IPAddress ip in ips)
                {
                    if (!ip.ToString().Contains("::"))
                    {
                        retip = ip.ToString();
                        break;
                    }
                }
            }
            else if (IpVersion == 6)
            {
                foreach (IPAddress ip in ips)
                {
                    if (ip.ToString().Contains("::"))
                    {
                        retip = ip.ToString();
                        break;
                    }
                }
            }
            return IPAddress.Parse(retip);
        }

        public static string[] GetNetworkInterfaces()
        {
            string[] adapters = new string[NetworkInterface.GetAllNetworkInterfaces()[0].GetIPProperties().UnicastAddresses.Count];
            for (int i = 0; i <= adapters.Length - 1; i++)
            {
                adapters[i] = NetworkInterface.GetAllNetworkInterfaces()[0].GetIPProperties().UnicastAddresses[i].Address.ToString();
            }
            return adapters;
        }

        public static bool CheckForTheInternet(string uri)
        {
            try
            {
                if (!NetworkPing(uri, 200))
                    return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool NetworkPing(string mServer, int mTimeout)
        {
            try
            {
                Ping p = new();
                PingReply pr = p.Send(mServer, mTimeout);
                if (pr.Status == IPStatus.Success)
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static SocketType GetSocketType(ProtocolType e)
        {
            return e switch
            {
                ProtocolType.IP => SocketType.Stream,
                ProtocolType.Tcp => SocketType.Stream,
                ProtocolType.Udp => SocketType.Dgram,
                ProtocolType.Icmp => SocketType.Raw,
                ProtocolType.Igmp => SocketType.Raw,
                _ => SocketType.Unknown,
            };
        }

        public static uint StringIPAddressToUInt32(string ip)
        {
            // convert string IP to uint IP e.g. "1.2.3.4" -> 16909060

            IPAddress i = System.Net.IPAddress.Parse(ip);
            byte[] ipByteArray = i.GetAddressBytes();

            uint ipUint = (uint)ipByteArray[0] << 24;
            ipUint += (uint)ipByteArray[1] << 16;
            ipUint += (uint)ipByteArray[2] << 8;
            ipUint += (uint)ipByteArray[3];

            return ipUint;
        }

        public static string UInt32IPAddressToString(uint ip)
        {
            // convert uint IP to string IP e.g. 16909060 -> "1.2.3.4"

            IPAddress i = new(ip);
            string[] ipArray = i.ToString().Split('.');

            return ipArray[3] + "." + ipArray[2] + "." + ipArray[1] + "." + ipArray[0];
        }

        public static IPAddress GenerateIpAddress(int trys)
        {

            if (trys == 30)
                return null;

            Random r = new();
            IPAddress ip = null;

            try
            {
                ip = new IPAddress(((int)r.Next(1, 255) << 24) + ((int)r.Next(1, 255) << 16) + ((int)r.Next(1, 255) << 8) + (int)r.Next(1, 255));
            }
            catch
            {
                GenerateIpAddress((trys + 1));
            }

            return ip;

        }

        public static IPAddress IpNumber2IpAddress(long e)
        {
            string ip = string.Empty;
            for (int i = 0; i < 4; i++)
            {
                int num = (int)(e / Math.Pow(256, (3 - i)));
                e -= (long)(num * Math.Pow(256, (3 - i)));
                if (i == 0)
                    ip = num.ToString();
                else
                    ip += "." + num.ToString();
            }
            return IPAddress.Parse(ip);
        }

        public static long IpAddress2IpNumber(this IPAddress e)
        {
            string[] ipBytes;
            double num = 0;

            ipBytes = e.ToString().Split('.');

            for (int i = ipBytes.Length - 1; i >= 0; i--)
                num += ((int.Parse(ipBytes[i]) % 256) * Math.Pow(256, (3 - i)));

            return (long)num;
        }

        public static string MacAddressGenerator()
        {

            const string _allPossibleCharacters = "0123456789ABCDEF";
            Random _randomGenerator = new();
            StringBuilder macAddress = new();

            for (int i2 = 0; i2 < 12; i2++)
                macAddress.Append(_allPossibleCharacters[_randomGenerator.Next(16)]);

            string ret = string.Empty;

            for (int i = 0; i < 12; i += 2)
                ret += macAddress.ToString().Substring(i, 2) + ":";

            return ret.Remove(ret.Length - 1, 1);

        }

        public static string FormatMacAddress(this string mac)
        {
            try
            {
                string newmac = string.Empty;
                List<string> listx = new();
                for (int x = 0; x <= mac.Length - 1; x += 2)
                {
                    listx.Add(mac.Substring(x, 2));
                }
                for (int x = 0; x <= listx.Count - 1; x++)
                {
                    newmac += listx[x].ToString() + ":";
                }
                newmac = newmac.Remove(newmac.Length - 1);
                return newmac;
            }
            catch
            {
                return mac;
            }
        }

        public static IPAddress LocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                return IPAddress.None;

            return Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

        public static IPAddress GetLocalIPv4(NetworkInterfaceType type)
        {

            IPAddress output = IPAddress.None;

            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {

                if (item.NetworkInterfaceType == type && item.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties adapterProperties = item.GetIPProperties();

                    if (adapterProperties.GatewayAddresses.FirstOrDefault() != null)
                        foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                output = ip.Address;
                                break;
                            }

                }

                if (output != IPAddress.None)
                    break;

            }

            return output;
        }

        #endregion

    }

}
