using System.Buffers.Binary;
using System.Net;
using System.Text.Json;
using Skaza.MapGen.Api.Rendering;

namespace Skaza.MapGen.IntegrationTests;

public class MapImageEndpointTests : MapApiTestBase
{
    private static readonly byte[] PngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };

    [Test]
    public async Task GetMapImage_WithScale_ReturnsPngScaledToMapSize()
    {
        // Arrange
        const string url = "/api/maps/42/image?width=40&height=32&scale=3";

        // Act
        HttpResponseMessage response = await Client.GetAsync(url);
        byte[] png = await response.Content.ReadAsByteArrayAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("image/png"));
        Assert.That(png.Take(PngSignature.Length), Is.EqualTo(PngSignature));
        Assert.That(ReadPngWidth(png), Is.EqualTo(40 * 3));
        Assert.That(ReadPngHeight(png), Is.EqualTo(32 * 3));
    }

    [Test]
    public async Task GetMapImage_WithoutScale_UsesDefaultScale()
    {
        // Arrange
        const string url = "/api/maps/42/image?width=32&height=32";

        // Act
        byte[] png = await Client.GetByteArrayAsync(url);

        // Assert
        Assert.That(ReadPngWidth(png), Is.EqualTo(32 * MapImageRenderer.DefaultScale));
    }

    [TestCase(0)]
    [TestCase(17)]
    public async Task GetMapImage_WithScaleOutOfRange_ReturnsValidationProblemForScale(int scale)
    {
        // Arrange
        string url = $"/api/maps/42/image?scale={scale}";

        // Act
        HttpResponseMessage response = await Client.GetAsync(url);
        using JsonDocument problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(problem.RootElement.GetProperty("errors").TryGetProperty("scale", out _), Is.True);
    }

    private static int ReadPngWidth(byte[] png) => BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4));

    private static int ReadPngHeight(byte[] png) => BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4));
}
