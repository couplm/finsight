namespace Jellyfin.Plugin.Finsight.Data.Models;

public record ArtistStats(Guid ArtistId, int PlayCount, long TotalPlaytime)
{
    public string ArtistName { get; set; } = string.Empty;
}
