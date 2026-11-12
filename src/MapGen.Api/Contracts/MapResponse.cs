namespace Skaza.MapGen.Api.Contracts;

public sealed record PositionDto(int X, int Y);

public sealed record ShrineDto(int X, int Y, string Zone);

public sealed record MapResponse(
    ulong Seed,
    int Width,
    int Height,
    PositionDto Heart,
    IReadOnlyList<ShrineDto> Shrines,
    IReadOnlyList<string> Tiles,
    IReadOnlyList<string> Zones,
    IReadOnlyDictionary<string, string> TileLegend,
    IReadOnlyDictionary<string, string> ZoneLegend);
