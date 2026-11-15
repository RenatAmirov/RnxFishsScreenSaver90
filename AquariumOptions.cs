using System.Text.Json;

namespace Aquarium90;

internal sealed record AquariumOptions(int FishCount = 14, int Speed = 100, bool Bubbles = true)
{
    private static readonly string DirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Aquarium90");
    private static readonly string FilePath = Path.Combine(DirectoryPath, "settings.json");

    internal static AquariumOptions Load()
    {
        try
        {
            if (JsonSerializer.Deserialize<AquariumOptions>(File.ReadAllText(FilePath)) is { } options)
                return options with { FishCount = Math.Clamp(options.FishCount, 6, 28), Speed = Math.Clamp(options.Speed, 40, 160) };
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException) { }
        return new();
    }

    internal void Save()
    {
        Directory.CreateDirectory(DirectoryPath);
        string temp = Path.Combine(DirectoryPath, $"{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(temp, JsonSerializer.Serialize(this));
            File.Move(temp, FilePath, true);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
}
