using System;

namespace Io.Autometa.LobbyContract
{
    /// All messages must implement this interface.
    public interface IMessage
    {
        ValidationCheck Validate();
    }
}