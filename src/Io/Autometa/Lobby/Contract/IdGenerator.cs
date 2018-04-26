using System;
using System.Collections.Generic;

namespace Io.Autometa.Lobby.Contract
{
    static class IdGenerator
    {
        // Intentionally no IO0
        private static string allowedChars = "QWFPGJLUYARSTDHNEZXCVBKM123456789";

        private static Random random = new Random();
        
        private static char GetChar()
        {
            return allowedChars[random.Next(0, allowedChars.Length)];
        }

        public static string GetId()
        {
            
            char[] id = new char[]
            {
                GetChar(),
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