using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace Skaza.MapGen.Api.Rendering;

public static class PngEncoder
{
    private const byte ColorTypeRgb = 2;
    private const byte BitDepth = 8;
    private const int BytesPerPixel = 3;

    private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
    private static readonly uint[] CrcTable = BuildCrcTable();

    public static byte[] EncodeRgb(int width, int height, byte[] rgb)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Image size must be positive.");
        }

        if (rgb.Length != width * height * BytesPerPixel)
        {
            throw new ArgumentException("Pixel buffer size does not match the image size.", nameof(rgb));
        }

        using var output = new MemoryStream();
        output.Write(Signature);
        WriteChunk(output, "IHDR", BuildHeader(width, height));
        WriteChunk(output, "IDAT", Compress(width, height, rgb));
        WriteChunk(output, "IEND", Array.Empty<byte>());
        return output.ToArray();
    }

    private static byte[] BuildHeader(int width, int height)
    {
        var header = new byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(0, 4), (uint)width);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4, 4), (uint)height);
        header[8] = BitDepth;
        header[9] = ColorTypeRgb;
        return header;
    }

    private static byte[] Compress(int width, int height, byte[] rgb)
    {
        int rowLength = width * BytesPerPixel;
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            for (int y = 0; y < height; y++)
            {
                zlib.WriteByte(0);
                zlib.Write(rgb, y * rowLength, rowLength);
            }
        }

        return compressed.ToArray();
    }

    private static void WriteChunk(Stream output, string type, byte[] data)
    {
        Span<byte> number = stackalloc byte[4];
        byte[] typeBytes = Encoding.ASCII.GetBytes(type);

        BinaryPrimitives.WriteUInt32BigEndian(number, (uint)data.Length);
        output.Write(number);
        output.Write(typeBytes);
        output.Write(data);

        uint crc = Crc32(typeBytes, data);
        BinaryPrimitives.WriteUInt32BigEndian(number, crc);
        output.Write(number);
    }

    private static uint Crc32(byte[] type, byte[] data)
    {
        uint crc = 0xFFFFFFFFu;
        foreach (byte value in type.Concat(data))
        {
            crc = CrcTable[(crc ^ value) & 0xFF] ^ (crc >> 8);
        }

        return ~crc;
    }

    private static uint[] BuildCrcTable()
    {
        var table = new uint[256];
        for (uint n = 0; n < table.Length; n++)
        {
            uint c = n;
            for (int k = 0; k < 8; k++)
            {
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            }

            table[n] = c;
        }

        return table;
    }
}
