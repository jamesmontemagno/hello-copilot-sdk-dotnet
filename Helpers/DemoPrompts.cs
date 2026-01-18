namespace HelloCopilotSdk.Helpers;

/// <summary>
/// Contains demo prompts for showcasing Copilot capabilities.
/// </summary>
public static class DemoPrompts
{
    /// <summary>
    /// All available demo prompts with their titles and content.
    /// </summary>
    public static readonly (int Num, string Title, string Prompt)[] AllDemos =
    [
        (1, "Code Review", """
            Review this C# code and suggest improvements:
            ```csharp
            public class UserService {
                public User GetUser(int id) {
                    var db = new Database();
                    var user = db.Query("SELECT * FROM users WHERE id = " + id);
                    return user;
                }
            }
            ```
            """),
        (2, "Algorithm Help", "Explain how to implement a binary search tree in C# with insert, search, and delete operations. Include code examples."),
        (3, "Bug Finding", """
            Find the bugs in this code:
            ```csharp
            public async Task<List<string>> ProcessItemsAsync(List<int> items) {
                var results = new List<string>();
                foreach(var item in items) {
                    results.Add(await ProcessItem(item));
                }
                return results;
            }
            ```
            """),
        (4, "Design Pattern", "Explain the Repository pattern and show me how to implement it in C# for a Product entity with Entity Framework Core."),
        (5, "API Design", "Help me design a REST API for a todo list application. Include endpoints, HTTP methods, request/response bodies, and error handling."),
        (6, "Performance", "What are the best practices for improving performance in a .NET 8 web API? Give me specific, actionable tips with code examples."),
    ];

    /// <summary>
    /// Dictionary for quick lookup of demo prompts by number.
    /// </summary>
    private static readonly Dictionary<string, string> DemoLookup = AllDemos
        .ToDictionary(d => d.Num.ToString(), d => d.Prompt);

    /// <summary>
    /// Displays all available demo prompts to the console.
    /// </summary>
    public static void ShowDemoPrompts()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("                    📚 Demo Prompts (Code-Focused)             ");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.ResetColor();
        
        foreach (var (num, title, _) in AllDemos)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"   demo {num}: ");
            Console.ResetColor();
            Console.WriteLine(title);
        }
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("   Type 'demo <number>' to run any of these, e.g., 'demo 1'");
        Console.ResetColor();
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
    }

    /// <summary>
    /// Gets the demo prompt for the given input command.
    /// </summary>
    /// <param name="input">Input string like "demo 1"</param>
    /// <returns>The prompt text, or null if invalid.</returns>
    public static string? GetDemoPrompt(string input)
    {
        var parts = input.Split(' ', 2);
        if (parts.Length >= 2 && DemoLookup.TryGetValue(parts[1].Trim(), out var prompt))
        {
            return prompt;
        }
        return null;
    }
}
