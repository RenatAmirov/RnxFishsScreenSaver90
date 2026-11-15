using System.Globalization;

namespace Aquarium90;

internal enum LaunchMode { Settings, FullScreen, Embedded, Window, Invalid }

internal readonly record struct LaunchRequest(LaunchMode Mode, nint Parent = 0)
{
    internal static LaunchRequest Parse(string[] args)
    {
        if (args.Length == 0) return new(LaunchMode.Settings);
        var parts = args[0].Split(':', 2);
        string? parameter = parts.Length == 2 ? parts[1] : args.ElementAtOrDefault(1);
        nint parent = long.TryParse(parameter, NumberStyles.None, CultureInfo.InvariantCulture, out var handle)
                      && handle > 0 ? (nint)handle : 0;
        return parts[0].ToLowerInvariant() switch
        {
            "/s" or "-s" => new(LaunchMode.FullScreen),
            "/c" or "-c" => new(LaunchMode.Settings, parent),
            "/p" or "-p" when parent != 0 => new(LaunchMode.Embedded, parent),
            "/window" or "-window" => new(LaunchMode.Window),
            _ => new(LaunchMode.Invalid)
        };
    }
}
