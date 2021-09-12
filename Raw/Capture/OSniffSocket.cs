/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using SharpPcap;
using SharpPcap.LibPcap;

using PacketDotNet;

using K2host.Sockets.Raw.Enums;
using K2host.Threading.Classes;
using K2host.Threading.Extentions;
using System.Runtime.InteropServices;

namespace K2host.Sockets.Raw.Capture
{

    /// <summary>
    /// This class creates the listener socket for packet sniffing at network traffic.
    /// </summary>
    public class OSniffSocket : IDisposable
    {

        #region Event Handlers

        public delegate void OnPacketSniffedEventHandler(OSniffSocket sender, Packet e);
        public delegate void OnStoppedEventHandler(OSniffSocket sender);
        public delegate void OnErrorEventHandler(OSniffSocket sender, Exception e);

        public OnPacketSniffedEventHandler OnPacketSniffed;
        public OnStoppedEventHandler OnStopped;
        public OnErrorEventHandler OnError;

        #endregion

        #region Fields

        private readonly int BufferLength = 65535;
        private readonly byte[] Buffer;
        private readonly OThreadManager ThreadManager = null;

        bool IsDisposed = false;

        #endregion

        #region Properties

        public bool IsMTA
        {
            get;
            set;
        }

        public bool IsNpCap
        {
            get;
            set;
        }

        public Socket MySocket
        {
            get;
            set;
        }

        public LibPcapLiveDevice MyDevice
        {
            get;
            set;
        }

        public string MyDeviceFilter
        {
            get;
            set;
        }

        public IPEndPoint LocalEndPoint
        {
            get;
            set;
        }

        public string LocalIP
        {
            get;
            set;
        }

        public int LocalPort
        {
            get;
            set;
        }

        public OSniffSocketDirection SocketDirection
        {
            get;
            set;
        }

        #endregion

        #region Constructor

        public OSniffSocket()
        {
            SocketDirection = OSniffSocketDirection.BOTH;
            Buffer = new byte[BufferLength];
            IsMTA = false;
            IsNpCap = false;
            MyDevice = null;
            MyDeviceFilter = string.Empty;
        }

        public OSniffSocket(OThreadManager threadManager, string ipAddress, int port = 0)
            : this()
        {
            LocalIP = ipAddress;
            LocalPort = port;
            LocalEndPoint = new IPEndPoint(IPAddress.Parse(LocalIP), LocalPort);
            ThreadManager = threadManager;
        }

        public OSniffSocket(OThreadManager threadManager, IPAddress ipAddress, int port = 0)
            : this()
        {
            LocalIP = ipAddress.ToString();
            LocalPort = port;
            LocalEndPoint = new IPEndPoint(ipAddress, LocalPort);
            ThreadManager = threadManager;
        }

        public OSniffSocket(OThreadManager threadManager, IPEndPoint endpoint)
            : this()
        {
            LocalEndPoint = endpoint;
            LocalIP = LocalEndPoint.Address.ToString();
            LocalPort = LocalEndPoint.Port;
            ThreadManager = threadManager;
        }

        #endregion

        #region Private Methods

        private void CreateSocket()
        {
            try
            {

                if (MySocket != null)
                    MySocket = null;

                MySocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, System.Net.Sockets.ProtocolType.IP);

            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
        }

        private void CreateDevice()
        {
            try
            {

                foreach (LibPcapLiveDevice d in CaptureDeviceList.Instance)
                    if (d.Addresses.ToArray().GetIpAddress(4) == LocalIP)
                    {
                        MyDevice = d;
                        break;
                    }


                if (MyDevice != null)
                {

                    int readTimeoutMilliseconds = 1000;

                    MyDevice.OnPacketArrival += new PacketArrivalEventHandler(EndReceive);

                    MyDevice.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);

                    if (!string.IsNullOrEmpty(MyDeviceFilter))
                        MyDevice.Filter = MyDeviceFilter;

                    MyDevice.StartCapture();

                }

            }
            catch (Exception ex)
            {

                MyDevice.OnPacketArrival -= new PacketArrivalEventHandler(EndReceive);

                try { MyDevice.StopCapture(); } catch { }
                try { MyDevice.Close(); } catch { }

                OnError?.Invoke(this, ex);

            }
        }

