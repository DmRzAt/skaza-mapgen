namespace Skaza.MapGen.Api.Rendering;

public readonly record struct Rgb(byte R, byte G, byte B);

public static class MapImageRenderer
{
    public const int MinScale = 1;
    public const int MaxScale = 16;
    public const int DefaultScale = 4;

    public static readonly Rgb HeartColor = new(220, 60, 200);
    public static readonly Rgb ShrineColor = new(240, 210, 90);

    private static readonly IReadOnlyDictionary<TileType, Rgb> CleanColors = new Dictionary<TileType, Rgb>
    {
        [TileType.Meadow] = new(122, 138, 84),
        [TileType.Forest] = new(52, 82, 48),
        [TileType.Swamp] = new(78, 92, 96),
        [TileType.Rock] = new(70, 66, 64),
    };

    private static readonly IReadOnlyDictionary<TileType, Rgb> CorruptedColors = new Dictionary<TileType, Rgb>
    {
        [TileType.Meadow] = new(110, 96, 118),
        [TileType.Forest] = new(62, 52, 78),
        [TileType.Swamp] = new(70, 58, 86),
        [TileType.Rock] = new(70, 66, 64),
    };

    public static byte[] RenderPng(GeneratedMap map, int scale)
    {
        if (scale < MinScale || scale > MaxScale)
        {
            throw new ArgumentOutOfRangeException(nameof(scale), scale, $"Scale must be between {MinScale} and {MaxScale}.");
        }

        int width = map.Width * scale;
        int height = map.Height * scale;
        var pixels = new byte[width * height * 3];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Rgb color = ColorAt(map, new GridPosition(x / scale, y / scale));
                int offset = (y * width + x) * 3;
                pixels[offset] = color.R;
                pixels[offset + 1] = color.G;
                pixels[offset + 2] = color.B;
            }
        }

        return PngEncoder.EncodeRgb(width, height, pixels);
    }

    public static Rgb ColorAt(GeneratedMap map, GridPosition position)
    {
        if (position == map.Heart)
        {
            return HeartColor;
        }

        if (map.Shrines.Contains(position))
        {
            return ShrineColor;
        }

        var palette = map.GetZone(position) == Zone.DeepCorruption ? CorruptedColors : CleanColors;
        return palette[map.Grid.GetTile(position)];
    }
}
