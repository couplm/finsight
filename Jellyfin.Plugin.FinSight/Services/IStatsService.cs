namespace Jellyfin.Plugin.Finsight.Services;

using Jellyfin.Plugin.Finsight.Data.Models;

public interface IStatsService
{
    Task<UserYearStats> GetUserYearStatsAsync(Guid userId, int year);

    Task<List<ArtistStats>> GetUserArtistsWithStatsAsync(Guid userId, int year = 2025);

    Task<List<SongStats>> GetUserSongsWithStatsAsync(Guid userId, int year = 2025);
}

