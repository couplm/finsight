namespace Jellyfin.Plugin.Finsight.Data.Models;

public record SongStats
{
    public Guid ItemId { get; set; }

    public string ItemName { get; set; } = string.Empty;

    public Guid ArtistId { get; set; }

    public string ArtistName { get; set; } = string.Empty;

    public Guid AlbumId { get; set; }

    public string AlbumName { get; set; } = string.Empty;

    public int PlayCount { get; set; }

    public long TotalPlaytime { get; set; }

    public SongStats(Guid ItemId, string ItemName, Guid ArtistId, string ArtistName, Guid AlbumId, string AlbumName, int PlayCount, long TotalPlaytime)
    {
        this.ItemId = ItemId;
        this.ItemName = ItemName;
        this.ArtistId = ArtistId;
        this.ArtistName = ArtistName;
        this.AlbumId = AlbumId;
        this.AlbumName = AlbumName;
        this.PlayCount = PlayCount;
        this.TotalPlaytime = TotalPlaytime;
    }
}
