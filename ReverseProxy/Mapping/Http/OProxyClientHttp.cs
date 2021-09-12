/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

using K2host.Sockets.Tcp;
using K2host.Sockets.ReverseProxy.Abstract;
using K2host.Sockets.ReverseProxy.Extentions;
using K2host.Sockets.ReverseProxy.Interface;
using K2host.Sockets.Delegates;

using gs = K2host.Sockets.OHelpers;

namespace K2host.Sockets.ReverseProxy.Mapping.Http
{

    /// <summary>
    /// The client for port mapping with http filter intevention.
    /// </summary>
    public class OProxyClientHttp : AProxyClient
    {

        /// <summary>
        /// The selected filter based on the incomming request.
        /// </summary>
        public IProxyFilter SelectedFilter { get; set; }

        /// <summary>
        /// The listed filters to use when a connection is made.
        /// </summary>
        public IProxyFilter[] HttpFilters { get; set; }

        /// <summary>
        /// The request pased and used to filter the traffic.
        /// </summary>
        public IProxyHttpRequest WebRequest { get; set; }

        /// <summary>
        /// The server name indication data used to determin and ssl / tls connection stream.
        /// </summary>
        public OServerSNI ServerSNI { get; set; }

        /// <summary>
        /// The certificate used when creating the stream to filter data on the incomming traffic.
        /// </summary>
        public X509Certificate2 AssignedX509Certificate { get; set; }

        /// <summary>
        /// The constuctor used to create an instance from the inherited type.
        /// </summary>
        public OProxyClientHttp() 
        {



        }

        /// <summary>
        /// The overridden method for starting the connection process.
        /// Which also starts the relay of network information when connected.
        /// </summary>
        public override void StartHandShake()
        {
            try
            {
                //This slows the connection to makesure we can detect all the SNI data on connection.
                Thread.Sleep(40);

                ServerSNI           = new OServerSNI(ClientSocket);
                ClientInnerStream   = new NetworkStream(ClientSocket) { ReadTimeout = 5000, WriteTimeout = 5000 };
                ClientStream        = ClientInnerStream;

                OnClientRelayData   = new OnProxyClientProcessRelay(this.OnProcessRequest);
                OnRemoteRelayData   = new OnProxyClientProcessRelay(this.OnProcessResponse);

                //If there is SNI we know we are a https connection and can deal with the cert and mapping correctly
                if (!ServerSNI.IsValid)
                    ClientStream.BufferReadHttpAsync(new MemoryStream(), (4096 * 2), (a, b) => { this.OnClientConneced(b); });
                else
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    SelectedFilter          = HttpFilters.Where(f => f.UrlHostName == ServerSNI.ServerNameIndication).FirstOrDefault();
                    AssignedX509Certificate = gs.GetCertificate(SelectedFilter.SslCertificates);

                    ClientStream = new SslStream(ClientInnerStream, false);
                    ((SslStream)ClientStream).AuthenticateAsServer(AssignedX509Certificate, false, SslProtocols.Tls12 | SslProtocols.Tls13, false);
                    ClientStream.BufferReadHttpAsync(new MemoryStream(), (4096 * 2), (a, b) => { this.OnClientConneced(b); });
                }

            }
            catch (Exception)
            {
                if (!IsDisposed)
                    Dispose();
            }
        }

    }

}
