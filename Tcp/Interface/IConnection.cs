/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Net.Sockets;

namespace K2host.Sockets.Tcp.Interface
{

    /// <summary>
    /// This is used to help create the client object class you define.
    /// </summary>
    public interface IConnection : IDisposable
    {

        /// <summary>
        /// This is the <see cref="TcpClient"/> client connection object
        /// </summary>
        TcpClient Client { get; set; }

        /// <summary>
        /// Used to store extra values as <see cref="ValueType"/>
        /// </summary>
        ValueType ExtraValues { get; set; }

        /// <summary>
        /// Used to store extra data as a string
        /// </summary>       
        string ExtraData { get; set; }

        /// <summary>
        /// This is used to end reads async when the client connection is created
        /// </summary>
        /// <param name="ar"></param>
        void ConnectionReader(IAsyncResult ar);

        /// <summary>
        /// This is used to end writes async when the client connection has returned data.
        /// </summary>
        /// <param name="ar"></param>
        void ConnectionWriter(IAsyncResult ar);

        /// <summary>
        /// Used to send the data down the stream to the client and starts a begin read.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="async"></param>
        void SendData(byte[] e, bool async);

        /// <summary>
        /// Used to close the client from the server.
        /// </summary>
        void CloseClientConnection();

        /// <summary>
        /// Used to close the client from the server. and trigger an event 
        /// </summary>
        void ConnectionClosed();

        /// <summary>
        /// Used to declare a connected socket on the client connection.
        /// </summary>
        /// <returns></returns>
        bool IsConnected();


    }

}
