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
using System.Text;
using System.Linq;
using System.Net.Sockets;
using System.Collections.Generic;

using K2host.Core;

namespace K2host.Sockets.Tcp
{
    /// <summary>
    /// This class help obtain the uri in the tls handshake in and SSL call.
    /// </summary>
    public class OServerSNI : IDisposable
    {

        /// <summary>
        /// This is set upon trying to get the hostname from the handshake.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// This contains the host name request in the handshake.
        /// </summary>
        public string ServerNameIndication { get; set; }

        /// <summary>
        /// This contains the Uri request in the handshake.
        /// </summary>
        public Uri UrlIndication { get; set; }

        /// <summary>
        /// This contains the byte array of the TLS handshake before the connection is made.
        /// Reference this site for the break down to each byte in this stream https://tls.ulfheim.net/
        /// </summary>
        public MemoryStream HandShake { get; set; }

        /// <summary>
        /// This holds the error if the property IsValid is false.
        /// </summary>
        public string ErrorMesssage { get; set; }

        /// <summary>
        /// This is the constuctor that creates the instance of this object.
        /// </summary>
        public OServerSNI()
        {
            HandShake               = null;
            ServerNameIndication    = string.Empty;
            IsValid                 = false;
            ErrorMesssage           = string.Empty;
        }

        /// <summary>
        /// This is the constuctor that creates the instance of this object.
        /// Usesing the socket to which the connection is asking for a ssl stream.
        /// </summary>
        public OServerSNI(Socket e)
        {

            Exception ex;

            byte[] p = Peek(e);

            //This makes the memory stream resizable
            HandShake = new MemoryStream(0);
            HandShake.Write(p, 0, p.Length);

            int _port = ((IPEndPoint)e.LocalEndPoint).Port;

            if (HandShake.Length > 0)
            {
                ServerNameIndication = Parse(HandShake.ToArray(), out Exception Error);

                //its SNI, so its always https
                if (!string.IsNullOrEmpty(ServerNameIndication))
                    UrlIndication = new Uri("https://" + ServerNameIndication + (_port == 443 ? string.Empty : ":" + _port.ToString()));
                else
                    Error = new Exception("Non-SSL SNI.");

                ex = Error;

                if (ex != null)
                    ErrorMesssage = ex.Message;

                if (ex == null)
                    IsValid = true;
            }
            else
            {
                ErrorMesssage = "Non-SSL SNI.";
                IsValid = false;
            }

        }

        /// <summary>
        /// Used to create an instance of this object using the client socket incomming connection.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static OServerSNI Get(Socket e)
        {

            Exception ex = null;

            OServerSNI o = new() {
                HandShake = new MemoryStream(0)
            };

            byte[] p = Peek(e);

            //This makes the memory stream resizable
            o.HandShake.Write(p, 0, p.Length);

            int _port = ((IPEndPoint)e.LocalEndPoint).Port;

            if (o.HandShake.Length > 0)
            {
                o.ServerNameIndication = Parse(o.HandShake.ToArray(), out Exception Error);

                //its SNI, so its always https
                if (!string.IsNullOrEmpty(o.ServerNameIndication))
                    o.UrlIndication = new Uri("https://" + o.ServerNameIndication + (_port == 443 ? string.Empty : ":" + _port.ToString()));

                ex = Error;
            }

            if (ex != null)
                o.ErrorMesssage = ex.Message;

            if (ex == null)
                o.IsValid = true;

            return o;

        }

        /// <summary>
        /// This is used on the client socket incomming connection to peek for TLS data
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static byte[] Peek(Socket e)
        {

            int r = 0;
            byte[] b = new byte[e.Available];

            while (r < e.Available)
                r = e.Receive(b, SocketFlags.Peek);

            return b;

        }

        /// <summary>
        /// This is used on the client socket incomming connection to parse the TLS data for the SNI information.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static string Parse(byte[] e, out Exception Error)
        {

            Error = null;

            string output = string.Empty;
            byte[] marker = new byte[3] { 0x00, 0x00, 0x00 };
            int trimby = 6;

            try
            {

                IEnumerable<int> found = e.WhereIs(marker);

                if (found.Any())
                    foreach (int position in found)
                    {

                        byte[] item = new byte[e.Length - (position + marker.Length)];

                        Array.Copy(e, (position + marker.Length + trimby), item, 0, item.Length - trimby);

                        int eof = item.FindIndex(w => w == (byte)0x00);

                        output = Encoding.ASCII.GetString(item, 0, eof);

                        if (Uri.CheckHostName(output) == UriHostNameType.Dns)
                            break;

                    }


            }
            catch (Exception ex)
            {
                Error = ex;
            }

            return output;

        }

        #region Deconstuctor

        public bool IsDisposed { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

            if (!IsDisposed)
                if (disposing)
                {
                    HandShake?.Dispose();
                }

            IsDisposed = true;
        }

        #endregion

    }

}
