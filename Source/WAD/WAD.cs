using System.Diagnostics;
using System.Text;

namespace DoomToFA;

public struct Lump
{
    public string Name { get; private set; }
    public byte[] Data { get; private set; }

    public Lump(string name, byte[] data)
    {
        this.Name = name;
        this.Data = data;
    }
}

public class WAD
{
    private static readonly int IWAD = 'I' | ('W' << 8) | ('A' << 16) | ('D' << 24);
    private static readonly int PWAD = 'P' | ('W' << 8) | ('A' << 16) | ('D' << 24);

    public bool valid = true;

    private int lumpCount;
    private int directoryStart;

    public Lump[] Lumps { get; private set; }

    public WAD() { }

    public WAD(string path)
    {
        Load(path);
    }

    public void Load(string path)
    {
        using(Stream stream = File.OpenRead(path))
        {
            using(BinaryReader br = new BinaryReader(stream))
            {
                var stopwatch = Stopwatch.StartNew();

                ReadHeader(br);
                ReadDirectory(br);

                Console.WriteLine($"Finished loading WAD in {stopwatch.Elapsed.TotalMilliseconds}ms");
            }
        }
    }

    private void ReadHeader(BinaryReader br)
    {
        int magic = br.ReadInt32();
        if(magic == IWAD || magic == PWAD)
        {
            lumpCount = br.ReadInt32();
            directoryStart = br.ReadInt32();
            //Console.WriteLine($"Lump Count: {lumpCount}\nDirectory Start: {directoryStart}");
        }
        else
        {
            Console.WriteLine("File provided is not a valid WAD.");
            valid = false;
        }
    }

    private void ReadDirectory(BinaryReader br)
    {
        if (!valid) return;

        Lumps = new Lump[lumpCount];
        br.BaseStream.Seek(directoryStart, SeekOrigin.Begin);

        for(int i = 0; i < lumpCount; i++)
        {
            int offset = br.ReadInt32();
            int size = br.ReadInt32();
            string name = Utils.ReadTerminatedString(br, 8);

            //Console.WriteLine($"{name}: offset:{offset}, size:{size}");

            long currentPosition = br.BaseStream.Position;

            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            byte[] data = br.ReadBytes(size);

            br.BaseStream.Seek(currentPosition, SeekOrigin.Begin);

            Lumps[i] = new Lump(name, data);
        }
    }
}