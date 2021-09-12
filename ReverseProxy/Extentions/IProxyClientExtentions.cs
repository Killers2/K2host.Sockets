/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.IO;
using System.Net.Sockets;

using K2host.Sockets.ReverseProxy.Interface;

namespace K2host.Sockets.ReverseProxy.Extentions
{

    public static class IProxyClientExtentions
    {
        /// <summary>
        /// Used on the <see cref="IProxyClient"/> to start the relay on netowork information between client and destination.
        /// This uses raw sockets and can be used for port mapping or raw data filters.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static IProxyClient SocketStartRelay(this IProxyClient e)
        {
            if (e.IsDisposed)
                return e;

            try
            {
                e.ClientSocket.BeginReceive(e.Buffer, 0, e.Buffer.Length, SocketFlags.None, new AsyncCallback(e.OnSocketClientReceive), e.ClientSocket);
                e.DestinationSocket.BeginReceive(e.RemoteBuffer, 0, e.RemoteBuffer.Length, SocketFlags.None, new AsyncCallback(e.OnSocketRemoteReceive), e.DestinationSocket);
            }
            catch (Exception)
            {
                
                if (!e.IsDisposed)
                    e.Dispose();
            }

            return e;

        }
        
        /// <summary>
        /// Used to relay data from the client to the destination connection using sockets.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ar"></param>
        public static void OnSocketClientReceive(this IProxyClient e, IAsyncResult ar)
        {
            if (e.IsDisposed)
                return;

            try
            {
                int ret;

                ret = e.ClientSocket.EndReceive(ar);

                if (ret <= 0)
                {
                    if(!e.IsDisposed)
                        e.Dispose();

                    return;
                }

                //EnableRequestRelayCapture false : Normal operation is to only look at the connecting request header not all of them comming thought the relay
                //EnableRequestRelayCapture true  : Capture segmented requests comming in though the same relay

                if (!e.EnableRequestRelayCapture)
                    e.DestinationSocket.BeginSend(e.Buffer, 0, ret, SocketFlags.None, new AsyncCallback(e.OnSocketRemoteSent), e.DestinationSocket);
                else
                    if (e.OnClientRelayData.Invoke(e.Buffer, ret, true, out byte[] newBuffer))
                        e.DestinationSocket.BeginSend(newBuffer, 0, newBuffer.Length, SocketFlags.None, new AsyncCallback(e.OnSocketRemoteSent), e.DestinationSocket);

            }
            catch (Exception)
            {
                if (!e.IsDisposed)
                    e.Dispose();
            }

        }

        /// <summary>
        /// Used to relay data from the destination to the client connection using sockets.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ar"></param>
        public static void OnSocketRemoteSent(this IProxyClient e, IAsyncResult ar)
        {
            if (e.IsDisposed)
                return;

            try
            {

                int ret = e.DestinationSocket.EndSend(ar);

                if (ret > 0)
                {

                    e.ClientSocket.BeginReceive(e.Buffer, 0, e.Buffer.Length, SocketFlags.None, new AsyncCallback(e.OnSocketClientReceive), e.ClientSocket);
                    return;
                }

            }
            catch (Exception) { }

            if (!e.IsDisposed)
                e.Dispose();
        }

        /// <summary>
        /// Used to relay data from the destination to the client connection using sockets.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ar"></param>
        public static void OnSocketRemoteReceive(this IProxyClient e, IAsyncResult ar)
        {
            if (e.IsDisposed)
                return;

            try
            {
                int ret = e.DestinationSocket.EndReceive(ar);

                if (ret <= 0)
                {
                    if(!e.IsDisposed)
                        e.Dispose();
                    return;
                }

                //EnableResponseRelayCapture false : Normal operation is to relay the data from the server stright back to the client.
                //EnableResponseRelayCapture true  : capture segmented requests comming out from the server though the same relay

                if (!e.EnableResponseRelayCapture)
                    e.ClientSocket.BeginSend(e.RemoteBuffer, 0, ret, SocketFlags.None, new AsyncCallback(e.OnSocketClientSent), e.ClientSocket);
                else
                    if (e.OnRemoteRelayData.Invoke(e.RemoteBuffer, ret, true, out byte[] newBuffer))
                        e.ClientSocket.BeginSend(newBuffer, 0, newBuffer.Length, SocketFlags.None, new AsyncCallback(e.OnSocketClientSent), e.ClientSocket);
            }
            catch (Exception)
            {
                if (!e.IsDisposed)
                    e.Dispose();
            }
        }

