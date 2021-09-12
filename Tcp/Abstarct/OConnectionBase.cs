/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Net.Sockets;

using K2host.Sockets.Tcp.Interface;

namespace K2host.Sockets.Tcp.Abstarct
{
    /// <summary>
    /// Used to create a base class for your instance that inherits this abstract.
    /// </summary>
    public abstract class OConnectionBase : IConnection
    {
        /// <summary>
        /// This is the <see cref="TcpClient"/> client connection object
        /// </summary>
        public virtual TcpClient Client { get; set; }

        /// <summary>
        /// Used to store extra values as <see cref="ValueType"/>
        /// </summary>
        public virtual ValueType ExtraValues { get; set; }

        /// <summary>
        /// Used to store extra data as a string
        /// </summary>     
        public virtual string ExtraData { get; set; }

        /// <summary>
        /// This is used to end reads async when the client connection is created
        /// </summary>
        /// <param name="ar"></param>
        public abstract void ConnectionReader(IAsyncResult ar);

        /// <summary>
        /// This is used to end writes async when the client connection has returned data.
        /// </summary>
        /// <param name="ar"></param>
        public abstract void ConnectionWriter(IAsyncResult ar);

        /// <summary>
        /// Used to send the data down the stream to the client and starts a begin read.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="async"></param>
        public abstract void SendData(byte[] e, bool async);

        /// <summary>
        /// Used to close the client from the server.
        /// </summary>
        public abstract void CloseClientConnection();

        /// <summary>
        /// Used to close the client from the server. and trigger an event 
        /// </summary>
        public abstract void ConnectionClosed();

        /// <summary>
        /// Used to declare a connected socket on the client connection.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsConnected()
        {

            bool connected;

            try
            {

                connected = !(Client.Client.Poll(1, SelectMode.SelectRead) && Client.Client.Available == 0);

            }
            catch
            {

                connected = false;

            }

            return connected;

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
