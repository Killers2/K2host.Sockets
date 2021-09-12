/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Collections.Generic;

using K2host.Sockets.Delegates;

namespace K2host.Sockets.ReverseProxy.Interface
{

    /// <summary>
    /// This is used to help create the object class you define.
    /// </summary>
    public interface IProxyFilter : IDisposable
    {

        /// <summary>
        /// The given name for the filter on this listener.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The web host name of the web site.
        /// </summary>
        string UrlHostName { get; set; }

        /// <summary>
        /// The ssl certificate names you want to use for the hostname.
        /// </summary>
        string[] SslCertificates { get; set; }

        /// <summary>
        /// The denyed accept content types read from the web request.
        /// </summary>
        string[] DeniedAcceptContentTypes { get; set; }

        /// <summary>
        /// The denyed url paths read from the web request.
        /// </summary>
        string[] DeniedUrlPaths { get; set; }

        /// <summary>
        /// The denyed accept encoding types read from the web request.
        /// </summary>
        string[] DeniedAcceptEncoding { get; set; }

        /// <summary>
        /// The denyed languages read from the web request.
        /// </summary>
        string[] DeniedLanguages { get; set; }

        /// <summary>
        /// The denyed user agents read from the web request.
        /// </summary>
        string[] DeniedUserAgents { get; set; }

        /// <summary>
        /// The denyed content types read from the web request.
        /// </summary>
        string[] DeniedContentTypes { get; set; }

        /// <summary>
        /// The denyed max content length read from the web request.
        /// </summary>
        long MaxContentLength { get; set; }

        /// <summary>
        /// The denyed cors Sec-Fetch-Mode from the web request.
        /// </summary>
        string[] DeniedCorsSecFetchMode { get; set; }

        /// <summary>
        /// The denyed cors Sec-Fetch-Site from the web request.
        /// </summary>
        string[] DeniedCorsSecFetchSite { get; set; }

        /// <summary>
        /// The denyed referers from the web request.
        /// </summary>
        string[] DeniedReferers { get; set; }

        /// <summary>
        /// The denyed cors origins from the web request.
        /// </summary>
        string[] DeniedOrigins { get; set; }

        /// <summary>
        /// The username and passwords in basic base64 if required for this proxy filter
        /// </summary>
        string[] ProxyAuthorizationBasic { get; set; }

        /// <summary>
        /// The list of custom search or compare filters to use on the header of a webrequest.
        /// </summary>
        IProxyFilterSearch[] DeniedCustom { get; set; }

        /// <summary>
        /// Allows a hidden redirect to the client based on the header
        /// The header in the request must be revprox-url-rewrite.
        /// </summary>
        IDictionary<string, string> UrlReWrites { get; set; }

        /// <summary>
        /// Allows a hidden remap to another <see cref="IProxyFailOver"/> based on the header
        /// The header in the request must be revprox-map-service.
        /// </summary>
        IDictionary<string, IProxyFailOver> MappedServices { get; set; }

        /// <summary>
        /// The list of fail overs servers running the same web sites.
        /// </summary>
        IProxyFailOver[] Failovers { get; set; }

        /// <summary>
        /// The current selected fail over based on the connectivity and latency.
        /// </summary>
        IProxyFailOver CurrentFailover { get; set; }

        /// <summary>
        /// The latency time out for thew filter to select a different failover item.
        /// </summary>
        int LatencyTimeOut { get; set; }
       
        /// <summary>
        /// This list is comiled a the startup of the filter and the initiation of the listener
        /// This list is used to run filters on the header of any http requests
        /// </summary>
        IList<OnProxyHttpFilterCheck> CheckList { get; set; }
        
        /// <summary>
        /// Used to enable the capture of requests comming in on the same relay of the proxy.
        /// </summary>
        bool EnableRequestRelayCapture { get; set; }

        /// <summary>
        /// Used to enable the capture of reponses comming out on the same relay of the proxy.
        /// </summary>
        bool EnableResponseRelayCapture { get; set; }

    }

}
