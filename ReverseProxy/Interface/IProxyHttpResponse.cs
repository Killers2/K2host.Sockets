/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.IO;
using System.Collections.Generic;

using K2host.Sockets.Raw.Enums;

namespace K2host.Sockets.ReverseProxy.Interface
{

    /// <summary>
    /// This is used to help create the object class you define.
    /// </summary>
    public interface IProxyHttpResponse : IDisposable
    {

        /// <summary>
        /// This is the http status code that always needs setting.
        /// </summary>
        int StatusCode { get; set; }

        /// <summary>
        /// This is the http status description that always needs setting.
        /// </summary>
        string StatusDescription { get; set; }

        /// <summary>
        /// This is where you will add the http response heads for the reply message.
        /// </summary>
        IDictionary<string, string> Headers { get; }

        /// <summary>
        /// This the length in bytes of the content body and not the header, This is normally set automatically.
        /// </summary>
        long ContentLength64 { get; set; }

        /// <summary>
        /// This is where data is stored prior the information being formatted for sending back to the client connection.
        /// </summary>
        MemoryStream OutputStream { get; }

        /// <summary>
        /// This holds the type of data being sent and sets Content-Type.
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// This adds the connection header Content-Encoding: gzip.
        /// </summary>
        bool IsCompressed { get; set; }

        /// <summary>
        /// This adds the connection header Keep-Alive.
        /// </summary>
        OProxyHttpConnectionType Connection { get; set; }

        /// <summary>
        /// If the Connection Keep-Alive is set we can define the timeout here in seconds.
        /// </summary>
        int KeepAliveTimeout { get; set; }

        /// <summary>
        /// If the Connection Keep-Alive is set we can define the max timeout here in seconds.
        /// </summary>
        int KeepAliveTimeoutMax { get; set; }

        /// <summary>
        /// This hold the header data from the stream.
        /// </summary>
        byte[] Header { get; set; }

        /// <summary>
        /// This hold the body data from the stream.
        /// </summary>
        byte[] Body { get; set; }

        /// <summary>
        /// The original http header in one string.
        /// </summary>
        string ResponseData { get; set; }

    }

}
