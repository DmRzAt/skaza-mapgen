using Skaza.MapGen;

namespace Skaza.MapGen.UnitTests;

public class GeneratorPartsTests
{
    [TestCase(31, 64)]
    [TestCase(64, 257)]
    [TestCase(0, 0)]
    public void MapSettings_WithSizeOutsideAllowedRange_ThrowsArgumentOutOfRange(int width, int height)
    {
        // Arrange
        TestDelegate act = () => new MapSettings(width, height);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Test]
    public void ValueNoise_ForSameSeedLayerAndPoint_ReturnsSameValue()
    {
        // Arrange
        var first = new ValueNoise(99, 1);
        var second = new ValueNoise(99, 1);

        // Act
        double a = first.Fractal(12.3, 45.6, 3);
        double b = second.Fractal(12.3, 45.6, 3);

        // Assert
        Assert.That(b, Is.EqualTo(a));
    }

    [Test]
    public void ValueNoise_ManySamples_StaysBetweenZeroAndOne()
    {
        // Arrange
        var noise = new ValueNoise(5, 2);

        // Act
        var samples = Enumerable.Range(0, 2000).Select(i => noise.Fractal(i * 0.37, i * 0.11, 3)).ToArray();

        // Assert
        Assert.That(samples, Has.All.GreaterThanOrEqualTo(0.0).And.All.LessThan(1.0));
    }

    [Test]
    public void ValueNoise_NeighbouringPoints_ChangeSmoothly()
    {
        // Arrange
        var noise = new ValueNoise(5, 2);

        // Act
        double largestStep = Enumerable.Range(0, 1000)
            .Select(i => Math.Abs(noise.Sample(i * 0.05, 3.0) - noise.Sample((i + 1) * 0.05, 3.0)))
            .Max();

        // Assert
        Assert.That(largestStep, Is.LessThan(0.15));
    }

    [TestCase(0.0, Zone.DeepCorruption)]
    [TestCase(0.32, Zone.DeepCorruption)]
    [TestCase(0.33, Zone.Borderland)]
    [TestCase(0.65, Zone.Borderland)]
    [TestCase(0.66, Zone.Clean)]
    [TestCase(1.4, Zone.Clean)]
    public void ZoneLayoutClassify_ForNormalizedDistance_ReturnsRing(double distance, Zone expected)
    {
        // Arrange
        double normalizedDistance = distance;

        // Act
        Zone zone = ZoneLayout.Classify(normalizedDistance);

        // Assert
        Assert.That(zone, Is.EqualTo(expected));
    }

    [Test]
    public void ConnectivitySealUnreachable_ForWalledOffPocket_TurnsItIntoRock()
    {
        // Arrange
        var grid = new WorldGrid(5, 5);
        for (int i = 0; i < 5; i++)
        {
            grid.SetTile(new GridPosition(2, i), TileType.Rock);
        }

        // Act
        int sealedTiles = Connectivity.SealUnreachable(grid, new GridPosition(0, 0));

        // Assert
        Assert.That(sealedTiles, Is.EqualTo(10));
        Assert.That(grid.GetTile(new GridPosition(4, 4)), Is.EqualTo(TileType.Rock));
        Assert.That(grid.GetTile(new GridPosition(0, 4)), Is.EqualTo(TileType.Meadow));
    }

    [Test]
    public void ConnectivityFindReachable_FromRock_ReturnsNothingReachable()
    {
        // Arrange
        var grid = new WorldGrid(3, 3);
        grid.SetTile(new GridPosition(1, 1), TileType.Rock);

        // Act
        bool[] reachable = Connectivity.FindReachable(grid, new GridPosition(1, 1));

        // Assert
        Assert.That(reachable, Has.All.False);
    }
}
