namespace Skaza.MapGen
{
    public static class BiomeLayer
    {
        private const double MoistureScale = 0.06;
        private const double ForestScale = 0.09;
        private const double ForestThreshold = 0.55;
        private const double SwampThreshold = 0.64;
        private const double CorruptedSwampThreshold = 0.55;
        private const uint MoistureLayer = 2;
        private const uint ForestLayer = 3;

        public static void Paint(WorldGrid grid, Zone[] zones, ulong seed)
        {
            var moisture = new ValueNoise(seed, MoistureLayer);
            var forest = new ValueNoise(seed, ForestLayer);

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    double wetness = moisture.Fractal(x * MoistureScale, y * MoistureScale, 3);
                    double trees = forest.Fractal(x * ForestScale, y * ForestScale, 3);
                    double swampThreshold = zones[y * grid.Width + x] == Zone.DeepCorruption ? CorruptedSwampThreshold : SwampThreshold;
                    grid.SetTile(new GridPosition(x, y), ChooseBiome(wetness, trees, swampThreshold));
                }
            }
        }

        private static TileType ChooseBiome(double wetness, double trees, double swampThreshold)
        {
            if (wetness > swampThreshold)
            {
                return TileType.Swamp;
            }

            return trees > ForestThreshold ? TileType.Forest : TileType.Meadow;
        }
    }
}
