/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

using K2host.Sockets.Delegates;
using K2host.Sockets.Tcp.Abstarct;

namespace K2host.Sockets.Tcp
{

    /// <summary>
    /// This class is used to create a connection instance with the base class over a statndard tcp network
    /// </summary>
    public class OConnection : OConnectionBase
    {

        #region Attribute Fields

        /// <summary>
        /// This is used to help collect all the data from a stream in read mode.
        /// Try and get all the data, this will slow down the read as the client be slower at writing data to the stream. (chunked for file uploads)
        /// </summary>
        public readonly int SleepTimeCheck = 200;

        /// <summary>
        /// This is used to collect the data returned from the client stream in chunks.
        /// </summary>
        public readonly byte[] ReadBuffer = new byte[4096 * 2];

        /// <summary>
        /// Used to create a full buffer of data to send back though the event <see cref="ODataReceivedEventHandler"/>
        /// </summary>
        public readonly MemoryStream OutputBuffer = new();

        bool _isDisposed = false;


        #endregion

        #region Properties

        /// <summary>
        /// This is used to authenticate a tls (ssl) stream.
        /// </summary>
        public X509Certificate2 AssignedCertificate { get; set; }

        /// <summary>
        /// This is the stream in the connection, this can be either a <see cref="NetworkStream"/> or an <see cref="SslStream"/>
        /// based on the connection handshake.
        /// </summary>
        public Stream ClientStream { get; set; }

        /// <summary>
        /// This is set if the incomming connection requires a tls stream.
        /// </summary>
        public bool IsSecureSocket { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Used to trigger and send the received data from the client.
        /// </summary>
        public ODataReceivedEventHandler DataReceived { get; set; }

        /// <summary>
        /// Used to trigger on closing the client connection.
        /// </summary>
        public OClientCloseEventHandler ClientClose { get; set; }

        /// <summary>
        /// Used to trigger on closing the client connection the client side.
        /// </summary>
        public OClientRemoteCloseEventHandler ClientRemoteClose { get; set; }

        /// <summary>
        /// Used to trigger when data has been sent to the client.
        /// </summary>
        public ODataSentEventHandler DataSent { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Used to create the instance passsing the <see cref="TcpClient"/> connection. 
        /// </summary>
        /// <param name="client"></param>
        public OConnection(TcpClient client)
        {
            IsSecureSocket = false;
            try
            {
                Client = client;
                ClientStream = Client.GetStream();
                ClientStream.WriteTimeout   = 1000;
                ClientStream.ReadTimeout    = 1000;
                Client.ReceiveTimeout       = 1000;
                Client.ReceiveBufferSize    = 16384;
            }
            catch
            {
                Client.Close();
                ClientClose?.Invoke(this);
            }
        }

        /// <summary>
        /// Used to create the instance passsing the <see cref="TcpClient"/> connection and the <see cref="X509Certificate2"/> if tls is required. 
        /// </summary>
        /// <param name="client"></param>
        public OConnection(TcpClient client, X509Certificate2 certificate)
        {
            //Setup .net system security as tls1.2 is disabled by default. (this normally applies to .NET 4.5.X)
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            AssignedCertificate = certificate;
            IsSecureSocket = true;
            try
            {
                Client = client;
                ClientStream = new SslStream(Client.GetStream(), false);
                ((SslStream)ClientStream).AuthenticateAsServer(AssignedCertificate, false, SslProtocols.Tls12 | SslProtocols.Tls13, false);
                ((SslStream)ClientStream).WriteTimeout  = 1000;
                ((SslStream)ClientStream).ReadTimeout   = 1000;
                Client.ReceiveTimeout                   = 1000;
                Client.ReceiveBufferSize                = 16384;
            }
            catch (Exception)
            {
                Client.Close(); //Authentication failed because the remote party has closed the transport stream.
                ClientClose?.Invoke(this);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the read operation on the stream .
        /// </summary>
        public void Start()
        {
            try
            {
                if (IsSecureSocket)
                    ((SslStream)ClientStream).BeginRead(ReadBuffer, 0, ReadBuffer.Length, ConnectionReader, 0);
                else
                   ClientStream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, ConnectionReader, 0);
            }
            catch
            {
                Client.Close();
                ClientClose?.Invoke(this);
            }
        }

        /// <summary>
        /// Used to reset this instance with a new <see cref="TcpClient"/> which will allready have a <see cref="X509Certificate2"/> if tls is required.
        /// </summary>
        /// <param name="client"></param>
        public void ReInitiate(TcpClient client)
        {
            try
            {

                if (ClientStream != null)
                    ClientStream.Dispose();

                Client = client;

                if (IsSecureSocket)
                {
                    ClientStream = new SslStream(Client.GetStream(), false);
                    ((SslStream)ClientStream).AuthenticateAsServer(AssignedCertificate, false, SslProtocols.Tls12 | SslProtocols.Tls13, false);
                }
                else
                    ClientStream = Client.GetStream();

                ClientStream.WriteTimeout   = 1000;
                ClientStream.ReadTimeout    = 1000;
                Client.ReceiveTimeout       = 1000;
                Client.ReceiveBufferSize    = 16384;

                ClientStream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, ConnectionReader, 0);

            }
            catch (Exception)
            {
                Client.Close(); //Authentication failed because the remote party has closed the transport stream.
                ClientClose?.Invoke(this);
            }
        }

        /// <summary>
        /// Used to EndRead with the <see cref="IAsyncResult"/> and collect the data comming from the client.
        /// </summary>
        /// <param name="ar"></param>
        public override void ConnectionReader(IAsyncResult ar)
        {

            int BytesRead;

            try
            {

                int DataLength = Convert.ToInt32(ar.AsyncState);

                lock (ClientStream)
                    BytesRead = ClientStream.EndRead(ar);

                OutputBuffer.Write(ReadBuffer, 0, BytesRead);

                Thread.Sleep(5);

                if (Client.Available <= 0) {

                    while (true) {
                        try {
                            ClientStream.Flush();
                            BytesRead = ClientStream.Read(ReadBuffer, 0, ReadBuffer.Length);
                            OutputBuffer.Write(ReadBuffer, 0, BytesRead);
                        }
                        catch { break; }
                    }

                    ReceivedData();
                    ClientStream.Flush();
                    ClientStream.Close();
                }
                else
                {
                    ClientStream.Flush();
                    ClientStream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, ConnectionReader, DataLength);
                }

            }
            catch (Exception)
            {
                //The decryption operation failed, see inner exception. The supplied message is incomplete. The signature was not verified
                ConnectionClosed();
            }

        }

        /// <summary>
        /// Used to pass back the data collected by the connection and the memory buffer reset.
        /// </summary>
        public void ReceivedData()
        {

            DataReceived?.Invoke(this, OutputBuffer.ToArray());

            OutputBuffer.Seek(0, SeekOrigin.Begin);
            OutputBuffer.Flush();
            OutputBuffer.SetLength(0);

        }

        /// <summary>
        /// Used to send data to the client though the connection stream.
        /// </summary>
        /// <param name="e">The data you want to send</param>
        /// <param name="async">Used to make the send async</param>
        public override void SendData(byte[] e, bool async)
        {
            try
            {

                if (async)
                    ClientStream.BeginWrite(e, 0, e.Length, new AsyncCallback(ConnectionWriter), null);
                else
                {
                    ClientStream.Write(e, 0, e.Length);

                    DataSent?.Invoke(this, ClientStream);

                    Array.Clear(e, 0, e.Length);
                    e = null;

                    ClientStream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, ConnectionReader, null);
                }

            }
            catch (Exception)
            {
                ConnectionClosed();
            }
        }

