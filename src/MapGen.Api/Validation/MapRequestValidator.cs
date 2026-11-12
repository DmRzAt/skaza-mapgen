namespace Skaza.MapGen.Api.Validation;

public static class MapRequestValidator
{
    public static bool TryCreateSettings(int? width, int? height, out MapSettings settings, out Dictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>();
        int actualWidth = width ?? MapSettings.DefaultSize;
        int actualHeight = height ?? MapSettings.DefaultSize;

        AddSizeError(errors, "width", actualWidth);
        AddSizeError(errors, "height", actualHeight);

        settings = errors.Count == 0 ? new MapSettings(actualWidth, actualHeight) : MapSettings.Default;
        return errors.Count == 0;
    }

    private static void AddSizeError(Dictionary<string, string[]> errors, string name, int value)
    {
        if (value < MapSettings.MinSize || value > MapSettings.MaxSize)
        {
            errors[name] = new[] { $"{name} must be between {MapSettings.MinSize} and {MapSettings.MaxSize}, got {value}." };
        }
    }
}
