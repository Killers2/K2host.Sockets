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
    public class OStandardException : Exception
    {

        #region Properties

        public OStandardErrorCode ErrorCode
        {
            get;
            set;
        }

        #endregion

        #region Instance

        public OStandardException()
            : base()
        {
        }

        public OStandardException(string message, OStandardErrorCode code)
            : base(message)
        {
            ErrorCode = code;
        }

        #endregion

    }

}
