using System;
using System.IO;
using System.Threading.Tasks;
using EmailStudio.Preview;

namespace EmailStudio.Preview;

/// <summary>
/// Entry point for running EmailStudio.Preview directly from Visual Studio or CLI.
/// </summary>
public static class Program
{
    public static async Task Main(string[] args)
    {
        var options = new PreviewServerOptions();

        // If local Templates folder doesn't exist, check standard gallery paths for out-of-the-box F5 debugging
        if (!Directory.Exists(options.TemplateDirectory))
        {
            string[] candidateRoots = [
                Directory.GetCurrentDirectory(),
                Path.Combine(Directory.GetCurrentDirectory(), "samples", "Gallery"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "samples", "Gallery"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "samples", "Gallery"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "samples", "Gallery")
            ];

            foreach (var root in candidateRoots)
            {
                string candidateTemplates = Path.Combine(root, "Templates");
                if (Directory.Exists(candidateTemplates))
                {
                    options.TemplateDirectory = candidateTemplates;
                    string candidateLocales = Path.Combine(root, "Locales");
                    if (Directory.Exists(candidateLocales))
                    {
                        options.LocalesDirectory = candidateLocales;
                    }
                    break;
                }
            }
        }

        var app = PreviewServer.CreateApp(args, options);
        await app.RunAsync();
    }
}

