using System.Text.Json;
using System.IO;

namespace SongGrab.UI.Services
{
    public class ConfigurationService
    {
        private readonly string _configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SongGrab",
            "config.json"
        );

        private AppSettings _settings = new();

        public ConfigurationService()
        {
            LoadSettings();
        }

        public AppSettings Settings => _settings;

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    _settings = new AppSettings();
                    SaveSettings();
                }
            }
            catch
            {
                _settings = new AppSettings();
            }
        }

        public void SaveSettings()
        {
            try
            {
                var directory = Path.GetDirectoryName(_configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_configPath, json);
            }
            catch
            {
                // Gérer les erreurs de sauvegarde
            }
        }
    }

    public class AppSettings
    {
        public string DefaultDownloadPath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            "SongGrab"
        );

        public bool IsDarkTheme { get; set; }
        public bool CreatePlaylistFolder { get; set; } = true;
        public bool CheckForUpdatesAtStartup { get; set; } = true;
        public string LastUsedPlaylistId { get; set; } = string.Empty;
        public bool CompactMode { get; set; }
        public bool VerifyDownloads { get; set; } = true;
        public string FileNameTemplate { get; set; } = "{artiste} - {titre}";
        public string AudioQuality { get; set; } = "320 kbps";
    }
} 