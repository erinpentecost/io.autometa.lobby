using System;

namespace Io.Autometa.Lobby.Message
{
    /// All messages must implement this interface.
    public interface IMessage
    {
        ValidationCheck Validate();
    }
}