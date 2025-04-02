using System.Text.Json;

namespace TeamGeneration;

public class NameGenerator
{
    private readonly List<string> _prefixes;
    private readonly List<string> _suffixes;
    private readonly Random _random;

    private NameGenerator(string jsonFilePath)
    {
        _random = new Random();
        var jsonData = LoadJsonData(jsonFilePath);
        _prefixes = jsonData.Prefixes;
        _suffixes = jsonData.Suffixes;
    }

    public static NameGenerator CreateFromJson(string jsonFilePath)
    {
        return new NameGenerator(jsonFilePath);
    }

    public string GenerateTeamName()
    {
        string prefix = _prefixes[_random.Next(_prefixes.Count)];
        string suffix = _suffixes[_random.Next(_suffixes.Count)];
        return $"{prefix} {suffix}";
    }

    private TeamNameData LoadJsonData(string jsonFilePath)
    {
        try
        {
            var jsonString = File.ReadAllText(jsonFilePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<TeamNameData>(jsonString, options);
            
            if (data == null || data.Prefixes == null || data.Suffixes == null || 
                data.Prefixes.Count == 0 || data.Suffixes.Count == 0)
            {
                throw new InvalidDataException("JSON file must contain non-empty 'prefixes' and 'suffixes' arrays.");
            }
            
            return data;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load or parse JSON file: {ex.Message}", ex);
        }
    }

    private class TeamNameData
    {
        public required List<string> Prefixes { get; init; }
        public required List<string> Suffixes { get; init; }
    }
}