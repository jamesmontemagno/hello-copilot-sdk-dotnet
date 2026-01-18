using System.Diagnostics;

namespace HelloCopilotSdk.Helpers;

/// <summary>
/// Checks if the Copilot CLI is installed and the user is authenticated.
/// Based on: https://github.com/jamesmontemagno/podcast-metadata-generator
/// </summary>
public static class CliChecker
{
    /// <summary>
    /// Result of the Copilot readiness check.
    /// </summary>
    public record CopilotStatus(
        bool IsInstalled,
        bool IsTokenSet,
        bool IsAuthenticated,
        string? ErrorMessage);

    /// <summary>
    /// Verifies Copilot CLI installation and authentication status.
    /// </summary>
    /// <returns>CopilotStatus with detailed information.</returns>
    public static async Task<CopilotStatus> CheckCopilotStatusAsync()
    {
        Console.Write("   Checking for Copilot CLI... ");
        
        // Check GH_TOKEN environment variable
        var ghToken = Environment.GetEnvironmentVariable("GH_TOKEN");
        var isTokenSet = !string.IsNullOrEmpty(ghToken);
        
        // Check if copilot CLI is installed
        var isInstalled = await CheckCopilotInstalledAsync();
        
        if (!isInstalled)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Not found!");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("   Please install Copilot CLI:");
            Console.WriteLine("   macOS/Linux: brew install copilot-cli");
            Console.WriteLine("   Windows:     winget install GitHub.Copilot");
            Console.WriteLine("   npm:         npm install -g @github/copilot");
            Console.WriteLine("   Script:      curl -fsSL https://gh.io/copilot-install | bash");
            
            return new CopilotStatus(
                IsInstalled: false,
                IsTokenSet: isTokenSet,
                IsAuthenticated: false,
                ErrorMessage: "Copilot CLI is not installed.");
        }
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ Installed");
        Console.ResetColor();
        
        // Check GH_TOKEN
        Console.Write("   Checking GH_TOKEN environment variable... ");
        if (isTokenSet)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Set");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("○ Not set (will use interactive auth)");
            Console.ResetColor();
        }
        
        // Check authentication status
        Console.Write("   Checking authentication... ");
        var authResult = await CheckCopilotAuthAsync();
        
        if (authResult.isAuthenticated)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Authenticated");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Not authenticated");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("   Please authenticate with Copilot:");
            Console.WriteLine("   Run: copilot");
            Console.WriteLine("   Then type: /login");
            Console.WriteLine("   Or set GH_TOKEN environment variable with a token that has 'Copilot Requests' permission.");
        }
        
        Console.WriteLine();
        
        return new CopilotStatus(
            IsInstalled: true,
            IsTokenSet: isTokenSet,
            IsAuthenticated: authResult.isAuthenticated,
            ErrorMessage: authResult.error);
    }

    /// <summary>
    /// Quick check if Copilot is ready (installed + authenticated or has token).
    /// </summary>
    public static bool IsReady(CopilotStatus status)
    {
        return status.IsInstalled && (status.IsTokenSet || status.IsAuthenticated);
    }

    private static async Task<bool> CheckCopilotInstalledAsync()
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "copilot",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            process.Start();
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<(bool isAuthenticated, string? error)> CheckCopilotAuthAsync()
    {
        // If GH_TOKEN is set, assume authenticated
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GH_TOKEN")))
        {
            return (true, null);
        }
        
        try
        {
            // Try running a minimal copilot command to check auth
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
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            // Check for authentication errors in stderr
            if (stderr.Contains("not authenticated", StringComparison.OrdinalIgnoreCase) ||
                stderr.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) ||
                stderr.Contains("login required", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Not authenticated. Run 'copilot' and type '/login', or set GH_TOKEN environment variable.");
            }
            
            // If --help works without auth errors, consider it ready
            return process.ExitCode == 0 ? (true, null) : (false, "Copilot CLI returned an error.");
        }
        catch (Exception ex)
        {
            return (false, $"Failed to check authentication: {ex.Message}");
        }
    }
}
