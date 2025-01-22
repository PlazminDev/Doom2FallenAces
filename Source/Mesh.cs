using OpenTK.Graphics.OpenGL4;

namespace DoomToFA;

public class Mesh
{
    public int VertexCount { get; private set; }

    int vertexBufferHandle;
    int vertexArrayHandle;
    int indexBufferHandle = -1;

    private bool initalized = false;

    public int VAO => vertexArrayHandle;

    public void SetVertices<T>(T[] vertices, int[] indices) where T : struct, IVertex
    {
        SetVertices(vertices, default(T).Info, indices);
    }

    public void SetVertices<T>(T[] vertices, VertexInfo info, int[] indices) where T : struct, IVertex
    {
        if (initalized)
            Cleanup();

        vertexArrayHandle = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayHandle);

        vertexBufferHandle = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * info.SizeInBytes, vertices, BufferUsageHint.StaticDraw);

        foreach(var attr in info.VertexAttributes)
        {
            GL.EnableVertexAttribArray(attr.Index);
            GL.VertexAttribPointer(attr.Index, attr.ComponentCount, VertexAttribPointerType.Float, false, info.SizeInBytes, attr.Offset);
        }

        indexBufferHandle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferHandle);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

        this.VertexCount = indices.Length;

        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        //this.vertices = Array.ConvertAll(vertices, item => (IVertex)item);
        //this.indices = indices;

        initalized = true;
    }

    public void Cleanup()
    {
        GL.DeleteBuffer(indexBufferHandle);
        GL.DeleteBuffer(vertexBufferHandle);
        GL.DeleteVertexArray(vertexArrayHandle);
    }
}
