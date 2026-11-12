using System;
using System.Collections.Generic;

namespace Skaza.MapGen
{
    public sealed class MapStatistics
    {
        private MapStatistics(IReadOnlyDictionary<TileType, int> tileCounts, IReadOnlyDictionary<Zone, int> zoneCounts, int walkableTiles, int totalTiles)
        {
            TileCounts = tileCounts;
            ZoneCounts = zoneCounts;
            WalkableTiles = walkableTiles;
            TotalTiles = totalTiles;
        }

        public IReadOnlyDictionary<TileType, int> TileCounts { get; }
        public IReadOnlyDictionary<Zone, int> ZoneCounts { get; }
        public int WalkableTiles { get; }
        public int TotalTiles { get; }
        public double WalkableShare => TotalTiles == 0 ? 0.0 : (double)WalkableTiles / TotalTiles;

        public static MapStatistics Calculate(GeneratedMap map)
        {
            var tileCounts = CreateCounter<TileType>();
            var zoneCounts = CreateCounter<Zone>();
            int walkable = 0;

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var position = new GridPosition(x, y);
                    tileCounts[map.Grid.GetTile(position)]++;
                    zoneCounts[map.GetZone(position)]++;
                    if (map.Grid.IsWalkable(position))
                    {
                        walkable++;
                    }
                }
            }

            return new MapStatistics(tileCounts, zoneCounts, walkable, map.Width * map.Height);
        }

        private static Dictionary<T, int> CreateCounter<T>() where T : struct, Enum
        {
            var counter = new Dictionary<T, int>();
            foreach (T value in (T[])Enum.GetValues(typeof(T)))
            {
                counter[value] = 0;
            }

            return counter;
        }
    }
}
