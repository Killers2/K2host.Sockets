/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using K2host.Core.Delegates;
using K2host.Sockets.Raw.Enums;

namespace K2host.Sockets.ReverseProxy.Interface
{

    /// <summary>
    /// This is used to help create the object class you define.
    /// </summary>
    public interface IProxyListener : IDisposable
    {

        /// <summary>
        /// The given name for the listener.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The port number you want to listen to.
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// The ip address you want to listen to.
        /// </summary>
        IPAddress Address { get; set; }

        /// <summary>
        /// The session type for the mapping.
        /// </summary>
        OProxySessionType SessionType { get; set; }

        /// <summary>
        /// The protocol type you want to listen to.
        /// </summary>
        ProtocolType ListenerProtocolType { get; set; }

        /// <summary>
        /// The destination protocol type created when a client has connected.
        /// </summary>
        ProtocolType DestinationProtocolType { get; set; }

        /// <summary>
        /// The socket used to listen for incomming connections.
        /// </summary>
        Socket ListenSocket { get; set; }

        /// <summary>
        /// The connected connections accepted from the listener.
        /// </summary>
        Dictionary<IPEndPoint, IProxyClient> Clients { get; }

        /// <summary>
        /// The listening state of the socket.
        /// </summary>
        bool Listening { get; }

        /// <summary>
        /// The shutting down state of the socket.
        /// </summary>
        bool ShuttingDown { get; set; }

        /// <summary>
        /// The service method invoked if required before the socket is in listening mode.
        /// </summary>
        OServiceMethod OnBeforeStart { get; set; }

        /// <summary>
        /// The service method invoked if required after the socket is in listening mode.
        /// </summary>
        OServiceMethod OnAfterStart { get; set; }

        /// <summary>
        /// The callback method invoked when a client connection has been accepted.
        /// </summary>
        AsyncCallback OnAcceptConnection { get; set; }

        /// <summary>
        /// This is used on disposable objects and used generically for remotely disposing a client.
        /// </summary>
        bool IsDisposed { get; set; }

    }

}
