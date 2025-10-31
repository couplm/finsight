namespace Jellyfin.Plugin.Finsight.API;

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Plugin.Finsight.Services;
using MediaBrowser.Controller.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("Finsight")]
[Produces(MediaTypeNames.Application.Json)]
internal class StatsController: ControllerBase
{
    private readonly IStatsService statsService;
    private readonly ISessionManager sessionManager;

    /// <summary>
    /// Stats Controller.
    /// </summary>
    /// <param name="statsService">Stats service.</param>
    /// <param name="sessionManager">Session manager.</param>
    public StatsController(IStatsService statsService, ISessionManager sessionManager)
    {
        this.statsService = statsService;
        this.sessionManager = sessionManager;
    }

    /// <summary>
    /// Get listening stats for a user for a specific year.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="year">Year to get stats for.</param>
    /// <response code="200">Stats retrieved successfully.</response>
    /// <response code="404">User not found.</response>
    /// <returns>User stats for a specified year.</returns>
    [HttpGet("User/{userId}/Year/{year}")]
    [Authorize(Policy = "DefaultAuthorization")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetUserYearStats (
        [FromRoute, Required] Guid userId,
        [FromRoute, Required] int year)
    {
        var stats = await this.statsService.GetUserYearStatsAsync(userId, year);
            
        if (stats is null)
        {
            return this.NotFound();
        }

        return this.Ok(stats);
    }

    /// <summary>
    /// Get all artists that the user has listened to, with play counts.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="year">Optional year to filter by.</param>
    /// <response code="200">Artists returned successfully.</response>
    /// <returns>List of artists with play counts.</returns>
    [HttpGet("User/{userId}/Artists")]
    [Authorize(Policy = "DefaultAuthorization")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetUserArtists(
        [FromRoute, Required] Guid userId,
        [FromQuery] int? year = null)
    {
        var artists = await this.statsService.GetUserArtistsWithStatsAsync(userId, year);
        return this.Ok(artists);
    }

    /// <summary>
    /// Get all songs in the library.
    /// </summary>
    /// <response code="200">Songs returned successfully.</response>
    /// <returns>List of all songs.</returns>
    [HttpGet("Songs")]
    [Authorize(Policy = "DefaultAuthorization")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAllSongs()
    {
        var songs = await this.statsService.GetAllSongsAsync();

        var songDtos = songs.Select(s => new
        {
            Id = s.ItemId,
            Name = s.ItemName,
            Album = s is MediaBrowser.Controller.Entities.Audio.Audio audio ? audio.Album : null,
            Artists = s is MediaBrowser.Controller.Entities.Audio.Audio audioTrack ? audioTrack.Artists : null,
        }).ToList();

        return this.Ok(songDtos);
    }

    /// <summary>
    /// Get current user's stats for the current year.
    /// </summary>
    /// <response code="200">Stats returned successfully.</response>
    /// <returns>Current user's listening stats for the current year.</returns>
    [HttpGet("MyStats")]
    [Authorize(Policy = "DefaultAuthorization")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMyCurrentYearStats()
    {
        var authorizationInfo = await this.sessionManager.GetSession(HttpContext.Request.Headers["Authorization"]);

        if (authorizationInfo?.UserId == null || authorizationInfo.UserId == Guid.Empty)
        {
            return this.Unauthorized();
        }

        var currentYear = DateTime.UtcNow.Year;
        var stats = await this.statsService.GetUserYearStatsAsync(authorizationInfo.UserId, currentYear);

        return this.Ok(stats);
    }
}
