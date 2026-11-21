using System.Net;
using System.Text.Json;

namespace Skaza.MapGen.IntegrationTests;

public class DocumentationAndPreviewTests : MapApiTestBase
{
    [Test]
    public async Task OpenApiDocument_Always_DescribesAllMapEndpoints()
    {
        // Arrange
        const string url = "/openapi/v1.json";

        // Act
        HttpResponseMessage response = await Client.GetAsync(url);
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        IEnumerable<string> paths = document.RootElement.GetProperty("paths").EnumerateObject().Select(path => path.Name);
        Assert.That(paths, Is.SupersetOf(new[] { "/api/maps/{seed}", "/api/maps/{seed}/image", "/api/maps/{seed}/stats" }));
    }

    [Test]
    public async Task RootPath_Always_ServesPreviewPage()
    {
        // Arrange
        const string url = "/";

        // Act
        HttpResponseMessage response = await Client.GetAsync(url);
        string html = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/html"));
        Assert.That(html, Does.Contain("id=\"map-form\""));
        Assert.That(html, Does.Contain("app.js"));
    }

    [Test]
    public async Task PreviewScript_Always_IsServedAsJavaScript()
    {
        // Arrange
        const string url = "/app.js";

        // Act
        HttpResponseMessage response = await Client.GetAsync(url);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/javascript"));
    }
}
