namespace DoomToFA;

public readonly struct VertexAttribute
{
    public readonly string Name;
    public readonly int Index;
    public readonly int ComponentCount;
    public readonly int Offset;

    public VertexAttribute(string name, int index, int componentCount, int offset)
    {
        this.Name = name;
        this.Index = index;
        this.ComponentCount = componentCount;
        this.Offset = offset;
    }
}

public class VertexInfo
{
    public readonly Type Type;
    public readonly int SizeInBytes;
    public readonly VertexAttribute[] VertexAttributes;

    public VertexInfo(Type type, params VertexAttribute[] vertexAttributes)
    {
        this.Type = type;
        this.SizeInBytes = 0;
        this.VertexAttributes = vertexAttributes;

        for(int i = 0; i < this.VertexAttributes.Length; i++)
        {
            VertexAttribute attribute = this.VertexAttributes[i];
            this.SizeInBytes += attribute.ComponentCount * sizeof(float);
        }
    }
}