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

    public class OStandardEventArgsOnData : EventArgs
    {

        public OStandardSocketState State
        {
            get;
            set;
        }

        public int DataSize
        {
            get;
            set;
        }

        public OStandardEventArgsOnData(OStandardSocketState iState, int iDataSize)
        {
            State = iState;
            DataSize = iDataSize;

        }

    }


}
