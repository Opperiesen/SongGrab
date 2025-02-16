using DeezNET;
using System.Diagnostics;
using System.Text.RegularExpressions;
using SongGrab.UI.ViewModels;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Windows.Threading;

namespace SongGrab.UI.Services
{
    public class DownloadService
    {
        private readonly string _ytDlpPath;
        private readonly string _ffmpegPath;
        private readonly DeezerClient _deezer;
        private readonly Dictionary<int, string> _genreCache;
        private readonly ConfigurationService _configService;

        public event EventHandler<string>? LogMessage;
        public event EventHandler<(string message, double progress)>? ProgressChanged;

        public DownloadService(ConfigurationService configService)
        {
            _ytDlpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe");
            _ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
            _deezer = new DeezerClient();
            _genreCache = new Dictionary<int, string>();
            _configService = configService;
        }

        public async Task<bool> CheckDependencies()
        {
            if (!File.Exists(_ytDlpPath) || !File.Exists(_ffmpegPath))
            {
                return false;
            }

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _ytDlpPath,
                        Arguments = "--version",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<TrackViewModel>> LoadPlaylist(long playlistId)
        {
            var tracks = new List<TrackViewModel>();
            var playlistData = await _deezer.PublicApi.GetPlaylist(playlistId);

            if (playlistData?["tracks"]?["data"] is JArray tracksData)
            {
                foreach (var track in tracksData)
                {
                    var duration = TimeSpan.FromSeconds(track["duration"]?.Value<int>() ?? 0);
                    var title = track["title"]?.ToString() ?? string.Empty;
                    var artist = track["artist"]?["name"]?.ToString() ?? string.Empty;
                    var album = track["album"]?["title"]?.ToString() ?? string.Empty;
                    var genreId = track["genre_id"]?.Value<int>() ?? 0;
                    var deezerUrl = track["link"]?.ToString() ?? string.Empty;
                    var previewUrl = track["preview"]?.ToString() ?? string.Empty;

                    string genreName = string.Empty;
                    if (genreId > 0 && !_genreCache.TryGetValue(genreId, out genreName))
                    {
                        try
                        {
                            var genreData = await _deezer.PublicApi.GetGenre(genreId);
                            genreName = genreData?["name"]?.ToString() ?? string.Empty;
                            if (!string.IsNullOrEmpty(genreName))
                            {
                                _genreCache[genreId] = genreName;
                            }
                        }
                        catch
                        {
                            // Ignorer les erreurs de récupération du genre
                        }
                    }

                    var trackVm = new TrackViewModel
                    {
                        Title = title,
                        Artist = artist,
                        Album = album,
                        Duration = duration,
                        Genre = genreName,
                        DeezerTrackId = track["id"]?.Value<long>() ?? 0,
                        DeezerUrl = deezerUrl
                    };

                    tracks.Add(trackVm);
                }
            }

            return tracks;
        }

        public async Task DownloadTrack(TrackViewModel track, string outputPath)
        {
            try
            {
                // Rechercher la vidéo YouTube
                var searchProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _ytDlpPath,
                        Arguments = $"ytsearch1:\"{track.Artist} {track.Title}\" --get-id --get-title --get-duration",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                searchProcess.Start();
                var output = await searchProcess.StandardOutput.ReadToEndAsync();
                await searchProcess.WaitForExitAsync();

                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length >= 2)
                {
                    var videoId = lines[1];
                    track.YoutubeUrl = $"https://youtube.com/watch?v={videoId}";

                    // Créer le nom de fichier selon le template
                    var template = _configService.Settings.FileNameTemplate;
                    var fileName = template
                        .Replace("{artiste}", track.Artist)
                        .Replace("{titre}", track.Title)
                        .Replace("{album}", track.Album)
                        .Split(Path.GetInvalidFileNameChars())
                        .Aggregate((a, b) => a + b) + ".mp3";

                    var filePath = Path.Combine(outputPath, fileName);

                    // Déterminer la qualité audio
                    var audioQuality = _configService.Settings.AudioQuality.Replace(" kbps", "");

                    // Télécharger la vidéo
                    var downloadProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = _ytDlpPath,
                            Arguments = $"-x --audio-format mp3 --audio-quality {audioQuality}k --ffmpeg-location \"{_ffmpegPath}\" " +
                                      $"--embed-thumbnail " +
                                      $"--parse-metadata \"title:{track.Title}\" " +
                                      $"--parse-metadata \"artist:{track.Artist}\" " +
                                      $"--parse-metadata \"album_artist:{track.Artist}\" " +
                                      $"--ppa \"ffmpeg:-metadata title=\\\"{track.Title}\\\" " +
                                      $"-metadata artist=\\\"{track.Artist}\\\" " +
                                      $"-metadata album=\\\"{track.Album}\\\" " +
                                      (!string.IsNullOrEmpty(track.Genre) ? $"-metadata genre=\\\"{track.Genre}\\\" " : "") +
                                      "\" " +
                                      $"-o \"{filePath}\" {track.YoutubeUrl}",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    var progressRegex = new Regex(@"\[download\]\s+(\d+\.?\d*)%");
                    downloadProcess.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            var match = progressRegex.Match(e.Data);
                            if (match.Success && double.TryParse(match.Groups[1].Value, out double progress))
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    track.DownloadProgress = progress;
                                    track.Status = $"Téléchargement : {progress:F1}%";
                                    ProgressChanged?.Invoke(this, (track.Status, progress));
                                });
                            }
                            LogMessage?.Invoke(this, e.Data);
                        }
                    };

                    downloadProcess.Start();
                    downloadProcess.BeginOutputReadLine();
                    string error = await downloadProcess.StandardError.ReadToEndAsync();
                    await downloadProcess.WaitForExitAsync();

                    if (downloadProcess.ExitCode == 0 && File.Exists(filePath))
                    {
                        if (_configService.Settings.VerifyDownloads)
                        {
                            // Vérifier l'intégrité du fichier
                            var verifyProcess = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = _ffmpegPath,
                                    Arguments = $"-v error -i \"{filePath}\" -f null -",
                                    RedirectStandardError = true,
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                }
                            };

                            verifyProcess.Start();
                            var verifyError = await verifyProcess.StandardError.ReadToEndAsync();
                            await verifyProcess.WaitForExitAsync();

                            if (verifyProcess.ExitCode != 0)
                            {
                                throw new Exception($"Le fichier téléchargé est corrompu : {verifyError}");
                            }
                        }

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            track.Status = "Téléchargé";
                            track.DownloadProgress = 100;
                        });
                        LogMessage?.Invoke(this, $"Téléchargé : {filePath}");
                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            track.Status = "Erreur";
                            track.DownloadProgress = 0;
                        });
                        LogMessage?.Invoke(this, $"Erreur : {error}");
                        throw new Exception($"Erreur de téléchargement : {error}");
                    }
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        track.Status = "Non trouvé";
                        track.DownloadProgress = 0;
                    });
                    throw new Exception("Vidéo non trouvée sur YouTube");
                }
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    track.Status = "Erreur";
                    track.DownloadProgress = 0;
                });
                LogMessage?.Invoke(this, $"Erreur : {ex.Message}");
                throw;
            }
        }
    }
} 