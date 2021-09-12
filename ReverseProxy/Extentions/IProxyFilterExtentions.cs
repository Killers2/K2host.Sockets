/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

using K2host.Core;
using K2host.Threading.Classes;
using K2host.Sockets.ReverseProxy.Interface;
using K2host.Threading.Interface;
using K2host.Threading.Extentions;

namespace K2host.Sockets.ReverseProxy.Extentions
{

    public static class IProxyFilterExtentions
    {

        /// <summary>
        /// Used on the <see cref="IProxyFilter"/> to start the watch process to switch failovers incase of a server outage.
        /// </summary>
        /// <param name="e"></param>
        public static void Start(this IProxyFilter e, IThreadManager threadManager)
        {
            threadManager.Add(
                new OThread(
                    new ParameterizedThreadStart(delegate (object e) {

                        TcpClient       tcli                = null;
                        IProxyFilter    filter              = (IProxyFilter)e;
                        Stopwatch       stopwatch           = new();
                        string          httpQuery           = "GET / HTTP/1.1\r\nHost: {0}\r\n\r\n";
                        int             connectionTimeout   = 5000;
                        int             receiveTimeout      = 0;

                        while (true)
                        {
                            filter.Failovers.ForEach(f => {

                                if (f.MappedHost.ToString() == IPAddress.Any.ToString())
                                    filter.CurrentFailover = f;
                                else
                                {

                                    stopwatch.Start();

                                    tcli = new TcpClient { ReceiveTimeout = 30000 };

                                    try
                                    {
                                        tcli.Connect(f.MappedHost, f.MappedPort);
                                    }
                                    catch (Exception)
                                    {
                                        stopwatch.Reset();
                                        goto Finish;
                                    }

                                    if (tcli.Connected)
                                    {

                                        int length = 1;

                                        NetworkStream ns = tcli.GetStream();
                                        StreamWriter nw = new(ns);

                                        lock (ns)
                                        {
                                            nw.Write(string.Format(httpQuery, filter.UrlHostName));
                                            nw.Flush();
                                        }

                                        while (!ns.DataAvailable)
                                        {

                                            Thread.Sleep(1);
                                            receiveTimeout += 1;

                                            if (receiveTimeout == connectionTimeout)
                                            {
                                                try
                                                {
                                                    tcli.Close();
                                                    tcli = null;
                                                }
                                                catch { }

                                                receiveTimeout = 0;
                                                stopwatch.Reset();

                                                goto Finish;
                                            }
                                        }

                                        Thread.Sleep(150);

                                        byte[] buffer_in = new byte[1024];
                                        string httpresult = string.Empty;

                                        while (length > 0)
                                        {
                                            length = ns.Read(buffer_in, 0, buffer_in.Length);

                                            httpresult += Encoding.ASCII.GetString(buffer_in, 0, length);

                                            if (length <= 0 || length <= buffer_in.Length)
                                                break;
                                        }

                                        buffer_in.Dispose();

                                        ns.Close();
                                        ns.Dispose();
                                        ns = null;

                                        tcli.Close();
                                        tcli = null;

                                        if (httpresult != string.Empty)
                                        {
                                            if (httpresult.StartsWith("HTTP/1.1 200 OK"))
                                            {
                                                if (stopwatch.ElapsedMilliseconds < filter.LatencyTimeOut)
                                                    filter.CurrentFailover = f;
                                                else
                                                {
                                                    stopwatch.Reset();
                                                    goto Finish;
                                                }
                                            }
                                            else
                                            {
                                                stopwatch.Reset();
                                                goto Finish;
                                            }
                                        }
                                        else
                                        {
                                            stopwatch.Reset();
                                            goto Finish;
                                        }
                                    }
                                    else
                                    {
                                        stopwatch.Reset();
                                        goto Finish;
                                    }

                                Finish:

                                    stopwatch.Reset();
                                    Thread.Sleep(10);

                                }

                            });

                            Thread.Sleep(5000);

                        }

                    })
                )
            ).Start(e);
        }
  
    }

}
