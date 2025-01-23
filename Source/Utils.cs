using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace DoomToFA;

public static class Utils
{
    public static float DOOM2ACE_SCALAR = 0.3f;

    public static Vector2 CoordinateConversion(short x, short y)
    {
        return new Vector2(-x, y) * DOOM2ACE_SCALAR;
    }

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

    public static Vector3 GetVector(string data)
    {
        data = data.Remove(0, 1);
        data = data.Remove(data.Length - 1, 1);
        data = new Regex(@"\s+").Replace(data, "");

        string[] split = data.Split(",");

        if (split.Length != 3) return Vector3.Zero;

        Vector3 v = new();
        for (int i = 0; i < split.Length; i++)
            v[i] = float.Parse(split[i]);
        return v;
    }
}