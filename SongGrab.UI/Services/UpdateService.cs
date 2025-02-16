using System.Net.Http;
using System.Text.Json;
using System.Reflection;

namespace SongGrab.UI.Services
{
    public class UpdateService
    {
        private const string GithubApiUrl = "https://api.github.com/repos/VOTRE_USERNAME/SongGrab/releases/latest";
        private readonly HttpClient _httpClient;

        public UpdateService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "SongGrab");
        }

        public async Task<UpdateInfo> CheckForUpdates()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(GithubApiUrl);
                var releaseInfo = JsonSerializer.Deserialize<GithubReleaseInfo>(response);

                if (releaseInfo != null)
                {
                    var currentVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0, 0);
                    var latestVersion = Version.TryParse(releaseInfo.TagName.TrimStart('v'), out var version) 
                        ? version 
                        : new Version(1, 0, 0, 0);

                    return new UpdateInfo
                    {
                        IsUpdateAvailable = latestVersion > currentVersion,
                        LatestVersion = latestVersion.ToString(),
                        CurrentVersion = currentVersion.ToString(),
                        ReleaseNotes = releaseInfo.Body,
                        DownloadUrl = releaseInfo.HtmlUrl
                    };
                }
            }
            catch
            {
                // Gérer les erreurs de vérification de mise à jour
            }

            var defaultVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0, 0);
            return new UpdateInfo
            {
                IsUpdateAvailable = false,
                CurrentVersion = defaultVersion.ToString(),
                LatestVersion = string.Empty,
                ReleaseNotes = string.Empty,
                DownloadUrl = string.Empty
            };
        }
    }

    public class UpdateInfo
    {
        public bool IsUpdateAvailable { get; set; }
        public string CurrentVersion { get; set; } = string.Empty;
        public string LatestVersion { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
    }

    public class GithubReleaseInfo
    {
        public string TagName { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
    }
} 