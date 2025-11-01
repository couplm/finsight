namespace Jellyfin.Plugin.Finsight.Data.Models;

using System;
using System.Collections.Generic;

public class UserYearStats
{
    public Guid UserId { get; set; }

    public int Year { get; set; }

    public int TotalSongsPlayed { get; set; }

    public long TotalMinutesListened { get; set; }

    public List<ArtistStats> TopArtists { get; set; } = new();

    public List<SongStats> TopSongs { get; set; } = new();

    public List<string> TopGenres { get; set; } = new();
}
