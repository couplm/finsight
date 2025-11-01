namespace Jellyfin.Plugin.Finsight.Services;

using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.Finsight.Data.Models;
using Jellyfin.Plugin.Finsight.Data.Repository;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

internal class StatsService : IStatsService
{
    private readonly IStatsRepository repository;
    private readonly ILibraryManager libraryManager;
    private readonly ILogger<StatsService> logger;

    public StatsService(
        IStatsRepository repository,
        ILibraryManager libraryManager,
        ILogger<StatsService> logger)
    {
        this.repository = repository;
        this.libraryManager = libraryManager;
        this.logger = logger;
    }

    /// <summary>
    /// Get general user stats for a specified year.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="year">Year to filter to.</param>
    /// <returns>User stats for a specified year.</returns>
    public async Task<UserYearStats> GetUserYearStatsAsync(Guid userId, int year)
    {
        var stats = new UserYearStats
        {
            UserId = userId,
            Year = year,
        };

        try
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31, 23, 59, 59);

            var sessions = await this.repository.GetUserSessionsAsync(userId, startDate, endDate);
            var completedSessions = sessions.Where(s => s.Completed).ToList();

            stats.TotalSongsPlayed = completedSessions.Count;
            stats.TotalMinutesListened = completedSessions.Sum(s => s.PlaybackDuration) / 60;

            stats.TopArtists = await this.repository.GetTopArtistsAsync(userId, year, 10);
            stats.TopSongs = await this.repository.GetTopSongsAsync(userId, year, 10);

            var topGenres = new Dictionary<string, int>();
            foreach (var session in completedSessions)
            {
                var item = this.libraryManager.GetItemById(session.ItemId);
                if (item is Audio audioItem && audioItem.Genres != null)
                {
                    foreach (var genre in audioItem.Genres)
                    {
                        if (!string.IsNullOrEmpty(genre))
                        {
                            if (topGenres.ContainsKey(genre))
                            {
                                topGenres[genre]++;
                            }
                            else
                            {
                                topGenres[genre] = 1;
                            }
                        }
                    }
                }
            }

            stats.TopGenres = topGenres
                .OrderByDescending(kvp => kvp.Value)
                .Take(5)
                .Select(kvp => kvp.Key)
                .ToList();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting user year stats");
        }

        return stats;
    }

    /// <summary>
    /// Get artist stats for a user, optionally filtered by year.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="year">Year.</param>
    /// <returns>List of artist stats.</returns>
    public async Task<List<ArtistStats>> GetUserArtistsWithStatsAsync(Guid userId, int? year = null)
    {
        return await this.repository.GetUserArtistsWithStatsAsync(userId, year);
    }

    /// <summary>
    /// Get song stats for a user, optionally filtered by year.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="year">Year.</param>
    /// <returns>List of song stats.</returns>
    public async Task<List<SongStats>> GetUserSongsWithStatsAsync(Guid userId, int? year = null)
    {
        return await this.repository.GetUserSongsWithStatsAsync(userId, year);
    }

    /// <summary>
    /// Get all artists in the library.
    /// </summary>
    /// <returns>List of artists.</returns>
    public async Task<List<ArtistStats>> GetAllArtistsAsync()
    {
        return await Task.Run(() =>
        {
            var query = new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.MusicArtist },
                Recursive = true,
            };

            var result = this.libraryManager.GetItemsResult(query);
            return result.Items.ToList();
        });
    }

    /// <summary>
    /// Get all songs in the library.
    /// </summary>
    /// <returns>List of songs.</returns>
    public async Task<List<SongStats>> GetAllSongsAsync()
    {
        return await Task.Run(() =>
        {
            var query = new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.Audio },
                Recursive = true,
            };

            var result = this.libraryManager.GetItemsResult(query);
            return result.Items.ToList();
        });
    }
}