        /// <summary>
        /// Used to relay data from the client to the destination connection using sockets.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ar"></param>
        public static void OnSocketClientSent(this IProxyClient e, IAsyncResult ar)
        {
            if (e.IsDisposed)
                return;

            try
            {

                int ret = e.ClientSocket.EndSend(ar);

                if (ret > 0)
                {
                    e.DestinationSocket.BeginReceive(e.RemoteBuffer, 0, e.RemoteBuffer.Length, SocketFlags.None, new AsyncCallback(e.OnSocketRemoteReceive), e.DestinationSocket);
                    return;
                }

            }
            catch (Exception) { }

            if (!e.IsDisposed)
                e.Dispose();

        }

        /// <summary>
        /// Used on the <see cref="IProxyClient"/> to start the relay on netowork information between client and destination.
        /// This uses the socket netowkr streams and can be used for http filter mapping.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static IProxyClient StreamStartRelay(this IProxyClient e)
        {
            if (e.IsDisposed)
                return e;

            try
            {
                e.ClientStream.BeginRead(e.Buffer, 0, e.Buffer.Length, new AsyncCallback(e.OnStreamClientReceive), e.ClientStream);
                e.DestinationStream.BeginRead(e.RemoteBuffer, 0, e.RemoteBuffer.Length, new AsyncCallback(e.OnStreamRemoteReceive), e.DestinationStream);
            }
            catch (Exception)
            {

                if (!e.IsDisposed)
                    e.Dispose();
            }

            return e;

        }

        /// <summary>
        /// Used to relay data from the client to the destination connection using network stream.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ar"></param>
        public static void OnStreamClientReceive(this IProxyClient e, IAsyncResult ar)
        {
            if (e.IsDisposed)
                return;

            try
            {
                int ret;

                lock (e.ClientStream)
                    ret = e.ClientStream.EndRead(ar);

                if (ret <= 0)
                {
                    if (!e.IsDisposed)
                        e.Dispose();

                    return;
                }

                e.ClientStream.Flush();

                if (!e.EnableRequestRelayCapture)
                    e.DestinationStream.BeginWrite(e.Buffer, 0, ret, new AsyncCallback(e.OnStreamRemoteSent), e.DestinationStream);
                else
                    if (e.OnClientRelayData.Invoke(e.Buffer, ret, true, out byte[] newBuffer))
                        e.DestinationStream?.BeginWrite(newBuffer, 0, newBuffer.Length, new AsyncCallback(e.OnStreamRemoteSent), e.DestinationStream);
            }
            catch (Exception)
            {
                if (!e.IsDisposed)
                    e.Dispose();
            }

        }

        /// <summary>
        /// Used to relay data from the destination to the client connection using network stream.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ar"></param>
        public static void OnStreamRemoteSent(this IProxyClient e, IAsyncResult ar)
        {
            if (e.IsDisposed)
                return;

            try
            {
                if(ar != null)
                    lock (e.DestinationStream)
                        e.DestinationStream.EndWrite(ar);

                e.DestinationStream.Flush();

                e.ClientStream.BeginRead(e.Buffer, 0, e.Buffer.Length, new AsyncCallback(e.OnStreamClientReceive), e.ClientStream);

                return;

            }
            catch (Exception) { }

            if (!e.IsDisposed)
                e.Dispose();
        }

        /// <summary>
        /// Used to relay data from the destination to the client connection using network stream.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ar"></param>
        public static void OnStreamRemoteReceive(this IProxyClient e, IAsyncResult ar)
        {
            if (e.IsDisposed)
                return;

            try
            {
                int ret;

                lock (e.DestinationStream)
                    ret = e.DestinationStream.EndRead(ar);

                if (ret <= 0)
                {
                    if (!e.IsDisposed)
                        e.Dispose();
                    return;
                }
             
                e.DestinationStream.Flush();

                if (!e.EnableResponseRelayCapture)
                    e.ClientStream.BeginWrite(e.RemoteBuffer, 0, ret, new AsyncCallback(e.OnStreamClientSent), e.ClientStream);
                else
                    if (e.OnRemoteRelayData.Invoke(e.RemoteBuffer, ret, true, out byte[] newBuffer))
                        e.ClientStream.BeginWrite(newBuffer, 0, newBuffer.Length, new AsyncCallback(e.OnStreamClientSent), e.ClientStream);

            }
            catch (Exception)
            {
                if (!e.IsDisposed)
                    e.Dispose();
            }
        }

        /// <summary>
        /// Used to relay data from the client to the destination connection using network stream.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ar"></param>
        public static void OnStreamClientSent(this IProxyClient e, IAsyncResult ar)
        {
            if (e.IsDisposed)
                return;

            try
            {
                if (ar != null)
                    lock (e.ClientStream)
                        e.ClientStream.EndWrite(ar);

                e.ClientStream.Flush();

                e.DestinationStream.BeginRead(e.RemoteBuffer, 0, e.RemoteBuffer.Length, new AsyncCallback(e.OnStreamRemoteReceive), e.DestinationStream);

            }
            catch (Exception) 
            {
                if (!e.IsDisposed)
                    e.Dispose();            
            }

        }

    }

}
