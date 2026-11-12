using Skaza.MapGen;
using Skaza.MapGen.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WorldGenerator>();
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapOpenApi();
app.MapMapEndpoints();

app.Run();
