/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using K2host.Sockets.ReverseProxy.Interface;

using gl = K2host.Sockets.OHelpers;

namespace K2host.Sockets.ReverseProxy.Extentions
{

    public static class IProxyListenerExtention
    {

        /// <summary>
        /// Used for any <see cref="IProxyListener"/> type to start the listener.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static IProxyListener Start(this IProxyListener e)
        {
            try
            {
                e.ShuttingDown = false;
                e.OnBeforeStart?.Invoke(e);
                e.ListenSocket = new Socket(e.Address.AddressFamily, gl.GetSocketType(e.ListenerProtocolType), e.ListenerProtocolType);
                e.ListenSocket.Bind(new IPEndPoint(e.Address, e.Port));
                e.ListenSocket.Listen(100);
                e.ListenSocket.BeginAccept(e.OnAcceptConnection, e.ListenSocket);
                e.OnAfterStart?.Invoke(e);
            }
            catch (Exception)
            {
                e.ShuttingDown = true;
                e.ListenSocket.Dispose();
                e.ListenSocket = null;
                e.Restart();
            }

            return e;

        }

        /// <summary>
        /// Used for any <see cref="IProxyListener"/> type to re-start the listener.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static IProxyListener Restart(this IProxyListener e)
        {
            if (e.ListenSocket == null)
                return e;

            e.ListenSocket.Close();
            e.Start();

            return e;

        }

        /// <summary>
        /// Used on the <see cref="IProxyListener"/> to add client connections stored in a list.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static IProxyClient Add(this IProxyListener e, IProxyClient client)
        {
         
            if (!e.Clients.ContainsKey(client.ClientEndPoint))
                e.Clients.Add(client.ClientEndPoint, client);

            return client;
        }

        /// <summary>
        /// Used on the <see cref="IProxyListener"/> to remove a client connection from the list.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static IProxyClient Remove(this IProxyListener e, IProxyClient client)
        {
            if (client == null)
                return null;

            e.Clients.Remove(client.ClientEndPoint);

            return client;

        }

        /// <summary>
        /// Used on the <see cref="IProxyListener"/> to to return the connection instance from the list by the ip address.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static IProxyClient GetClient(this IProxyListener e, IPAddress ipAddress)
        {
            var kp = e.Clients.Keys.Where(k => k.Address == ipAddress);
            if (kp.Any())
                return e.Clients[kp.FirstOrDefault()];
            else
                return null;
        }

        /// <summary>
        /// Used on the <see cref="IProxyListener"/> to to return the connection instance from the list by the ip end point.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ipEndPoint"></param>
        /// <returns></returns>
        public static IProxyClient GetClient(this IProxyListener e, IPEndPoint ipEndPoint)
        {
            if (e.Clients.ContainsKey(ipEndPoint))
                return e.Clients[ipEndPoint];
            else
                return null;
        }

    }

}
