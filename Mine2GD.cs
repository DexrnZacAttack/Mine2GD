using System.Numerics;
using System.Text;

namespace mine2gd
{
    public enum Block
    {
        Air = 0,                // 0x00
        Stone = 1,              // 0x01
        GrassBlock = 2,         // 0x02
        Dirt = 3,               // 0x03
        Cobblestone = 4,        // 0x04
        Planks = 5,             // 0x05
        Sapling = 6,            // 0x06
        Bedrock = 7,            // 0x07
        FlowingWater = 8,       // 0x08
        StationaryWater = 9,    // 0x09
        FlowingLava = 10,       // 0x0A
        StationaryLava = 11,    // 0x0B
        Sand = 12,              // 0x0C
        Gravel = 13,            // 0x0D
        GoldOre = 14,           // 0x0E
        IronOre = 15,           // 0x0F
        CoalOre = 16,           // 0x10
        Wood = 17,              // 0x11
        Leaves = 18,            // 0x12
        Sponge = 19,            // 0x13
        Glass = 20,             // 0x14
        RedCloth = 21,          // 0x15
        OrangeCloth = 22,       // 0x16
        YellowCloth = 23,       // 0x17
        ChartreuseCloth = 24,   // 0x18
        GreenCloth = 25,        // 0x19
        SpringGreenCloth = 26,  // 0x1A
        CyanCloth = 27,         // 0x1B
        CapriCloth = 28,        // 0x1C
        UltramarineCloth = 29,  // 0x1D
        PurpleCloth = 30,       // 0x1E
        VioletCloth = 31,       // 0x1F
        MagentaCloth = 32,      // 0x20
        RoseCloth = 33,         // 0x21
        DarkGrayCloth = 34,     // 0x22
        LightGrayCloth = 35,    // 0x23
        WhiteCloth = 36,        // 0x24
        Flower = 37,            // 0x25
        Rose = 38,              // 0x26
        BrownMushroom = 39,     // 0x27
        RedMushroom = 40,       // 0x28
        GoldBlock = 41,         // 0x29
        IronBlock = 42,         // 0x2A
        DoubleSlab = 43,        // 0x2B
        Slab = 44,              // 0x2C
        Bricks = 45,            // 0x2D
        TNT = 46,               // 0x2E
        Bookshelf = 47,         // 0x2F
        MossyCobblestone = 48,  // 0x30
        Obsidian = 49,          // 0x31
        Unsupported = 50        // 0x32+
    }

