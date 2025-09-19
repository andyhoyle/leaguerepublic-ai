namespace LeagueRepublic.AI.Web.Service.Models;

public record LeagueRepublicConfig
{
    public required string LeagueId { get; init; }
    public required string BaseUrl { get; init; } = "https://www.leaguerepublic.com";
}

public record AzureAIConfig
{
    public required string Endpoint { get; init; }
    public required string ApiKey { get; init; }
    public required string ModelName { get; init; } = "gpt-4o";
}