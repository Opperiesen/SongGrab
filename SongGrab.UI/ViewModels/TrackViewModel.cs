using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace SongGrab.UI.ViewModels
{
    public partial class TrackViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string artist = string.Empty;

        [ObservableProperty]
        private string album = string.Empty;

        [ObservableProperty]
        private TimeSpan duration;

        [ObservableProperty]
        private string genre = string.Empty;

        [ObservableProperty]
        private double downloadProgress;

        [ObservableProperty]
        private string status = "En attente";

        [ObservableProperty]
        private string youtubeUrl = string.Empty;

        [ObservableProperty]
        private string deezerUrl = string.Empty;

        public long DeezerTrackId { get; set; }
    }
} 