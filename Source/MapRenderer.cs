using OpenTK.Graphics.OpenGL;
using System.Numerics;

namespace DoomToFA;

public static class MapRenderer
{
    private static Shader shader;
    private static bool shaderCompiled = false;

    public static void RenderMap(Map map)
    {
        GL.ClearColor(new OpenTK.Mathematics.Color4(0, 0, 255, 255));
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        Camera camera = new Camera(1, 1);
        camera.position.X = 0.0f;
        camera.position.Y = 15.0f;
        camera.orthographicSize = 30f;

        Vector3 min = new(float.MaxValue, float.MaxValue, 0);
        Vector3 max = new(float.MinValue, float.MinValue, 0);
        for (int i = 0; i < map.vertices.Length; i++)
        {
            min.X = MathF.Min(min.X, map.vertices[i].X);
            min.Y = MathF.Min(min.Y, map.vertices[i].Y);

            max.X = MathF.Max(max.X, map.vertices[i].X);
            max.Y = MathF.Max(max.Y, map.vertices[i].Y);
        }
        Vector3 mid = (min + max) / 2.0f;
        camera.position.X = mid.X;
        camera.position.Y = mid.Y;

        Vector3 sub = max - min;
        float newOrthoSize = MathF.Max(sub.X, sub.Y);
        camera.orthographicSize = newOrthoSize + 5.0f;
        camera.RefreshMatrix(1, 1);

        if (!shaderCompiled)
        {
            shader = new Shader(Shader.ParseShader("map", shaderSrc), true);
            shaderCompiled = true;
        }

        List<WireVertex> vertices = new();
        List<int> indices = new();

        for (int i = 0; i < map.vertices.Length; i++)
        {
            vertices.Add(new WireVertex(new Vector3(map.vertices[i].X, map.vertices[i].Y, 0), new Vector4(1, 1, 0, 1)));
        }

        for(int i = 0; i < map.linedefs.Length; i++)
        {
            indices.Add(map.linedefs[i].v1);
            indices.Add(map.linedefs[i].v2);
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices.ToArray(), indices.ToArray());

        shader.Bind();

        shader.SetMatrix("projectionMatrix", camera.ProjectionMatrix);
        shader.SetMatrix("viewMatrix", camera.GetViewMatrix());

        GL.BindVertexArray(mesh.VAO);
        GL.EnableVertexAttribArray(0);
        GL.EnableVertexAttribArray(1);
        GL.PointSize(5.0f);
        GL.DrawElements(PrimitiveType.Lines, mesh.VertexCount, DrawElementsType.UnsignedInt, 0);
        GL.DisableVertexAttribArray(0);
        GL.DisableVertexAttribArray(1);

        shader.Unbind();
        mesh.Cleanup();
    }

    public static void Cleanup()
    {
        shader.Cleanup();
    }

    private static string shaderSrc = "" +
        "#shader vertex" + "\n" +
        "#version 330 core" + "\n" +
        "layout (location=0) in vec3 position;" + "\n" +
        "layout (location=1) in vec4 color;" + "\n" +

        "uniform mat4 projectionMatrix;" + "\n" +
        "uniform mat4 viewMatrix;" + "\n" +

        "void main() {" + "\n" +
        "   gl_Position = projectionMatrix * viewMatrix * vec4(position, 1.0);" + "\n" +
        "}" + "\n" +

        "#shader fragment" + "\n" +
        "#version 330 core" + "\n" +
        "" + "\n" +
        "out vec4 color;" + "\n" +
        "" + "\n" +
        "void main() {" + "\n" +
        "   color = vec4(1, 1, 0, 1);" + "\n" +
        "}" + "\n" +
        "";
}