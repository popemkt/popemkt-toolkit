# ProcRespawn

A robust process management tool that automatically respawns configured processes when they stop or crash, ensuring your critical applications stay running.

## Overview

ProcRespawn is designed for users who need reliable process persistence, particularly for GUI applications that need to run continuously. It monitors and automatically restarts specified processes using a configuration-driven approach.

## Features

- Automatic process respawning
- Platform-specific configurations (Windows/Linux)
- GUI application support
- Configuration-driven process management
- Desktop integration for autostart

## Prerequisites

- .NET 8.0 SDK
- Git
- Linux or Windows operating system
- (Linux) Desktop environment supporting .desktop files
- Visual Studio Code (recommended) or any .NET IDE

## Development Setup

1. Clone the repository:
```bash
git clone [repository-url]
cd ProcRespawn
```

2. Install .NET 8.0 SDK:
- Linux: `sudo apt-get install dotnet-sdk-8.0` (Ubuntu/Debian)
- Windows: Download from [.NET Download Page](https://dotnet.microsoft.com/download/dotnet/8.0)

3. Restore dependencies:
```bash
dotnet restore
```

## Build and Run

### Development

```bash
# Build the project
dotnet build

# Run in development mode
dotnet run

# Build release version
dotnet publish -c Release
```

### Configuration

The application uses platform-specific configuration files:
- `appsettings.linux.json`: Linux-specific settings
- `appsettings.windows.json`: Windows-specific settings

Example configuration:
```json
{
  "Processes": [
    {
      "Name": "MyApp",
      "Path": "/path/to/application",
      "Arguments": "--optional-args"
    }
  ]
}
```

## Installation

### Linux
```bash
# Install
./scripts/install.sh

# Uninstall
./scripts/uninstall.sh
```

### Windows
```powershell
# Install
.\scripts\install.ps1

# Uninstall
.\scripts\uninstall.ps1
```

## Development Workflow

1. Create a feature branch:
```bash
git checkout -b feature/your-feature-name
```

2. Make your changes and ensure they follow the project's coding style
3. Test your changes locally
4. Commit your changes with descriptive messages
5. Create a pull request

## Common Tasks

### Adding a New Process to Monitor

1. Open the appropriate `appsettings.{platform}.json`
2. Add a new process configuration entry
3. Test locally using `dotnet run`

### Debugging

1. Using Visual Studio Code:
   - Open the project in VSCode
   - Press F5 to start debugging
   - Set breakpoints as needed

2. Using Command Line:
```bash
dotnet run --configuration Debug
```

## Project Structure

- `/`: Root directory containing solution file
- `/ProcRespawn/`: Main project directory
  - `Program.cs`: Application entry point
  - `ProcRespawnDaemon.cs`: Core monitoring logic
  - `Configuration.cs`: Configuration models
  - `ProcessWrapper.cs`: Process management
  - `ProcessUtils.cs`: Utility functions
  - `scripts/`: Installation scripts
  - `appsettings.*.json`: Platform-specific configurations

## Technical Limitations

- GUI applications cannot be launched directly from systemd units
- Currently uses autostart desktop file (`proc-respawn.desktop`) for Linux GUI support
- Process monitoring is limited to the current user session

## Troubleshooting

1. Process not respawning:
   - Check process configuration in appsettings
   - Verify file paths and permissions
   - Check application logs

2. Installation issues:
   - Ensure correct permissions for script execution
   - Verify .NET SDK installation
   - Check system requirements

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

[Add your license information here]