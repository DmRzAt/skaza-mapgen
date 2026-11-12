using System.Text;
using Skaza.MapGen.Api.Contracts;

namespace Skaza.MapGen.Api.Mapping;

public static class MapResponseMapper
{
    private static readonly IReadOnlyDictionary<TileType, char> TileSymbols = new Dictionary<TileType, char>
    {
        [TileType.Meadow] = 'm',
        [TileType.Forest] = 'f',
        [TileType.Swamp] = 's',
        [TileType.Rock] = 'r',
    };

    private static readonly IReadOnlyDictionary<Zone, char> ZoneSymbols = new Dictionary<Zone, char>
    {
        [Zone.Clean] = 'c',
        [Zone.Borderland] = 'b',
        [Zone.DeepCorruption] = 'd',
    };

    public static MapResponse ToResponse(GeneratedMap map)
    {
        return new MapResponse(
            map.Seed,
            map.Width,
            map.Height,
            new PositionDto(map.Heart.X, map.Heart.Y),
            map.Shrines.Select(s => new ShrineDto(s.X, s.Y, map.GetZone(s).ToString())).ToList(),
            BuildRows(map, p => TileSymbols[map.Grid.GetTile(p)]),
            BuildRows(map, p => ZoneSymbols[map.GetZone(p)]),
            Legend(TileSymbols),
            Legend(ZoneSymbols));
    }

    public static MapStatisticsResponse ToStatisticsResponse(GeneratedMap map)
    {
        MapStatistics statistics = MapStatistics.Calculate(map);
        return new MapStatisticsResponse(
            map.Seed,
            map.Width,
            map.Height,
            statistics.TotalTiles,
            statistics.WalkableTiles,
            Math.Round(statistics.WalkableShare, 4),
            statistics.TileCounts.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value),
            statistics.ZoneCounts.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value),
            map.Shrines.Count);
    }

    private static IReadOnlyList<string> BuildRows(GeneratedMap map, Func<GridPosition, char> symbolAt)
    {
        var rows = new List<string>(map.Height);
        var row = new StringBuilder(map.Width);
        for (int y = 0; y < map.Height; y++)
        {
            row.Clear();
            for (int x = 0; x < map.Width; x++)
            {
                row.Append(symbolAt(new GridPosition(x, y)));
            }

            rows.Add(row.ToString());
        }

        return rows;
    }

    private static IReadOnlyDictionary<string, string> Legend<T>(IReadOnlyDictionary<T, char> symbols) where T : notnull
    {
        return symbols.ToDictionary(pair => pair.Value.ToString(), pair => pair.Key.ToString()!);
    }
}
