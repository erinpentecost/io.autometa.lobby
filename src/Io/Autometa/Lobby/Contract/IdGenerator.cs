using System;
using System.Collections.Generic;

namespace Io.Autometa.Lobby.Contract
{
    static class IdGenerator
    {
        // Intentionally no ILO0. They can be confused with one-another.
        private static string allowedChars = "QWFPGJUYARSTDHNEZXCVBKM123456789";

        private static Random random = new Random();
        
        private static char GetChar()
        {
            return allowedChars[random.Next(0, allowedChars.Length)];
        }

        public static string GetId()
        {
            // 3355443233554432 possible combinations. I'm not going to worry about
            // hash id collisions.
            char[] id = new char[]
            {
                GetChar(),
                GetChar(),
                GetChar(),
                GetChar(),
                GetChar()
            };

            return new string(id).ToUpperInvariant();
        }
    }
}