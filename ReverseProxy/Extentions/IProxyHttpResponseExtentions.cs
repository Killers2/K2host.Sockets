/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Linq;
using System.Text;

using K2host.Core;
using K2host.Sockets.ReverseProxy.Interface;

using gl = K2host.Core.OHelpers;
using gs = K2host.Sockets.OHelpers;

namespace K2host.Sockets.ReverseProxy.Extentions
{

    public static class IProxyHttpResponseExtentions
    {

        /// <summary>
        /// Used on the <see cref="IProxyHttpResponse"/> to grab the data and split a web response based on its header and body.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="e"></param>
        public static void InitiateData(this IProxyHttpResponse r, byte[] e)
        {

            byte[]  marker = gl.StrToByteArray("\r\n\r\n");
            int     found = e.WhereIsFirstIndex(marker);

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
                r.ResponseData = new string(Encoding.UTF8.GetChars(r.Header));
            else
                r.ResponseData = string.Empty;

            r.OutputStream.Write(r.Body);

        }

        /// <summary>
        /// Used on the <see cref="IProxyHttpResponse"/> to write data to the stream for the http response.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IProxyHttpResponse Write(this IProxyHttpResponse e, string data)
        {

            byte[] buffer = Encoding.UTF8.GetBytes(data);

            e.OutputStream.Write(buffer, 0, buffer.Length);

            return e;

        }

        /// <summary>
        /// Used on the <see cref="IProxyHttpResponse"/> to get the entrie data content to send over sockets or streams.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static byte[] GetData(this IProxyHttpResponse e)
        {

            byte[] body = e.OutputStream.ToArray();

            if (e.IsCompressed && e.Body.Length <= 0)
                body = gs.CompressGZIP(body);

            e.ContentLength64 = body.Length;

            byte[] header = e.GetHeader();

            e.Dispose();

            return gl.CombineByteArrays(
                header,
                body
            );

        }

        /// <summary>
        /// Used on the <see cref="IProxyHttpResponse"/> to get the header data from the http response object.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static byte[] GetHeader(this IProxyHttpResponse e)
        {
            StringBuilder output = new StringBuilder();

            output.Append("HTTP/1.1 " + e.StatusCode.ToString() + " " + e.StatusDescription);

            e.Headers.ForEach((kvp) => { output.Append("\r\n" + kvp.Key + ": " + kvp.Value); });

            output.Append("\r\n\r\n");

            return Encoding.UTF8.GetBytes(output.ToString());
        }

    }

}
