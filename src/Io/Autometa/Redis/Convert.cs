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

        public static bool ParseBoolean(byte[] s) => ParseBoolean(Encoding.UTF8.GetString(s));
        public static bool ParseBoolean(string s)
        {
            bool intermediate;
            if (bool.TryParse(s, out intermediate)) return intermediate;

            if (string.IsNullOrEmpty(s))
            {
                return false;
            }
            if (s == "1" || string.Equals(s, "yes", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "on", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "ok", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (s == "0" || string.Equals(s, "no", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "off", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            throw new ArgumentException("can't parse "+s);
        }
    }
}