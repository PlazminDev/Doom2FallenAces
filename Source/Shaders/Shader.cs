using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Numerics;
using System.Reflection;
using GLShaderType = OpenTK.Graphics.OpenGL.ShaderType;

namespace DoomToFA;

public class Shader
{
    public int ProgramID { get; private set; }

    public string FileName => _shaderProgramSrc.Name;

    private ShaderProgramSrc _shaderProgramSrc { get; }

    public bool Compiled { get; private set; }

    private Dictionary<string, int> uniforms = new();

    public Shader(ShaderProgramSrc shaderProgramSource, bool compile)
    {
        _shaderProgramSrc = shaderProgramSource;
        if (compile)
        {
            CompileShader();
        }
    }

    public bool CompileShader()
    {
        if (_shaderProgramSrc == null)
        {
            Console.Error.WriteLine("Shader.cs: Shader Program Source is Null!");
            return false;
        }
        if (Compiled)
        {
            Console.Error.WriteLine("Shader.cs: Shader is already compiled!");
            return false;
        }

        int vertexShaderID = GL.CreateShader(GLShaderType.VertexShader);
        GL.ShaderSource(vertexShaderID, _shaderProgramSrc.VertexShaderSrc);
        GL.CompileShader(vertexShaderID);
        GL.GetShader(vertexShaderID, ShaderParameter.CompileStatus, out var vertexShaderCompilationCode);
        if (vertexShaderCompilationCode != (int)All.True)
        {
            Console.Error.WriteLine(GL.GetShaderInfoLog(vertexShaderID));
            return false;
        }

        int fragmentShaderID = GL.CreateShader(GLShaderType.FragmentShader);
        GL.ShaderSource(fragmentShaderID, _shaderProgramSrc.FragmentShaderSrc);
        GL.CompileShader(fragmentShaderID);
        GL.GetShader(fragmentShaderID, ShaderParameter.CompileStatus, out var fragmentShaderCompilationCode);
        if (fragmentShaderCompilationCode != (int)All.True)
        {
            Console.Error.WriteLine(GL.GetShaderInfoLog(fragmentShaderID));
            return false;
        }

        ProgramID = GL.CreateProgram();
        GL.AttachShader(ProgramID, vertexShaderID);
        GL.AttachShader(ProgramID, fragmentShaderID);
        GL.LinkProgram(ProgramID);

        GL.DetachShader(ProgramID, vertexShaderID);
        GL.DetachShader(ProgramID, fragmentShaderID);

        GL.DeleteShader(vertexShaderID);
        GL.DeleteShader(fragmentShaderID);

        // Load uniforms
        GL.GetProgram(ProgramID, GetProgramParameterName.ActiveUniforms, out int count);
        //Console.WriteLine("Uniforms Count: " + count);

        for (int i = 0; i < count; i++)
        {
            GL.GetActiveUniform(ProgramID, i, 32, out int length, out int size, out ActiveUniformType type, out string name);
        }

        Compiled = true;
        return true;
    }

    private void CreateUniform(string name)
    {
        int uniformLocation = GL.GetUniformLocation(ProgramID, name);
        if (uniformLocation < 0)
        {
            // this should never happen
            Console.Error.WriteLine("Shader.cs: Uniform of name " + name + " not found.");
            return;
        }
        uniforms.Add(name, uniformLocation);
    }

    public void SetInt(string uniformName, int value)
    {
        GL.Uniform1(GL.GetUniformLocation(ProgramID, uniformName), value);
    }

    public void SetFloat(string uniformName, float value)
    {
        GL.Uniform1(GL.GetUniformLocation(ProgramID, uniformName), value);
    }

    public void SetVector2(string uniformName, System.Numerics.Vector2 value)
    {
        GL.Uniform2(GL.GetUniformLocation(ProgramID, uniformName), value.X, value.Y);
    }

    public void SetVector3(string uniformName, System.Numerics.Vector3 value)
    {
        GL.Uniform3(GL.GetUniformLocation(ProgramID, uniformName), value.X, value.Y, value.Z);
    }

    public void SetVector3(string uniformName, OpenTK.Mathematics.Vector3 value)
    {
        GL.Uniform3(GL.GetUniformLocation(ProgramID, uniformName), value);
    }

    public void SetVector4(string uniformName, System.Numerics.Vector4 value)
    {
        GL.Uniform4(GL.GetUniformLocation(ProgramID, uniformName), value.X, value.Y, value.Z, value.W);
    }

    public void SetMatrix(string uniformName, Matrix4x4 value)
    {
        OpenTK.Mathematics.Matrix4 mat = new OpenTK.Mathematics.Matrix4();

        mat.M11 = value.M11;
        mat.M12 = value.M12;
        mat.M13 = value.M13;
        mat.M14 = value.M14;
        mat.M21 = value.M21;
        mat.M22 = value.M22;
        mat.M23 = value.M23;
        mat.M24 = value.M24;
        mat.M31 = value.M31;
        mat.M32 = value.M32;
        mat.M33 = value.M33;
        mat.M34 = value.M34;
        mat.M41 = value.M41;
        mat.M42 = value.M42;
        mat.M43 = value.M43;
        mat.M44 = value.M44;

        GL.UniformMatrix4(GL.GetUniformLocation(ProgramID, uniformName), false, ref mat);
    }

