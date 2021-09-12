/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using K2host.Core.JsonConverters;
using K2host.Sockets.Delegates;
using K2host.Sockets.ReverseProxy.Abstract;
using K2host.Sockets.ReverseProxy.Extentions;
using K2host.Sockets.ReverseProxy.Interface;
using K2host.Sockets.ReverseProxy.Mapping.Http;

namespace K2host.Sockets.ReverseProxy.Mapping.Port
{
    
    /// <summary>
    /// This class helps create an socket port mapper with no intervention
    /// </summary>
    public class OProxyListenerPortMap : AProxyListener
    {
        /// <summary>
        /// The destination end point
        /// </summary>
        public IPEndPoint Destination { get; set; }

        /// <summary>
        /// The constructor that creates and sets the listener to accept clients.
        /// </summary>
        public OProxyListenerPortMap() 
        {

            OnAcceptConnection = new AsyncCallback(e => {

                if (!ShuttingDown) {
                    try {

                        Socket s = ListenSocket.EndAccept(e);

                        if (s != null)
                            ((OProxyClientPortMap)this.Add(new OProxyClientPortMap() {
                                ClientSocket        = s,
                                ClientEndPoint      = (IPEndPoint)s.RemoteEndPoint,
                                ClientProtocol      = s.ProtocolType,
                                Destroyer           = new ProxyClientDestroyer(this.Remove),
                                DestinationEndPoint = Destination,
                                DestinationProtocol = DestinationProtocolType,
                                Parent              = this
                            })).StartHandShake();

                    } catch { }

                    try
                    {
                        ListenSocket.BeginAccept(new AsyncCallback(OnAcceptConnection), ListenSocket);
                    } catch {
                        Dispose();
                    }

                }

            });

        }
        
        /// <summary>
        ///This abstract method to convert the object and its counter parts to json for saving
        /// </summary>
        public override JObject Save(JsonSerializerSettings e = null)
        {
            return JObject.Parse(JsonConvert.SerializeObject(this, new JsonSerializerSettings() { 
                 Converters = new List<JsonConverter>() {
                     { new IPAddressConverter() },
                     { new IPEndPointConverter() },
                     { new InterfaceConverter<IProxyFilterSearch, OProxyFilterSearch>() },
                     { new InterfaceConverter<IProxyFailOver, OProxyFailOver>() }
                 },
                Formatting = Formatting.Indented
            }));
        }

    }

}
