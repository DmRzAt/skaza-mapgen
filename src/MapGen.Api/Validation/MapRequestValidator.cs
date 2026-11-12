using Skaza.MapGen.Api.Rendering;

namespace Skaza.MapGen.Api.Validation;

public static class MapRequestValidator
{
    public static bool TryCreateSettings(int? width, int? height, out MapSettings settings, out Dictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>();
        int actualWidth = width ?? MapSettings.DefaultSize;
        int actualHeight = height ?? MapSettings.DefaultSize;

        AddRangeError(errors, "width", actualWidth, MapSettings.MinSize, MapSettings.MaxSize);
        AddRangeError(errors, "height", actualHeight, MapSettings.MinSize, MapSettings.MaxSize);

        settings = errors.Count == 0 ? new MapSettings(actualWidth, actualHeight) : MapSettings.Default;
        return errors.Count == 0;
    }

    public static bool TryValidateScale(int? scale, out int actualScale, Dictionary<string, string[]> errors)
    {
        actualScale = scale ?? MapImageRenderer.DefaultScale;
        return AddRangeError(errors, "scale", actualScale, MapImageRenderer.MinScale, MapImageRenderer.MaxScale);
    }

    private static bool AddRangeError(Dictionary<string, string[]> errors, string name, int value, int min, int max)
    {
        if (value >= min && value <= max)
        {
            return true;
        }

        errors[name] = new[] { $"{name} must be between {min} and {max}, got {value}." };
        return false;
    }
}
