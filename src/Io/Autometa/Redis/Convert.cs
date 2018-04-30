using System;
using System.Globalization;
using System.Net;
using System.Text;

namespace Io.Autometa.Redis
{
    public static class Convert
    {
        public static long ParseInt64(byte[] s) => ParseInt64(Encoding.UTF8.GetString(s));
        public static long ParseInt64(string s) => long.Parse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
    }
}