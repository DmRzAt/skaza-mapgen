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
        var maps = app.MapGroup("/api/maps");
        maps.MapGet("/{seed}", GetMap);
        maps.MapGet("/{seed}/image", GetMapImage);
        return app;
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
