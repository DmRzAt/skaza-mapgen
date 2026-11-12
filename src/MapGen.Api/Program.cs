using Skaza.MapGen;
using Skaza.MapGen.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WorldGenerator>();

var app = builder.Build();
app.MapMapEndpoints();

app.Run();
