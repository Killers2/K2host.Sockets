/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/
using System;
using System.Net;
using System.Net.Sockets;

using K2host.Sockets.Delegates;

using gs = K2host.Sockets.OHelpers;

namespace K2host.Sockets.Raw
{
   
    public class OStandardSocket : IDisposable
    {

        #region Event Handlers

        public OnClosedEventHandler         OnClosed;
        public OnSocketErrorEventHandler    OnError;
        public OnConnectionEventHandler     OnConnected;
        public OnConnectionEventHandler     OnConnectionLost;
        public OnDataReceivedEventHandler   OnDataReceived;
        public OnDataSentEventHandler       OnDataSent;

        #endregion

        #region Fields

        readonly ProtocolType ThisSocketType;
        bool IsInReceive = false;

        #endregion

        #region Properties

        public Socket MySocket { get; set; }

        public byte[] GetData { get; set; }

        public string GetDataString { get { return System.Text.Encoding.ASCII.GetString(GetData); } }

        public bool Bind { get; set; }

        private IPEndPoint RemoteEndPoint { get; set; }

        private IPEndPoint LocalEndPoint { get; set; }

        public string LocalIP { get; set; }

        public int LocalPort { get; set; }

        public string RemoteIP { get; set; }

        public int RemotePort { get; set; }

        #endregion

        #region Instance

        public OStandardSocket(ProtocolType e)
        {
            ThisSocketType = e;
        }

        #endregion

        #region Private Voids

        private void CreateSocket()
        {
            try
            {
                if (MySocket != null)
                    MySocket = null;

                MySocket = new Socket(AddressFamily.InterNetwork, gs.GetSocketType(ThisSocketType), ThisSocketType);

            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new OStandardEventArgsSocketError(ex.Message, ex.ToString()));
            }
        }

        private void EndConnect(IAsyncResult Result)
        {
            try
            {
                MySocket.EndConnect(Result);
                if (!Bind)
                {
                    LocalEndPoint = (IPEndPoint)MySocket.LocalEndPoint;
                    LocalIP = LocalEndPoint.Address.ToString();
                    LocalPort = Convert.ToInt32(LocalEndPoint.Port.ToString());
                }
                OnConnected?.Invoke(this, new EventArgs());
                BeginReceive();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new OStandardEventArgsSocketError("ERROR: Connection timed out or failed on " + RemoteIP + ":" + RemotePort, ex.ToString()));
            }
        }

        private void BeginReceive()
        {
            try
            {

                if (IsInReceive)
                    return;

                IsInReceive = true;
                GetData = null;
                OStandardSocketState State = new OStandardSocketState(MySocket);

                MySocket.BeginReceive(State.Buffer, 0, State.Size, SocketFlags.None, EndReceive, State);

            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Cannot access a disposed object"))
                    OnError?.Invoke(this, new OStandardEventArgsSocketError(ex.Message, ex.ToString()));
            }
        }

        private void EndReceive(IAsyncResult Result)
        {
            OStandardSocketState ss = (OStandardSocketState)Result.AsyncState;
            int DataSize = 0;
            IsInReceive = false;

            try
            {
                DataSize = ss.Sock.EndReceive(Result);

                if (DataSize == 0)
                {
                    OnConnectionLost?.Invoke(this, new EventArgs());
                    ss.Sock.Shutdown(SocketShutdown.Both);
                    ss.Sock.Close();
                    ss.Sock = null;
                }
                else
                {
                    OnDataReceived?.Invoke(this, ss.Buffer);
                    BeginReceive();
                }
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Cannot access a disposed object"))
                    OnError?.Invoke(this, new OStandardEventArgsSocketError(ex.Message, ex.ToString()));
            }
        }

        private void EndSend(IAsyncResult Result)
        {
            try
            {
                Socket iSocket = (Socket)Result.AsyncState;
                int Length = iSocket.EndSend(Result);
                OnDataSent?.Invoke(this, new OStandardEventArgsOnSendComplete(Length));
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Cannot access a disposed object"))
                {
                    OnError?.Invoke(this, new OStandardEventArgsSocketError(ex.Message, ex.ToString()));
                }
                else
                {
                    OnClosed?.Invoke(this);
                }
            }
        }

        private void EndDisconnect(IAsyncResult Result)
        {
            try
            {
                Socket iSocket = (Socket)Result.AsyncState;
                iSocket.EndDisconnect(Result);
                if (iSocket != null)
                {
                    iSocket.Close(1000);
                    iSocket = null;
                }
                OnClosed?.Invoke(this);
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Cannot access a disposed object"))
                {
                    OnError?.Invoke(this, new OStandardEventArgsSocketError(ex.Message, ex.ToString()));
                }
                else
                {
                    OnClosed?.Invoke(this);
                }
            }
        }

        #endregion

        #region Public Voids

        public void Connect()
        {
            try
            {
                CreateSocket();
                RemoteEndPoint = new IPEndPoint(IPAddress.Parse(RemoteIP), RemotePort);
                if (Bind)
                    MySocket.Bind(new IPEndPoint(IPAddress.Parse(LocalIP), LocalPort));
                MySocket.BeginConnect(RemoteEndPoint, EndConnect, MySocket);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new OStandardEventArgsSocketError(ex.Message, ex.ToString()));
            }
        }

        public void Send(byte[] Data)
        {
            try
            {
                MySocket.BeginSend(Data, 0, Data.Length, SocketFlags.None, EndSend, MySocket);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new OStandardEventArgsSocketError(ex.Message, ex.ToString()));
            }
        }

        public void Disconnect()
        {
            try
            {
                if (MySocket.SocketType == SocketType.Dgram)
                {
                    MySocket.Close(1000);
                    MySocket = null;
                    OnClosed?.Invoke(this);
                }
                else
                {
                    MySocket.BeginDisconnect(false, EndDisconnect, MySocket);
                }
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Cannot access a disposed object"))
                {
                    OnError?.Invoke(this, new OStandardEventArgsSocketError(ex.Message, ex.ToString()));
                }
                else
                {
                    OnClosed?.Invoke(this);
                }
            }
        }

        public void Dispose()
        {
            try
            {
                MySocket.Shutdown(SocketShutdown.Both);
            }
            catch { }
            try
            {
                MySocket.Disconnect(false);
            }
            catch { }
            try
            {
                MySocket.Close(1000);
            }
            catch { }
        }

        #endregion

    }

}
