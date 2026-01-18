# hello-copilot-sdk-dotnet

Interactive console demo for the **GitHub Copilot SDK for .NET** (`GitHub.Copilot.SDK`). It starts a Copilot client, lets you pick a model, and streams responses in an interactive chat loop.

## What’s in here

- Interactive chat in the terminal with streaming output
- Model picker (dynamically fetched from Copilot CLI)
- Built-in demo prompts (`demo 1` … `demo 6`)
- Prerequisite checks for Copilot CLI and authentication

## Prerequisites

- **.NET SDK** that supports `net10.0`
- **GitHub Copilot CLI**: Install via one of:
  ```bash
  # macOS/Linux (Homebrew)
  brew install copilot-cli
  
  # Windows (WinGet)
  winget install GitHub.Copilot
  
  # npm (requires Node.js 22+)
  npm install -g @github/copilot
  
  # macOS/Linux (install script)
  curl -fsSL https://gh.io/copilot-install | bash
  ```
- You need access to **GitHub Copilot** on your GitHub account/org
- Authenticate via one of:
  - Run `copilot`, then type `/login` and follow prompts
  - Set `GH_TOKEN` environment variable with a token that has "Copilot Requests" permission

## Project Structure

```
hello-copilot-sdk-dotnet/
├── Program.cs                    # Main flow - chat loop
└── Helpers/
    ├── CliChecker.cs            # Copilot CLI & auth checks
    ├── ModelSelector.cs         # Model selection (fetches from CLI)
    ├── DemoPrompts.cs           # Demo prompts storage & display
    └── ChatHelper.cs            # Message streaming logic
```

## Run

From the repo root:

```bash
dotnet restore
dotnet run
```

You'll see a status check, then be prompted to select a model. Then:

- Type messages normally to chat
- Type `demo 1` (or any number shown) to run a demo prompt
- Type `model` to switch models (creates a new session)
- Type `clear` to start a fresh session
- Type `exit` / `quit` to stop

## Troubleshooting

- **Copilot CLI not found**: Install using one of the methods above
- **Not authenticated**: Run `copilot`, type `/login`, and follow the browser prompts
- **Using GH_TOKEN**: Set the environment variable with a fine-grained PAT that has "Copilot Requests" permission
- **net10.0 fails to build**: Install a newer .NET SDK or change the target framework in the project file

## Repo metadata

Project metadata lives in [hello-copilot-sdk-dotnet.csproj](hello-copilot-sdk-dotnet.csproj). If you plan to publish broadly, update:

- `RepositoryUrl`
- `Authors`
- `Description`

## License

Choose a license (MIT / Apache-2.0 / etc.) and add a `LICENSE` file before making the repo public.
