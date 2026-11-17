using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EmailStudio.Tests;

public sealed class EmailDraftTests
{
    [Fact]
    public void EmailDraft_Properties_ShouldInitializeCorrectly()
    {
        // Arrange
        var subject = "Welcome to Contoso!";
        var html = "<h1>Welcome</h1>";
        var text = "Welcome";

        // Act
        var draft = new EmailDraft(subject, html, text);

        // Assert
        draft.Subject.Should().Be(subject);
        draft.HtmlBody.Should().Be(html);
        draft.TextBody.Should().Be(text);
    }

    [Fact]
    public void AddEmailStudio_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEmailStudio();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();
    }
}

