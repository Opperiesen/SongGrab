using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SongGrab.UI.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SongGrab.UI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly UpdateService _updateService;
        private readonly ConfigurationService _configService;
        private readonly IDialogService _dialogService;

        public SettingsViewModel(ConfigurationService configService, IDialogService dialogService)
        {
            _configService = configService;
            _dialogService = dialogService;
            _updateService = new UpdateService();

            // Charger les paramètres actuels
            LoadSettings();

            // Initialiser les templates de nom de fichier
            FileNameTemplates = new List<string>
            {
                "{artiste} - {titre}",
                "{titre} ({artiste})",
                "{artiste} - {album} - {titre}",
                "{titre}",
                "{artiste}/{album}/{titre}"
            };
            SelectedFileNameTemplate = _configService.Settings.FileNameTemplate;

            // Initialiser les qualités audio
            AudioQualities = new List<string>
            {
                "128 kbps",
                "192 kbps",
                "320 kbps"
            };
            SelectedAudioQuality = _configService.Settings.AudioQuality;
        }

        [ObservableProperty]
        private bool isDarkTheme;

        [ObservableProperty]
        private bool checkForUpdatesAtStartup = true;

        [ObservableProperty]
        private bool createPlaylistFolder = true;

        [ObservableProperty]
        private string updateStatus = "Aucune mise à jour disponible";

        [ObservableProperty]
        private bool compactMode;

        [ObservableProperty]
        private bool verifyDownloads = true;

        [ObservableProperty]
        private List<string> fileNameTemplates = new();

        [ObservableProperty]
        private string selectedFileNameTemplate = "{artiste} - {titre}";

        [ObservableProperty]
        private List<string> audioQualities = new();

        [ObservableProperty]
        private string selectedAudioQuality = "320 kbps";

        public string Version => $"{typeof(SettingsViewModel).Assembly.GetName().Version}";

        private void LoadSettings()
        {
            var settings = _configService.Settings;

            CompactMode = settings.CompactMode;
            CreatePlaylistFolder = settings.CreatePlaylistFolder;
            VerifyDownloads = settings.VerifyDownloads;
            CheckForUpdatesAtStartup = settings.CheckForUpdatesAtStartup;
            SelectedFileNameTemplate = settings.FileNameTemplate;
            SelectedAudioQuality = settings.AudioQuality;
        }

        [RelayCommand]
        private void Save()
        {
            var settings = _configService.Settings;

            settings.CompactMode = CompactMode;
            settings.CreatePlaylistFolder = CreatePlaylistFolder;
            settings.VerifyDownloads = VerifyDownloads;
            settings.CheckForUpdatesAtStartup = CheckForUpdatesAtStartup;
            settings.FileNameTemplate = SelectedFileNameTemplate;
            settings.AudioQuality = SelectedAudioQuality;

            _configService.SaveSettings();
            _dialogService.Close();
        }

        [RelayCommand]
        private void Cancel()
        {
            _dialogService.Close();
        }

        partial void OnIsDarkThemeChanged(bool value)
        {
            // La sauvegarde se fera lors de la validation
        }

        partial void OnCompactModeChanged(bool value)
        {
            // La sauvegarde se fera lors de la validation
        }

        partial void OnVerifyDownloadsChanged(bool value)
        {
            // La sauvegarde se fera lors de la validation
        }

        partial void OnSelectedFileNameTemplateChanged(string value)
        {
            // La sauvegarde se fera lors de la validation
        }

        partial void OnSelectedAudioQualityChanged(string value)
        {
            // La sauvegarde se fera lors de la validation
        }

        partial void OnCheckForUpdatesAtStartupChanged(bool value)
        {
            // La sauvegarde se fera lors de la validation
        }

        partial void OnCreatePlaylistFolderChanged(bool value)
        {
            // La sauvegarde se fera lors de la validation
        }

        [RelayCommand]
        private async Task CheckForUpdates()
        {
            try
            {
                UpdateStatus = "Vérification des mises à jour...";
                var updateInfo = await _updateService.CheckForUpdates();
                
                if (updateInfo.IsUpdateAvailable)
                {
                    UpdateStatus = $"Une mise à jour est disponible : v{updateInfo.LatestVersion}";
                }
                else
                {
                    UpdateStatus = "Vous utilisez la dernière version";
                }
            }
            catch (Exception ex)
            {
                UpdateStatus = $"Erreur lors de la vérification : {ex.Message}";
            }
        }

        [RelayCommand]
        private void OpenDonation()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://paypal.me/gabinlgn1",
                UseShellExecute = true
            });
        }
    }
} 