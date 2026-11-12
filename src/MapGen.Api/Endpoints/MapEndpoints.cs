using Microsoft.AspNetCore.Http.HttpResults;
using Skaza.MapGen.Api.Contracts;
using Skaza.MapGen.Api.Mapping;
using Skaza.MapGen.Api.Rendering;
using Skaza.MapGen.Api.Validation;

namespace Skaza.MapGen.Api.Endpoints;

public static class MapEndpoints
{
    public static IEndpointRouteBuilder MapMapEndpoints(this IEndpointRouteBuilder app)
    {
        var maps = app.MapGroup("/api/maps").WithTags("Maps");

        maps.MapGet("/{seed}", GetMap)
            .WithName("GetMap")
            .WithSummary("Generates a map for the given seed")
            .WithDescription("Returns tiles and zones as rows of symbols, the Heart position and three shrines. Width and height must be between 32 and 256 (default 96).");

        maps.MapGet("/{seed}/image", GetMapImage)
            .WithName("GetMapImage")
            .WithSummary("Renders the map as a PNG image")
            .WithDescription("Scale is the number of pixels per tile, between 1 and 16 (default 4).");

        maps.MapGet("/{seed}/stats", GetMapStatistics)
            .WithName("GetMapStatistics")
            .WithSummary("Returns tile and zone statistics of the map");

        return app;
    }

    private static Results<Ok<MapStatisticsResponse>, ValidationProblem> GetMapStatistics(ulong seed, int? width, int? height, WorldGenerator generator)
    {
        if (!MapRequestValidator.TryCreateSettings(width, height, out MapSettings settings, out var errors))
        {
            return TypedResults.ValidationProblem(errors);
        }

        GeneratedMap map = generator.Generate(seed, settings);
        return TypedResults.Ok(MapResponseMapper.ToStatisticsResponse(map));
    }

    private static Results<Ok<MapResponse>, ValidationProblem> GetMap(ulong seed, int? width, int? height, WorldGenerator generator)
    {
        if (!MapRequestValidator.TryCreateSettings(width, height, out MapSettings settings, out var errors))
        {
            return TypedResults.ValidationProblem(errors);
        }

        GeneratedMap map = generator.Generate(seed, settings);
        return TypedResults.Ok(MapResponseMapper.ToResponse(map));
    }

    private static Results<FileContentHttpResult, ValidationProblem> GetMapImage(ulong seed, int? width, int? height, int? scale, WorldGenerator generator)
    {
        bool sizeValid = MapRequestValidator.TryCreateSettings(width, height, out MapSettings settings, out var errors);
        bool scaleValid = MapRequestValidator.TryValidateScale(scale, out int actualScale, errors);
        if (!sizeValid || !scaleValid)
        {
            return TypedResults.ValidationProblem(errors);
        }

        GeneratedMap map = generator.Generate(seed, settings);
        byte[] png = MapImageRenderer.RenderPng(map, actualScale);
        return TypedResults.File(png, "image/png");
    }
}
