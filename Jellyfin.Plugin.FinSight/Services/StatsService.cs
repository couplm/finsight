namespace Jellyfin.Plugin.Finsight.Services;

using System.Threading.Tasks;
using Jellyfin.Plugin.Finsight.Data.Models;
using Jellyfin.Plugin.Finsight.Data.Repository;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

public class StatsService : IStatsService
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

    public async Task<List<ArtistStats>> GetUserArtistsWithStatsAsync(Guid userId, int year = 2025)
    {
        var artistStats = await this.repository.GetTopArtistsAsync(userId, year, 50);

        return artistStats;
    }

    public async Task<List<SongStats>> GetUserSongsWithStatsAsync(Guid userId, int year = 2025)
    {
        var songStats = await this.repository.GetTopSongsAsync(userId, year, 50);

        return songStats;
    }
}
