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
    public class OProxyFilterSearch : IProxyFilterSearch
    {

        /// <summary>
        /// The search type of this instance
        /// </summary>
        public OProxyFilterSearchType SearchType { get; set; }

        /// <summary>
        /// The operator to which will filter the content
        /// </summary>
        public OProxyFilterSearchOperator Operator { get; set; }

        /// <summary>
        /// The content you are looking for or RegEx to compare
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The content you are looking for or RegEx to compare
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The constuctor used to create an instance from the inherited type.
        /// </summary>
        public OProxyFilterSearch() { }

        /// <summary>
        /// This is used on disposable objects and used generically for remotely disposing a client.
        /// </summary>
        bool IsDisposed = false;

        /// <summary>
        /// Used to dispose the object instance from the abstract class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The method used to dispose the properties in the instance of this class.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {

            if (!IsDisposed)
                if (disposing)
                {


                }

            IsDisposed = true;
        }

    }

}
