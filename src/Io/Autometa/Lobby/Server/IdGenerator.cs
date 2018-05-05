using System;
using System.Collections.Generic;

namespace Io.Autometa.Lobby.Server
{
    static class IdGenerator
    {
        // Intentionally no ILO0. They can be confused with one-another.
        // Intentionally no S either, since that is used for the secret prefix
        private static string allowedChars = "QWFPGJUYARTDHNEZXCVBKM123456789";

        private static Random random = new Random();
        
        private static char GetChar()
        {
            return allowedChars[random.Next(0, allowedChars.Length)];
        }

        public static string GetId(bool hidden)
        {
            char[] id = new char[]
            {
                hidden ? RedisLobby.SecretPrefix : GetChar(),
                GetChar(),
                GetChar(),
                GetChar(),
                GetChar()
            };

            return new string(id).ToUpperInvariant();
        }
    }
}