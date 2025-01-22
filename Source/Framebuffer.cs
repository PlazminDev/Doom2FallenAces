using OpenTK.Graphics.OpenGL;

namespace DoomToFA;

public class Framebuffer
{
    private int fbo;

    private int width;
    private int height;

    public int colorTex { get; private set; }
    public int depthTex { get; private set; }

    private int depthBuf;

    public Framebuffer(int width, int height, int depthBufferType)
    {
        this.width = width;
        this.height = height;

        fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
        GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

        CreateTextureAttachment();
        if (depthBufferType == 2)
        {
            CreateDepthBufferAttachment();
            CreateDepthTextureAttachment();
        }
        else if (depthBufferType == 1)
            CreateDepthTextureAttachment();
        Unbind();
    }

    private void CreateTextureAttachment()
    {
        colorTex = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, colorTex);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, 0);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)TextureMagFilter.Nearest });
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)TextureMinFilter.Nearest });
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, new int[] { (int)TextureWrapMode.ClampToEdge });
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, new int[] { (int)TextureWrapMode.ClampToEdge });
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTex, 0);
    }

    private void CreateDepthTextureAttachment()
    {
        depthTex = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, depthTex);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, 0);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)TextureMagFilter.Nearest });
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)TextureMinFilter.Nearest });
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTex, 0);
    }

    private void CreateDepthBufferAttachment()
    {
        depthBuf = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuf);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, width, height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthBuf);
    }

    public void Bind()
    {
        GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, fbo);
        GL.Viewport(0, 0, width, height);
    }

    public void Unbind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Viewport(0, 0, (int)Window.Size.X, (int)Window.Size.Y);
    }

    public void Dispose()
    {
        GL.DeleteFramebuffer(fbo);
        GL.DeleteTexture(colorTex);
        GL.DeleteTexture(depthTex);
        GL.DeleteRenderbuffer(depthBuf);
    }
}
