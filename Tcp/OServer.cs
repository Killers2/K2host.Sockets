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
using System.Security.Cryptography.X509Certificates;
using System.Threading;

using K2host.Sockets.Tcp.Abstarct;
using K2host.Sockets.Tcp.Interface;
using K2host.Sockets.Delegates;
using K2host.Threading.Classes;

using gl = K2host.Core.OHelpers;
using K2host.Threading.Interface;

namespace K2host.Sockets.Tcp
{

    /// <summary>
    /// This class is used to create the server to send and return data from clients
    /// </summary>
    public class OServer : OServerBase
    {

        #region Attribute Fields

        bool _isDisposed = false;

        /// <summary>
        /// Used to help tls connections that have the missing tls client hello message if the requests are fast.
        /// </summary>
        X509Certificate2 cachedCert = null;

        #endregion

        #region Events

        /// <summary>
        /// Used to trigger on any update with the client connection on the server.
        /// </summary>
        public OStatusUpdateEventHandler StatusUpdate { get; set; }

        /// <summary>
        /// Used to trigger on data sent with the client connection on the server.
        /// </summary>
        public ODataSentEventHandler DataSent { get; set; }

        /// <summary>
        /// Used to trigger on data received with the client connection on the server.
        /// </summary>
        public ODataReceivedEventHandler DataReceived { get; set; }

        /// <summary>
        /// Used to trigger on client connection on the server.
        /// </summary>
        public OClientConnectedEventHandler ClientConnected { get; set; }

        /// <summary>
        /// Used to trigger on client connection on the server has been disconnected.
        /// </summary>
        public OClientDisConnectedEventHandler ClientDisConnected { get; set; }

        /// <summary>
        /// Used to get the cert installed on this computer related to the SNI data on any tls connection.
        /// </summary>
        public OnGetCertificateFromSNIEvent OnGetCertificateFromSNI { get; set; }
      
