using GitHub.Copilot.SDK;
using HelloCopilotSdk.Helpers;

// ════════════════════════════════════════════════════════════════════════════
// Program.cs - Main entry point for CLI chat demo
// See Helpers/ folder for: CliChecker, ModelSelector, DemoPrompts, ChatHelper
// ════════════════════════════════════════════════════════════════════════════

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║          GitHub Copilot SDK - Interactive Demo               ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
Console.ResetColor();
Console.WriteLine();

// ============================================================
// Step 1: Check if Copilot CLI is installed and user is authenticated
// ============================================================
Console.WriteLine("🔍 Checking prerequisites...\n");

var copilotStatus = await CliChecker.CheckCopilotStatusAsync();

if (!CliChecker.IsReady(copilotStatus))
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Press any key to exit...");
    Console.ResetColor();
    Console.ReadKey(true);
    return;
}

// ============================================================
// Step 2: Model Selection
// ============================================================
var selectedModel = await ModelSelector.SelectModelAsync();
if (selectedModel == null)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Press any key to exit...");
    Console.ResetColor();
    Console.ReadKey(true);
    return;
}
Console.WriteLine();

// ============================================================
// Step 3: Start Copilot Client
// ============================================================
Console.WriteLine("🚀 Starting Copilot client...");

CopilotClient? client = null;
CopilotSession? session = null;

try
{
    client = new CopilotClient();
    await client.StartAsync();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("✅ Copilot client started successfully!\n");
    Console.ResetColor();

    // Ping to verify connection
    var pingResponse = await client.PingAsync("hello");
    Console.WriteLine($"📡 Server connection verified (ping response received)\n");

    // Create session with selected model
    Console.WriteLine($"📝 Creating session with model: {selectedModel}...");
    session = await client.CreateSessionAsync(new SessionConfig
    {
        Model = selectedModel,
        Streaming = true // Enable streaming for real-time output
    });
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"✅ Session created! (ID: {session.SessionId})\n");
    Console.ResetColor();

    // Show demo prompts
    DemoPrompts.ShowDemoPrompts();

    // ============================================================
    // Step 4: Interactive Chat Loop
    // ============================================================
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("💬 Interactive Chat Mode");
    Console.WriteLine("   Type your message and press Enter to send.");
    Console.WriteLine("   Type 'exit' or 'quit' to end the session.");
    Console.WriteLine("   Type 'demo <number>' to run a demo prompt (e.g., 'demo 1')");
    Console.WriteLine("   Type 'model' to change the model.");
    Console.WriteLine("   Type 'clear' to start a new session.\n");
    Console.ResetColor();

    while (true)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("You: ");
        Console.ResetColor();
        
        var input = Console.ReadLine()?.Trim();
        
        if (string.IsNullOrEmpty(input))
            continue;

        // Handle special commands
        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) || 
            input.Equals("quit", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("\n👋 Goodbye!");
            break;
        }

        if (input.Equals("model", StringComparison.OrdinalIgnoreCase))
        {
            var newModel = await ModelSelector.SelectModelAsync();
            if (newModel == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not fetch models. Keeping current model.\n");
                Console.ResetColor();
                continue;
            }
            selectedModel = newModel;
            Console.WriteLine($"\n🔄 Recreating session with model: {selectedModel}...");
            await session.DisposeAsync();
            session = await client.CreateSessionAsync(new SessionConfig
            {
                Model = selectedModel,
                Streaming = true
            });
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ New session created! (ID: {session.SessionId})\n");
            Console.ResetColor();
            continue;
        }

        if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"\n🔄 Starting new session...");
            await session.DisposeAsync();
            session = await client.CreateSessionAsync(new SessionConfig
            {
                Model = selectedModel,
                Streaming = true
            });
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ New session created! (ID: {session.SessionId})\n");
            Console.ResetColor();
            continue;
        }

        // Handle demo commands
        if (input.StartsWith("demo ", StringComparison.OrdinalIgnoreCase))
        {
            var demoPrompt = DemoPrompts.GetDemoPrompt(input);
            if (demoPrompt != null)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"Running: {demoPrompt}\n");
                Console.ResetColor();
                input = demoPrompt;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid demo number. Use 'demo 1' through 'demo 6'.\n");
                Console.ResetColor();
                continue;
            }
        }

        // Send message and stream response
        await ChatHelper.SendMessageAndStreamResponse(session, input);
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n❌ Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"   Inner: {ex.InnerException.Message}");
    }
    Console.ResetColor();
}
finally
{
    if (session != null)
    {
        await session.DisposeAsync();
    }
    if (client != null)
    {
        await client.DisposeAsync();
    }
    Console.WriteLine("\n🛑 Client stopped.");
}