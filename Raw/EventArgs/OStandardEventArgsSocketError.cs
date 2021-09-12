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
    public class OStandardEventArgsSocketError : EventArgs
    {

        public string Message
        {
            get;
            set;
        }

        public string ErrorCode
        {
            get;
            set;
        }

        public OStandardEventArgsSocketError(string msg)
        {
            Message = msg;
        }

        public OStandardEventArgsSocketError(string msg, string code)
        {
            Message = msg;
            ErrorCode = code;
        }

    }

}
