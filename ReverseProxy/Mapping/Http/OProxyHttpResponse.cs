/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Collections.Generic;
using System.IO;
using K2host.Sockets.Raw.Enums;
using K2host.Sockets.ReverseProxy.Interface;
using K2host.Sockets.ReverseProxy.Extentions;

namespace K2host.Sockets.ReverseProxy.Mapping.Http
{
    
    /// <summary>
    /// This class is used to collect and set data for web http comunication between a client and server web pages.
    /// </summary>
    public class OProxyHttpResponse : IProxyHttpResponse
    {

        /// <summary>
        /// This is the http status code that always needs setting.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// This is the http status description that always needs setting.
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// This is where you will add the http response heads for the reply message.
        /// </summary>
        public IDictionary<string, string> Headers { get; }

        /// <summary>
        /// This the length in bytes of the content body and not the header, This is normally set automatically.
        /// </summary>
        public long ContentLength64 { get; set; }

        /// <summary>
        /// This is where data is stored prior the information being formatted for sending back to the client connection.
        /// </summary>
        public MemoryStream OutputStream { get; }

        /// <summary>
        /// This holds the type of data being sent and sets Content-Type.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// This adds the connection header Content-Encoding: gzip.
        /// </summary>
        public bool IsCompressed { get; set; }

        /// <summary>
        /// This adds the connection header Keep-Alive.
        /// </summary>
        public OProxyHttpConnectionType Connection { get; set; }

        /// <summary>
        /// If the Connection Keep-Alive is set we can define the timeout here in seconds.
        /// </summary>
        public int KeepAliveTimeout { get; set; }

        /// <summary>
        /// If the Connection Keep-Alive is set we can define the max timeout here in seconds.
        /// </summary>
        public int KeepAliveTimeoutMax { get; set; }

        /// <summary>
        /// This hold the header data from the stream.
        /// </summary>
        public byte[] Header { get; set; }

        /// <summary>
        /// This hold the body data from the stream.
        /// </summary>
        public byte[] Body { get; set; }

        /// <summary>
        /// The original http header in one string.
        /// </summary>
        public string ResponseData { get; set; }

        /// <summary>
        /// Creates the instance of the responce object
        /// </summary>
        public OProxyHttpResponse()
        {
            Headers             = new Dictionary<string, string>();
            OutputStream        = new MemoryStream();
            KeepAliveTimeout    = 30;
            KeepAliveTimeoutMax = 60;
            Header              = Array.Empty<byte>();
            Body                = Array.Empty<byte>();
            ResponseData        = string.Empty;
        }

        /// <summary>
        /// Creates the instance of the responce object based on a buffer
        /// </summary>
        /// <param name="e"></param>
        public OProxyHttpResponse(byte[] e)
            : this()
        {

            this.InitiateData(e);

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
