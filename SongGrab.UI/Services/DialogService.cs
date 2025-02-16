using MaterialDesignThemes.Wpf;
using System;

namespace SongGrab.UI.Services
{
    public class DialogService : IDialogService
    {
        private readonly string _dialogIdentifier;

        public DialogService(string dialogIdentifier = "RootDialog")
        {
            _dialogIdentifier = dialogIdentifier;
        }

        public void Close()
        {
            DialogHost.Close(_dialogIdentifier);
        }
    }
} 