        private void EndReceive(object sender, CaptureEventArgs e)
        {
            try
            {

                if (IsMTA == true && ThreadManager != null)
                {
                    ThreadManager.Add(
                        new OThread(
                            new ParameterizedThreadStart(
                                delegate (object data) {
                                    OnPacketSniffed?.Invoke(this, Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data));
                                }
                            )
                        )
                    ).Start(e.Packet);
                }
                else
                {
                    OnPacketSniffed?.Invoke(this, Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data));
                }

            }
            catch (Exception ex)
            {

                if (!ex.Message.Contains("Cannot access a disposed object"))
                {
                    OnError?.Invoke(this, ex);
                }

                MyDevice.OnPacketArrival -= new PacketArrivalEventHandler(EndReceive);

                try { MyDevice.StopCapture(); } catch { }

                try { MyDevice.Close(); } catch { }

                Start();

            }
        }

        private void EndReceive(IAsyncResult Result)
        {
            try
            {

                if (MySocket != null)
                {

                    int BytesRead = MySocket.EndReceive(Result);

                    byte[] Data = new byte[BytesRead];

                    Array.Copy(Buffer, Data, BytesRead);

                    if (IsMTA == true && ThreadManager != null)
                    {
                        ThreadManager.Add(
                            new OThread(
                                new ParameterizedThreadStart(
                                    delegate (object e) {
                                        OnPacketSniffed?.Invoke(this, Packet.ParsePacket(LinkLayers.Ethernet, (byte[])e));
                                    }
                                )
                            )
                        ).Start(Data.Clone());
                    }
                    else
                    {
                        OnPacketSniffed?.Invoke(this, Packet.ParsePacket(LinkLayers.Ethernet, Data));
                    }

                    Array.Clear(Buffer, 0, Buffer.Length);

                    MySocket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(EndReceive), null);

                }

            }
            catch (Exception ex)
            {

                if (!ex.Message.Contains("Cannot access a disposed object"))
                    OnError?.Invoke(this, ex);

                if (MySocket != null)
                {
                    MySocket.Close(100);
                    MySocket = null;
                }

                Start();
            }
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            try
            {

                if (IsNpCap)
                    CreateDevice();
                else
                {

                    CreateSocket();

                    MySocket.Bind(new IPEndPoint(IPAddress.Parse(LocalIP), LocalPort));
                    MySocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

                    int InBuff = 0;
                    int OutBuff = 0;

                    switch (SocketDirection)
                    {
                        case OSniffSocketDirection.IN:
                            InBuff = 1;
                            break;
                        case OSniffSocketDirection.OUT:
                            OutBuff = 1;
                            break;
                        case OSniffSocketDirection.BOTH:
                            InBuff = 1;
                            OutBuff = 1;
                            break;
                        default:
                            InBuff = 1;
                            OutBuff = 1;
                            break;
                    }

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        MySocket.IOControl(IOControlCode.ReceiveAll, BitConverter.GetBytes(InBuff), BitConverter.GetBytes(OutBuff));

                    MySocket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(EndReceive), null);

                }

            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
        }

        public void Stop()
        {
            try
            {

                if (MySocket != null)
                {
                    MySocket.Close(100);
                    MySocket = null;
                }

                if (MyDevice != null)
                {
                    MyDevice.StopCapture();
                    MyDevice.Close();
                    MyDevice = null;
                }

                Thread.Sleep(100);

                OnStopped?.Invoke(this);

            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
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

                    if (MySocket != null)
                    {
                        try { MySocket.Shutdown(SocketShutdown.Both); } catch { }
                        try { MySocket.Disconnect(false); } catch { }
                        try { MySocket.Close(1000); } catch { }
                    }
                    if (MyDevice != null)
                    {
                        try { MyDevice.OnPacketArrival -= new PacketArrivalEventHandler(EndReceive); } catch { }
                        try { MyDevice.StopCapture(); } catch { }
                        try { MyDevice.Close(); } catch { }
                    }

                }

            IsDisposed = true;
        }

        #endregion

    }

}