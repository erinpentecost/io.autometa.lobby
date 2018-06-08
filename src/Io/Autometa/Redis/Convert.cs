using System;
using System.Globalization;
using System.Net;
using System.Text;

namespace Io.Autometa.Redis
{
    /// <summary>
    /// Holds some utility methods for converting strings and byte arrays into longs.
    /// </summary>
    public static class Convert
    {
        public static long ParseInt64(byte[] s) => ParseInt64(Encoding.UTF8.GetString(s));
        public static long ParseInt64(string s) => long.Parse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
    }
}