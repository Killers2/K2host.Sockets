/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/
namespace K2host.Sockets.Raw.Enums
{
    public enum OStandardCommunicationCode : int
    {
        ClientAsk = 1,
        ServerRespond = 2,
        ClientError = 4,
        ServerError = 8,
        PacketCodeApps = 232,
        PacketCodeGeneral = 233
    }
}
