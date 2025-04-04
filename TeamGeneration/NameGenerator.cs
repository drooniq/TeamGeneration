using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeamGeneration;

public class NameGenerator
{
    private readonly List<string> _prefixes;
    private readonly List<string> _suffixes;
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the NameGenerator class with specified prefixes and suffixes.
    /// </summary>
    /// <param name="prefixes">The list of prefix strings to use in name generation.</param>
    /// <param name="suffixes">The list of suffix strings to use in name generation.</param>
    private NameGenerator(List<string> prefixes, List<string> suffixes)
    {
        _prefixes = prefixes;
        _suffixes = suffixes;
        _random = new Random();
    }

    /// <summary>
    /// Creates a NameGenerator instance from a JSON file containing prefix and suffix data.
    /// </summary>
    /// <param name="jsonFilePath">The path to the JSON file containing team name data.</param>
    /// <returns>A new NameGenerator instance initialized with data from the JSON file.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the JSON file cannot be loaded or parsed.</exception>
    public static NameGenerator CreateFromJson(string jsonFilePath)
    {
        var teamNameData = LoadJsonData(jsonFilePath);
        return new NameGenerator(teamNameData.Prefixes, teamNameData.Suffixes);
    }

    /// <summary>
    /// Generates a random team name by combining a prefix and suffix.
    /// </summary>
    /// <returns>A string representing a randomly generated team name.</returns>
    public string GenerateTeamName()
    {
        string prefix = _prefixes[_random.Next(_prefixes.Count)];
        string suffix = _suffixes[_random.Next(_suffixes.Count)];
        return $"{prefix} {suffix}";
    }

    /// <summary>
    /// Loads team name data from a JSON file.
    /// </summary>
    /// <param name="jsonFilePath">The path to the JSON file to load.</param>
    /// <returns>A TeamNameData object containing the deserialized prefix and suffix lists.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the JSON file cannot be loaded, parsed, or contains invalid data.</exception>
    /// <exception cref="InvalidDataException">Thrown when the JSON data is missing required arrays or they are empty.</exception>
    private static TeamNameData LoadJsonData(string jsonFilePath)
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

    /// <summary>
    /// Represents the structure of team name data loaded from JSON.
    /// </summary>
    private class TeamNameData
    {
        /// <summary>
        /// Gets the list of prefixes for team name generation.
        /// </summary>
        [JsonPropertyName("Prefixes")]
        public required List<string> Prefixes { get; init; }
        
        /// <summary>
        /// Gets the list of suffixes for team name generation.
        /// </summary>
        [JsonPropertyName("Suffixes")]
        public required List<string> Suffixes { get; init; }
    }
}