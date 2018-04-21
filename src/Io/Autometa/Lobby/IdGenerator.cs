using System;
using System.Collections.Generic;
using Io.Autometa.Lobby.Message;

namespace Io.Autometa.Lobby
{
    public class IdGenerator
    {
        private Random random = new Random();

        // Intentionally no IO0
        private static string allowedChars = "QWFPGJLUYARSTDHNEZXCVBKM123456789";
        
        private char GetChar()
        {
            return allowedChars[this.random.Next(0, allowedChars.Length)];
        }

        public string GetId()
        {
            char[] id = new char[]
            {
                this.GetChar(),
                this.GetChar(),
                this.GetChar(),
                this.GetChar(),
                this.GetChar(),
                this.GetChar()
            };

            return new string(id).ToUpperInvariant();
        }
    }
}