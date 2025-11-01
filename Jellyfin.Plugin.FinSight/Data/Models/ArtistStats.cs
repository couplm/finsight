namespace Jellyfin.Plugin.Finsight.Data.Models;

public record ArtistStats
{
    public Guid ArtistId { get; set; }

    public string ArtistName { get; set; } = string.Empty;

    public int PlayCount { get; set; }

    public long TotalPlaytime { get; set; }

    public ArtistStats(Guid ArtistId, string ArtistName, int PlayCount, long TotalPlaytime)
    {
        this.ArtistId = ArtistId;
        this.ArtistName = ArtistName;
        this.PlayCount = PlayCount;
        this.TotalPlaytime = TotalPlaytime;
    }
}
