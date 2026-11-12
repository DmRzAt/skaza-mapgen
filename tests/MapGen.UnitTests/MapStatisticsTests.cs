using Skaza.MapGen;

namespace Skaza.MapGen.UnitTests;

public class MapStatisticsTests
{
    [Test]
    public void Calculate_ForGeneratedMap_CountsEveryTileExactlyOnce()
    {
        // Arrange
        GeneratedMap map = new WorldGenerator().Generate(42, MapSettings.Default);

        // Act
        MapStatistics statistics = MapStatistics.Calculate(map);

        // Assert
        Assert.That(statistics.TotalTiles, Is.EqualTo(map.Width * map.Height));
        Assert.That(statistics.TileCounts.Values.Sum(), Is.EqualTo(statistics.TotalTiles));
        Assert.That(statistics.ZoneCounts.Values.Sum(), Is.EqualTo(statistics.TotalTiles));
    }

    [Test]
    public void Calculate_ForGeneratedMap_WalkableTilesEqualAllButRock()
    {
        // Arrange
        GeneratedMap map = new WorldGenerator().Generate(7, MapSettings.Default);

        // Act
        MapStatistics statistics = MapStatistics.Calculate(map);

        // Assert
        Assert.That(statistics.WalkableTiles, Is.EqualTo(statistics.TotalTiles - statistics.TileCounts[TileType.Rock]));
        Assert.That(statistics.WalkableShare, Is.EqualTo((double)statistics.WalkableTiles / statistics.TotalTiles));
    }

    [Test]
    public void Calculate_ForGeneratedMap_ListsEveryTileTypeAndZone()
    {
        // Arrange
        GeneratedMap map = new WorldGenerator().Generate(1, new MapSettings(32, 32));

        // Act
        MapStatistics statistics = MapStatistics.Calculate(map);

        // Assert
        Assert.That(statistics.TileCounts.Keys, Is.EquivalentTo(Enum.GetValues<TileType>()));
        Assert.That(statistics.ZoneCounts.Keys, Is.EquivalentTo(Enum.GetValues<Zone>()));
    }
}
