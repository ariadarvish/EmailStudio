using System.Linq;
using System.Reflection;
using EmailStudio.Generator;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EmailStudio.Generator.Tests;

public sealed class DiagnosticDescriptorsTests
{
    [Fact]
    public void AllNineDiagnostics_ShouldBeDefinedWithUniqueIds()
    {
        // Arrange
        var fields = typeof(DiagnosticDescriptors)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(DiagnosticDescriptor))
            .Select(f => (DiagnosticDescriptor)f.GetValue(null)!)
            .ToList();

        // Assert
        fields.Should().HaveCount(9);

        var ids = fields.Select(d => d.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
        ids.Should().Contain([
            "TPL001", "TPL002", "TPL003", "TPL004", "TPL005",
            "TPL006", "TPL007", "TPL008", "TPL009"
        ]);

        fields.Should().OnlyContain(d => d.Category == "EmailStudio");
    }
}

