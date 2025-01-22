using System.Text;

public static class Utils
{
    public static string GetTerminatedString(byte[] data)
    {
        int i = 0;
        for (i = 0; i < data.Length && data[i] != 0; i++) { }

        return Encoding.ASCII.GetString(data[0..i]);
    }

    public static string ReadTerminatedString(BinaryReader br, int len)
    {
        byte[] chrs = br.ReadBytes(len);

        int i = 0;
        for (i = 0; i < chrs.Length && chrs[i] != 0; i++) { }

        return Encoding.ASCII.GetString(chrs[0..i]);
    }

    public static int ToInt32(byte[] buffer, int offset)
    {
        return buffer[offset + 3] << 24 | buffer[offset + 2] << 16 | buffer[offset + 1] << 8 | buffer[offset];
    }

    public static int ToInt16(byte[] buffer, int offset)
    {
        return buffer[offset + 2] << 16 | buffer[offset + 1] << 8 | buffer[offset];
    }
}