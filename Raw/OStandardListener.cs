/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using Microsoft.VisualBasic;

using K2host.Sockets.Raw.Enums;
using K2host.Sockets.Delegates;

using gs = K2host.Sockets.OHelpers;

namespace K2host.Sockets.Raw
{

    public class OStandardListener : IDisposable
    {

        #region Event Handlers

        public event OnClientGeneralEventHandler        OnClientDataResultApps;
        public event OnClientGeneralEventHandler        OnClientDataResultGeneral;
        public event OnIPEndPointEventHandler           OnClientAccept;
        public event OnIPEndPointEventHandler           OnClientAdded;
        public event OnIPEndPointEventHandler           OnClientRemoved;
        public event OnIPEndPointEventHandler           OnListenStarted;
        public event OnListenRestartEventHandler        OnListenRestart;
        public event OnClientDataSentEventHandler       OnClientDataSent;
        public event OnEventArgsEventHandler            OnClientLoggedOff;
        public event OnClientDataRecivedEventHandler    OnClientDataRecived;
        public event OnEventArgsEventHandler            OnClientLogPacket;
        public event OnEventArgsEventHandler            OnError;

        private delegate void OnClientsInvokeRequired(OStandardClient c);

        #endregion

        #region Properties

        public bool IsListening
        {
            get { return ListenSocket != null; }
        }

        public Socket ListenSocket
        {
            get;
            set;
        }

        public List<OStandardClient> Clients
        {
            get;
        }

        public OStandardClient NewClient
        {
            get;
            set;
        }

        public int ErrorCount
        {
            get;
            set;
        }

        public int BadPacketCount
        {
            get;
            set;
        }

        public int BlockedPacketCount
        {
            get;
            set;
        }

        private DateTime ServerStarted
        {
            get;
            set;
        }

        public long TotalDataStreamed
        {
            get;
            set;
        }

        private bool ClientsInvokeRequired
        {
            get;
            set;
        }

        #endregion

        #region Instance

        public OStandardListener()
        {

            Clients = new List<OStandardClient>();
            ClientsInvokeRequired = false;
            ErrorCount = 0;
            BadPacketCount = 0;
            BlockedPacketCount = 0;

        }

        #endregion

        #region Public Void

        public void Start()
        {
            try
            {
                ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ListenSocket.Bind(new IPEndPoint(gs.GetLocalInternalIp(), (int)OStandardPorts.REMOTE_SYSTEM_MANAGMENT));
                ListenSocket.Listen(50);
                ListenSocket.BeginAccept(new AsyncCallback(OnAccept), ListenSocket);
                OnListenStarted?.Invoke((IPEndPoint)ListenSocket.LocalEndPoint);
                ServerStarted = DateAndTime.Now;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(new OStandardEventArgs(ex.Message, OStandardErrorCode.ListenerError));
            }
        }

        public void Start(IPAddress ip, int port)
        {
            try
            {
                ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ListenSocket.Bind(new IPEndPoint(ip, port));
                ListenSocket.Listen(50);
                ListenSocket.BeginAccept(new AsyncCallback(OnAccept), ListenSocket);

                OnListenStarted?.Invoke((IPEndPoint)ListenSocket.LocalEndPoint);

                ServerStarted = DateAndTime.Now;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(new OStandardEventArgs(ex.Message, OStandardErrorCode.ListenerError));
            }
        }

        public void Restart()
        {
            OnListenRestart?.Invoke();
            if (ListenSocket == null)
                return;
            while (Clients.Count > 0)
            {
                Clients[0].Dispose();
            }
            try
            {
                ListenSocket.Close(1000);
            }
            catch
            {
            }
            ListenSocket.Shutdown(SocketShutdown.Both);
            ListenSocket = null;
            Start();
        }

        public int GetClientCount()
        {
            return Clients.Count;
        }

        public OStandardClient GetClientAt(int idx)
        {
            if (idx < 0 || idx >= GetClientCount())
                return null;
            return (OStandardClient)Clients[idx];
        }

        public OStandardClient GetClientByIp(IPAddress ip)
        {
            foreach (OStandardClient e in Clients)
            {
                if (e.ClientEndPoint.Address.Equals(ip))
                    return e;
            }
            return null;
        }

