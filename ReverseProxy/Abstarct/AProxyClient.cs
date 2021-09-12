/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using K2host.Sockets.Delegates;
using K2host.Sockets.ReverseProxy.Interface;

namespace K2host.Sockets.ReverseProxy.Abstract
{

    /// <summary>
    /// This class is used to create a connection instance with the base class over a statndard network
    /// </summary>
    public abstract class AProxyClient : IProxyClient
    {
        
        /// <summary>
        /// The listener that created this client object.
        /// </summary>
        public IProxyListener Parent { get; set; }
        
        /// <summary>
        /// The accepted socket as the listener accepts a connection.
        /// </summary>
        public Socket ClientSocket { get; set; }
        
        /// <summary>
        /// The accepted socket remote end point.
        /// </summary>
        public IPEndPoint ClientEndPoint { get; set; }

        /// <summary>
        /// The accepted socket protocol.
        /// </summary>
        public ProtocolType ClientProtocol { get; set; }

        /// <summary>
        /// The network stream / ssl created on the socket.
        /// </summary>
        public Stream ClientStream { get; set; }

        /// <summary>
        /// The network stream assosiated with an ssl stream.
        /// </summary>
        public Stream ClientInnerStream { get; set; }

        /// <summary>
        /// The socket created based on the mapping. 
        /// </summary>
        public Socket DestinationSocket { get; set; }

        /// <summary>
        /// The socket created based on the mappings end point.
        /// </summary>
        public IPEndPoint DestinationEndPoint { get; set; }

        /// <summary>
        /// The socket protocol created based on the mapping. 
        /// </summary>
        public ProtocolType DestinationProtocol { get; set; }

        /// <summary>
        /// The network stream / ssl created on the destination socket.
        /// </summary>
        public Stream DestinationStream { get; set; }

        /// <summary>
        /// The network stream assosiated with an ssl stream.
        /// </summary>
        public Stream DestinationInnerStream { get; set; }

        /// <summary>
        /// The buffer used on the client stream and socket.
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// The buffer used on the destination stream and socket.
        /// </summary>
        public byte[] RemoteBuffer { get; set; }

        /// <summary>
        /// This is used to remove and dispose the client when called remotely.
        /// </summary>
        public ProxyClientDestroyer Destroyer { get; set; }

        /// <summary>
        /// Used to catch the data from the client in the relay and process the info
        /// </summary>
        public OnProxyClientProcessRelay OnClientRelayData { get; set; }

        /// <summary>
        /// Used to catch the data from the destination in the relay and process the info
        /// </summary>
        public OnProxyClientProcessRelay OnRemoteRelayData { get; set; }
      
        /// <summary>
        /// Used to enable the capture of requests comming in on the same relay of the proxy.
        /// </summary>
        public bool EnableRequestRelayCapture { get; set; }

        /// <summary>
        /// Used to enable the capture of reponses comming out on the same relay of the proxy.
        /// </summary>
        public bool EnableResponseRelayCapture { get; set; }

        /// <summary>
        /// The constuctor used to create an instance from the inherited type.
        /// </summary>
        public AProxyClient()
        {
            Buffer                      = new byte[4096 * 2];
            RemoteBuffer                = new byte[4096 * 2];
            ClientSocket                = null;
            IsDisposed                  = false;
            OnClientRelayData           = new OnProxyClientProcessRelay(delegate (byte[] a, int b, bool fromRelay, out byte[] result) { result = a.BufferShrink(b); return true; });
            OnRemoteRelayData           = new OnProxyClientProcessRelay(delegate (byte[] a, int b, bool fromRelay, out byte[] result) { result = a.BufferShrink(b); return true; });
            EnableRequestRelayCapture   = false;
            EnableResponseRelayCapture  = false;
        }

        /// <summary>
        ///This abstract method to start a handshake connection
        /// </summary>
        public abstract void StartHandShake();

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
                    IsDisposed = true;

                    DestinationStream?.Flush();
                    DestinationStream?.Close();
                    DestinationStream?.Dispose();

                    DestinationStream = null;

                    try { DestinationSocket?.Shutdown(SocketShutdown.Both); } catch { }
                    try { DestinationSocket?.Disconnect(false); } catch { }
                    try { DestinationSocket?.Close(); } catch { }
                    try { DestinationSocket?.Dispose(); } catch { }

                    DestinationSocket = null;

                    ClientStream?.Flush();
                    ClientStream?.Close();
                    ClientStream?.Dispose();

                    ClientStream = null;

                    try { ClientSocket?.Shutdown(SocketShutdown.Both); } catch { }
                    try { ClientSocket?.Disconnect(false);  } catch { }
                    try { ClientSocket?.Close();  } catch { }
                    try { ClientSocket?.Dispose(); } catch { }

                    ClientSocket = null;

                    Destroyer?.Invoke(this);
                }

        }

    }
}
