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
    public class OStandardStrings
    {

        public static string Space = " ";
        public static string Error = "error";

        public class Communication
        {

            public static Int16 Ok = 255;
            public static Int16 Initiate = 1;
            public static Int16 Diagnostics = 2;
            public static Int16 Response = 3;
            public static Int16 WindowsControl = 4;
            public static Int16 CommandPromt = 6;
            public static Int16 Applications = 7;

        }

    }


}
