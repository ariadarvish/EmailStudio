using System;
using System.Threading.Tasks;

namespace EmailStudio.Tool;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length > 0 && (args[0] is "--version" or "-v"))
        {
            Console.WriteLine("EmailStudio.Tool 0.1.0-alpha");
            return 0;
        }

        if (args.Length == 0 || args[0] is "--help" or "-h")
        {
            PrintUsage();
            return 0;
        }

        var command = args[0].ToLowerInvariant();
        switch (command)
        {
            case "init":
                return CliCommands.Init(args);

            case "serve":
                return await CliCommands.ServeAsync(args[1..]);

            case "render":
                return CliCommands.Render(args);

            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Unknown command '{command}'.");
                Console.ResetColor();
                PrintUsage();
                return 1;
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("EmailStudio — The developer tool for transactional email in .NET");
        Console.WriteLine();
        Console.WriteLine("Usage: dotnet email-preview <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  init [path]                           Scaffold workspace with layouts, starter templates, and locales");
        Console.WriteLine("  serve [path] [--port 5173]            Start local preview server with WebSocket hot reload");
        Console.WriteLine("  render [path] [--culture cc] [--out]  Headless batch render of templates with size guard enforcement");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -v, --version                         Show version information");
        Console.WriteLine("  -h, --help                            Show help information");
    }
}
