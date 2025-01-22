namespace DoomToFA;

public class ShaderProgramSrc
{
    public string Name;
    public string VertexShaderSrc;
    public string FragmentShaderSrc;

    public ShaderProgramSrc(string name, string vertexShaderSrc, string fragmentShaderSrc)
    {
        Name = name;
        VertexShaderSrc = vertexShaderSrc;
        FragmentShaderSrc = fragmentShaderSrc;
    }
}
