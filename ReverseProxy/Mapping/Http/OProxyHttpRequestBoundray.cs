/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.IO;

namespace K2host.Sockets.ReverseProxy.Interface
{

    /// <summary>
    /// This is used to help create the object class you define.
    /// </summary>
    public class OProxyHttpRequestBoundray : IProxyHttpRequestBoundray
    {

        /// <summary>
        /// Holds the Boundray Id from the web request
        /// </summary>
        public string BoundrayId
        {
            get;
            set;
        }

        /// <summary>
        /// This the content disposition which is in each boundary part.
        /// </summary>
        public string ContentDisposition
        {
            get;
            set;
        }

        /// <summary>
        /// This the content type which is in each boundary part.
        /// </summary>      
        public string ContentType
        {
            get;
            set;
        }

        /// <summary>
        /// This the content data length from the value part of the bounary.
        /// </summary>  
        public long ContentLength64
        {
            get;
            set;
        }

        /// <summary>
        /// This the field name of the bounary which is in each boundary part.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// This the file name if the boundary containes a file upload.
        /// </summary>   
        public string Filename
        {
            get;
            set;
        }

        /// <summary>
        /// The vaalue data as a string if it is not a file upload.
        /// </summary>    
        public string Data
        {
            get;
            set;
        }

        /// <summary>
        /// The value content in a stream to read for later.
        /// </summary>
        public MemoryStream Stream
        {
            get;
            set;
        }

        /// <summary>
        /// The content you are looking for or RegEx to compare
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The constuctor used to create an instance from the inherited type.
        /// </summary>
        public OProxyHttpRequestBoundray() { }

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
