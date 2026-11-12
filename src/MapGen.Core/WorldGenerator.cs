using System.Collections.Generic;

namespace Skaza.MapGen
{
    public sealed class WorldGenerator
    {
        public const int HeartClearingRadius = 3;

        public GeneratedMap Generate(ulong seed, MapSettings settings)
        {
            var grid = new WorldGrid(settings.Width, settings.Height);
            var heart = new GridPosition(settings.Width / 2, settings.Height / 2);

            Zone[] zones = ZoneLayout.Build(settings.Width, settings.Height, heart, seed);
            BiomeLayer.Paint(grid, zones, seed);
            RockLayer.Paint(grid, seed);
            ClearAround(grid, heart, HeartClearingRadius);
            Connectivity.SealUnreachable(grid, heart);
            IReadOnlyList<GridPosition> shrines = ShrinePlacer.Place(grid, zones, heart, seed);

            return new GeneratedMap(seed, grid, zones, heart, shrines);
        }

        private static void ClearAround(WorldGrid grid, GridPosition center, int radius)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    var position = new GridPosition(center.X + dx, center.Y + dy);
                    bool insideCircle = dx * dx + dy * dy <= radius * radius;
                    if (insideCircle && grid.IsInside(position) && grid.GetTile(position) == TileType.Rock)
                    {
                        grid.SetTile(position, TileType.Meadow);
                    }
                }
            }
        }
    }
}
