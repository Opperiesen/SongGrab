using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SongGrab.UI.Services;
using SongGrab.UI.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace SongGrab.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DownloadService _downloadService;
        private readonly ConfigurationService _configService;
        private readonly UpdateService _updateService;
        private readonly DonationService _donationService;
        private readonly ThemeService _themeService;

        [ObservableProperty]
        private string playlistId = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilteredTracks))]
        private string searchQuery = string.Empty;

        [ObservableProperty]
        private string? downloadPath;

        [ObservableProperty]
        private ObservableCollection<TrackViewModel> tracks = new();

        [ObservableProperty]
        private ObservableCollection<TrackViewModel> filteredTracks = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private bool isDarkTheme;

        [ObservableProperty]
        private bool compactMode;

        public MainViewModel()
        {
            _configService = new ConfigurationService();
            _updateService = new UpdateService();
            _donationService = new DonationService();
            _themeService = new ThemeService();
            _downloadService = new DownloadService(_configService);

            LoadSettings();
            _themeService.SetTheme(IsDarkTheme);

            _ = CheckDependenciesAsync();

            if (_configService.Settings.CheckForUpdatesAtStartup)
            {
                _ = CheckForUpdatesAsync();
            }
        }

        partial void OnIsDarkThemeChanged(bool value)
        {
            _themeService.SetTheme(value);
            _configService.Settings.IsDarkTheme = value;
            _configService.SaveSettings();
        }

        partial void OnSearchQueryChanged(string value)
        {
            FilterTracks();
        }

        private void FilterTracks()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                FilteredTracks = new ObservableCollection<TrackViewModel>(Tracks);
                return;
            }

            var query = SearchQuery.ToLower();
            var filtered = Tracks.Where(t => 
                t.Title?.ToLower().Contains(query) == true || 
                t.Artist?.ToLower().Contains(query) == true || 
                t.Album?.ToLower().Contains(query) == true
            ).ToList();

            FilteredTracks = new ObservableCollection<TrackViewModel>(filtered);
        }

        private async Task CheckDependenciesAsync()
        {
            if (!await _downloadService.CheckDependencies())
            {
                StatusMessage = "Erreur : yt-dlp.exe et/ou ffmpeg.exe non trouvés. Veuillez les placer dans le dossier de l'application.";
            }
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                var updateInfo = await _updateService.CheckForUpdates();
                if (updateInfo.IsUpdateAvailable)
                {
                    StatusMessage = $"Une mise à jour est disponible : v{updateInfo.LatestVersion}";
                }
            }
            catch
            {
                // Ignorer les erreurs de vérification de mise à jour
            }
        }

        [RelayCommand]
        private async Task LoadPlaylist()
        {
            if (string.IsNullOrWhiteSpace(PlaylistId))
            {
                StatusMessage = "Veuillez entrer un ID de playlist valide";
                return;
            }

            IsLoading = true;
            StatusMessage = "Chargement de la playlist...";
            Tracks.Clear();
            FilteredTracks.Clear();

            try
            {
                if (!long.TryParse(PlaylistId, out long playlistId))
                {
                    StatusMessage = "ID de playlist invalide";
                    return;
                }

                var tracks = await _downloadService.LoadPlaylist(playlistId);
                foreach (var track in tracks)
                {
                    Tracks.Add(track);
                }

                FilterTracks();
                StatusMessage = $"{Tracks.Count} pistes trouvées";

                _configService.Settings.LastUsedPlaylistId = PlaylistId;
                _configService.SaveSettings();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur : {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void SelectDownloadFolder()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Sélectionner le dossier de téléchargement",
                UseDescriptionForTitle = true,
                SelectedPath = DownloadPath ?? string.Empty
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DownloadPath = dialog.SelectedPath ?? string.Empty;
                _configService.Settings.DefaultDownloadPath = DownloadPath;
                _configService.SaveSettings();
            }
        }

        [RelayCommand]
        private void SelectAllTracks()
        {
            foreach (var track in Tracks)
            {
                track.IsSelected = true;
            }
        }

        [RelayCommand]
        private void DeselectAllTracks()
        {
            foreach (var track in Tracks)
            {
                track.IsSelected = false;
            }
        }

        [RelayCommand]
        private async Task DownloadSelectedTracks()
        {
            if (string.IsNullOrEmpty(DownloadPath))
            {
                StatusMessage = "Veuillez sélectionner un dossier de destination";
                return;
            }

            var selectedTracks = Tracks.Where(t => t.IsSelected).ToList();
            if (!selectedTracks.Any())
            {
                StatusMessage = "Veuillez sélectionner au moins une piste à télécharger";
                return;
            }

            IsLoading = true;
            StatusMessage = "Téléchargement en cours...";

            try
            {
                foreach (var track in selectedTracks)
                {
                    try
                    {
                        await _downloadService.DownloadTrack(track, DownloadPath);
                    }
                    catch (Exception ex)
                    {
                        StatusMessage = $"Erreur lors du téléchargement de {track.Title} : {ex.Message}";
                    }
                }

                StatusMessage = "Téléchargement terminé";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur : {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
        }

        [RelayCommand]
        private void OpenDonation()
        {
            _donationService.OpenPayPalDonation();
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchQuery = string.Empty;
        }

        [RelayCommand]
        private async Task OpenSettings()
        {
            var settingsVm = new SettingsViewModel(_configService, new DialogService())
            {
                IsDarkTheme = IsDarkTheme,
                CheckForUpdatesAtStartup = _configService.Settings.CheckForUpdatesAtStartup,
                CreatePlaylistFolder = _configService.Settings.CreatePlaylistFolder
            };

            var view = new SettingsView
            {
                DataContext = settingsVm
            };

            var result = await MaterialDesignThemes.Wpf.DialogHost.Show(view, "RootDialog");
            
            // Mettre à jour les paramètres locaux
            IsDarkTheme = settingsVm.IsDarkTheme;
        }

        private void LoadSettings()
        {
            var settings = _configService.Settings;
            DownloadPath = settings.DefaultDownloadPath;
            IsDarkTheme = settings.IsDarkTheme;
            CompactMode = settings.CompactMode;
            PlaylistId = settings.LastUsedPlaylistId;
        }

        partial void OnCompactModeChanged(bool value)
        {
            _configService.Settings.CompactMode = value;
            _configService.SaveSettings();
        }
    }
} 