using System.Diagnostics;
using System.Text.RegularExpressions;

namespace HelloCopilotSdk.Helpers;

/// <summary>
/// Handles model selection for the Copilot session.
/// Fetches available models directly from the Copilot CLI.
/// Based on: https://github.com/jamesmontemagno/podcast-metadata-generator
/// </summary>
public static partial class ModelSelector
{
    [GeneratedRegex("\"([^\"]+)\"")]
    private static partial Regex QuotedModelRegex();

    /// <summary>
    /// Fetches the list of available models from the Copilot CLI.
    /// Returns null if the CLI is unavailable or models cannot be parsed.
    /// </summary>
    public static async Task<string[]?> GetModelsFromCliAsync()
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "copilot",
                Arguments = "--help",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            if (process.ExitCode != 0)
                return null;
            
            // Find the --model section and extract choices
            // The choices can span multiple lines, so we need to find the section
            var modelIndex = output.IndexOf("--model", StringComparison.OrdinalIgnoreCase);
            if (modelIndex < 0)
                return null;
            
            var choicesIndex = output.IndexOf("choices:", modelIndex, StringComparison.OrdinalIgnoreCase);
            if (choicesIndex < 0)
                return null;
            
            // Find the end of the choices (next -- option or end of section)
            var endIndex = output.IndexOf("\n  --", choicesIndex + 1);
            if (endIndex < 0)
                endIndex = output.Length;
            
            var choicesSection = output[choicesIndex..endIndex];
            
            // Extract all quoted model names using regex
            var matches = QuotedModelRegex().Matches(choicesSection);
            var models = matches.Select(m => m.Groups[1].Value).ToArray();
            
            return models.Length > 0 ? models : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Prompts the user to select a model from available options.
    /// </summary>
    /// <returns>The selected model ID, or null if no models available.</returns>
    public static async Task<string?> SelectModelAsync()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("   Fetching available models from Copilot CLI...");
        Console.ResetColor();
        
        var models = await GetModelsFromCliAsync();
        
        if (models == null || models.Length == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Could not fetch models from Copilot CLI.");
            Console.WriteLine("   Make sure 'copilot' is installed and working.");
            Console.ResetColor();
            return null;
        }
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🤖 Select a model:");
        Console.ResetColor();
        
        for (int i = 0; i < models.Length; i++)
        {
            Console.WriteLine($"   {i + 1}. {models[i]}");
        }
        
        Console.Write($"\nEnter choice (1-{models.Length}) [default: 1]: ");
        var choice = Console.ReadLine()?.Trim();
        
        if (string.IsNullOrEmpty(choice) || !int.TryParse(choice, out int index) || index < 1 || index > models.Length)
        {
            index = 1;
        }

        var selected = models[index - 1];
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ Selected: {selected}");
        Console.ResetColor();
        
        return selected;
    }
}
