namespace Skaza.MapGen.Api.Contracts;

public sealed record MapStatisticsResponse(
    ulong Seed,
    int Width,
    int Height,
    int TotalTiles,
    int WalkableTiles,
    double WalkableShare,
    IReadOnlyDictionary<string, int> TileCounts,
    IReadOnlyDictionary<string, int> ZoneCounts,
    int ShrineCount);
