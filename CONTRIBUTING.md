# Contributing to EmailStudio

Thank you for your interest in contributing to EmailStudio!

## Development Setup

1. Prerequisites:
   - [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
   - Git
2. Clone and build:
   ```bash
   git clone https://github.com/emailstudio/emailstudio.git
   cd emailstudio
   dotnet build
   dotnet test
   ```

## Contribution Workflow

- Follow [Conventional Commits](https://www.conventionalcommits.org/): `feat:`, `fix:`, `docs:`, `test:`, `chore:`.
- Ensure all tests pass: `dotnet test`.
- Ensure clean code formatting: `dotnet format --verify-no-changes`.
- Treat compiler warnings as errors (`TreatWarningsAsErrors=true`).
- Keep dependency additions minimal and strictly MIT / Apache-2.0 licensed.