    public class Mine2GD
    {
        public static void Main(string[] args)
        {
            List<Tuple<Vector3, Block>> blockPos = new List<Tuple<Vector3, Block>>();

            var writeAll = false;

            if (args.Length == 0)
                throw new ArgumentException(
                    "Please add the world file as a parameter.\nUse -h for help.");

            if (args.Contains("-h") || args.Contains("--help"))
            {
                Console.WriteLine(
                    "Usage: mine2gd.exe <file> [options] [chunk_width] [chunk_height] [chunk_length]\n");
                Console.WriteLine("Options:");
                Console.WriteLine("  -h, --help      Show help information.");
                Console.WriteLine(
                    "  -w, --write-all Write all blocks instead of only border/exposed blocks.\n");
                Console.WriteLine(
                    "You can set any of the chunk size options to 0 to not use chunks for that axis. (will probably generate big file)");
                return;
            }

            if (args.Contains("-w") || args.Contains("--write-all"))
            {
                writeAll = true;
                args = args.Where(arg => arg != "-w" && arg != "--write-all").ToArray();
            }

            var path = args[0];

            Mine mine = MineV2.ReadMine(path);

            Console.WriteLine("Processing blocks...");
            for (int y = 0; y < mine.Height; y++)
                for (int z = 0; z < mine.Depth; z++)
                    for (int x = 0; x < mine.Width; x++)
                    {
                        int index = y * (mine.Depth * mine.Width) + z * mine.Width + x;

                        // check if it's air
                        if (mine.Blocks[index] == 0)
                            continue;

                        // add everything if write all is on
                        if (writeAll)
                        {
                            blockPos.Add(new Tuple<Vector3, Block>(
                                new Vector3(x, y, z), (mine.Blocks[index] < 51)
                                                          ? (Block)mine.Blocks[index]
                                                          : (Block)50));
                            continue;
                        }

                        // check if border
                        bool isAtBorder =
                            (x == 0 || x == mine.Width - 1 || y == 0 ||
                             y == mine.Height - 1 || z == 0 || z == mine.Depth - 1);

                        bool nearTransparent = false;

                        if (!isAtBorder)
                        {
                            for (int dy = -1; dy <= 1 && !nearTransparent; dy++)
                                for (int dz = -1; dz <= 1 && !nearTransparent; dz++)
                                    for (int dx = -1; dx <= 1 && !nearTransparent; dx++)
                                    {
                                        if (dx == 0 && dy == 0 && dz == 0)
                                            continue;

                                        int nx = x + dx;
                                        int ny = y + dy;
                                        int nz = z + dz;

                                        if (nx >= 0 && nx < mine.Width && ny >= 0 &&
                                            ny < mine.Height && nz >= 0 && nz < mine.Depth)
                                        {
                                            int neighbor =
                                                ny * (mine.Depth * mine.Width) + nz * mine.Width + nx;
                                            if ((Block)mine.Blocks[neighbor] == Block.Air ||
                                                (Block)mine.Blocks[neighbor] == Block.FlowingWater ||
                                                (Block)mine.Blocks[neighbor] == Block.StationaryWater ||
                                                (Block)mine.Blocks[neighbor] == Block.FlowingLava ||
                                                (Block)mine.Blocks[neighbor] == Block.StationaryLava)
                                                nearTransparent = true;
                                        }
                                    }
                        }

                        if (isAtBorder || nearTransparent)
                            blockPos.Add(new Tuple<Vector3, Block>(
                                new Vector3(x, y, z), (mine.Blocks[index] < 51)
                                                          ? (Block)mine.Blocks[index]
                                                          : (Block)50));
                    }

            Console.WriteLine("Placing blocks...");

            var chunkSizeX = 16;
            var chunkSizeY = 256;
            var chunkSizeZ = 16;

            if (args.Length > 1)
                chunkSizeX = int.Parse(args[1]) != 0 ? int.Parse(args[1]) : mine.Width;

            if (args.Length > 2)
                chunkSizeY = int.Parse(args[2]) != 0 ? int.Parse(args[2]) : mine.Height;

            if (args.Length > 3)
                chunkSizeZ = int.Parse(args[3]) != 0 ? int.Parse(args[3]) : mine.Depth;

            var chunkNumX =
                chunkSizeX != 0 ? (mine.Width + chunkSizeX - 1) / chunkSizeX : 1;
            var chunkNumY =
                chunkSizeY != 0 ? (mine.Height + chunkSizeY - 1) / chunkSizeY : 1;
            var chunkNumZ =
                chunkSizeZ != 0 ? (mine.Depth + chunkSizeZ - 1) / chunkSizeZ : 1;

            for (var chunkY = 0; chunkY < chunkNumY; chunkY++)
            {
                for (var chunkZ = 0; chunkZ < chunkNumZ; chunkZ++)
                {
                    for (var chunkX = 0; chunkX < chunkNumX; chunkX++)
                    {
                        var chunkPos =
                            blockPos
                                .Where(p => p.Item1.X >= chunkX * chunkSizeX &&
                                            p.Item1.X < (chunkX + 1) * chunkSizeX &&
                                            p.Item1.Y >= chunkY * chunkSizeY &&
                                            p.Item1.Y < (chunkY + 1) * chunkSizeY &&
                                            p.Item1.Z >= chunkZ * chunkSizeZ &&
                                            p.Item1.Z < (chunkZ + 1) * chunkSizeZ)
                                .ToArray();

                        var tscn = PlaceBlocks(chunkPos, new Vector3(chunkX, chunkY, chunkZ));

                        var file =
                            $"{Path.GetFileName(path)}_{chunkX}_{chunkY}_{chunkZ}.tscn";
                        Console.WriteLine($"Writing {file}...");
                        File.WriteAllText(file, tscn);
                    }
                }
            }
            Console.WriteLine("Done!");
        }

        // this is really janky
        private static string PlaceBlocks(Tuple<Vector3, Block>[] blocks,
                                          Vector3 chunkCoords)
        {
            var sb = new StringBuilder();

            // header ig
            sb.AppendLine("[gd_scene load_steps=2 format=3]\n");

            // so for every block we create a texture for it
            for (int id = 0; id < 50; id++)
            {
                sb.AppendLine(
                    $"[ext_resource type=\"Texture2D\" path=\"res://Blocks/{((Block)id).ToString().ToLower()}.png\" id=\"Texture_{(Block)id}\"]");
            }

            // do the same for the material
            for (int id = 0; id < 50; id++)
            {
                sb.AppendLine(
                    $"[sub_resource type=\"StandardMaterial3D\" id=\"Material_{(Block)id}\"]");
                sb.AppendLine("transparency = 1");
                sb.AppendLine($"albedo_texture = ExtResource(\"Texture_{(Block)id}\")");
                sb.AppendLine("uv1_scale = Vector3(3, 2, 1)");
                sb.AppendLine("texture_filter = 0");
            }

            sb.AppendLine($"[sub_resource type=\"BoxMesh\" id=\"Block\"]\n");

            sb.AppendLine(
                $"[node name=\"Chunk_{chunkCoords.X}-{chunkCoords.Y}-{chunkCoords.Z}\" type=\"Node3D\"]");

            foreach (var block in blocks)
            {
                string blockCoords = $"{block.Item1.X}-{block.Item1.Y}-{block.Item1.Z}";

                sb.AppendLine(
                    $"[node name=\"{block.Item2}_{blockCoords}\" type=\"Node3D\" parent=\".\"]");
                sb.AppendLine(
                    $"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {block.Item1.X}, {block.Item1.Y}, {block.Item1.Z})");

                sb.AppendLine(
                    $"[node name=\"{block.Item2}\" type=\"MeshInstance3D\" parent=\"{block.Item2}_{blockCoords}\"]");
                sb.AppendLine($"mesh = SubResource(\"Block\")");

                sb.AppendLine(
                    $"material_override = SubResource(\"Material_{block.Item2}\")");
            }

            return sb.ToString();
        }
    }
}