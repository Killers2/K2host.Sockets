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
    public interface IProxyHttpRequestBoundray : IDisposable
    {

        /// <summary>
        /// Holds the Boundray Id from the web request
        /// </summary>
        string BoundrayId { get; set; }

        /// <summary>
        /// This the content disposition which is in each boundary part.
        /// </summary>
        string ContentDisposition { get; set; }

        /// <summary>
        /// This the content type which is in each boundary part.
        /// </summary>      
        string ContentType { get; set; }

        /// <summary>
        /// This the content data length from the value part of the bounary.
        /// </summary>  
        long ContentLength64 { get; set; }

        /// <summary>
        /// This the field name of the bounary which is in each boundary part.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// This the file name if the boundary containes a file upload.
        /// </summary>   
        string Filename { get; set; }

        /// <summary>
        /// The vaalue data as a string if it is not a file upload.
        /// </summary>    
        string Data { get; set; }

        /// <summary>
        /// The value content in a stream to read for later.
        /// </summary>
        MemoryStream Stream { get; set; }

    }

}
