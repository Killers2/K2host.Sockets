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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using K2host.Core;
using K2host.Sockets.Tcp.Interface;
using K2host.Threading.Classes;
using K2host.Threading.Extentions;
using K2host.Threading.Interface;

namespace K2host.Sockets.Tcp.Abstarct
{

    /// <summary>
    /// Used to create a base class for your instance that inherits this abstract.
    /// </summary>
    public abstract class OServerBase : IServer
    {
       
        public static readonly ManualResetEvent ResetClientConnected = new(false);

        /// <summary>
        /// The port number of the listener.
        /// </summary>
        public virtual int Port { get; set; }

        /// <summary>
        /// The IPAddress v4 as a string of the listener.
        /// </summary>
        public virtual string IPAddress { get; set; }

        /// <summary>
        /// This list holds the current connected client connections on the listener.
        /// This is usually the <see cref="IPEndPoint"/> as string and the <see cref="OTCPUserConnection"/>
        /// </summary>
        public virtual Dictionary<TcpClient, IConnection> Clients { get; set; }

        /// <summary>
        /// The listener object dirived from the <see cref="TcpListener"/> class.
        /// </summary>
        public virtual TcpListener Listener { get; set; }

        /// <summary>
        /// The thread thats hosted in the thread manager used to help run things in threads MTA
        /// </summary>
        public virtual IThread ListenerThread { get; set; }

        /// <summary>
        /// The thread manager used to help run things in threads MTA
        /// </summary>
        public virtual IThreadManager ListenerThreadManager { get; set; }

        /// <summary>
        /// Used to start the listener object once setup in your class.
        /// </summary>
        public virtual void StartServer()
        {

            if (Listener != null)
                return;

            ListenerThread = new OThread( new ThreadStart(ListenAsync) );

            ListenerThreadManager.Add( ListenerThread ).Start(null);

            Clients = new Dictionary<TcpClient, IConnection>();

        }

        /// <summary>
        /// Used to the stop the server listener object
        /// </summary>
        public virtual void StopServer()
        {

            try
            {
                if (Clients != null)
                {
                    foreach (OConnectionBase client in Clients.Values)
                    {
                        CloseClient(client);
                        client.Client.Close();
                        OnClientConnect(client);
                    }

                    Clients.Clear();
                    Clients = null;
                }
            }
            catch { }

            if (Listener != null)
            {
                Listener.Stop();
                Listener = null;
            }

        }

        /// <summary>
        /// Used to close the client connection.
        /// </summary>
        /// <param name="sender"></param>
        public virtual void CloseClient(IConnection sender)
        {

            try
            {
                NetworkStream nw = sender.Client.GetStream();
                nw.Close();
            }
            catch { }

            try
            {
                sender.Client.Close();
            }
            catch { }

            Clients.Remove(sender.Client);

            OnClientDisconnect(sender);

        }

        /// <summary>
        /// Used to start the listener in async mode.
        /// </summary>
        public virtual void ListenAsync()
        {
            try
            {
                // Set the event to nonsignaled state.
                ResetClientConnected.Reset();

                Listener = new TcpListener(System.Net.IPAddress.Parse(IPAddress), Port);
                Listener.Start(100);
                Listener.BeginAcceptTcpClient(new AsyncCallback(OnAccept), Listener);

                // Wait until a connection is made and processed before continuing.
                ResetClientConnected.WaitOne();

            }
            catch (Exception)
            {
                Listener.Stop();
                Listener = null;
                Thread.Sleep(1000);
                ListenAsync();
            }
        }

        /// <summary>
        /// Used to connect a client connection when the listener picks up a connection.
        /// </summary>
        /// <param name="ar"></param>
        public abstract void OnAccept(IAsyncResult ar);

        /// <summary>
        /// This is for when the server you create send data to the client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void OnDataSent(IConnection sender, Stream e);

        /// <summary>
        /// This is used to pick up any data returned by the client connection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public abstract void OnDataReceived(IConnection sender, byte[] data);

        /// <summary>
        /// This is used to trigger from an event when a client has connected.
        /// </summary>
        /// <param name="sender"></param>
        public abstract void OnClientConnect(IConnection sender);

        /// <summary>
        /// This is used to trigger from an event when a client has disconnected.
        /// </summary>
        /// <param name="sender"></param>
        public virtual void OnClientDisconnect(IConnection sender)
        {
            try
            {
                //Remove this dissconnected client.
                Clients.Remove(sender.Client);
                sender.Dispose();

                //remove anyother ones that are not connected.
                KeyValuePair<TcpClient, IConnection>[] Disconnected = Clients.Where(w => !w.Value.IsConnected()).ToArray();
                if (Disconnected.Length > 0)
                    Disconnected.Each(c =>
                    {
                        c.Value.Dispose();
                        return Clients.Remove(c.Key);
                    });

            }
            catch { }
        }

        /// <summary>
        /// This will be used to close off the client connections that have been disconnected remotly
        /// </summary>
        public virtual void CloseDisonnectionUsers()
        {


        }

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract void Dispose(bool disposing);

        #endregion

    }

}
