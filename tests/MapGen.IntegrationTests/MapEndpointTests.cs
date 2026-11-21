using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Skaza.MapGen.Api.Contracts;

namespace Skaza.MapGen.IntegrationTests;

public class MapEndpointTests : MapApiTestBase
{
    [Test]
    public async Task GetMap_WithRequestedSize_ReturnsMapOfThatSize()
    {
        // Arrange
        const string url = "/api/maps/42?width=64&height=48";

        // Act
        HttpResponseMessage response = await Client.GetAsync(url);
        MapResponse? map = await response.Content.ReadFromJsonAsync<MapResponse>();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(map, Is.Not.Null);
        Assert.That(map!.Seed, Is.EqualTo(42UL));
        Assert.That(map.Width, Is.EqualTo(64));
        Assert.That(map.Height, Is.EqualTo(48));
        Assert.That(map.Tiles, Has.Count.EqualTo(48));
        Assert.That(map.Tiles, Has.All.Length.EqualTo(64));
        Assert.That(map.Zones, Has.Count.EqualTo(48));
    }

    [Test]
    public async Task GetMap_WithoutSize_UsesDefaultSize()
    {
        // Arrange
        const string url = "/api/maps/7";

        // Act
        MapResponse? map = await Client.GetFromJsonAsync<MapResponse>(url);

        // Assert
        Assert.That(map!.Width, Is.EqualTo(MapSettings.DefaultSize));
        Assert.That(map.Height, Is.EqualTo(MapSettings.DefaultSize));
    }

    [Test]
    public async Task GetMap_SameSeedTwice_ReturnsIdenticalBody()
    {
        // Arrange
        const string url = "/api/maps/123456789?width=80&height=80";

        // Act
        string first = await Client.GetStringAsync(url);
        string second = await Client.GetStringAsync(url);

        // Assert
        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public async Task GetMap_DifferentSeeds_ReturnDifferentTiles()
    {
        // Arrange
        const string firstUrl = "/api/maps/1?width=64&height=64";
        const string secondUrl = "/api/maps/2?width=64&height=64";

        // Act
        MapResponse? first = await Client.GetFromJsonAsync<MapResponse>(firstUrl);
        MapResponse? second = await Client.GetFromJsonAsync<MapResponse>(secondUrl);

        // Assert
        Assert.That(second!.Tiles, Is.Not.EqualTo(first!.Tiles));
    }

    [Test]
    public async Task GetMap_Always_PlacesHeartInCentreAndShrinesOnWalkableTiles()
    {
        // Arrange
        const string url = "/api/maps/2024?width=96&height=64";

        // Act
        MapResponse? map = await Client.GetFromJsonAsync<MapResponse>(url);

        // Assert
        Assert.That(map!.Heart, Is.EqualTo(new PositionDto(48, 32)));
        Assert.That(map.Shrines, Has.Count.EqualTo(3));
        Assert.That(map.Shrines.Select(shrine => shrine.Zone), Is.Unique);
        foreach (ShrineDto shrine in map.Shrines)
        {
            Assert.That(map.Tiles[shrine.Y][shrine.X], Is.Not.EqualTo('r'));
        }
    }

    [Test]
    public async Task GetMap_WithLargestSeed_ReturnsOk()
    {
        // Arrange
        const string url = "/api/maps/18446744073709551615?width=32&height=32";

        // Act
        HttpResponseMessage response = await Client.GetAsync(url);
        MapResponse? map = await response.Content.ReadFromJsonAsync<MapResponse>();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(map!.Seed, Is.EqualTo(ulong.MaxValue));
    }

    [TestCase("width=31&height=64", "width")]
    [TestCase("width=257&height=64", "width")]
    [TestCase("width=64&height=0", "height")]
    [TestCase("width=64&height=1000", "height")]
    public async Task GetMap_WithSizeOutOfRange_ReturnsValidationProblemForThatField(string query, string field)
    {
        // Arrange
        string url = $"/api/maps/42?{query}";

        // Act
        HttpResponseMessage response = await Client.GetAsync(url);
        using JsonDocument problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
        Assert.That(problem.RootElement.GetProperty("errors").TryGetProperty(field, out _), Is.True);
    }

    [TestCase("abc")]
    [TestCase("-1")]
    [TestCase("18446744073709551616")]
    public async Task GetMap_WithSeedThatIsNotUnsignedNumber_ReturnsBadRequest(string seed)
    {
        // Arrange
        string url = $"/api/maps/{seed}";

        // Act
        HttpResponseMessage response = await Client.GetAsync(url);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
