using System.Numerics;

namespace DoomToFA;

// https://github.com/jmickle66666666/wad-js/blob/develop/src/wad/mapdata.js#L59
public class Map
{
    private static readonly string[] MAPLUMPS = ["THINGS","LINEDEFS","SIDEDEFS","VERTEXES","SEGS","TEXTMAP",
                "SSECTORS","NODES","SECTORS","REJECT","BLOCKMAP","BEHAVIOR","ZNODES"];

    private static readonly int[] LUMPLENGTHS = [10, 14, 30, 4, 12, -1, 4, -1, 26];

    public string name;

    public Vector2[] vertices;
    public Linedef[] linedefs;
    public Sidedef[] sidedefs;
    public Seg[] segs;
    public Sector[] sectors;
    public SubSector[] ssectors;
    public Thing[] things;

    public Map(string name, int lumpIndex, Lump[] lumps)
    {
        this.name = name;

        List<string> lumpNames = new();
        int pos = 1;
        string nextLump = lumps[lumpIndex + pos].Name;
        while (Array.IndexOf(MAPLUMPS, nextLump) > -1)
        {
            lumpNames.Add(nextLump);
            pos++;
            if (lumpIndex + pos == lumps.Length) break;
            nextLump = lumps[lumpIndex + pos].Name;
        }
        ParseThings(GetMapLump(MAPLUMPS[0], lumpIndex, lumpNames, lumps));
        ParseSegs(GetMapLump(MAPLUMPS[4], lumpIndex, lumpNames, lumps));
        ParseSidedefs(GetMapLump(MAPLUMPS[2], lumpIndex, lumpNames, lumps));
        ParseSSectors(GetMapLump(MAPLUMPS[6], lumpIndex, lumpNames, lumps));
        ParseSectors(GetMapLump(MAPLUMPS[8], lumpIndex, lumpNames, lumps));
        ParseVertices(GetMapLump(MAPLUMPS[3], lumpIndex, lumpNames, lumps));
        ParseLinedefs(GetMapLump(MAPLUMPS[1], lumpIndex, lumpNames, lumps));
    }

    private void ParseVertices(Lump lump)
    {
        using(MemoryStream stream = new MemoryStream(lump.Data))
        {
            using (BinaryReader br = new BinaryReader(stream))
            {
                int numVertices = lump.Data.Length / LUMPLENGTHS[3];
                vertices = new Vector2[numVertices];
                for (int i = 0; i < numVertices; i++)
                {
                    short x = br.ReadInt16();
                    short y = br.ReadInt16();

                    vertices[i] = Utils.CoordinateConversion(x, y);
                }
            }
        }
    }

    private void ParseLinedefs(Lump lump)
    {
        using (MemoryStream stream = new MemoryStream(lump.Data))
        {
            using (BinaryReader br = new BinaryReader(stream))
            {
                int numLinedefs = lump.Data.Length / LUMPLENGTHS[1];
                linedefs = new Linedef[numLinedefs];
                for (int i = 0; i < numLinedefs; i++)
                {
                    linedefs[i] = new Linedef(
                        br.ReadInt16(),
                        br.ReadInt16(),
                        br.ReadInt16(),
                        br.ReadInt16(),
                        br.ReadInt16(),
                        br.ReadInt16(),
                        br.ReadInt16()
                    );
                }
            }
        }
    }

    private void ParseSidedefs(Lump lump)
    {
        using (MemoryStream stream = new MemoryStream(lump.Data))
        {
            using (BinaryReader br = new BinaryReader(stream))
            {
                int num = lump.Data.Length / LUMPLENGTHS[2];
                sidedefs = new Sidedef[num];
                for (int i = 0; i < num; i++)
                {
                    sidedefs[i] = new Sidedef(
                        br.ReadUInt16(),
                        br.ReadUInt16(),
                        Utils.ReadTerminatedString(br, 8),
                        Utils.ReadTerminatedString(br, 8),
                        Utils.ReadTerminatedString(br, 8),
                        br.ReadUInt16()
                    );
                }
            }
        }
    }

    private void ParseSegs(Lump lump)
    {
        using (MemoryStream stream = new MemoryStream(lump.Data))
        {
            using (BinaryReader br = new BinaryReader(stream))
            {
                int numSegs = lump.Data.Length / LUMPLENGTHS[4];
                segs = new Seg[numSegs];
                for (int i = 0; i < numSegs; i++)
                {
                    segs[i] = new Seg(
                        br.ReadUInt16(),
                        br.ReadUInt16(),
                        br.ReadUInt16(),
                        br.ReadUInt16(),
                        br.ReadUInt16(),
                        br.ReadUInt16()
                    );
                }
            }
        }
    }

    private void ParseSectors(Lump lump)
    {
        using (MemoryStream stream = new MemoryStream(lump.Data))
        {
            using (BinaryReader br = new BinaryReader(stream))
            {
                int num = lump.Data.Length / LUMPLENGTHS[8];
                sectors = new Sector[num];
                for (int i = 0; i < num; i++)
                {
                    sectors[i] = new Sector(
                        br.ReadUInt16(),
                        br.ReadUInt16(),
                        Utils.ReadTerminatedString(br, 8),
                        Utils.ReadTerminatedString(br, 8),
                        br.ReadUInt16(),
                        br.ReadUInt16(),
                        br.ReadUInt16()
                    );
                }
            }
        }
    }

    private void ParseSSectors(Lump lump)
    {
        using (MemoryStream stream = new MemoryStream(lump.Data))
        {
            using (BinaryReader br = new BinaryReader(stream))
            {
                int num = lump.Data.Length / LUMPLENGTHS[6];
                ssectors = new SubSector[num];
                for (int i = 0; i < num; i++)
                {
                    ssectors[i] = new SubSector(
                        br.ReadUInt16(),
                        br.ReadUInt16()
                    );
                }
            }
        }
    }

    private void ParseThings(Lump lump)
    {
        using (MemoryStream stream = new MemoryStream(lump.Data))
        {
            using (BinaryReader br = new BinaryReader(stream))
            {
                int num = lump.Data.Length / LUMPLENGTHS[0];
                things = new Thing[num];
                for (int i = 0; i < num; i++)
                {
                    short x = br.ReadInt16();
                    short y = br.ReadInt16();

                    Vector2 pos = Utils.CoordinateConversion(x, y);

                    things[i] = new Thing(
                        pos.X,
                        pos.Y,
                        br.ReadInt16(),
                        br.ReadInt16(),
                        br.ReadInt16()
                    );
                }
            }
        }
    }

    private Lump GetMapLump(string lumpName, int lumpIndex, List<string> lumpNames, Lump[] lumps)
    {
        return lumps[lumpIndex + lumpNames.IndexOf(lumpName) + 1];
    }
}