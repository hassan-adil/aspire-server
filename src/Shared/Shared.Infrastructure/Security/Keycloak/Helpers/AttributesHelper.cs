namespace Shared.Infrastructure.Security.Keycloak.Helpers;

public static class AttributesHelper
{
    public static T ReadScalar<T>(
    this Dictionary<string, List<string>>? attributes,
    string key,
    T defaultValue = default!)
    where T : struct
    {
        if (attributes?.TryGetValue(key, out var values) == true &&
            values.Count > 0 &&
            TryConvert(values[0], out T result))
        {
            return result;
        }

        return defaultValue;
    }

    public static string? ReadString(
    this Dictionary<string, List<string>>? attributes,
    string key,
    string? defaultValue = null)
    {
        if (attributes?.TryGetValue(key, out var values) == true &&
            values.Count > 0)
        {
            return values[0];
        }

        return defaultValue;
    }

    public static IReadOnlyList<T> ReadList<T>(
    this Dictionary<string, List<string>>? attributes,
    string key,
    IReadOnlyList<T>? defaultValue = null)
    {
        if (attributes?.TryGetValue(key, out var values) == true &&
            values.Count > 0)
        {
            var result = new List<T>(values.Count);

            foreach (var value in values)
            {
                if (TryConvert(value, out T converted))
                {
                    result.Add(converted);
                }
            }

            return result;
        }

        return defaultValue ?? [];
    }

    private static bool TryConvert<T>(string value, out T result)
    {
        result = default!;

        var targetType = typeof(T);

        if (targetType.IsEnum)
        {
            if (Enum.TryParse(targetType, value, true, out var enumValue))
            {
                result = (T)enumValue!;
                return true;
            }
            return false;
        }

        if (targetType == typeof(Guid))
        {
            if (Guid.TryParse(value, out var guid))
            {
                result = (T)(object)guid;
                return true;
            }
            return false;
        }

        try
        {
            result = (T)Convert.ChangeType(value, targetType);
            return true;
        }
        catch
        {
            return false;
        }
    }

}
