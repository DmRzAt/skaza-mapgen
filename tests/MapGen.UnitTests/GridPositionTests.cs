using Skaza.MapGen;

namespace Skaza.MapGen.UnitTests;

public class GridPositionTests
{
    [Test]
    public void Equality_ForSameCoordinates_IsTrueAndHashCodesMatch()
    {
        // Arrange
        var first = new GridPosition(3, 7);
        var second = new GridPosition(3, 7);

        // Act
        bool equalByOperator = first == second;

        // Assert
        Assert.That(equalByOperator, Is.True);
        Assert.That(second, Is.EqualTo(first));
        Assert.That(second.GetHashCode(), Is.EqualTo(first.GetHashCode()));
    }

    [Test]
    public void Equality_ForSwappedCoordinates_IsFalse()
    {
        // Arrange
        var position = new GridPosition(3, 7);
        var swapped = new GridPosition(7, 3);

        // Act
        bool differentByOperator = position != swapped;

        // Assert
        Assert.That(differentByOperator, Is.True);
        Assert.That(swapped, Is.Not.EqualTo(position));
    }

    [Test]
    public void HashSetContains_ForEqualPosition_FindsIt()
    {
        // Arrange
        var visited = new HashSet<GridPosition> { new GridPosition(3, 7) };

        // Act
        bool containsEqual = visited.Contains(new GridPosition(3, 7));
        bool containsSwapped = visited.Contains(new GridPosition(7, 3));

        // Assert
        Assert.That(containsEqual, Is.True);
        Assert.That(containsSwapped, Is.False);
    }
}
