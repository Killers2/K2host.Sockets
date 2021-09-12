/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/
namespace K2host.Sockets.Raw.Enums
{

    public enum OInetConnState : int
    {
        Modem = 0x1,
        LAN = 0x2,
        Proxy = 0x4,
        RasInstalled = 0x10,
        Offline = 0x20,
        Configured = 0x40,
    }

}
