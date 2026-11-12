using Skaza.MapGen;

namespace Skaza.MapGen.UnitTests;

public class WorldGeneratorTests
{
    private static readonly ulong[] SampleSeeds = { 1, 7, 42, 100, 2026, 123456789, ulong.MaxValue };

    [Test]
    public void Generate_WithSameSeed_ProducesIdenticalMaps()
    {
        // Arrange
        var generator = new WorldGenerator();

        // Act
        GeneratedMap first = generator.Generate(42, MapSettings.Default);
        GeneratedMap second = generator.Generate(42, MapSettings.Default);

        // Assert
        Assert.That(Describe(second), Is.EqualTo(Describe(first)));
        Assert.That(second.Shrines, Is.EqualTo(first.Shrines));
        Assert.That(second.Heart, Is.EqualTo(first.Heart));
    }

    [Test]
    public void Generate_WithDifferentSeeds_ProducesDifferentMaps()
    {
        // Arrange
        var generator = new WorldGenerator();

        // Act
        string first = Describe(generator.Generate(1, MapSettings.Default));
        string second = Describe(generator.Generate(2, MapSettings.Default));

        // Assert
        int differentTiles = first.Zip(second).Count(pair => pair.First != pair.Second);
        Assert.That(differentTiles, Is.GreaterThan(first.Length / 4));
    }

    [TestCase(32, 32)]
    [TestCase(64, 48)]
    [TestCase(256, 256)]
    public void Generate_WithGivenSettings_ReturnsMapOfThatSize(int width, int height)
    {
        // Arrange
        var settings = new MapSettings(width, height);

        // Act
        GeneratedMap map = new WorldGenerator().Generate(7, settings);

        // Assert
        Assert.That(map.Width, Is.EqualTo(width));
        Assert.That(map.Height, Is.EqualTo(height));
    }

    [TestCaseSource(nameof(SampleSeeds))]
    public void Generate_ForAnySeed_PlacesWalkableHeartInTheCenter(ulong seed)
    {
        // Arrange
        var settings = MapSettings.Default;

        // Act
        GeneratedMap map = new WorldGenerator().Generate(seed, settings);

        // Assert
        Assert.That(map.Heart, Is.EqualTo(new GridPosition(settings.Width / 2, settings.Height / 2)));
        Assert.That(map.Grid.IsWalkable(map.Heart), Is.True);
        Assert.That(map.GetZone(map.Heart), Is.EqualTo(Zone.DeepCorruption));
    }

    [TestCaseSource(nameof(SampleSeeds))]
    public void Generate_ForAnySeed_MakesEveryWalkableTileReachableFromTheHeart(ulong seed)
    {
        // Arrange
        GeneratedMap map = new WorldGenerator().Generate(seed, MapSettings.Default);

        // Act
        bool[] reachable = Connectivity.FindReachable(map.Grid, map.Heart);

        // Assert
        int unreachableWalkable = AllPositions(map).Count(p => map.Grid.IsWalkable(p) && !reachable[p.Y * map.Width + p.X]);
        Assert.That(unreachableWalkable, Is.Zero);
    }

    [TestCaseSource(nameof(SampleSeeds))]
    public void Generate_ForAnySeed_PlacesOneWalkableShrinePerZone(ulong seed)
    {
        // Arrange
        var generator = new WorldGenerator();

        // Act
        GeneratedMap map = generator.Generate(seed, MapSettings.Default);

        // Assert
        Assert.That(map.Shrines, Has.Count.EqualTo(ShrinePlacer.ShrineCount));
        Assert.That(map.Shrines.Select(map.GetZone), Is.EqualTo(new[] { Zone.Clean, Zone.Borderland, Zone.DeepCorruption }));
        Assert.That(map.Shrines.All(map.Grid.IsWalkable), Is.True);
    }

    [TestCaseSource(nameof(SampleSeeds))]
    public void Generate_ForAnySeed_KeepsShrinesApartFromEachOther(ulong seed)
    {
        // Arrange
        GeneratedMap map = new WorldGenerator().Generate(seed, MapSettings.Default);

        // Act
        double minDistance = Pairs(map.Shrines).Min(pair => Distance(pair.Item1, pair.Item2));

        // Assert
        Assert.That(minDistance, Is.GreaterThanOrEqualTo(ShrinePlacer.MinDistanceBetweenShrines));
    }

    [Test]
    public void Generate_AnyMap_SurroundsTheMapWithRock()
    {
        // Arrange
        GeneratedMap map = new WorldGenerator().Generate(2026, MapSettings.Default);

        // Act
        var border = AllPositions(map).Where(p => p.X == 0 || p.Y == 0 || p.X == map.Width - 1 || p.Y == map.Height - 1);

        // Assert
        Assert.That(border.Select(map.Grid.GetTile), Has.All.EqualTo(TileType.Rock));
    }

    [Test]
    public void Generate_DefaultMap_ContainsEveryTileType()
    {
        // Arrange
        GeneratedMap map = new WorldGenerator().Generate(42, MapSettings.Default);

        // Act
        var tileTypes = AllPositions(map).Select(map.Grid.GetTile).Distinct().ToList();

        // Assert
        Assert.That(tileTypes, Is.EquivalentTo(Enum.GetValues<TileType>()));
    }

    [Test]
    public void Generate_DefaultMap_KeepsMostOfTheMapWalkable()
    {
        // Arrange
        GeneratedMap map = new WorldGenerator().Generate(7, MapSettings.Default);

        // Act
        double walkableShare = AllPositions(map).Count(map.Grid.IsWalkable) / (double)(map.Width * map.Height);

        // Assert
        Assert.That(walkableShare, Is.GreaterThan(0.5));
    }

    private static IEnumerable<GridPosition> AllPositions(GeneratedMap map)
    {
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                yield return new GridPosition(x, y);
            }
        }
    }

    private static string Describe(GeneratedMap map)
    {
        return string.Concat(AllPositions(map).Select(p => (char)('0' + (int)map.Grid.GetTile(p) * 3 + (int)map.GetZone(p))));
    }

    private static IEnumerable<(GridPosition, GridPosition)> Pairs(IReadOnlyList<GridPosition> positions)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            for (int j = i + 1; j < positions.Count; j++)
            {
                yield return (positions[i], positions[j]);
            }
        }
    }

    private static double Distance(GridPosition a, GridPosition b)
    {
        return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
    }
}
