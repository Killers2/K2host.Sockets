/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-12-05                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.IO;
using System.Text;
using System.Linq;

using K2host.Core;
using K2host.Sockets.ReverseProxy.Interface;

namespace K2host.Sockets.ReverseProxy.Extentions
{

    public static class IProxyHttpRequestBoundrayExtentions
    {

        /// <summary>
        /// This parses the data in to the objects property values.
        /// Perforance: parsing the data. needs work.
        /// </summary>
        /// <param name="segment"></param>
        public static IProxyHttpRequestBoundray Build(this IProxyHttpRequestBoundray e, byte[] segment)
        {

            byte[] boundaryBytesMarker  = Encoding.UTF8.GetBytes("\r\n" + "--" + e.BoundrayId + "\r\n");
            byte[] markerBreak          = Encoding.UTF8.GetBytes("\r\n");
            byte[] markerDoubleBreak    = Encoding.UTF8.GetBytes("\r\n\r\n");
                
            segment                     = segment.Skip(boundaryBytesMarker.Length - 1).ToArray();
            byte[] lineOne              = segment.Take(segment.WhereIsFirstIndex(markerBreak)).ToArray();
                
            segment                     = segment.Skip(lineOne.Length).ToArray();
            byte[] lineTwo              = segment.Take(segment.WhereIsFirstIndex(markerDoubleBreak)).ToArray();
                
            segment                     = segment.Skip(lineTwo.Length).ToArray();
            byte[] lineTree             = segment.Skip(markerDoubleBreak.Length).ToArray();

            //Lets Parse the data.
            string      line    = Encoding.UTF8.GetString(lineOne);
            string[]    parts   = line.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            string      key     = parts[1].Remove(0, parts[1].IndexOf("\"") + "\"".Length);

            e.ContentDisposition  = parts[0].Remove(0, parts[0].IndexOf(": ") + 2);
            e.Name                = key.Substring(0, key.IndexOf("\""));

            if (line.ToLower().Contains("filename"))
            {
                key         = parts[2].Remove(0, parts[2].IndexOf("\"") + "\"".Length);
                e.Filename  = key.Substring(0, key.IndexOf("\""));
            }

            line            = Encoding.UTF8.GetString(lineTwo);
            e.ContentType   = line.Remove(0, line.IndexOf(": ") + 2);

            e.Stream = new MemoryStream(lineTree);
            e.ContentLength64 = e.Stream.Length;
                
            if (string.IsNullOrEmpty(e.Filename))
                e.Data = line;

            return e;

        }

    }

}
