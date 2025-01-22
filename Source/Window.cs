using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using System.Numerics;
using System.Text;
using ImGuiNET;
using System.Drawing;
using IniParser;
using IniParser.Model;

namespace DoomToFA;

public class Window : GameWindow
{
    private ImGuiController _ImGuiController;

    private WAD loaded;
    private Map selectedMap;
    private int selectedLevel = -1;

    private Texture missingTex;
    private Framebuffer framebuffer;

    private IniData ini;

    public static Vector2 Size = new Vector2(800, 600);

    private static readonly string configFile = "user.ini";

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

        framebuffer = new Framebuffer(256, 256, 0);

        if (!File.Exists(configFile))
        {
            ini = new IniData();

            ini.Sections.Add(new SectionData("General"));

            ini.Sections["General"]["DrawThings"] = "0";
        }
        else
        {
            FileIniDataParser parser = new FileIniDataParser();
            ini = parser.ReadFile(configFile);

            Preferences.DrawThings = ini.Sections["General"]["DrawThings"] == "1" ? true : false;
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        _ImGuiController.Dispose();
        missingTex.Cleanup();

        MapRenderer.Cleanup();

        FileIniDataParser parser = new FileIniDataParser();
        parser.WriteFile(configFile, ini);
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
        if(ImGui.Button("Load WAD") && !prefPopup)
        {
            selectedMap = null;
            selectedLevel = -1;
            loaded = null;
            exportPopup = false;
        }
        if (ImGui.Button("Prefrences") && !exportPopup && loaded != null)
        {
            prefPopup = true;
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

        if (prefPopup)
        {
            ImGui.OpenPopup("Preferences");
            PreferencesPopup();
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
    bool prefPopup = false;

    private void OpenFile()
    {
        ImGui.SetNextWindowPos(new Vector2 (0, (ClientSize.Y / 2.0f) - (72 / 2)));
        ImGui.SetNextWindowSize(new Vector2 (ClientSize.X, 72));
        if (ImGui.BeginPopupModal("##LOADWAD", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar))
        {
            ImGui.InputText("Enter path to WAD", wadPath, 128);
            if (ImGui.Button("Paste"))
            {
                wadPath = new byte[128];
                byte[] data = Encoding.UTF8.GetBytes(ClipboardString);
                if (data.Length < wadPath.Length)
                {
                    for (int i = 0; i < data.Length; i++)
                        wadPath[i] = data[i];
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Submit"))
            {
                string path = Utils.GetTerminatedString(wadPath);
                path = path.Replace("\"", "");
                Console.WriteLine(path);
                if (!File.Exists(path))
                {
                    error = true;
                    if (Directory.Exists(path))
                    {
                        errorMsg = "Path doesn't reference a file!";
                    }
                    else
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
            if (ImGui.Button("Cancel"))
            {
                exportPopup = false;
                nameBuffer = new byte[32];
            }
            ImGui.SameLine();
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
                    if (ImGui.Button(loaded.Lumps[i].Name, new Vector2(ImGui.GetWindowSize().X - 30, 20)))
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

    private static readonly int PREVIEW_SCALE_OFFSET = 64;

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

            ImGui.Image(framebuffer.colorTex, new Vector2(Math.Min(
                ImGui.GetWindowWidth() - PREVIEW_SCALE_OFFSET, 
                ImGui.GetWindowHeight() - PREVIEW_SCALE_OFFSET)));

            ImGui.NewLine();

            ImGui.Text("NUMVERTICES: " + selectedMap.vertices.Length);
            ImGui.Text("NUMLINEDEFS: " + selectedMap.linedefs.Length);
            ImGui.Text("NUMSIDEDEFS: " + selectedMap.sidedefs.Length);
            ImGui.Text("NUMSEGS: " + selectedMap.segs.Length);
            ImGui.Text("NUMSSECTORS: " + selectedMap.ssectors.Length);
            ImGui.Text("NUMSECTORS: " + selectedMap.sectors.Length);
        }

        ImGui.End();
    }

    private void PreferencesPopup()
    {
        if (ImGui.BeginPopupModal("Preferences", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings))
        {
            if(ImGui.Checkbox("Draw Things", ref Preferences.DrawThings) && selectedMap != null)
            {
                ini.Sections["General"]["DrawThings"] = Preferences.DrawThings ? "1" : "0";
                framebuffer.Bind();
                MapRenderer.RenderMap(selectedMap);
                framebuffer.Unbind();
            }
            if (ImGui.Button("OK"))
            {
                prefPopup = false;
            }
            ImGui.EndPopup();
        }
    }
}