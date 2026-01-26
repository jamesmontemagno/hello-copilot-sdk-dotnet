using GitHub.Copilot.SDK;

namespace HelloCopilotSdk.Helpers;

/// <summary>
/// Handles model selection for the Copilot session.
/// Fetches available models from the Copilot SDK.
/// Based on: https://github.com/jamesmontemagno/podcast-metadata-generator
/// </summary>
public static class ModelSelector
{

    /// <summary>
    /// Fetches the list of available models from the Copilot SDK.
    /// Returns null if models cannot be fetched.
    /// </summary>
    public static async Task<List<ModelInfo>?> GetModelsFromSdkAsync()
    {
        try
        {
            using var client = new CopilotClient();
            await client.StartAsync();
            
            var models = await client.ListModelsAsync();
            
            await client.StopAsync();
            
            return models;
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
        Console.WriteLine("   Fetching available models from Copilot SDK...");
        Console.ResetColor();
        
        var models = await GetModelsFromSdkAsync();
        
        if (models == null || models.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Could not fetch models from Copilot SDK.");
            Console.WriteLine("   Make sure 'copilot' is installed and working.");
            Console.ResetColor();
            return null;
        }
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🤖 Select a model:");
        Console.ResetColor();
        
        for (int i = 0; i < models.Count; i++)
        {
            var model = models[i];
            var multiplierText = model.Billing?.Multiplier != null 
                ? $" (multiplier: {model.Billing.Multiplier}x)" 
                : "";
            Console.WriteLine($"   {i + 1}. {model.Name}{multiplierText}");
        }
        
        Console.Write($"\nEnter choice (1-{models.Count}) [default: 1]: ");
        var choice = Console.ReadLine()?.Trim();
        
        if (string.IsNullOrEmpty(choice) || !int.TryParse(choice, out int index) || index < 1 || index > models.Count)
        {
            index = 1;
        }

        var selected = models[index - 1];
        var selectedMultiplierText = selected.Billing?.Multiplier != null 
            ? $" (multiplier: {selected.Billing.Multiplier}x)" 
            : "";
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ Selected: {selected.Name}{selectedMultiplierText}");
        Console.ResetColor();
        
        return selected.Id;
    }
}
