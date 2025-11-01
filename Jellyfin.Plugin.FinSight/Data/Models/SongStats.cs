namespace Jellyfin.Plugin.Finsight.Data.Models;

public class SongStats
{
    public Guid ItemId { get; set; }

    public string ItemName { get; set; } = string.Empty;

    public Guid ArtistId { get; set; }

    public string ArtistName { get; set; } = string.Empty;

    public Guid AlbumId { get; set; }

    public string AlbumName { get; set; } = string.Empty;

    public int PlayCount { get; set; }

    public long TotalPlaytime { get; set; }
}
