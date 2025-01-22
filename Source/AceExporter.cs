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

        if (!Directory.Exists(name))
            Directory.CreateDirectory(name);

        using (FileStream stream = File.Create(Path.Join(name, Regex.Replace(name, @"\s+", "") + ".txt")))
        {
            byte[] data = Encoding.UTF8.GetBytes(mapFile.ToString());
            stream.Write(data, 0, data.Length);
        }
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

        mapFile.AppendLine("");
        mapFile.AppendLine("LayerInfo");
        mapFile.AppendLine("{");
        mapFile.AppendLine("id = 1;");
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
            mapFile.AppendLine("side_middle = " + i + ";");
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
        for(int s = 0; s < map.sectors.Length; s++)
        {
            for (int i = map.sectors[s].light; i < map.ssectors.Length; i++)
            {
                mapFile.AppendLine("Sector // " + i);
                mapFile.AppendLine("{");
                mapFile.AppendLine("layer = 0;");

                List<int> vertices = new();
                Console.WriteLine(map.ssectors[i].segCount);
                for (int j = map.ssectors[i].first; j < map.ssectors[i].first + map.ssectors[i].segCount; j++)
                {
                    vertices.Add(map.segs[j].v1);
                    vertices.Add(map.segs[j].v2);
                }

                var verticesText = "";
                for (int j = 0; j < vertices.Count; j++)
                {
                    verticesText += vertices[j] + ",";
                }

                mapFile.AppendLine("vertices = " + verticesText + ";");

                mapFile.AppendLine("height_floor = " + map.sectors[i].floor + ";");
                mapFile.AppendLine("height_ceiling = " + map.sectors[i].ceil + ";");
                //mapFile.AppendLine("lighting = 1, 1, 1, 1;");
                mapFile.AppendLine("lighting = " + (1) + ", " + (1) + ", " + (1) + ", " + (1));
                mapFile.AppendLine("floor_slope ( sloped = False; direction = 0; height = 0; )");
                mapFile.AppendLine("ceiling_slope( sloped = False; direction = 0; height = 0; )");

                mapFile.AppendLine();
            }
        }
    }
}