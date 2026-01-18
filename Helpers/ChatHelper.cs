using GitHub.Copilot.SDK;

namespace HelloCopilotSdk.Helpers;

/// <summary>
/// Helper methods for chat interactions with Copilot.
/// </summary>
public static class ChatHelper
{
    /// <summary>
    /// Sends a message and streams the response to the console.
    /// </summary>
    /// <param name="session">The Copilot session.</param>
    /// <param name="message">The message to send.</param>
    public static async Task SendMessageAndStreamResponse(CopilotSession session, string message)
    {
        var done = new TaskCompletionSource();
        var hasStartedResponse = false;

        var subscription = session.On(evt =>
        {
            switch (evt)
            {
                case AssistantMessageDeltaEvent delta:
                    if (!hasStartedResponse)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("\nCopilot: ");
                        Console.ResetColor();
                        hasStartedResponse = true;
                    }
                    Console.Write(delta.Data.DeltaContent);
                    break;
                    
                case AssistantMessageEvent msg:
                    if (!hasStartedResponse)
                    {
                        // Non-streaming fallback
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("\nCopilot: ");
                        Console.ResetColor();
                        Console.Write(msg.Data.Content);
                    }
                    break;
                    
                case SessionIdleEvent:
                    done.TrySetResult();
                    break;
                    
                case SessionErrorEvent err:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n❌ Error: {err.Data.Message}");
                    Console.ResetColor();
                    done.TrySetResult();
                    break;
            }
        });

        try
        {
            await session.SendAsync(new MessageOptions { Prompt = message });
            await done.Task;
            Console.WriteLine("\n");
        }
        finally
        {
            subscription.Dispose();
        }
    }
}
