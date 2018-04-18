namespace Io.Autometa.Redis
{
    internal enum RespType : byte
    {
        SimpleStrings = (byte)'+',
        Errors = (byte)'-',
        Integers = (byte)':',
        BulkStrings = (byte)'$',
        Arrays = (byte)'*'
    }
}