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
using K2host.Sockets.Raw.Enums;

namespace K2host.Sockets.Raw
{
    public class OStandardClient : IDisposable
    {

        #region Event Handlers

        public event OnEventArgsEventHandler        OnLogPacket;
        public event OnBadPacketEventHandler        OnBadPacket;
        public event OnBlockedPacketEventHandler    OnBlockedPacket;
        public event OnDataCountEventHandler        OnDataCount;
        public event OnDataRecivedEventHandler      OnDataRecived;
        public event OnClientGeneralEventHandler    OnDataResultGeneral;
        public event OnClientGeneralEventHandler    OnDataResultApps;
        public event OnClientDataSentEventHandler   OnDataSent;
        public event OnEventArgsEventHandler        OnLoggedOff;
        public event OnEventArgsEventHandler        OnError;
        private event OnEventArgsEventHandler       OnDataParse;

        #endregion

        #region Fields

        bool IsDisposed = false;
        bool IsInReceive = false;

        #endregion

        #region Properties

        public OStandardPacket ClientPacketManager
        {
            get;
            set;
        }

        public IPEndPoint ClientEndPoint
        {
            get;
            set;
        }

        public Socket ClientSocket
        {
            get;
            set;
        }

        public long ClientTotalDataLength
        {
            get;
            set;
        }

        private ODestroyDelegate Destroyer
        {
            get;
            set;
        }

        private byte[] Buffer
        {
            get;
            set;
        }

        #endregion

        #region Instance

        public OStandardClient(Socket ClientSocket, ODestroyDelegate Destroyer)
        {
            OnDataParse += DataParser;
            this.ClientSocket = ClientSocket;
            this.ClientEndPoint = (IPEndPoint)this.ClientSocket.RemoteEndPoint;
            this.Destroyer = Destroyer;
            ClientPacketManager = new OStandardPacket();
            Buffer = new byte[4096];

            //4kb PACKET_SIZE = 4096;

        }

        #endregion

        #region Public Void

        public void StartHandShake()
        {
            ClientPacketManager.WriteData(OStandardStrings.Communication.Ok);
            ClientPacketManager.WriteData(OStandardStrings.Communication.Initiate);
            Send(ClientPacketManager.Data);
            ClientPacketManager.Truncate();
        }

        public void Send(byte[] b)
        {
            try
            {

                ClientSocket.BeginSend(b, 0, b.Length, SocketFlags.None, new AsyncCallback(EndSend), null);
                OnDataSent?.Invoke(b);
            }
            catch
            {
            }
        }

        public void DataParser(OStandardEventArgs e)
        {
            try
            {

                OnDataCount?.Invoke(e.Packet.Data.Length);

                if (e.Message != string.Empty)
                    OnDataRecived?.Invoke(e.Message);

                OStandardCommunicationCode tmp = (OStandardCommunicationCode)e.Packet.ReadInteger();

                switch (tmp)
                {
                    case OStandardCommunicationCode.PacketCodeGeneral:
                        OnDataResultGeneral?.Invoke(e.Packet, this);
                        break;
                    case OStandardCommunicationCode.PacketCodeApps:
                        OnDataResultApps?.Invoke(e.Packet, this);
                        break;
                    default:
                        OnBlockedPacket?.Invoke();
                        break;
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(new OStandardEventArgs(ex.Message, OStandardErrorCode.ParserError));
                if (!IsDisposed)
                    Dispose();
            }
        }
     
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

                    ClientPacketManager.Truncate();
                    ClientPacketManager.Dispose();
                    ClientPacketManager = null;
                    try
                    {
                        OnLoggedOff?.Invoke(new OStandardEventArgs());
                    }
                    catch
                    {
                    }
                    try
                    {
                        ClientSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                    }
                    if (ClientSocket != null)
                        ClientSocket.Close(1000);
                    ClientSocket = null;
                    Destroyer(this);

                }

            IsDisposed = true;
        }

        #endregion

        #region Private Void

        private void EndSend(IAsyncResult ar)
        {
            try
            {

                int e = ClientSocket.EndSend(ar);
                BeginReceive();

            }
            catch
            {
            }
        }

        private void BeginReceive()
        {
            try
            {
                if (IsInReceive)
                    return;

                IsInReceive = true;
                ClientSocket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(EndReceive), null);
            }
            catch
            {
            }
        }

        private void EndReceive(IAsyncResult ar)
        {
            try
            {
                IsInReceive = false;
                int Length = ClientSocket.EndReceive(ar);

                if (Length == 0)
                {

                    if (!IsDisposed)
                        Dispose();

                }

                byte[] newBuff = new byte[Length];

                Array.Copy(Buffer, newBuff, newBuff.Length);

                Array.Clear(Buffer, 0, Buffer.Length);

                ClientTotalDataLength += newBuff.Length;

                OnLogPacket?.Invoke(new OStandardEventArgs(newBuff));

                OnDataParse?.Invoke(new OStandardEventArgs(newBuff));

                BeginReceive();

            }
            catch
            {
                OnBadPacket?.Invoke();
            }
        }

        #endregion

    }

}
