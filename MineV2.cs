using System.IO.Compression;

namespace mine2gd;

public struct Mine
{
    public byte Version;
    public string WorldName;
    public string CreatorName;
    public ulong TimeCreated;
    public ushort Width;
    public ushort Height;
    public ushort Depth;
    public byte[] Blocks;
}

public class MineV2
{
    public static Mine ReadMine(string filename)
    {
        Mine mine = new Mine();

        Console.WriteLine($"Decompressing...");
        using (GZipStream gzipStream = new GZipStream(File.Open(filename, FileMode.Open), CompressionMode.Decompress))
        using (BinaryReader reader = new BinaryReader(gzipStream))
        {
            reader.ReadUInt32(); // don't care atm
            
            Console.WriteLine($"Reading world...");
            mine.Version = reader.ReadByte();
            Console.WriteLine($"Version: {mine.Version}");

            mine.WorldName = reader.ReadString();
            mine.CreatorName = reader.ReadString();
            Console.WriteLine($"World: {mine.WorldName}, Creator: {mine.CreatorName}");

            mine.TimeCreated = reader.ReadUInt64();
            Console.WriteLine($"Timestamp: {mine.TimeCreated}");

            mine.Width = reader.ReadUInt16();
            mine.Height = reader.ReadUInt16();
            mine.Depth = reader.ReadUInt16();
            Console.WriteLine($"Width: {mine.Width}, Height: {mine.Height}, Depth: {mine.Depth}");

            Console.WriteLine($"Reading blocks...");
            mine.Blocks = reader.ReadBytes(mine.Width * mine.Height * mine.Depth);
        }
        
        return mine;
    }
}