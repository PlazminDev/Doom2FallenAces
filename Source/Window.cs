using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using System.Numerics;
using System.Text;
using ImGuiNET;
using System.Drawing;

namespace DoomToFA;

public class Window : GameWindow
{
    private ImGuiController _ImGuiController;

    private WAD loaded;
    private Map selectedMap;
    private int selectedLevel = -1;

    private Texture missingTex;
    private Framebuffer framebuffer;

    public static Vector2 Size = new Vector2(800, 600);

    public Window() : base(GameWindowSettings.Default, new NativeWindowSettings() { 
        Title = "Doom To Fallen Aces",
        ClientSize = new OpenTK.Mathematics.Vector2i(800, 600), 
        Vsync = OpenTK.Windowing.Common.VSyncMode.On,
        APIVersion = new Version(3, 3) 
    }){}

    protected override void OnLoad()
    {
        base.OnLoad();

        _ImGuiController = new ImGuiController(ClientSize.X, ClientSize.Y);

        ImGui.LoadIniSettingsFromDisk("imgui.ini");

        missingTex = Texture.GenMissingTexture();
        missingTex.SetWrapMode(TextureWrapMode.Clamp);

        framebuffer = new Framebuffer(256, 256, 2);
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        _ImGuiController.Dispose();
        missingTex.Cleanup();

        MapRenderer.Cleanup();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
        Size = new Vector2(ClientSize.X, ClientSize.Y);
        _ImGuiController.WindowResized(ClientSize.X, ClientSize.Y);
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);

        _ImGuiController.PressChar((char)e.Unicode);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.ClearColor(new OpenTK.Mathematics.Color4(0, 0, 0, 255));
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        _ImGuiController.Update(this, (float)(args.Time), out bool inputConsumed);
        ImGui.DockSpaceOverViewport();

        ImGui.BeginMainMenuBar();
        if(ImGui.Button("Load New Map"))
        {
            selectedMap = null;
            selectedLevel = -1;
            loaded = null;
            exportPopup = false;
        }
        ImGui.EndMainMenuBar();

        LumpView();
        Inspector();

        if (loaded == null)
        {
            ImGui.OpenPopup("##LOADWAD");
            OpenFile();
        }

        if (exportPopup)
        {
            ImGui.OpenPopup("##EXPORT");
            ExportPopup();
        }

        _ImGuiController.Render();
        ImGuiController.CheckGLError("End of frame");

        SwapBuffers();
        base.OnRenderFrame(args);
    }

    private byte[] wadPath = new byte[128];
    private byte[] nameBuffer = new byte[32];
    bool error = false;
    string errorMsg = string.Empty;

    bool exportPopup = false;

    private void OpenFile()
    {
        ImGui.SetNextWindowPos(new Vector2 (0, (ClientSize.Y / 2.0f) - (74 / 2)));
        ImGui.SetNextWindowSize(new Vector2 (ClientSize.X, 74));
        if (ImGui.BeginPopupModal("##LOADWAD", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar))
        {
            ImGui.InputText("Enter path to WAD", wadPath, 128);
            if (ImGui.Button("Paste"))
            {
                wadPath = new byte[128];
                byte[] data = Encoding.UTF8.GetBytes(ClipboardString);
                for(int i = 0; i < data.Length; i++)
                    wadPath[i] = data[i];
            }
            ImGui.SameLine();
            if (ImGui.Button("Submit"))
            {
                string path = Utils.GetTerminatedString(wadPath);
                if (!File.Exists(path))
                {
                    error = true;
                    errorMsg = "Path does not exist!";
                }
                else
                {
                    var wad = new WAD(path);
                    if(wad.valid)
                    {
                        this.loaded = wad;
                    }
                    else
                    {
                        error = true;
                        errorMsg = "File is not a valid .WAD file!";
                    }
                }
            }
            if (error) ImGui.TextColored(new Vector4(1, 0.5f, 0.4f, 1.0f), errorMsg);
            ImGui.EndPopup();
        }
    }

    private void ExportPopup()
    {
        ImGui.SetNextWindowPos(new Vector2(0, (ClientSize.Y / 2.0f) - (74 / 2)));
        ImGui.SetNextWindowSize(new Vector2(ClientSize.X, 74));
        if (ImGui.BeginPopupModal("##EXPORT", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar))
        {
            ImGui.InputText("Enter name of exported map", nameBuffer, 32);
            if (ImGui.Button("Paste"))
            {
                wadPath = new byte[128];
                byte[] data = Encoding.UTF8.GetBytes(ClipboardString);
                for (int i = 0; i < data.Length; i++)
                    wadPath[i] = data[i];
            }
            ImGui.SameLine();
            if (ImGui.Button("Export"))
            {
                AceExporter.Export(Utils.GetTerminatedString(nameBuffer), selectedMap);
                exportPopup = false;
            }
            //if (error) ImGui.TextColored(new Vector4(1, 0.5f, 0.4f, 1.0f), errorMsg);
            ImGui.EndPopup();
        }
    }

    private void LumpView()
    {
        ImGui.Begin("Map List");

        if(loaded != null)
        {
            for (int i = 0; i < loaded.Lumps.Length; i++)
            {
                if (loaded.Lumps[i].Name.StartsWith("MAP") || (loaded.Lumps[i].Name[0] == 'E' && char.IsNumber(loaded.Lumps[i].Name[1])
                    && loaded.Lumps[i].Name[2] == 'M' && char.IsNumber(loaded.Lumps[i].Name[3])))
                {
                    if (ImGui.Button(loaded.Lumps[i].Name, new Vector2(64, 20)))
                    {
                        selectedMap = new Map(loaded.Lumps[i].Name, i, loaded.Lumps);
                        selectedLevel = i;

                        framebuffer.Bind();
                        MapRenderer.RenderMap(selectedMap);
                        framebuffer.Unbind();
                    }
                }
            }
        }

        ImGui.End();
    }

    private void Inspector()
    {
        ImGui.Begin("Inspector");

        if (loaded != null && selectedLevel != -1)
        {
            ImGui.SetWindowFontScale(2.0f);
            ImGui.Text(selectedMap.name);
            ImGui.SetWindowFontScale(1.0f);

            if (ImGui.Button("Export"))
            {
                exportPopup = true;
            }

            ImGui.Image(framebuffer.colorTex, new Vector2(ImGui.GetWindowWidth() - 128));

            ImGui.NewLine();

            ImGui.Text("NUMVERTICES: " + selectedMap.vertices.Length);
            ImGui.Text("NUMLINEDEFS: " + selectedMap.linedefs.Length);
        }

        ImGui.End();
    }
}