/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using K2host.Sockets.Delegates;
using K2host.Sockets.ReverseProxy.Interface;

namespace K2host.Sockets.ReverseProxy.Mapping.Http
{

    /// <summary>
    /// This class is used to create an instance of an filter item
    /// </summary>
    public class OProxyFilter : IProxyFilter
    {

        /// <summary>
        /// The given name for the filter on this listener.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The web host name of the web site.
        /// </summary>
        public string UrlHostName { get; set; }

        /// <summary>
        /// The ssl certificate names you want to use for the hostname.
        /// </summary>
        public string[] SslCertificates { get; set; }

        /// <summary>
        /// The denyed content types read from the web request.
        /// </summary>
        public string[] DeniedAcceptContentTypes { get; set; }

        /// <summary>
        /// The denyed url paths read from the web request.
        /// </summary>
        public string[] DeniedUrlPaths { get; set; }

        /// <summary>
        /// The denyed encoding types read from the web request.
        /// </summary>
        public string[] DeniedAcceptEncoding { get; set; }

        /// <summary>
        /// The denyed languages read from the web request.
        /// </summary>
        public string[] DeniedLanguages { get; set; }

        /// <summary>
        /// The denyed user agents read from the web request.
        /// </summary>
        public string[] DeniedUserAgents { get; set; }

        /// <summary>
        /// The denyed content types read from the web request.
        /// </summary>
        public string[] DeniedContentTypes { get; set; }

        /// <summary>
        /// The denyed max content length read from the web request.
        /// </summary>
        public long MaxContentLength { get; set; }

        /// <summary>
        /// The denyed cors Sec-Fetch-Mode from the web request.
        /// </summary>
        public string[] DeniedCorsSecFetchMode { get; set; }

        /// <summary>
        /// The denyed cors Sec-Fetch-Site from the web request.
        /// </summary>
        public string[] DeniedCorsSecFetchSite { get; set; }

        /// <summary>
        /// The denyed referers from the web request.
        /// </summary>
        public string[] DeniedReferers { get; set; }

        /// <summary>
        /// The denyed cors origins from the web request.
        /// </summary>
        public string[] DeniedOrigins { get; set; }

        /// <summary>
        /// The username and passwords in basic base64 if required for this proxy filter
        /// The header in the request must be revprox-authorization.
        /// </summary>
        public string[] ProxyAuthorizationBasic { get; set; }

        /// <summary>
        /// The list of custom search or compare filters to use on the header of a webrequest.
        /// </summary>
        public IProxyFilterSearch[] DeniedCustom { get; set; }

        /// <summary>
        /// Allows a hidden redirect to the client based on the header
        /// The header in the request must be revprox-url-rewrite.
        /// </summary>
        public IDictionary<string, string> UrlReWrites { get; set; }

        /// <summary>
        /// Allows a hidden remap to another <see cref="IProxyFailOver"/> based on the header
        /// The header in the request must be revprox-map-service.
        /// </summary>
        public IDictionary<string, IProxyFailOver> MappedServices { get; set; }

        /// <summary>
        /// The list of fail overs servers running the same web sites.
        /// </summary>
        public IProxyFailOver[] Failovers { get; set; }

        /// <summary>
        /// The current selected fail over based on the connectivity and latency.
        /// </summary>
        public IProxyFailOver CurrentFailover { get; set; }

        /// <summary>
        /// The latency time out for thew filter to select a different failover item.
        /// </summary>
        public int LatencyTimeOut { get; set; }

        /// <summary>
        /// This list is comiled a the startup of the filter and the initiation of the listener
        /// This list is used to run filters on the header of any http requests
        /// </summary>
        [JsonIgnore]
        public IList<OnProxyHttpFilterCheck> CheckList { get; set; }

        /// <summary>
        /// Used to enable the capture of requests comming in on the same relay of the proxy.
        /// </summary>
        public bool EnableRequestRelayCapture { get; set; }

        /// <summary>
        /// Used to enable the capture of reponses comming out on the same relay of the proxy.
        /// </summary>
        public bool EnableResponseRelayCapture { get; set; }

        /// <summary>
        /// The constuctor used to create an instance from the inherited type.
        /// </summary>
        public OProxyFilter() 
        {

            SslCertificates             = Array.Empty<string>();
            DeniedAcceptContentTypes    = Array.Empty<string>();
            DeniedUrlPaths              = Array.Empty<string>();
            DeniedAcceptEncoding        = Array.Empty<string>();
            DeniedLanguages             = Array.Empty<string>();
            DeniedUserAgents            = Array.Empty<string>();
            DeniedContentTypes          = Array.Empty<string>();
            MaxContentLength            = -1;
            DeniedCorsSecFetchMode      = Array.Empty<string>();
            DeniedCorsSecFetchSite      = Array.Empty<string>();
            DeniedReferers              = Array.Empty<string>();
            DeniedOrigins               = Array.Empty<string>();
            ProxyAuthorizationBasic     = Array.Empty<string>();
            DeniedCustom                = Array.Empty<IProxyFilterSearch>();
            UrlReWrites                 = new Dictionary<string, string>();
            MappedServices              = new Dictionary<string, IProxyFailOver>();
            Failovers                   = Array.Empty<IProxyFailOver>();
            LatencyTimeOut              = 100;
            CheckList                   = new List<OnProxyHttpFilterCheck>();
            EnableRequestRelayCapture   = false;
            EnableResponseRelayCapture  = false;
        }

        /// <summary>
        /// This is used on disposable objects and used generically for remotely disposing a client.
        /// </summary>
        bool IsDisposed = false;

        /// <summary>
        /// Used to dispose the object instance from the abstract class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The method used to dispose the properties in the instance of this class.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {

            if (!IsDisposed)
                if (disposing)
                {


                }

            IsDisposed = true;
        }

    }

}
