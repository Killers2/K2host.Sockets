/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/
using System;

using K2host.Sockets.Raw.Enums;

namespace K2host.Sockets.Raw
{

    public class OStandardEventArgs : EventArgs
    {

        #region Properties

        public string Message
        {
            get;
            set;
        }

        public OStandardErrorCode ErrorCode
        {
            get;
            set;
        }

        public OStandardCommunicationCode CommunicationCode
        {
            get;
            set;
        }

        public OStandardPacket Packet
        {
            get;
            set;
        }

        #endregion

        #region Instance

        public OStandardEventArgs()
            : base()
        {
        }

        public OStandardEventArgs(string message, OStandardErrorCode code)
            : base()
        {
            Message = message;
            ErrorCode = code;
        }

        public OStandardEventArgs(string message, OStandardCommunicationCode code, byte[] data)
            : base()
        {
            Message = message;
            CommunicationCode = code;
            Packet = new OStandardPacket(data);
        }

        public OStandardEventArgs(byte[] data)
            : base()
        {
            Packet = new OStandardPacket(data);
        }

        #endregion

    }



}
