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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using K2host.Core;
using K2host.Core.Delegates;
using K2host.Sockets.ReverseProxy.Interface;
using K2host.Sockets.Raw.Enums;

namespace K2host.Sockets.ReverseProxy.Abstract
{

    /// <summary>
    /// This class is used to create socket server
    /// </summary>
    public abstract class AProxyListener : IProxyListener
    {

        /// <summary>
        /// The given name for the listener.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The port number you want to listen to.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The ip address you want to listen to.
        /// </summary>
        public IPAddress Address { get; set; }

        /// <summary>
        /// The protocol type you want to listen to.
        /// </summary>
        public ProtocolType ListenerProtocolType { get; set; }

        /// <summary>
        /// The destination protocol type created when a client has connected.
        /// </summary>
        public ProtocolType DestinationProtocolType { get; set; }

        /// <summary>
        /// The session type for the mapping.
        /// </summary>
        public OProxySessionType SessionType { get; set; }

        /// <summary>
        /// The socket used to listen for incomming connections.
        /// </summary>
        [JsonIgnore]
        public Socket ListenSocket { get; set; }

        /// <summary>
        /// The connected connections accepted from the listener.
        /// </summary>
        [JsonIgnore]
        public Dictionary<IPEndPoint, IProxyClient> Clients { get; }

        /// <summary>
        /// The listening state of the socket.
        /// </summary>
        public bool Listening
        {
            get
            {
                return ListenSocket != null;
            }
        }

        /// <summary>
        /// The shutting down state of the socket.
        /// </summary>
        public bool ShuttingDown { get; set; }

        /// <summary>
        /// The service method invoked if required before the socket is in listening mode.
        /// </summary>
        [JsonIgnore]
        public OServiceMethod OnBeforeStart { get; set; }

        /// <summary>
        /// The service method invoked if required after the socket is in listening mode.
        /// </summary>
        [JsonIgnore]
        public OServiceMethod OnAfterStart { get; set; }

        /// <summary>
        /// The callback method invoked when a client connection has been accepted.
        /// </summary>
        [JsonIgnore]
        public AsyncCallback OnAcceptConnection { get; set; }

        /// <summary>
        /// The constuctor used to create an instance from the inherited type.
        /// </summary>
        public AProxyListener()
        {
            IsDisposed      = false;
            Clients         = new Dictionary<IPEndPoint, IProxyClient>();
            ShuttingDown    = false;
        }

        /// <summary>
        /// The constuctor used to create an instance from the inherited type.
        /// </summary>
        /// <param name="port">The port number you want to listen to.</param>
        /// <param name="address">The ip address you want to listen to.</param>
        public AProxyListener(int port, IPAddress address)
            : this()
        {
            Port            = port;
            Address         = address;
        }

        /// <summary>
        /// The constuctor used to create an instance from the inherited type.
        /// </summary>
        /// <param name="port">The port number you want to listen to.</param>
        /// <param name="address">The ip address you want to listen to.</param>
        /// <param name="protocol">The protocol you are listen to.</param>
        public AProxyListener(int port, IPAddress address, ProtocolType protocol) 
            : this(port, address)
        {
            ListenerProtocolType = protocol;
        }
        
        /// <summary>
        ///This abstract method to convert the object and its counter parts to json for saving
        /// </summary>
        public abstract JObject Save(JsonSerializerSettings e = null);

        /// <summary>
        /// This is used on disposable objects and used generically for remotely disposing a client.
        /// </summary>
        public bool IsDisposed { get; set; }

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

                    ShuttingDown = true;

                    try
                    {

                        Clients.Values.ForEach(c => { try { c.Dispose(); } catch { } });
                      
                        Clients.Clear();

                        try { ListenSocket.Shutdown(SocketShutdown.Both); } catch { }

                        if (ListenSocket != null)
                        {
                            ListenSocket.Close();
                            ListenSocket.Dispose();
                        }

                    }
                    catch { }

                }

            IsDisposed = true;
        }

    }

}
