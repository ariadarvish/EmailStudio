using System.Net;
using System.Threading.Tasks;
using EmailStudio.Preview;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace EmailStudio.Preview.Tests;

public sealed class PreviewServerTests
{
    [Fact]
    public async Task HealthEndpoint_ShouldReturnOk()
    {
        // Arrange
        var app = PreviewServer.CreateApp();
        app.Urls.Add("http://127.0.0.1:0");
        await app.StartAsync();

        try
        {
            using var client = new System.Net.Http.HttpClient();
            var serverAddress = app.Urls.GetEnumerator();
            serverAddress.MoveNext();
            var url = serverAddress.Current + "/health";

            // Act
            var response = await client.GetAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("Healthy");
            body.Should().Contain("0.1.0-alpha");
        }
        finally
        {
            await app.StopAsync();
        }
    }
}

