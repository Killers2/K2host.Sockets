/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Net;
using System.Net.Sockets;

namespace K2host.Sockets.ReverseProxy.Interface
{

    /// <summary>
    /// This is used to help create the object class you define.
    /// </summary>
    public interface IProxyFailOver : IDisposable
    {
        /// <summary>
        /// The name of this failover
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Traffic going to Object(Computer or ip)
        /// </summary>
        IPAddress MappedHost { get; set; }

        /// <summary>
        ///  Traffic protocols (HTTP)
        /// </summary>
        int MappedPort { get; set; }
        
        /// <summary>
        /// Traffic protocols (HTTPS)
        /// </summary>
        int MappedSSLPort { get; set; }

        /// <summary>
        /// Traffic Protocol
        /// </summary>
        ProtocolType Protocol { get; set; }

    }

}
