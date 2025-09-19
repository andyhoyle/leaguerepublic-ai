using Microsoft.AspNetCore.Mvc;
using LeagueRepublic.AI.Web.Service;
using LeagueRepublic.AI.Web.Service.Models;

namespace LeagueRepublic.AI.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchController : ControllerBase
{
    private readonly IMatchProcessingService _matchProcessingService;
    private readonly ILogger<MatchController> _logger;

    public MatchController(IMatchProcessingService matchProcessingService, ILogger<MatchController> logger)
    {
        _matchProcessingService = matchProcessingService;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeMatchImage(IFormFile imageFile, CancellationToken cancellationToken)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            return BadRequest("No image file provided");
        }

        if (!IsValidImageFile(imageFile))
        {
            return BadRequest("Invalid image file type. Only JPEG, PNG, and GIF files are supported.");
        }

        try
        {
            using var stream = imageFile.OpenReadStream();
            var result = await _matchProcessingService.ProcessMatchImageAsync(stream, imageFile.FileName, cancellationToken);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing match image: {FileName}", imageFile.FileName);
            return StatusCode(500, "An error occurred while analyzing the image");
        }
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitMatchResult([FromBody] SubmitMatchRequest request, CancellationToken cancellationToken)
    {
        if (request?.MatchData == null)
        {
            return BadRequest("Match data is required");
        }

        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Username and password are required");
        }

        try
        {
            var result = await _matchProcessingService.SubmitMatchResultAsync(request.MatchData, request.Username, request.Password, cancellationToken);
            
            if (result)
            {
                return Ok(new { Success = true, Message = "Match result submitted successfully" });
            }
            else
            {
                return BadRequest(new { Success = false, Message = "Failed to submit match result" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting match result");
            return StatusCode(500, "An error occurred while submitting the match result");
        }
    }

    private static bool IsValidImageFile(IFormFile file)
    {
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
        return allowedTypes.Contains(file.ContentType.ToLower());
    }
}

public record SubmitMatchRequest
{
    public required MatchData MatchData { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}