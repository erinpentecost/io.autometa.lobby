using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Io.Autometa.Redis
{
    /// <summary>
    /// Utility methods to treat a stream as if it was a UTF8 string stream.
    /// </summary>
    internal static class StreamExtensions
    {
        private static byte[] endLine = Encoding.UTF8.GetBytes("\r\n");

        public static void WriteString(this Stream ms, string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            ms.Write(data, 0, data.Length);
        }

        public static void WriteLine(this Stream ms, string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            ms.Write(data, 0, data.Length);
            ms.Write(endLine, 0, endLine.Length);
        }

        public static string ReadFirstLine(this Stream ms)
        {
            var sb = new StringBuilder();

            int current;
            var prev = default(char);
            while ((current = ms.ReadByte()) != -1)
            {
                var c = (char)current;
                if (prev == '\r' && c == '\n') // reach at TerminateLine
                {
                    break;
                }
                else if (prev == '\r' && c == '\r')
                {
                    sb.Append(prev); // append prev '\r'
                    continue;
                }
                else if (c == '\r')
                {
                    prev = c; // not append '\r'
                    continue;
                }

                prev = c;
                sb.Append(c);
            }

            return sb.ToString();
        }

    }
}