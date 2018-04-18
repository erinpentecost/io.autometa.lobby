namespace Io.Autometa.Redis
{
    internal enum RespType : byte
    {
        SimpleStrings = (byte)'+',
        Erorrs = (byte)'-',
        Integers = (byte)':',
        BulkStrings = (byte)'$',
        Arrays = (byte)'*'
    }
}