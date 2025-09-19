namespace LeagueRepublic.AI.Web.Service.Models;

public record MatchData
{
    public required string HomeTeam { get; init; }
    public required string AwayTeam { get; init; }
    public required int HomeScore { get; init; }
    public required int AwayScore { get; init; }
    public required DateTime MatchDate { get; init; }
    public required List<GameResult> Games { get; init; } = [];
    public string? ImagePath { get; init; }
}

public record GameResult
{
    public required string HomePlayer { get; init; }
    public required string AwayPlayer { get; init; }
    public required string Winner { get; init; }
    public int? HomePlayerScore { get; init; }
    public int? AwayPlayerScore { get; init; }
}

public record ImageAnalysisResult
{
    public required MatchData MatchData { get; init; }
    public required string RawAnalysis { get; init; }
    public required double Confidence { get; init; }
}