/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

using K2host.Sockets.Delegates;

namespace K2host.Sockets.ReverseProxy.Interface
{

    /// <summary>
    /// This is used to help create the object class you define.
    /// </summary>
    public interface IProxyClient : IDisposable
    {

        /// <summary>
        /// The listener that created this client object.
        /// </summary>
        IProxyListener Parent { get; set; }

        /// <summary>
        /// The accepted socket as the listener accepts a connection.
        /// </summary>
        Socket ClientSocket { get; set; }

        /// <summary>
        /// The accepted socket remote end point.
        /// </summary>
        IPEndPoint ClientEndPoint { get; set; }

        /// <summary>
        /// The accepted socket protocol.
        /// </summary>
        ProtocolType ClientProtocol { get; set; }

        /// <summary>
        /// The network stream / ssl created on the socket.
        /// </summary>
        Stream ClientStream { get; set; }

        /// <summary>
        /// The network stream assosiated with an ssl stream.
        /// </summary>
        Stream ClientInnerStream { get; set; }

        /// <summary>
        /// The socket created based on the mapping. 
        /// </summary>
        Socket DestinationSocket { get; set; }

        /// <summary>
        /// The socket created based on the mappings end point.
        /// </summary>
        IPEndPoint DestinationEndPoint { get; set; }

        /// <summary>
        /// The socket protocol created based on the mapping. 
        /// </summary>
        ProtocolType DestinationProtocol { get; set; }

        /// <summary>
        /// The network stream / ssl created on the destination socket.
        /// </summary>
        Stream DestinationStream { get; set; }
        
        /// <summary>
        /// The network stream assosiated with an ssl stream.
        /// </summary>
        Stream DestinationInnerStream { get; set; }

        /// <summary>
        /// The buffer used on the client stream and socket.
        /// </summary>
        byte[] Buffer { get; set; }

        /// <summary>
        /// The buffer used on the destination stream and socket.
        /// </summary>
        byte[] RemoteBuffer { get; set; }

        /// <summary>
        /// This is used to remove and dispose the client when called remotely.
        /// </summary>
        ProxyClientDestroyer Destroyer { get; set; }

        /// <summary>
        /// Used to catch the data from the client in the relay and process the info
        /// </summary>
        OnProxyClientProcessRelay OnClientRelayData { get; set; }

        /// <summary>
        /// Used to catch the data from the destination in the relay and process the info
        /// </summary>
        OnProxyClientProcessRelay OnRemoteRelayData { get; set; }

        /// <summary>
        /// Used to enable the capture of requests comming in on the same relay of the proxy.
        /// private set by the filter option on the listener
        ///false : Normal operation is to only look at the connecting request header not all of them comming thought the relay
        ///true  : Capture segmented requests comming in though the same relay
        /// </summary>
        bool EnableRequestRelayCapture { get; }

        /// <summary>
        /// Used to enable the capture of reponses comming out on the same relay of the proxy.
        /// private set by the filter option on the listener
        ///false : Normal operation is to relay the data from the server stright back to the client.
        ///true  : capture segmented requests comming out from the server though the same relay
        /// </summary>
        bool EnableResponseRelayCapture { get; }

        /// <summary>
        /// This is used on disposable objects and used generically for remotely disposing a client.
        /// </summary>
        bool IsDisposed { get; set; }

    }

}
