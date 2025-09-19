using LeagueRepublic.AI.Web.Adapters.AzureAI;
using LeagueRepublic.AI.Web.Adapters.LeagueRepublic;
using LeagueRepublic.AI.Web.Service.Models;

namespace LeagueRepublic.AI.Web.Service;

public interface IMatchProcessingService
{
    Task<ImageAnalysisResult> ProcessMatchImageAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default);
    Task<bool> SubmitMatchResultAsync(MatchData matchData, string username, string password, CancellationToken cancellationToken = default);
}

public class MatchProcessingService : IMatchProcessingService
{
    private readonly IAzureAIAdapter _azureAIAdapter;
    private readonly ILeagueRepublicAdapter _leagueRepublicAdapter;
    private readonly ILogger<MatchProcessingService> _logger;
    private readonly LeagueRepublicConfig _config;

    public MatchProcessingService(
        IAzureAIAdapter azureAIAdapter,
        ILeagueRepublicAdapter leagueRepublicAdapter,
        ILogger<MatchProcessingService> logger,
        LeagueRepublicConfig config)
    {
        _azureAIAdapter = azureAIAdapter;
        _leagueRepublicAdapter = leagueRepublicAdapter;
        _logger = logger;
        _config = config;
    }

    public async Task<ImageAnalysisResult> ProcessMatchImageAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing match image: {FileName}", fileName);

        try
        {
            // Analyze the image using Azure AI
            var analysisResult = await _azureAIAdapter.AnalyzeMatchImageAsync(imageStream, fileName, cancellationToken);
            
            _logger.LogInformation("Successfully analyzed image with confidence: {Confidence}", analysisResult.Confidence);
            return analysisResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing match image: {FileName}", fileName);
            throw;
        }
    }

    public async Task<bool> SubmitMatchResultAsync(MatchData matchData, string username, string password, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Submitting match result for {HomeTeam} vs {AwayTeam}", matchData.HomeTeam, matchData.AwayTeam);

        try
        {
            // Validate league exists
            var isValidLeague = await _leagueRepublicAdapter.IsValidLeagueAsync(_config.LeagueId, cancellationToken);
            if (!isValidLeague)
            {
                _logger.LogWarning("Invalid league ID: {LeagueId}", _config.LeagueId);
                return false;
            }

            // Submit match result
            var submitResult = await _leagueRepublicAdapter.SubmitMatchResultAsync(matchData, username, password, cancellationToken);
            
            // Upload match image if available
            if (!string.IsNullOrEmpty(matchData.ImagePath))
            {
                var uploadResult = await _leagueRepublicAdapter.UploadMatchImageAsync(_config.LeagueId, matchData.ImagePath, cancellationToken);
                _logger.LogInformation("Image upload result: {UploadResult}", uploadResult);
            }

            _logger.LogInformation("Match result submission completed: {Result}", submitResult);
            return submitResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting match result for {HomeTeam} vs {AwayTeam}", matchData.HomeTeam, matchData.AwayTeam);
            throw;
        }
    }
}