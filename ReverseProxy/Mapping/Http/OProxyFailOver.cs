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

using K2host.Sockets.ReverseProxy.Interface;

namespace K2host.Sockets.ReverseProxy.Mapping.Http
{

    /// <summary>
    /// This class is used to create an instance of this type.
    /// </summary>
    public class OProxyFailOver : IProxyFailOver
    {
        /// <summary>
        /// The name of this failover
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Traffic going to Object(Computer or IpAddress)
        /// </summary>
        public IPAddress MappedHost { get; set; }

        /// <summary>
        ///  Traffic protocols (HTTP)
        /// </summary>
        public int MappedPort { get; set; }

        /// <summary>
        /// Traffic protocols (HTTPS)
        /// </summary>
        public int MappedSSLPort { get; set; }

        /// <summary>
        /// Traffic Protocol
        /// </summary>
        public ProtocolType Protocol { get; set; }

        /// <summary>
        /// The constuctor used to create an instance from the inherited type.
        /// </summary>
        public OProxyFailOver() { }
        
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
