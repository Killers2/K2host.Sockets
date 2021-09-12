/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System.Net;

using K2host.Sockets.ReverseProxy.Interface;

namespace K2host.Sockets.ReverseProxy.Extentions
{

    public static class IProxyFailOverExtentions
    {
        
        /// <summary>
        /// Used for the <see cref="IProxyFailOver"/> to get the remote host as an end point.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="isSsl"></param>
        /// <returns></returns>
        public static IPEndPoint ToIPEndPoint(this IProxyFailOver e, bool isSsl)
        {
            return new IPEndPoint(e.MappedHost, (isSsl ? e.MappedSSLPort : e.MappedPort));
        }

    }

}
