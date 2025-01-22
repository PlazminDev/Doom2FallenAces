using OpenTK.Mathematics;

namespace DoomToFA;

public class Camera
{
    public Vector2 position;
    public float orthographicSize = 20.0f;

    private int WindowWidth, WindowHeight;

    public Matrix4 ProjectionMatrix { get; private set; }
    public Matrix4 ViewMatrix { get; private set; }
    public Matrix4 InverseProjection { get; private set; }
    public Matrix4 InverseView { get; private set; }

    public Camera(int Width, int Height)
    {
        this.position = new Vector2();
        this.ProjectionMatrix = new Matrix4();
        this.ViewMatrix = new Matrix4();
        this.InverseProjection = new Matrix4();
        this.InverseView = new Matrix4();

        RefreshMatrix(Width, Height);
    }

    public void RefreshMatrix(int Width, int Height)
    {
        ProjectionMatrix = Matrix4.Identity;
        ProjectionMatrix = GodotOrtho(orthographicSize, (float)Width / (float)Height, 0.01f, 100.0f);
        InverseProjection = Matrix4.Invert(ProjectionMatrix);

        this.WindowWidth = Width; this.WindowHeight = Height;
    }

    private Matrix4 GodotOrtho(float size, float aspect, float near, float far)
    {
        return GodotOrtho(-size / 2.0f, size / 2.0f, -size / aspect / 2.0f, size / aspect / 2.0f, near, far);
    }

    private Matrix4 GodotOrtho(float left, float right, float bottom, float top, float near, float far)
    {
        Matrix4 proj = Matrix4.Identity;

        proj.M11 = 2.0f / (right - left);
        proj.M41 = -((right + left) / (right - left));
        proj.M22 = 2.0f / (top - bottom);
        proj.M42 = -((top + bottom) / (top - bottom));
        proj.M33 = -2.0f / (far - near);
        proj.M43 = -((far + near) / (far - near));
        proj.M44 = 1.0f;

        return proj;
    }

    public Matrix4 GetViewMatrix()
    {
        Vector3 cameraFwd = -Vector3.UnitZ;
        Vector3 cameraUp = Vector3.UnitY;
        this.ViewMatrix = Matrix4.Identity;
        this.ViewMatrix = Matrix4.LookAt(new Vector3(position.X, position.Y, 10.0f), cameraFwd + (position.X, position.Y, 0.0f), cameraUp);
        this.InverseView = Matrix4.Invert(InverseProjection);

        return this.ViewMatrix;
    }

    public Vector2 GetRelativeMousePos(Vector2 mousePos)
    {
        float x = mousePos.X;
        float y = mousePos.Y;

        x = (x / (float)WindowWidth) * 2.0f - 1.0f;
        y = (y / (float)WindowHeight) * 2.0f - 1.0f;
        Vector4 tmp = new Vector4(x, y, 0, 1);
        tmp = (tmp * InverseProjection);
        x = tmp.X + position.X;
        y = -tmp.Y + position.Y;

        return new Vector2(x, y);
    }
}