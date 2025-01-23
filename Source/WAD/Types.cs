namespace DoomToFA;

public struct Linedef
{
    public short v1;
    public short v2;
    public short flags;
    public short action;
    public short tag;
    public short front;
    public short back;

    public Linedef(short v1, short v2, short flags, short action, short tag, short front, short back)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.flags = flags;
        this.action = action;
        this.tag = tag;
        this.front = front;
        this.back = back;
    }

    public override string ToString()
    {
        return v1 + ":" + v2;
    }
}

public struct Sidedef
{
    public ushort xOffset;
    public ushort yOffset;
    public string upper;
    public string lower;
    public string middle;
    public ushort sector;

    public Sidedef(ushort xOffset, ushort yOffset, string upper, string lower, string middle, ushort sector)
    {
        this.xOffset = xOffset;
        this.yOffset = yOffset;
        this.upper = upper;
        this.lower = lower;
        this.middle = middle;
        this.sector = sector;
    }
}

public struct Seg
{
    public ushort v1;
    public ushort v2;
    public ushort angle;
    public ushort linedef;
    public ushort direction;
    public ushort offset;

    public Seg(ushort v1, ushort v2, ushort angle, ushort linedef, ushort direction, ushort offset)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.angle = angle;
        this.linedef = linedef;
        this.direction = direction;
        this.offset = offset;
    }
}
public struct Sector
{
    public ushort floor;
    public ushort ceil;
    public string floorName;
    public string ceilName;
    public ushort light;
    public ushort type;
    public ushort tag;

    public List<int> vertices;
    public List<int> lines;

    public Sector(ushort floor, ushort ceil, string floorName, string ceilName, ushort light, ushort type, ushort tag)
    {
        this.floor = floor;
        this.ceil = ceil;
        this.floorName = floorName;
        this.ceilName = ceilName;
        this.light = light;
        this.type = type;
        this.tag = tag;
    }
}

public struct SubSector
{
    public ushort segCount;
    public ushort first;

    public SubSector(ushort segCount, ushort first)
    {
        this.segCount = segCount;
        this.first = first;
    }
}

public struct Thing
{
    public float x;
    public float y;
    public short angle;
    public short type;
    public short flags;

    public Thing(float x, float y, short angle, short type, short flags)
    {
        this.x = x;
        this.y = y;
        this.angle = angle;
        this.type = type;
        this.flags = flags;
    }
}