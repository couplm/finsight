namespace Jellyfin.Plugin.Finsight.Data.Repository;

using Jellyfin.Plugin.Finsight.Data.Models;

internal interface IStatsRepository
{
    Task<List<ArtistStats>> GetTopArtistsAsync(Guid userId, int year, int limit = 10);

    Task<List<SongStats>> GetTopSongsAsync(Guid userId, int year, int limit = 10);

    Task<List<ListeningSession>> GetUserSessionsAsync(Guid userId, DateTime startDate, DateTime endDate);

    Task<List<ArtistStats>> GetUserArtistsWithStatsAsync(Guid userId, int? year = null);

    Task<List<SongStats>> GetUserSongsWithStatsAsync(Guid userId, int? year = null);
}
