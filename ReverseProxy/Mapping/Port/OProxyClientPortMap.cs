/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/
using System;
using System.Net.Sockets;

using K2host.Sockets.ReverseProxy.Abstract;
using K2host.Sockets.ReverseProxy.Extentions;

using gl = K2host.Sockets.OHelpers;

namespace K2host.Sockets.ReverseProxy.Mapping.Port
{

    /// <summary>
    /// The client for port mapping with no intevention.
    /// </summary>
    public class OProxyClientPortMap : AProxyClient
    {
        
        /// <summary>
        /// The constructor for creating the instance.
        /// </summary>
        public OProxyClientPortMap() { }
       
        /// <summary>
        /// The overridden method for starting the connection process.
        /// Which also starts the relay of network information when connected.
        /// </summary>
        public override void StartHandShake()
        {
            try
            { 

                DestinationSocket = new Socket(
                    DestinationEndPoint.AddressFamily, 
                    gl.GetSocketType(DestinationProtocol), 
                    DestinationProtocol
                );

                DestinationSocket.BeginConnect(
                    DestinationEndPoint, 
                    new AsyncCallback(e => {
                        try
                        {
                            DestinationSocket.EndConnect(e);
                            if (IsDisposed)
                                return;
                            this.SocketStartRelay();
                        }
                        catch
                        {
                            if (!IsDisposed)
                                Dispose();
                        }
                    }), 
                    DestinationSocket
                );

            }
            catch
            {
                if (!IsDisposed)
                    Dispose();
            }
        }

    }

}
