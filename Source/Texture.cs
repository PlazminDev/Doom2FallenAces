using OpenTK.Graphics.OpenGL;

namespace DoomToFA;

public class Texture
{
    public int TextureID { get; private set; }

    public int Width { get; private set; }
    public int Height { get; private set; }

    public string Path { get; private set; }

    public Texture(int width, int height, byte[] data, string name = "generated")
    {
        this.Path = name;
        TextureID = GL.GenTexture();

        Width = width;
        Height = height;

        Bind();

        GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)TextureMagFilter.Nearest });
        GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)TextureMinFilter.Nearest });
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D); // for some reason this line is crucial to the texture loading

        Unbind();

        //GL.TextureStorage2D(TextureID, 0, SizedInternalFormat.Rgba8, width, height);
        //GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext, width, height, 0, data.Length, data);
    }

    public Texture SetFilterMode(TextureMinFilter minFilter, TextureMagFilter magFilter)
    {
        Bind();

        GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)magFilter });
        GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)minFilter });

        Unbind();

        return this;
    }

    public Texture SetWrapMode(TextureWrapMode wrapMode)
    {
        Bind();

        GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, new int[] { (int)wrapMode });
        GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, new int[] { (int)wrapMode });

        Unbind();

        return this;
    }

    public static Texture GenMissingTexture()
    {
        //                     width    height    color channels
        //                     128 *    128 *     4
        byte[] data = new byte[65536];
        bool swap = false;  
        int counter = 0;
        int yCounter = 0;
        for(int i = 0; i < data.Length; i+=4)
        {
            int x = i % 128;
            int y = i / 128;

            if (counter % 16 == 0) swap = !swap;
                counter++;

            if (yCounter > 128 * 16) { swap = !swap; yCounter = 0; }
                yCounter++;

            // Set Alpha channel to 255 and green channel to 0
            data[i + 1] = 0x00;
            data[i + 3] = 0xFF;

            if (swap) // Purple
            {
                data[i + 0] = 0xFF;
                data[i + 2] = 0xFF;
            }
            else // Black
            {
                data[i + 0] = 0x00;
                data[i + 2] = 0x00;
            }
        }

        return new Texture(128, 128, data, "missing").SetFilterMode(TextureMinFilter.Linear, TextureMagFilter.Linear);
    }

    public void Bind()
    {
        GL.BindTexture(TextureTarget.Texture2D, TextureID);
    }

    public void Unbind()
    {
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Cleanup()
    {
        GL.DeleteTexture(TextureID);
    }
}
