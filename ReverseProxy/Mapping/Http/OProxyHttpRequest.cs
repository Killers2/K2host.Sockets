/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Collections.Generic;

using K2host.Sockets.Raw.Enums;
using K2host.Sockets.ReverseProxy.Extentions;
using K2host.Sockets.ReverseProxy.Interface;

namespace K2host.Sockets.ReverseProxy.Mapping.Http
{
    
    /// <summary>
    /// This class is used to collect and set data for web http comunication between a client and server web pages.
    /// </summary>
    public class OProxyHttpRequest : IProxyHttpRequest
    {

        /// <summary>
        /// This holds the request headers in a default keyValuePair
        /// </summary>
        public IDictionary<string, string> Requests { get; set; }

        /// <summary>
        /// This helps get the values by default as in the class.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string this[string item]
        {
            get
            {
                try
                {
                    return Requests[item];
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                try
                {
                    Requests[item] = value;
                }
                catch { }
            }
        }

        /// <summary>
        /// This hold the header data from the stream.
        /// </summary>
        public byte[] Header { get; set; }

        /// <summary>
        /// This hold the body data from the stream.
        /// </summary>
        public byte[] Body { get; set; }

        /// <summary>
        /// This hold the method type of <see cref="OProxyHttpMethodType"/>
        /// </summary>
        public OProxyHttpMethodType Method { get; set; }

        /// <summary>
        /// This hold the request type of <see cref="OProxyHttpRequestType"/>
        /// </summary>
        public OProxyHttpRequestType Type { get; set; }

        /// <summary>
        /// The complete path as a string excluding the querystring.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// This hold the path as a list of path segments.
        /// </summary
        public IList<string> Paths { get; set; }

        /// <summary>
        /// The complete path as a string including the querystring.
        /// </summary>
        public string CompletePath { get; set; }

        /// <summary>
        /// This holds any query string or path routing values
        /// </summary>
        public IDictionary<string, string> Parms { get; set; }

        /// <summary>
        /// This hold the boundary elements send over as multipart/form-data
        /// </summary>
        public IDictionary<string, IProxyHttpRequestBoundray> Boundries { get; set; }

        /// <summary>
        /// The http header referer
        /// </summary>
        public string Referer { get; set; }
        
        /// <summary>
        /// The http header Access-Control-Request-Method for CORS
        /// </summary>
        public string AccessControlRequestMethod { get; set; }

        /// <summary>
        /// The http header Access-Control-Request-Headers for CORS
        /// </summary>
        public string AccessControlRequestHeaders { get; set; }

        /// <summary>
        /// The http header Origin for CORS
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// The http header Sec-Fetch-Mode for CORS
        /// </summary
        public string SecFetchMode { get; set; }

        /// <summary>
        /// The http header for user-agent
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// The http header content-length
        /// </summary>
        public int ContentLength { get; set; }

        /// <summary>
        /// The http header host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// This hold the Url information.
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        /// The http protocol / schema
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// The http content type
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The http header 'via' normally filled from a proxy server
        /// </summary>
        public string Via { get; set; }

        /// <summary>
        /// The http accept header
        /// </summary>
        public string Accept { get; set; }

        /// <summary>
        /// The http connection header for keep alive etc.
        /// </summary>
        public OProxyHttpConnectionType Connection { get; set; }

        /// <summary>
        /// The original http header in one string.
        /// </summary>
        public string RequestData { get; set; }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        public OProxyHttpRequest() 
        {
            Requests                    = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            Type                        = OProxyHttpRequestType.HttpPage;
            Parms                       = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            Boundries                   = new Dictionary<string, IProxyHttpRequestBoundray>();
            Url                         = null;
            ContentLength               = 0;
            Protocol                    = string.Empty;
            ContentType                 = string.Empty;
            Via                         = string.Empty;
            Accept                      = string.Empty;
            Connection                  = OProxyHttpConnectionType.Close;
            UserAgent                   = string.Empty;
            Host                        = string.Empty;
            CompletePath                = string.Empty;
            Referer                     = string.Empty;
        }

        /// <summary>
        /// Creates the instance from the data held in the stream.
        /// </summary>
        /// <param name="e"></param>
        public OProxyHttpRequest(byte[] e) 
            : this()
        {
            this.InitiateData(e);
        }

                /// <summary>
        /// Creates the instance from the data held in the stream.
        /// </summary>
        /// <param name="e"></param>
        public OProxyHttpRequest(byte[] buffer, int length) 
            : this()

        {
            this.InitiateData(buffer, length);
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