        /// <summary>
        /// Used return exception errors in an event rather than thrown in a try and catch.
        /// </summary>
        public OnErrorEventHandler OnError { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Used to define this server as a tls or non tls listener server
        /// </summary>
        public bool IsSecure { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates the instance of the server passing a thread manager instance.
        /// </summary>
        /// <param name="e">The thread manager instance</param>
        public OServer(IThreadManager e)
        {
            IsSecure = false;
            ListenerThreadManager = e;
        }

        /// <summary>
        /// Creates the instance of the server passing a thread manager instance.
        /// </summary>
        /// <param name="e">The thread manager instance</param>
        /// <param name="ipaddress">The IPAddress</param>
        /// <param name="port">The Port Number</param>
        public OServer(IThreadManager e, string ipaddress, int port)
            : this(e)
        {
            IPAddress = ipaddress;
            Port = port;
            IsSecure = false;
        }

        /// <summary>
        /// Creates the instance of the server passing a thread manager instance.
        /// </summary>
        /// <param name="e">The thread manager instance</param>
        /// <param name="ipaddress">The IPAddress</param>
        /// <param name="port">The Port Number</param>
        public OServer(IThreadManager e, IPAddress ipaddress, int port)
            : this(e)
        {
            IPAddress = ipaddress.ToString();
            Port = port;
            IsSecure = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Used to start the server listener and wait for connections.
        /// </summary>
        public override void StartServer()
        {

            base.StartServer();

            StatusUpdate?.Invoke("Server started");

        }

        /// <summary>
        /// Used to stop the server and close / clear current connections.
        /// </summary>
        public override void StopServer()
        {

            base.StopServer();

            StatusUpdate?.Invoke("Server stopped");

        }
      
        /// <summary>
        /// Used once a client connection is made and created async.
        /// </summary>
        /// <param name="ar"></param>
        public override void OnAccept(IAsyncResult ar)
        {

            try
            {
                TcpClient connection = Listener?.EndAcceptTcpClient(ar);

                if (connection != null) {
                    GetConnection(connection, out Exception ConError);
                    if (ConError != null)
                        throw ConError;
                }

                ResetClientConnected.Set();

            }
            catch (Exception ex) {

                if (!ex.Message.ToLower().Contains("unable to read data"))
                    if (!ex.Message.ToLower().Contains("the handshake failed"))
                        if (!ex.Message.ToLower().Contains("invalid uri"))
                            if (ex.GetType() != typeof(NullReferenceException))
                                OnError?.Invoke(ex);

            }

            try
            {
                Listener?.BeginAcceptTcpClient(new AsyncCallback(OnAccept), Listener);
            }
            catch (Exception ex)
            {
                if (Listener != null)
                {
                    Listener.Stop();
                    Listener = null;
                    Thread.Sleep(700);
                    ListenAsync();
                }
                OnError?.Invoke(ex);

            }

        }

        /// <summary>
        /// This is used to return a <see cref="OTCPUserConnection"/> from the incomming <see cref="TcpClient"/> from the <see cref="OnAccept(IAsyncResult)"/>
        /// This is also where the SNI on any TLS connection is created.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private OConnection GetConnection(TcpClient e, out Exception ConnectionError)
        {
            Thread.Sleep(1);

            OConnection userConnection = null;
            IPEndPoint  clientEndPoint = (IPEndPoint)e.Client.RemoteEndPoint;
         
            ConnectionError = null;


            try
            {

                if (IsSecure)
                {

                    Thread.Sleep(50);

                    OServerSNI          serverNameIndicatation = new(e.Client);
                    X509Certificate2    certificate = null;

                    if (!serverNameIndicatation.IsValid)
                    {
                        e.Client.Close();
                        e.Dispose();
                        StatusUpdate?.Invoke("New connection invalid: disconnected.");
                        throw new Exception("New connection invalid: disconnected.");
                    }

                    certificate = OnGetCertificateFromSNI?.Invoke(serverNameIndicatation);

                    if (certificate == null && cachedCert != null)
                        certificate = cachedCert;
                    else if (certificate == null && cachedCert == null)
                    {
                        e.Client.Close();
                        e.Dispose();
                        StatusUpdate?.Invoke("New uri has no certificate: disconnected.");
                        throw new Exception("New uri has no certificate: disconnected.");
                    }
                    else
                        cachedCert = certificate;

                    userConnection = new OConnection(e, certificate);
                }
                else
                    userConnection = new OConnection(e);
            }
            catch (Exception ex)
            {
                ConnectionError = ex;
            }


            if (userConnection != null) {
                userConnection.DataSent += OnDataSent;
                userConnection.DataReceived += OnDataReceived;
                Clients.Add(e, userConnection);
                OnClientConnect(userConnection);
                StatusUpdate?.Invoke("New connection found.");
                userConnection.Start();
            }

            return userConnection;

        }

        /// <summary>
        /// Used on trigger to relay any sent data in the event <see cref="ODataSentEventHandler"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnDataSent(IConnection sender, Stream e)
        {
            DataSent?.Invoke((OConnection)sender, e);
        }

        /// <summary>
        /// Used on trigger to relay any sent data in the event <see cref="ODataReceivedEventHandler"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public override void OnDataReceived(IConnection sender, byte[] data)
        {
            if (data.Length > 0)
                DataReceived?.Invoke((OConnection)sender, data);

        }

        /// <summary>
        /// Used on trigger to relay the connection and the close any disconnected users in the event <see cref="OClientConnectedEventHandler"/>
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClientConnect(IConnection sender)
        {
            ClientConnected?.Invoke((OConnection)sender);
            CloseDisonnectionUsers();
        }

        /// <summary>
        /// Used on trigger to relay the connection and the close the connection in the event <see cref="OClientDisConnectedEventHandler"/>
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClientDisconnect(IConnection sender)
        {
            base.OnClientDisconnect((OConnection)sender);
            ClientDisConnected?.Invoke((OConnection)sender);
        }

        #endregion

        #region Dispose

        public override void Dispose(bool disposing)
        {
            if (!_isDisposed)
                if (disposing)
                {

                    try
                    {
                        cachedCert = null;
                        StopServer();
                        Clients = null;
                        Listener = null;
                    }
                    catch { }

                }
            _isDisposed = true;
        }

        #endregion

    }


}
