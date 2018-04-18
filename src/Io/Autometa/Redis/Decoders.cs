using System;
using System.Globalization;
using System.Net;
using System.Text;

namespace Io.Autometa.Redis
{
    internal static class Decoders
    {
        public static int ParseInt32(byte[] s) => ParseInt32(Encoding.UTF8.GetString(s));
        public static int ParseInt32(string s) => int.Parse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);

        public static long ParseInt64(byte[] s) => ParseInt64(Encoding.UTF8.GetString(s));
        public static long ParseInt64(string s) => long.Parse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);

        public static Boolean ParseBoolean(byte[] s) => ParseBoolean(Encoding.UTF8.GetString(s));
        public static Boolean ParseBoolean(string s)
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

        public static Double ParseDouble(byte[] s) => ParseDouble(Encoding.UTF8.GetString(s));
        public static Double ParseDouble(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentException("can't parse "+s);
            }
            if (s.Length == 1 && s[0] >= '0' && s[0] <= '9')
            {
                return (int)(s[0] - '0');
            }
            // need to handle these
            if (string.Equals("+inf", s, StringComparison.OrdinalIgnoreCase) || string.Equals("inf", s, StringComparison.OrdinalIgnoreCase))
            {
                return double.PositiveInfinity;
            }
            if (string.Equals("-inf", s, StringComparison.OrdinalIgnoreCase))
            {
                return double.NegativeInfinity;
            }
            Double intermediate;
            if (double.TryParse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out intermediate)) return intermediate;
            throw new ArgumentException("can't parse "+s);
        }
    }
}