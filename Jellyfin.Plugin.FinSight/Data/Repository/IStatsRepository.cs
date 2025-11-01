namespace Jellyfin.Plugin.Finsight.Data.Repository;

using Jellyfin.Plugin.Finsight.Data.Models;

public interface IStatsRepository
{
    Task SaveListeningSessionAsync(ListeningSession session);

    Task<List<ArtistStats>> GetTopArtistsAsync(Guid userId, int year, int limit = 10);

    Task<List<SongStats>> GetTopSongsAsync(Guid userId, int year, int limit = 10);

    Task<List<ListeningSession>> GetUserSessionsAsync(Guid userId, DateTime? startDate, DateTime? endDate);
}
