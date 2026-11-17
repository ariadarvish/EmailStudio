using System;
using System.Threading.Tasks;
using EmailStudio.Preview;

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
            case "serve":
                Console.WriteLine("Starting EmailStudio preview server on http://localhost:5173...");
                var app = PreviewServer.CreateApp(args[1..]);
                await app.RunAsync();
                return 0;

            case "init":
                Console.WriteLine("Initializing new EmailStudio template workspace...");
                // CLI init implementation in M4
                return 0;

            case "render":
                Console.WriteLine("Rendering email templates...");
                // CLI headless render implementation in M4
                return 0;

            default:
                Console.Error.WriteLine($"Unknown command '{command}'.");
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
        Console.WriteLine("  init      Scaffold template workspace with layouts, starter templates, and locales");
        Console.WriteLine("  serve     Start local preview server with hot reload");
        Console.WriteLine("  render    Headless compile and render of templates with size guard validation");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -v, --version    Show version information");
        Console.WriteLine("  -h, --help       Show help information");
    }
}

