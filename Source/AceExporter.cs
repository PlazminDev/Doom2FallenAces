using System.Text;
using System.Text.RegularExpressions;

namespace DoomToFA;

public static class AceExporter
{
    public static void Export(string name, Map map)
    {
        StringBuilder mapFile = new StringBuilder();
        StringBuilder infoFile = new StringBuilder();

        Global(mapFile);
        WriteVertices(mapFile, map);
        WriteLines(mapFile, map);
        WriteSectors(mapFile, map);
        WriteThings(mapFile, map);

        WriteInfo(infoFile, name);

        if (!Directory.Exists(name))
            Directory.CreateDirectory(name);

        using (FileStream stream = File.Create(Path.Join(name, Regex.Replace(name, @"\s+", "") + ".txt")))
        {
            byte[] data = Encoding.UTF8.GetBytes(mapFile.ToString());
            stream.Write(data, 0, data.Length);
        }

        using (FileStream stream = File.Create(name + "/chapterInfo.txt"))
        {
            byte[] data = Encoding.UTF8.GetBytes(infoFile.ToString());
            stream.Write(data, 0, data.Length);
        }
    }

    private static void WriteInfo(StringBuilder infoFile, string _mapName)
    {
        infoFile.AppendLine(@"title = """ + _mapName + @""";");
        infoFile.AppendLine(@"over_title_text = ""Custom Level"";");
        infoFile.AppendLine(@"order = 99;");
        infoFile.AppendLine(@"secret_count = 0;");
        infoFile.AppendLine(@"loading_screen_ambience = 9;");
        infoFile.AppendLine(@"loading_screen_music = 27;");
        infoFile.AppendLine(@"world_file_name = """ + Regex.Replace(_mapName, @"\s+", "") + @".txt"";");
        infoFile.AppendLine(@"sprite_groups = ""Level 1"", ""Note Backgrounds"";");
        infoFile.AppendLine(@"always_unlocked = true;");
        infoFile.AppendLine(@"description_text = ""Custom Level Description"";");
        infoFile.AppendLine(@"faction_name = 0 ""Glasshearts"";");
        infoFile.AppendLine(@"faction_color = 0 ""red"";");
        infoFile.AppendLine(@"faction_name = 1 ""Benedettos"";");
        infoFile.AppendLine(@"faction_color = 1 ""purple"";");
        infoFile.AppendLine();
    }

    private static void Global(StringBuilder mapFile)
    {
        // WRITE MAP DATA
        mapFile.AppendLine("Global");
        mapFile.AppendLine("{");
        mapFile.AppendLine("editor_version_major = 0;");
        mapFile.AppendLine("editor_version_minor = 7;");
        mapFile.AppendLine("editor_version_revision = 7;");
        mapFile.AppendLine("map_version_major = 1;");
        mapFile.AppendLine("map_version_minor = 7;");
        mapFile.AppendLine("acesdata_version = 14;");
        mapFile.AppendLine("default_texture_scale = 1.0;");
        mapFile.AppendLine("skybox = 3;");
        mapFile.AppendLine("skybox_rotation = 0;");
        mapFile.AppendLine("skybox_height_offset = 0.4;");
        mapFile.AppendLine("fog_density = 0.0075;");
        mapFile.AppendLine("fog_color = 0.6737489, 0.7926024, 0.9900501, 1;");
        mapFile.AppendLine("weather = 0;");
        mapFile.AppendLine("backup_index = 2;");
        mapFile.AppendLine("}");
        mapFile.AppendLine();

        mapFile.AppendLine("LayerInfo");
        mapFile.AppendLine("{");
        mapFile.AppendLine("id = 0;");
        mapFile.AppendLine(@"name = Base;");
        mapFile.AppendLine("}");
        mapFile.AppendLine();
    }

    private static void WriteVertices(StringBuilder mapFile, Map map)
    {
        for (int i = 0; i < map.vertices.Length; i++)
        {
            mapFile.AppendLine("Vertex // " + i);
            mapFile.AppendLine("{");
            mapFile.AppendLine("x = " + (map.vertices[i].X) + ";");
            mapFile.AppendLine("z = " + (map.vertices[i].Y) + ";");
            mapFile.AppendLine("}");
            mapFile.AppendLine();
        }
    }

