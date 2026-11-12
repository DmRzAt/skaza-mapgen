using Skaza.MapGen;

namespace Skaza.MapGen.UnitTests;

public class SimRandomTests
{
    [Test]
    public void NextUInt_WithSeed42AndStream54_MatchesPcg32ReferenceOutput()
    {
        // Arrange
        var random = new SimRandom(42, 54);
        uint[] expected = { 0xa15c02b7u, 0x7b47f409u, 0xba1d3330u, 0x83d2f293u, 0xbfa4784bu, 0xcbed606eu };

        // Act
        var actual = new uint[expected.Length];
        for (int i = 0; i < actual.Length; i++)
        {
            actual[i] = random.NextUInt();
        }

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void NextUInt_WithSameSeed_ProducesSameSequence()
    {
        // Arrange
        var first = new SimRandom(12345);
        var second = new SimRandom(12345);

        // Act
        var firstValues = Enumerable.Range(0, 1000).Select(_ => first.NextUInt()).ToArray();
        var secondValues = Enumerable.Range(0, 1000).Select(_ => second.NextUInt()).ToArray();

        // Assert
        Assert.That(secondValues, Is.EqualTo(firstValues));
    }

    [Test]
    public void NextUInt_WithDifferentSeeds_ProducesDifferentSequences()
    {
        // Arrange
        var first = new SimRandom(1);
        var second = new SimRandom(2);

        // Act
        int equalValues = Enumerable.Range(0, 100).Count(_ => first.NextUInt() == second.NextUInt());

        // Assert
        Assert.That(equalValues, Is.LessThan(5));
    }

    [Test]
    public void NextUInt_WithSameSeedAndDifferentStreams_ProducesDifferentSequences()
    {
        // Arrange
        var map = new SimRandom(12345, 1);
        var animals = new SimRandom(12345, 2);

        // Act
        int equalValues = Enumerable.Range(0, 100).Count(_ => map.NextUInt() == animals.NextUInt());

        // Assert
        Assert.That(equalValues, Is.LessThan(5));
    }

    [Test]
    public void NextInt_ManyCalls_StaysInRangeAndCoversEveryValue()
    {
        // Arrange
        var random = new SimRandom(7);
        var seen = new bool[8];

        // Act
        var values = Enumerable.Range(0, 10000).Select(_ => random.NextInt(-3, 5)).ToArray();
        foreach (int value in values)
        {
            seen[value + 3] = true;
        }

        // Assert
        Assert.That(values, Has.All.InRange(-3, 4));
        Assert.That(seen, Has.All.True);
    }

    [Test]
    public void NextInt_WithEmptyRange_ThrowsArgumentOutOfRange()
    {
        // Arrange
        var random = new SimRandom(7);

        // Act
        TestDelegate act = () => random.NextInt(5, 5);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Test]
    public void NextFloat_ManyCalls_StaysBetweenZeroInclusiveAndOneExclusive()
    {
        // Arrange
        var random = new SimRandom(99);

        // Act
        var values = Enumerable.Range(0, 10000).Select(_ => random.NextFloat()).ToArray();

        // Assert
        Assert.That(values, Has.All.GreaterThanOrEqualTo(0f).And.All.LessThan(1f));
    }
}
