/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Text;

using K2host.Core;
using K2host.Sockets.ReverseProxy.Interface;

using gl = K2host.Core.OHelpers;

namespace K2host.Sockets.ReverseProxy.Extentions
{

    public static class IProxyHttpRequestExtentions
    {

        /// <summary>
        /// Used on the <see cref="IProxyHttpRequest"/> to grab the data and split a web request based on its header and body.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="e"></param>
        public static void InitiateData(this IProxyHttpRequest r, byte[] e)
        {

            byte[] marker = gl.StrToByteArray("\r\n\r\n");
            int found = e.WhereIsFirstIndex(marker);

            if (found > 0)
            {
                r.Header = new byte[found];
                Buffer.BlockCopy(e, 0, r.Header, 0, r.Header.Length);

                r.Body = new byte[e.Length - (found + marker.Length)];
                Buffer.BlockCopy(e, (found + marker.Length), r.Body, 0, r.Body.Length);

            }
            else
            {
                r.Body = new byte[e.Length];
                Buffer.BlockCopy(e, 0, r.Body, 0, r.Body.Length);
            }

            if (r.Header != null)
                r.RequestData = new string(Encoding.UTF8.GetChars(r.Header));
            else
                r.RequestData = string.Empty;

        }
        
        /// <summary>
        /// Used on the <see cref="IProxyHttpRequest"/> to grab the data and split a web request based on its header and body.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="e"></param>
        public static void InitiateData(this IProxyHttpRequest r, byte[] e, int length)
        {

            byte[]  worker  = e.SystemSubArray(0, length);
            byte[]  marker  = gl.StrToByteArray("\r\n\r\n");
            int     found   = worker.WhereIsFirstIndex(marker);

            if (found > 0)
            {

                r.Header = new byte[found];
                Buffer.BlockCopy(worker, 0, r.Header, 0, r.Header.Length);

                r.Body = new byte[worker.Length - (found + marker.Length)];
                Buffer.BlockCopy(worker, (found + marker.Length), r.Body, 0, r.Body.Length);

            }
            else
            {
                r.Body = new byte[worker.Length];
                Buffer.BlockCopy(worker, 0, r.Body, 0, r.Body.Length);
            }

            if (r.Header != null)
                r.RequestData = new string(Encoding.UTF8.GetChars(r.Header));
            else
                r.RequestData = string.Empty;

        }

        /// <summary>
        /// Used on the <see cref="IProxyHttpRequest"/> to check the request headers for a key or value.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool ContainsRequest(this IProxyHttpRequest r, string key)
        {
            return r.Requests.ContainsKey(key);
        }

        /// <summary>
        /// Used on the <see cref="IProxyHttpRequest"/> to check the request for gzip compression.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool IsGZIPCommpressed(this IProxyHttpRequest r)
        {
            return (r.Requests != null && r.Requests.ContainsKey("Content-Encoding") && r.Requests["Content-Encoding"].Contains("gzip"));
        }

        /// <summary>
        /// Used on the <see cref="IProxyHttpRequest"/> to check the request for support of gzip compression.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool IsGZIPSupported(this IProxyHttpRequest r)
        {
            return (r.Requests != null && r.Requests.ContainsKey("Accept-Encoding") && r.Requests["Accept-Encoding"].Contains("gzip"));
        }


    }

}
