namespace Jellyfin.Plugin.Finsight.Data.Repository;

using System.Text.Json;
using Jellyfin.Plugin.Finsight.Data.Models;
using MediaBrowser.Common.Configuration;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

public class StatsRepository : IStatsRepository
{
    private readonly IApplicationPaths applicationPaths;
    private readonly ILogger<StatsRepository> logger;
    private readonly string dataPath;

    public StatsRepository(IApplicationPaths appPaths, ILogger<StatsRepository> logger)
    {
        this.applicationPaths = appPaths;
        this.logger = logger;
        this.dataPath = Path.Combine(this.applicationPaths.PluginConfigurationsPath, "ListeningStats");

        string dbPath = GetJellyfinDatabasePath();

        string connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadOnly,
            Cache = SqliteCacheMode.Shared,
            Pooling = false,
        }.ConnectionString;

        try
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                this.logger.LogInformation($"Successfully connected to Jellyfin database {connection}");

                // Create a command to query the database
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM PlaybackActivity LIMIT 10";
                    this.logger.LogInformation("Executing sample query on PlaybackActivity table");
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        this.logger.LogInformation("Sample PlaybackActivity Records:");
                        while (reader.Read())
                        {
                            this.logger.LogInformation("Sample PlaybackActivity Record: Id={Id}, UserId={UserId}, ItemId={ItemId}, PlayedAt={PlayedAt}",
                                reader["Id"], reader["UserId"], reader["ItemId"], reader["PlayedAt"]);
                        }
                    }
                }
            }
        }
        catch (SqliteException ex)
        {
            this.logger.LogError($"Error connecting to Jellyfin database: {ex.Message}");
        }

        if (!Directory.Exists(this.dataPath))
        {
            Directory.CreateDirectory(this.dataPath);
            this.logger.LogInformation("Created data directory at {DataPath}", this.dataPath);
        }
    }

    public async Task SaveListeningSessionAsync(ListeningSession session)
    {
        try
        {
            var userPath = Path.Combine(this.dataPath, session.UserId.ToString());
            if (!Directory.Exists(userPath))
            {
                Directory.CreateDirectory(userPath);
            }

            var yearPath = Path.Combine(userPath, session.PlayedAt.Year.ToString());
            if (!Directory.Exists(yearPath))
            {
                Directory.CreateDirectory(yearPath);
            }

            var monthFile = Path.Combine(yearPath, $"{session.PlayedAt.Month:D2}.json");

            List<ListeningSession> sessions;
            if (File.Exists(monthFile))
            {
                var json = await File.ReadAllTextAsync(monthFile);
                sessions = JsonSerializer.Deserialize<List<ListeningSession>>(json) ?? new List<ListeningSession>();
            }
            else
            {
                sessions = new List<ListeningSession>();
            }

            sessions.Add(session);

            var updatedJson = JsonSerializer.Serialize(sessions, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(monthFile, updatedJson);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error saving listening session for user {UserId}", session.UserId);
        }
    }

    public async Task<List<ListeningSession>> GetUserSessionsAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var sessions = new List<ListeningSession>();
        var userPath = Path.Combine(this.dataPath, userId.ToString());

        if (!Directory.Exists(userPath))
        {
            return sessions;
        }

        try
        {
            var files = Directory.GetFiles(userPath, "*.json", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var json = await File.ReadAllTextAsync(file);
                var fileSessions = JsonSerializer.Deserialize<List<ListeningSession>>(json);

                if (fileSessions != null)
                {
                    sessions.AddRange(fileSessions);
                }
            }

            if (startDate.HasValue)
            {
                sessions = sessions.Where(s => s.PlayedAt >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                sessions = sessions.Where(s => s.PlayedAt <= endDate.Value).ToList();
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error reading user sessions");
        }

        return sessions;
    }

    public async Task<List<ArtistStats>> GetTopArtistsAsync(Guid userId, int year, int limit = 10)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31, 23, 59, 59);

        var sessions = await GetUserSessionsAsync(userId, startDate, endDate);

        var artistStats = sessions
            .Where(s => s.Completed)
            .GroupBy(s => new { s.ArtistId, s.ArtistName })
            .Select(g => new ArtistStats(ArtistId: g.Key.ArtistId, ArtistName: g.Key.ArtistName, PlayCount: g.Count(), TotalPlaytime: g.Sum(s => s.PlaybackDuration)))
            .OrderByDescending(a => a.PlayCount)
            .Take(limit)
            .ToList();

        return artistStats;
    }

    public async Task<List<SongStats>> GetTopSongsAsync(Guid userId, int year, int limit = 10)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31, 23, 59, 59);

        var sessions = await GetUserSessionsAsync(userId, startDate, endDate);

        var songStats = sessions
            .Where(s => s.Completed)
            .GroupBy(s => new { s.ItemId, s.ItemName, s.ArtistId, s.ArtistName, s.AlbumId, s.AlbumName })
            .Select(g => new SongStats(
                ItemId: g.Key.ItemId,
                ItemName: g.Key.ItemName,
                ArtistId: g.Key.ArtistId,
                ArtistName: g.Key.ArtistName,
                AlbumId: g.Key.AlbumId,
                AlbumName: g.Key.AlbumName,
                PlayCount: g.Count(),
                TotalPlaytime: g.Sum(s => s.PlaybackDuration)))
            .OrderByDescending(s => s.PlayCount)
            .Take(limit)
            .ToList();

        return songStats;
    }

    private string GetJellyfinDatabasePath()
    {
        string baseDir = Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "jellyfin/data"
            ),
            PlatformID.Unix => Path.Combine(
                Environment.GetEnvironmentVariable("HOME"),
                ".local/share/jellyfin"
            ),
            PlatformID.MacOSX => Path.Combine(
                Environment.GetEnvironmentVariable("HOME"),
                "Library/Application Support/jellyfin"
            ),
            _ => throw new PlatformNotSupportedException("Unsupported operating system")
        };

        this.logger.LogInformation($"Base directory: {baseDir}");

        return Path.Combine(baseDir, "jellyfin.db");
    }
}
