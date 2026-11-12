using System;

namespace Skaza.MapGen
{
    public sealed class MapSettings
    {
        public const int MinSize = 32;
        public const int MaxSize = 256;
        public const int DefaultSize = 96;

        public MapSettings(int width, int height)
        {
            if (width < MinSize || width > MaxSize)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, $"Width must be between {MinSize} and {MaxSize}.");
            }

            if (height < MinSize || height > MaxSize)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, $"Height must be between {MinSize} and {MaxSize}.");
            }

            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }

        public static MapSettings Default => new MapSettings(DefaultSize, DefaultSize);
    }
}
