using Microsoft.AspNetCore.Mvc.Testing;

namespace Skaza.MapGen.IntegrationTests;

public abstract class MapApiTestBase
{
    private WebApplicationFactory<Program> _factory = null!;

    protected HttpClient Client { get; private set; } = null!;

    [OneTimeSetUp]
    public void StartApi()
    {
        _factory = new WebApplicationFactory<Program>();
        Client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void StopApi()
    {
        Client.Dispose();
        _factory.Dispose();
    }
}
