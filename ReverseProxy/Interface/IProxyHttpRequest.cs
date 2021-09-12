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

namespace K2host.Sockets.ReverseProxy.Interface
{

    /// <summary>
    /// This is used to help create the object class you define.
    /// </summary>
    public interface IProxyHttpRequest : IDisposable
    {

        /// <summary>
        /// Holds the key value pairs from the request items
        /// </summary>
        IDictionary<string, string> Requests { get; }

        /// <summary>
        /// Allows indexing on the request directly in the object
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        string this[string item] { get; set; }

        /// <summary>
        /// This hold the header data from the stream.
        /// </summary>
        byte[] Header { get; set; }

        /// <summary>
        /// This hold the body data from the stream.
        /// </summary>
        byte[] Body { get; set; }

        /// <summary>
        /// This hold the method type of <see cref="OProxyHttpMethodType"/>
        /// </summary>
        OProxyHttpMethodType Method { get; set; }

        /// <summary>
        /// This hold the request type of <see cref="OProxyHttpRequestType"/>
        /// </summary>
        OProxyHttpRequestType Type { get; set; }

        /// <summary>
        /// This hold the path as a list of path segments.
        /// </summary
        IList<string> Paths
        {
            get;
            set;
        }

        /// <summary>
        /// The complete path as a string excluding the querystring.
        /// </summary>
        string Path
        {
            get;
            set;
        }

        /// <summary>
        /// The complete path as a string including the querystring.
        /// </summary>
        string CompletePath
        {
            get;
            set;
        }

        /// <summary>
        /// This holds any query string or path routing values
        /// </summary>
        IDictionary<string, string> Parms
        {
            get;
            set;
        }

        /// <summary>
        /// This hold the boundary elements send over as multipart/form-data
        /// </summary>
        IDictionary<string, IProxyHttpRequestBoundray> Boundries
        {
            get;
            set;
        }

        /// <summary>
        /// The http header referer
        /// </summary>
        string Referer
        {
            get;
            set;
        }
        
        /// <summary>
        /// The http header Access-Control-Request-Method for CORS
        /// </summary>
        string AccessControlRequestMethod
        {
            get;
            set;
        }

        /// <summary>
        /// The http header Access-Control-Request-Headers for CORS
        /// </summary>
        string AccessControlRequestHeaders
        {
            get;
            set;
        }

        /// <summary>
        /// The http header Origin for CORS
        /// </summary>
        string Origin
        {
            get;
            set;
        }

        /// <summary>
        /// The http header Sec-Fetch-Mode for CORS
        /// </summary
        string SecFetchMode
        {
            get;
            set;
        }
        
        /// <summary>
        /// The http header for user-agent
        /// </summary>
        string UserAgent
        {
            get;
            set;
        }

        /// <summary>
        /// The http header content-length
        /// </summary>
        int ContentLength
        {
            get;
            set;
        }

        /// <summary>
        /// The http header host
        /// </summary>
        string Host
        {
            get;
            set;
        }

        /// <summary>
        /// This hold the Url information.
        /// </summary>
        Uri Url
        {
            get;
            set;
        }

        /// <summary>
        /// The http protocol / schema
        /// </summary>
        string Protocol
        {
            get;
            set;
        }

        /// <summary>
        /// The http content type
        /// </summary>
        string ContentType
        {
            get;
            set;
        }

        /// <summary>
        /// The http header 'via' normally filled from a proxy server
        /// </summary>
        string Via
        {
            get;
            set;
        }

        /// <summary>
        /// The http accept header
        /// </summary>
        string Accept
        {
            get;
            set;
        }

        /// <summary>
        /// The http connection header for keep alive etc.
        /// </summary>
        OProxyHttpConnectionType Connection
        {
            get;
            set;
        }

        /// <summary>
        /// The original http header in one string.
        /// </summary>
        string RequestData
        {
            get;
            set;
        }

    }

}