    public void SetMatrix(string uniformName, Matrix4 value)
    {
        GL.UniformMatrix4(GL.GetUniformLocation(ProgramID, uniformName), false, ref value);
    }

    // i don't like this one bit

    Matrix4[] NumericsToTK(Matrix4x4[] numerics)
    {
        Matrix4[] values = new Matrix4[numerics.Length];
        for (int i = 0; i < numerics.Length; i++)
        {
            values[i].M11 = numerics[i].M11;
            values[i].M12 = numerics[i].M12;
            values[i].M13 = numerics[i].M13;
            values[i].M14 = numerics[i].M14;
            values[i].M21 = numerics[i].M21;
            values[i].M22 = numerics[i].M22;
            values[i].M23 = numerics[i].M23;
            values[i].M24 = numerics[i].M24;
            values[i].M31 = numerics[i].M31;
            values[i].M32 = numerics[i].M32;
            values[i].M33 = numerics[i].M33;
            values[i].M34 = numerics[i].M34;
            values[i].M41 = numerics[i].M41;
            values[i].M42 = numerics[i].M42;
            values[i].M43 = numerics[i].M43;
            values[i].M44 = numerics[i].M44;
        }
        return values;
    }

    public void SetMatrixArray(string uniformName, Matrix4[] matrices)
    {
        GL.UniformMatrix4(GL.GetUniformLocation(ProgramID, uniformName), matrices.Length, false, ref matrices[0].Row0.X);
    }

    public void SetMatrixArray(string uniformName, Matrix4x4[] values)
    {
        Matrix4[] matrices = NumericsToTK(values);
        unsafe
        {
            fixed (float* matrix_ptr = &matrices[0].Row0.X)
            {
                GL.UniformMatrix4(GL.GetUniformLocation(ProgramID, uniformName), matrices.Length, false, matrix_ptr);
            }
        }
    }

    public void Bind()
    {
        if (Compiled)
        {
            GL.UseProgram(ProgramID);
        }
        else
        {
            Console.WriteLine("Shader.cs: Shader has not been compiled!");
        }
    }
    public void Unbind()
    {
        if (Compiled)
        {
            GL.UseProgram(0);
        }
        else
        {
            Console.WriteLine("Shader.cs: Shader has not been compiled!");
        }
    }

    public static ShaderProgramSrc ParseShader(string name, string src)
    {
        string[] shaderSource = new string[2];
        ShaderType shaderType = ShaderType.NONE;
        var allLines = src.Split("\n");
        for (int i = 0; i < allLines.Length; i++)
        {
            string current = allLines[i];
            if (current.ToLower().Contains("#shader"))
            {
                if (current.ToLower().Contains("vertex"))
                {
                    shaderType = ShaderType.VERTEX;
                }
                else if (current.ToLower().Contains("fragment"))
                {
                    shaderType = ShaderType.FRAGMENT;
                }
                else
                {
                    Console.Error.WriteLine("Shader.cs: No shader type identified in " + name + " at line " + i + "!");
                }
            }
            else
            {
                shaderSource[(int)shaderType] += current + Environment.NewLine;
            }
        }

        //Console.Write(shaderSource[0] + "\n");
        //Console.Write(shaderSource[1] + "\n");

        return new ShaderProgramSrc(name, shaderSource[(int)ShaderType.VERTEX], shaderSource[(int)ShaderType.FRAGMENT]);
    }

    /*
    public static ShaderProgramSrc ParseShader(string filePath)
    {
        string[] shaderSource = new string[2];
        ShaderType shaderType = ShaderType.NONE;
        var allLines = FileTools.ReadResource(filePath).Split("\n");
        for (int i = 0; i < allLines.Length; i++)
        {
            string current = allLines[i];
            if (current.ToLower().Contains("#shader"))
            {
                if (current.ToLower().Contains("vertex"))
                {
                    shaderType = ShaderType.VERTEX;
                }
                else if (current.ToLower().Contains("fragment"))
                {
                    shaderType = ShaderType.FRAGMENT;
                }
                else
                {
                    Console.Error.WriteLine("Shader.cs: No shader type identified in " + filePath + " at line " + i + "!");
                }
            }
            else
            {
                shaderSource[(int)shaderType] += current + Environment.NewLine;
            }
        }

        //Console.Write(shaderSource[0] + "\n");
        //Console.Write(shaderSource[1] + "\n");

        return new ShaderProgramSrc(filePath, shaderSource[(int)ShaderType.VERTEX], shaderSource[(int)ShaderType.FRAGMENT]);
    }
    */

    public void Cleanup()
    {
        Unbind();
        GL.DeleteProgram(ProgramID);
    }
}