        /// <summary>
        /// Used to EndWrite with the <see cref="IAsyncResult"/> is SendData is async.
        /// </summary>
        /// <param name="ar"></param>
        public override void ConnectionWriter(IAsyncResult ar)
        {

            try
            {

                lock (ClientStream)
                    ClientStream.EndWrite(ar);

                DataSent?.Invoke(this, ClientStream);

                ClientStream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, ConnectionReader, null);

            }
            catch (Exception)
            {
                ConnectionClosed();
            }

        }

        /// <summary>
        /// Used to close the clients connection from the server. Triggers the ClientClose event
        /// </summary>
        public override void CloseClientConnection()
        {
            try
            {
                ClientStream.Flush();
                ClientStream.Dispose();

            }
            catch (Exception)
            {

            }

            Client.Close();
            ClientClose?.Invoke(this);

        }

        /// <summary>
        /// Used to close the clients connection from the server. Triggers the ClientRemoteClose event
        /// </summary>
        public override void ConnectionClosed()
        {
            try
            {
                ClientStream.Flush();
                ClientStream.Dispose();
            }
            catch (Exception)
            {

            }

            Client.Close();
            ClientRemoteClose?.Invoke(this);

        }

        #endregion

        #region Dispose

        public override void Dispose(bool disposing)
        {
            if (!_isDisposed)
                if (disposing)
                {

                    OutputBuffer.Close();
                    OutputBuffer.Dispose();

                    if (ClientStream != null)
                        ClientStream.Dispose();

                    Client.Close();

                }
            _isDisposed = true;
        }

        #endregion

    }
}
