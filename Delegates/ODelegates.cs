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
using System.Security.Cryptography.X509Certificates;

using K2host.Sockets.Raw;
using K2host.Sockets.ReverseProxy.Interface;
using K2host.Sockets.ReverseProxy.Mapping.Http;
using K2host.Sockets.Tcp;

namespace K2host.Sockets.Delegates
{

    public delegate void OnClosedEventHandler(OStandardSocket sender);

    public delegate void OnSocketErrorEventHandler(OStandardSocket sender, OStandardEventArgsSocketError e);
  
    public delegate void OnErrorEventHandler(Exception e);

    public delegate void OnDataReceivedEventHandler(OStandardSocket Sender, byte[] e);

    public delegate void OnDataSentEventHandler(OStandardSocket Sender, OStandardEventArgsOnSendComplete e);

    public delegate void OnConnectionEventHandler(OStandardSocket Sender, EventArgs e);
    
    public delegate void OnEventArgsEventHandler(OStandardEventArgs e);

    public delegate void OnClientGeneralEventHandler(OStandardPacket e, OStandardClient client);

    public delegate void OnIPEndPointEventHandler(IPEndPoint e);

    public delegate void OnBadPacketEventHandler();

    public delegate void OnBlockedPacketEventHandler();

    public delegate void OnDataCountEventHandler(long e);

    public delegate void OnDataRecivedEventHandler(string e);

    public delegate void OnListenRestartEventHandler();

    public delegate void OnClientDataSentEventHandler(byte[] e);

    public delegate void OnClientDataRecivedEventHandler(string e);

    public delegate void ODestroyDelegate(OStandardClient c);

    public delegate void ODataReceivedEventHandler(OConnection sender, byte[] data);

    public delegate void OClientConnectedEventHandler(OConnection sender);

    public delegate void OClientDisConnectedEventHandler(OConnection sender);

    public delegate void OClientCloseEventHandler(OConnection sender);

    public delegate void OClientRemoteCloseEventHandler(OConnection sender);

    public delegate void ODataSentEventHandler(OConnection sender, Stream e);

    public delegate void OStatusUpdateEventHandler(string status);

    public delegate X509Certificate2 OnGetCertificateFromSNIEvent(OServerSNI e);

    public delegate IProxyClient ProxyClientDestroyer(IProxyClient c);

    public delegate bool OnProxyHttpFilterCheck(OProxyClientHttp e, out string errorOut);

    public delegate bool OnProxyClientProcessRelay(byte[] buffer, int read, bool fromRelay, out byte[] newBuffer);

    public delegate void OnWriteAsyncComplete(Stream e);

    public delegate void OnReadAsyncComplete(Stream e, MemoryStream output);

}
