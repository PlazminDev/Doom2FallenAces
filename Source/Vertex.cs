using System.Numerics;

namespace DoomToFA;

public readonly struct WireVertex : IVertex
{
    public readonly Vector3 Position;
    public readonly Vector4 Color;
    public VertexInfo Info { get => VertexInfo; }

    private static VertexInfo VertexInfo = new VertexInfo(typeof(WireVertex),
        new VertexAttribute("Position",     0, 3, 0),
        new VertexAttribute("Color",       1, 4, 3 * sizeof(float))
    );

    public WireVertex(Vector3 position, Vector4 color)
    {
        this.Position = position;
        this.Color = color;
    }
}