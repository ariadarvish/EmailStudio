using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace EmailStudio.Generator.Tests;

public sealed class TestAdditionalText : AdditionalText
{
    private readonly string _path;
    private readonly SourceText _text;

    public TestAdditionalText(string path, string content)
    {
        _path = path;
        _text = SourceText.From(content, Encoding.UTF8);
    }

    public override string Path => _path;
    public override SourceText GetText(CancellationToken cancellationToken = default) => _text;
}

public static class GeneratorTestRunner
{
    public static (ImmutableArray<Diagnostic> Diagnostics, string? GeneratedSource) Run(
        string userSourceCode,
        IEnumerable<AdditionalText> additionalTexts)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(userSourceCode);

        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.GCSettings).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(EmailDraft).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "GeneratorTestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new EmailStudioGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new ISourceGenerator[] { generator.AsSourceGenerator() },
            additionalTexts: additionalTexts);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        var runResult = driver.GetRunResult();
        string? emittedCode = runResult.GeneratedTrees.Length > 0
            ? runResult.GeneratedTrees[0].GetText().ToString()
            : null;

        return (diagnostics, emittedCode);
    }
}

