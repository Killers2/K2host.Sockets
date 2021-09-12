/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/
using System.Net.Sockets;

namespace K2host.Sockets.Raw
{
    public class OStandardSocketState
    {

        public Socket Sock
        {
            get;
            set;
        }

        public int Size
        {
            get;
            set;
        }

        public byte[] Buffer
        {
            get;
            set;
        }

        public OStandardSocketState(Socket iSocket)
        {
            Sock = iSocket;
            Size = 65335;
            Buffer = new byte[65335];
        }

    }

}
