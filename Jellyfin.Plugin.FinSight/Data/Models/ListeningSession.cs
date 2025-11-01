namespace Jellyfin.Plugin.Finsight.Data.Models;

internal class ListeningSession
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid ItemId { get; set; }

    public string ItemName { get; set; } = string.Empty;

    public Guid ArtistId { get; set; }

    public string ArtistName { get; set; } = string.Empty;

    public Guid AlbumId { get; set; }

    public string AlbumName { get; set; } = string.Empty;

    public DateTime PlayedAt { get; set; }

    public long PlaybackDuration { get; set; }

    public long TotalDuration { get; set; }

    public double PlaybackPercentage { get; set; }

    public bool Completed { get; set; }
}
