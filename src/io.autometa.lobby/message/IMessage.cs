using System;

namespace io.autometa.lobby.message
{
    /// All messages must implement this interface.
    public interface IMessage
    {
        ValidationCheck Validate();
    }
}