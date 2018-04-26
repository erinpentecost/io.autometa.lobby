using System;

namespace Io.Autometa.Lobby.Contract
{
    /// All messages must implement this interface.
    public interface IMessage
    {
        ValidationCheck Validate();
    }
}