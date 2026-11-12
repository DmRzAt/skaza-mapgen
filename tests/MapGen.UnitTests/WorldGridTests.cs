using Skaza.MapGen;

namespace Skaza.MapGen.UnitTests;

public class WorldGridTests
{
    [Test]
    public void Constructor_WithValidSize_CreatesGridFilledWithMeadow()
    {
        // Arrange
        const int width = 4;
        const int height = 3;

        // Act
        var grid = new WorldGrid(width, height);

        // Assert
        Assert.That(grid.Width, Is.EqualTo(width));
        Assert.That(grid.Height, Is.EqualTo(height));
        Assert.That(AllTiles(grid), Has.All.EqualTo(TileType.Meadow));
    }

    [TestCase(0, 5)]
    [TestCase(5, 0)]
    [TestCase(-1, 5)]
    public void Constructor_WithNonPositiveSize_ThrowsArgumentOutOfRange(int width, int height)
    {
        // Arrange
        TestDelegate act = () => new WorldGrid(width, height);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Test]
    public void SetTile_OnOnePosition_ChangesOnlyThatTile()
    {
        // Arrange
        var grid = new WorldGrid(3, 3);
        var center = new GridPosition(1, 1);

        // Act
        grid.SetTile(center, TileType.Forest);

        // Assert
        Assert.That(grid.GetTile(center), Is.EqualTo(TileType.Forest));
        Assert.That(AllTiles(grid).Count(tile => tile == TileType.Forest), Is.EqualTo(1));
    }

    [TestCase(0, 0, true)]
    [TestCase(4, 2, true)]
    [TestCase(-1, 0, false)]
    [TestCase(0, -1, false)]
    [TestCase(5, 0, false)]
    [TestCase(0, 3, false)]
    public void IsInside_ForBorderPositions_RecognizesGridBounds(int x, int y, bool expected)
    {
        // Arrange
        var grid = new WorldGrid(5, 3);

        // Act
        bool inside = grid.IsInside(new GridPosition(x, y));

        // Assert
        Assert.That(inside, Is.EqualTo(expected));
    }

    [Test]
    public void GetTileAndSetTile_OutsideGrid_ThrowArgumentOutOfRange()
    {
        // Arrange
        var grid = new WorldGrid(2, 2);
        var outside = new GridPosition(2, 0);

        // Act
        TestDelegate read = () => grid.GetTile(outside);
        TestDelegate write = () => grid.SetTile(outside, TileType.Rock);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(read);
        Assert.Throws<ArgumentOutOfRangeException>(write);
    }

    [Test]
    public void IsWalkable_ForRockAndOutsidePositions_ReturnsFalse()
    {
        // Arrange
        var grid = new WorldGrid(3, 3);
        var rock = new GridPosition(1, 1);
        grid.SetTile(rock, TileType.Rock);

        // Act
        bool meadowWalkable = grid.IsWalkable(new GridPosition(0, 0));
        bool rockWalkable = grid.IsWalkable(rock);
        bool outsideWalkable = grid.IsWalkable(new GridPosition(-1, 0));

        // Assert
        Assert.That(meadowWalkable, Is.True);
        Assert.That(rockWalkable, Is.False);
        Assert.That(outsideWalkable, Is.False);
    }

    private static IEnumerable<TileType> AllTiles(WorldGrid grid)
    {
        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                yield return grid.GetTile(new GridPosition(x, y));
            }
        }
    }
}
