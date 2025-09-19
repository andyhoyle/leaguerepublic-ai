using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using LeagueRepublic.AI.Web;
using LeagueRepublic.AI.Web.Service.Models;

namespace LeagueRepublic.AI.Tests;

public class MatchControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public MatchControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task AnalyzeMatchImage_WithValidImage_ReturnsSuccess()
    {
        // Arrange
        var imageBytes = File.ReadAllBytes("IMG_20250918_225720.jpg");
        
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "imageFile", "test_scorecard.jpg");

        // Act
        var response = await _client.PostAsync("/api/match/analyze", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ImageAnalysisResult>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(result);
        Assert.NotNull(result.MatchData);
        Assert.True(result.Confidence > 0);
    }

    [Fact]
    public async Task AnalyzeMatchImage_WithoutFile_ReturnsBadRequest()
    {
        // Arrange
        using var content = new MultipartFormDataContent();

        // Act
        var response = await _client.PostAsync("/api/match/analyze", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SubmitMatchResult_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var matchData = new MatchData
        {
            HomeTeam = "Test Home Team",
            AwayTeam = "Test Away Team",
            HomeScore = 3,
            AwayScore = 2,
            MatchDate = DateTime.UtcNow.Date,
            Games = [
                new GameResult { HomePlayer = "Player 1", AwayPlayer = "Player 2", Winner = "Player 1" }
            ]
        };

        var request = new
        {
            MatchData = matchData,
            Username = "testuser",
            Password = "testpass"
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/match/submit", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Success", responseContent);
    }
}