        public long GetTotalData()
        {
            long t = 0;
            foreach (OStandardClient c in Clients)
            {
                t += c.ClientTotalDataLength;
            }
            return t;
        }

        public long GetUpTime()
        {
            return DateAndTime.DateDiff(DateInterval.Second, ServerStarted, DateAndTime.Now, FirstDayOfWeek.Monday, FirstWeekOfYear.System);
        }

        public void Dispose()
        {
            while (Clients.Count > 0)
            {
                ((OStandardClient)Clients[0]).Dispose();
            }

            try
            {
                ListenSocket.Shutdown(SocketShutdown.Both);
            }
            catch { }

            if (ListenSocket != null)
                ListenSocket.Close(1000);

            ListenSocket = null;

        }

        #endregion

        #region Private Void

        private void OnAccept(IAsyncResult ar)
        {
            try
            {
                Socket NewSocket = ListenSocket.EndAccept(ar);
                if (NewSocket != null)
                {

                    OnClientAccept?.Invoke((IPEndPoint)NewSocket.RemoteEndPoint);

                    NewClient = new OStandardClient(NewSocket, new ODestroyDelegate(this.RemoveClient));
                    NewClient.OnDataSent += NewClient_OnDataSent;
                    NewClient.OnLoggedOff += NewClient_OnLoggedOff;
                    NewClient.OnError += NewClient_OnError;
                    NewClient.OnDataRecived += NewClient_OnDataRecived;
                    NewClient.OnDataCount += NewClient_OnDataCount;
                    NewClient.OnBadPacket += NewClient_OnBadPacket;
                    NewClient.OnBlockedPacket += NewClient_OnBlockedPacket;
                    NewClient.OnLogPacket += NewClient_OnLogPacket;
                    NewClient.OnDataResultGeneral += NewClient_OnDataResultGeneral;
                    NewClient.OnDataResultApps += NewClient_OnDataResultApps;

                    NewClient.StartHandShake();

                    AddClient(NewClient);

                }
            }
            catch
            {
            }
            try
            {
                ListenSocket.BeginAccept(new AsyncCallback(this.OnAccept), ListenSocket);
            }
            catch
            {
                Dispose();
            }
        }

        private void NewClient_OnDataResultApps(OStandardPacket e, OStandardClient client)
        {
            OnClientDataResultApps?.Invoke(e, client);
        }

        private void NewClient_OnDataResultGeneral(OStandardPacket e, OStandardClient client)
        {
            OnClientDataResultGeneral?.Invoke(e, client);
        }

        private void NewClient_OnLogPacket(OStandardEventArgs e)
        {
            OnClientLogPacket?.Invoke(e);
        }

        private void NewClient_OnBadPacket()
        {
            BadPacketCount += 1;
        }

        private void NewClient_OnBlockedPacket()
        {
            BlockedPacketCount += 1;
        }

        private void NewClient_OnDataCount(long e)
        {
            TotalDataStreamed += e;
        }

        private void NewClient_OnDataRecived(string e)
        {
            OnClientDataRecived?.Invoke(e);
        }

        private void NewClient_OnDataSent(byte[] e)
        {
            OnClientDataSent?.Invoke(e);
        }

        private void NewClient_OnLoggedOff(OStandardEventArgs e)
        {
            OnClientLoggedOff?.Invoke(e);
        }

        private void NewClient_OnError(OStandardEventArgs e)
        {
            ErrorCount += 1;
            OnError?.Invoke(e);
        }

        private void AddClient(OStandardClient c)
        {
            if (ClientsInvokeRequired)
            {
                OnClientsInvokeRequired a = new OnClientsInvokeRequired(AddClient);
                a.Invoke(c);
            }
            else
            {
                ClientsInvokeRequired = true;
                if (!Clients.Contains(c))
                {
                    Clients.Add(c);
                    OnClientAdded?.Invoke(c.ClientEndPoint);
                }
                ClientsInvokeRequired = false;
            }
        }

        private void RemoveClient(OStandardClient c)
        {
            IPEndPoint ipe = c.ClientEndPoint;
            Clients.Remove(c);
            c = null;
            OnClientRemoved?.Invoke(ipe);
        }

        #endregion

    }

}
