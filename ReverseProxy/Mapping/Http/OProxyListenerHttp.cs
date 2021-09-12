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

using K2host.Core;
using K2host.Core.Delegates;
using K2host.Core.JsonConverters;
using K2host.Threading.Classes;
using K2host.Sockets.Delegates;
using K2host.Sockets.ReverseProxy.Abstract;
using K2host.Sockets.ReverseProxy.Extentions;
using K2host.Sockets.ReverseProxy.Interface;

using sc = K2host.Sockets.OHelpers;

namespace K2host.Sockets.ReverseProxy.Mapping.Http
{
    /// <summary>
    /// This class helps create an socket port mapper with http filter intervention
    /// </summary>
    public class OProxyListenerHttp : AProxyListener
    {
        /// <summary>
        /// The http request filters used on this listener.
        /// </summary>
        public OProxyFilter[] Filters { get; set; }
        
        /// <summary>
        /// The thread manager used on the system.
        /// </summary>
        [JsonIgnore]
        public OThreadManager ThreadManager { get; set; }

        /// <summary>
        /// The default html denyed request content file.
        /// </summary>
        [JsonIgnore]
        public string DeniedRequestHtmlContent { get; set; }

        /// <summary>
        /// The default html bad request content file.
        /// </summary>
        [JsonIgnore]
        public string BadRequestHtmlContent { get; set; }

        /// <summary>
        /// The constructor that creates and sets the listener to accept clients.
        /// Passing the filters on the listener to the client connections for processing the request.
        /// </summary>
        public OProxyListenerHttp() 
        {

            OnBeforeStart = new OServiceMethod(e => { 
                
                Filters.ForEach(f => {

                    if (f.ProxyAuthorizationBasic.Length > 0) 
                        f.CheckList.Add(new OnProxyHttpFilterCheck(sc.IsAuthenticated));
                    
                    if (f.DeniedUrlPaths.Length > 0) 
                        f.CheckList.Add(new OnProxyHttpFilterCheck(sc.IsPathAllowed));
                    
                    if (f.MaxContentLength > -1) 
                        f.CheckList.Add(new OnProxyHttpFilterCheck(sc.IsContentLengthAllowed));
                    
                    if (f.DeniedAcceptEncoding.Length > 0) 
                        f.CheckList.Add(new OnProxyHttpFilterCheck(sc.IsAcceptEncodingAllowed));
                    
                    if (f.DeniedAcceptContentTypes.Length > 0) 
                        f.CheckList.Add(new OnProxyHttpFilterCheck(sc.IsAcceptContentAllowed));
                    
                    if (f.DeniedLanguages.Length > 0) 
                        f.CheckList.Add(new OnProxyHttpFilterCheck(sc.IsLanguageAllowed));
                    
                    if (f.DeniedUserAgents.Length > 0) 
                        f.CheckList.Add(new OnProxyHttpFilterCheck(sc.IsUserAgentAllowed));
                   
                    if (f.DeniedContentTypes.Length > 0) 
                        f.CheckList.Add(new OnProxyHttpFilterCheck(sc.IsContentTypesAllowed));
                    
                    if (f.DeniedReferers.Length > 0) 
                        f.CheckList.Add(new OnProxyHttpFilterCheck(sc.IsRefererAllowed));
                    
                    if (f.DeniedCorsSecFetchMode.Length > 0) 
                        f.CheckList.Add(new OnProxyHttpFilterCheck(sc.IsCorsSecFetchModeAllowed));
                    
                    if (f.DeniedCorsSecFetchSite.Length > 0) 
                        f.CheckList.Add(new OnProxyHttpFilterCheck(sc.IsCorsSecFetchSiteAllowed));
                    
                    if (f.DeniedOrigins.Length > 0) 
                        f.CheckList.Add(new OnProxyHttpFilterCheck(sc.IsOriginAllowed));

                    if (f.DeniedCustom.Length > 0)
                        f.CheckList.Add(new OnProxyHttpFilterCheck(sc.IsCustomAllowed));

                    f.Start(ThreadManager);

                }); 

            });

            OnAcceptConnection = new AsyncCallback(e => {

                if (!ShuttingDown) {
                    try {

                        Socket s = ListenSocket.EndAccept(e);

                        if (s != null)
                            ((OProxyClientHttp)this.Add(new OProxyClientHttp() {
                                ClientSocket    = s,
                                ClientEndPoint  = (IPEndPoint)s.RemoteEndPoint,
                                ClientProtocol  = s.ProtocolType,
                                Destroyer       = new ProxyClientDestroyer(this.Remove),
                                HttpFilters     = Filters,
                                Parent          = this
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
            return JObject.Parse(JsonConvert.SerializeObject(this, new JsonSerializerSettings()
            {
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
