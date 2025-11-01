namespace Jellyfin.Plugin.Finsight.Services;

using Jellyfin.Plugin.Finsight.Data.Models;

internal interface IStatsService
{
    Task<UserYearStats> GetUserYearStatsAsync(Guid userId, int year);

    Task<List<ArtistStats>> GetUserArtistsWithStatsAsync(Guid userId, int? year = null);

    Task<List<SongStats>> GetUserSongsWithStatsAsync(Guid userId, int? year = null);

    Task<List<ArtistStats>> GetAllArtistsAsync();

    Task<List<SongStats>> GetAllSongsAsync();
}

