namespace Jellyfin.Plugin.Finsight.API;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Jellyfin.Plugin.Finsight.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("Finsight")]
public class StatsController: ControllerBase
{
    private readonly IStatsService statsService;
    private readonly ILogger<StatsController> logger;

    public StatsController(IStatsService statsService, ILogger<StatsController> logger)
    {
        this.statsService = statsService;
        this.logger = logger;

        this.logger.LogInformation("Finsight Controller reached");
    }

    [HttpGet("User/{userId}/Year/{year}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetUserYearStats (
        [FromRoute, Required] Guid userId,
        [FromRoute, Required] int year)
    {
        var stats = await this.statsService.GetUserYearStatsAsync(userId, year);
            
        if (stats is null)
        {
            this.logger.LogError("Error: No content.");
            return this.NoContent();
        }
        this.logger.LogInformation("Successfully retrieved user year stats.");
        return this.Ok(stats);
    }

    [HttpGet("User/{userId}/Artists")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetUserArtists(
        [FromRoute, Required] Guid userId,
        [FromQuery] int year = 2025)
    {
        var artists = await this.statsService.GetUserArtistsWithStatsAsync(userId, year);

        if (artists is null)
        {
            this.logger.LogError("Error: Not found.");
            return this.NotFound();
        }
        this.logger.LogInformation("Successfully retrieved user artist stats");
        return this.Ok(artists);
    }
}
