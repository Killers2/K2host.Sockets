/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/
using System;
using System.Net;
using System.Net.NetworkInformation;

namespace K2host.Sockets.Classes
{

    public class ONetworkCard : IDisposable
    {


        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public NetworkInterfaceType NetworkInterfaceType { get; set; }

        public bool IsReceiveOnly { get; set; }

        public OperationalStatus OperationalStatus { get; set; }

        public long Speed { get; set; }

        public bool SupportsMulticast { get; set; }

        public IPAddress IPAddress { get; set; }

        public string MacAddress { get; set; }

        public IPv4InterfaceStatistics IPv4Statistics { get; set; }

        public IPAddressCollection DhcpServerAddresses { get; set; }

        public IPAddressCollection DnsAddresses { get; set; }

        public string DnsSuffix { get; set; }

        public GatewayIPAddressInformationCollection GatewayAddresses { get; set; }

        public IPAddressCollection WinsServersAddresses { get; set; }

        public MulticastIPAddressInformationCollection MulticastAddresses { get; set; }

        public UnicastIPAddressInformationCollection UnicastAddresses { get; set; }


        public ONetworkCard()
        {

        }


        private bool IsDisposed = false;

        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing) { }

            }
            IsDisposed = true;
        }


    }


}
