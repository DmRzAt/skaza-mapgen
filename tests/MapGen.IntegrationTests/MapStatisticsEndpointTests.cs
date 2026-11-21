using System.Net;
using System.Net.Http.Json;
using Skaza.MapGen.Api.Contracts;

namespace Skaza.MapGen.IntegrationTests;

public class MapStatisticsEndpointTests : MapApiTestBase
{
    [Test]
    public async Task GetMapStatistics_ForRequestedSize_CountsEveryTileOnce()
    {
        // Arrange
        const string url = "/api/maps/42/stats?width=50&height=40";

        // Act
        HttpResponseMessage response = await Client.GetAsync(url);
        MapStatisticsResponse? statistics = await response.Content.ReadFromJsonAsync<MapStatisticsResponse>();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(statistics!.TotalTiles, Is.EqualTo(50 * 40));
        Assert.That(statistics.TileCounts.Values.Sum(), Is.EqualTo(statistics.TotalTiles));
        Assert.That(statistics.ZoneCounts.Values.Sum(), Is.EqualTo(statistics.TotalTiles));
        Assert.That(statistics.ShrineCount, Is.EqualTo(3));
    }

    [Test]
    public async Task GetMapStatistics_ForSameSeed_AgreesWithMapEndpoint()
    {
        // Arrange
        const string query = "width=72&height=72";

        // Act
        MapResponse? map = await Client.GetFromJsonAsync<MapResponse>($"/api/maps/99?{query}");
        MapStatisticsResponse? statistics = await Client.GetFromJsonAsync<MapStatisticsResponse>($"/api/maps/99/stats?{query}");

        // Assert
        int rocksOnMap = map!.Tiles.Sum(row => row.Count(symbol => symbol == 'r'));
        Assert.That(statistics!.TileCounts["Rock"], Is.EqualTo(rocksOnMap));
        Assert.That(statistics.WalkableTiles, Is.EqualTo(statistics.TotalTiles - rocksOnMap));
    }

    [Test]
    public async Task GetMapStatistics_WithSizeOutOfRange_ReturnsBadRequest()
    {
        // Arrange
        const string url = "/api/maps/42/stats?width=300";

        // Act
        HttpResponseMessage response = await Client.GetAsync(url);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
