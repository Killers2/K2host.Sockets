/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/
using System;

namespace K2host.Sockets.Raw
{

    public class OStandardEventArgsOnSendComplete : EventArgs
    {

        public int SentBytes
        {
            get;
            set;
        }

        public OStandardEventArgsOnSendComplete(int iSentBytes)
        {
            SentBytes = iSentBytes;
        }

    }

}
