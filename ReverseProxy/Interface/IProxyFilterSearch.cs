/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;

using K2host.Sockets.Raw.Enums;

namespace K2host.Sockets.ReverseProxy.Interface
{
    
    /// <summary>
    /// This is used to help create the object class you define.
    /// </summary>
    public interface IProxyFilterSearch : IDisposable
    {

        /// <summary>
        /// The search type of this instance
        /// </summary>
        OProxyFilterSearchType SearchType { get; set; }

        /// <summary>
        /// The operator to which will filter the content
        /// </summary>
        OProxyFilterSearchOperator Operator { get; set; }
        
        /// <summary>
        /// The content you are looking for or RegEx to compare
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// The content you are looking for or RegEx to compare
        /// </summary>
        string Value { get; set; }

    }

}
