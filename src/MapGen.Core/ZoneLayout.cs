using System;

namespace Skaza.MapGen
{
    public static class ZoneLayout
    {
        public const double DeepCorruptionRadius = 0.33;
        public const double BorderlandRadius = 0.66;

        private const double EdgeNoiseScale = 0.08;
        private const double EdgeNoiseStrength = 0.16;
        private const uint EdgeNoiseLayer = 1;

        public static Zone[] Build(int width, int height, GridPosition heart, ulong seed)
        {
            var edgeNoise = new ValueNoise(seed, EdgeNoiseLayer);
            double maxRadius = Math.Min(width, height) / 2.0;
            var zones = new Zone[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double distance = Math.Sqrt(Square(x - heart.X) + Square(y - heart.Y)) / maxRadius;
                    double wobble = (edgeNoise.Fractal(x * EdgeNoiseScale, y * EdgeNoiseScale, 3) - 0.5) * EdgeNoiseStrength;
                    zones[y * width + x] = Classify(distance + wobble);
                }
            }

            return zones;
        }

        public static Zone Classify(double normalizedDistance)
        {
            if (normalizedDistance < DeepCorruptionRadius)
            {
                return Zone.DeepCorruption;
            }

            return normalizedDistance < BorderlandRadius ? Zone.Borderland : Zone.Clean;
        }

        private static double Square(double value)
        {
            return value * value;
        }
    }
}
