/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

using K2host.Threading.Classes;
using K2host.Threading.Interface;

namespace K2host.Sockets.Tcp.Interface
{

    /// <summary>
    /// This interface helps create objects passing of this type
    /// </summary>
    public interface IServer : IDisposable
    {
        /// <summary>
        /// The port number of the listener.
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// The IPAddress v4 as a string of the listener.
        /// </summary>
        string IPAddress { get; set; }

        /// <summary>
        /// This list holds the current connected client connections on the listener.
        /// This is usually the <see cref="IPEndPoint"/> as string and the <see cref="IConnection"/>
        /// </summary>
        Dictionary<TcpClient, IConnection> Clients { get; set; }

        /// <summary>
        /// The listener object dirived from the <see cref="TcpListener"/> class.
        /// </summary>
        TcpListener Listener { get; set; }

        /// <summary>
        /// The thread thats hosted in the thread manager used to help run things in threads MTA
        /// </summary>
        IThread ListenerThread { get; set; }

        /// <summary>
        /// The thread manager used to help run things in threads MTA
        /// </summary>
        IThreadManager ListenerThreadManager { get; set; }

        /// <summary>
        /// Used to start the listener object once setup in your class.
        /// </summary>
        void StartServer();

        /// <summary>
        /// Used to start the listener in async mode.
        /// </summary>
        void ListenAsync();

        /// <summary>
        /// Used to the stop the server listener object
        /// </summary>
        void StopServer();

        /// <summary>
        /// Used to connect a client connection when the listener picks up a connection.
        /// </summary>
        /// <param name="ar"></param>
        void OnAccept(IAsyncResult ar);

        /// <summary>
        /// This is for when the server you create send data to the client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnDataSent(IConnection sender, Stream e);

        /// <summary>
        /// This is used to pick up any data returned by the client connection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        void OnDataReceived(IConnection sender, byte[] data);

        /// <summary>
        /// Used to close the client connection.
        /// </summary>
        /// <param name="sender"></param>
        void CloseClient(IConnection sender);

        /// <summary>
        /// This is used to trigger from an event when a client has connected.
        /// </summary>
        /// <param name="sender"></param>
        void OnClientConnect(IConnection sender);

        /// <summary>
        /// This is used to trigger from an event when a client has disconnected.
        /// </summary>
        /// <param name="sender"></param>
        void OnClientDisconnect(IConnection sender);

        /// <summary>
        /// This will be used to close off the client connections that have been disconnected remotly
        /// </summary>
        void CloseDisonnectionUsers();

    }


}
