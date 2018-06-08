namespace Io.Autometa.Redis
{
    /// <summary>
    /// This is a list of Redis return types and their associated identifier characters.
    /// </summary>
    internal enum RespType : byte
    {
        SimpleStrings = (byte)'+',
        Errors = (byte)'-',
        Integers = (byte)':',
        BulkStrings = (byte)'$',
        Arrays = (byte)'*'
    }
}