    private static void WriteLines(StringBuilder mapFile, Map map)
    {
        for (int i = 0; i < map.linedefs.Length; i++)
        {
            mapFile.AppendLine("Line // " + i);
            mapFile.AppendLine("{");
            mapFile.AppendLine("v1 = " + (map.linedefs[i].v1) + ";");
            mapFile.AppendLine("v2 = " + (map.linedefs[i].v2) + ";");
            mapFile.AppendLine("side_middle = " + (map.linedefs[i].front) + ";");
            mapFile.AppendLine("}");
            mapFile.AppendLine();
        }
    }

    private static void WriteSideDefs(StringBuilder mapFile, Map map)
    {
        for (int i = 0; i < map.sidedefs.Length; i++)
        {
            mapFile.AppendLine("Side // " + i);
            mapFile.AppendLine("{");
            mapFile.AppendLine("line = " + i + ";");
            mapFile.AppendLine("sector = " + (map.sidedefs[i].sector) + ";");
            mapFile.AppendLine("side_plane ( )");
            //string texture = "NULL";
            //int s = map.sidedefs[i].sector;

            string texSettings = "offset = " + 0 + ", " + 0 + "; scale = " + 1.0 + ", " + 1.0 + "; angle = 0; )";
            mapFile.AppendLine(@"side_texture ( path = ""Editor/Default""; " + texSettings);
            mapFile.AppendLine("}");
            mapFile.AppendLine();
        }
    }

    private static void WriteSectors(StringBuilder mapFile, Map map)
    {

        for (int s = 0; s < map.sectors.Length; s++)
        {
            map.sectors[s].vertices = new();
            map.sectors[s].lines = new();
        }

        for (int i = 0; i < map.linedefs.Length; i++)
        {
            var linedefs = map.linedefs[i];

            map.sectors[map.sidedefs[map.linedefs[i].front].sector].lines.Add(i);
            map.sectors[map.sidedefs[map.linedefs[i].front].sector].vertices.Add(map.linedefs[i].v1);
        }

        for(int s = 0; s < map.sectors.Length; s++)
        {
            mapFile.AppendLine("Sector // " + s);
            mapFile.AppendLine("{");

            mapFile.AppendLine("layer = 0;");

            string vertices = "";
            for (int i = 0; i < map.sectors[s].vertices.Count + 5; i++)
                vertices += i + ",";

            mapFile.AppendLine("vertices = " + vertices + ";");

            string lines = "";
            for (int i = 0; i < map.sectors[s].vertices.Count + 5; i++)
                lines += i + ",";

            mapFile.AppendLine("lines = " + lines + ";");

            mapFile.AppendLine($"height_floor = {(float)map.sectors[s].floor * Utils.DOOM2ACE_SCALAR};");
            mapFile.AppendLine($"height_ceiling = {(float)map.sectors[s].ceil * Utils.DOOM2ACE_SCALAR};");

            mapFile.AppendLine("lighting = 1, 1, 1, 1");
            mapFile.AppendLine("floor_slope ( sloped = False; direction = 0; height = 0; )");
            mapFile.AppendLine("ceiling_slope ( sloped = False; direction = 0; height = 0; )");
            mapFile.AppendLine("floor_texture(path = \"Editor/Default\"; offset = 0, 0; scale = 1, 1; angle = 0; )");
            mapFile.AppendLine("ceiling_texture(path = \"Editor/Default\"; offset = 0, 0; scale = 1, 1; angle = 0; )");
            mapFile.AppendLine("floor_plane (visible = True; solid = True; brightness_offset = 0;)");
            mapFile.AppendLine("ceiling_plane (visible = True; solid = True; brightness_offset = 0;)");

            mapFile.AppendLine("}");
            mapFile.AppendLine();
        }
    }

    private static void WriteThings(StringBuilder mapFile, Map map)
    {
        int offset = 0;
        for(int i = 0; i < map.things.Length; i++)
        {
            if (ThingConversion(map.things[i].type) == -1) { offset--; continue; }

            mapFile.AppendLine("Thing // " + (i + offset));
            mapFile.AppendLine("{");

            mapFile.AppendLine("layer = 0;");
            mapFile.AppendLine($"x = {map.things[i].x};");
            mapFile.AppendLine($"y = {10.0f};");
            mapFile.AppendLine($"z = {map.things[i].y};");

            mapFile.AppendLine($"definition_id = {ThingConversion(map.things[i].type)};");

            mapFile.AppendLine("}");
            mapFile.AppendLine();
        }
    }

    private static int ThingConversion(int doom)
    {
        switch (doom)
        {
            case 1: // Player 1 (Mike)
                return 13484;
        }

        return -1;
    }
}