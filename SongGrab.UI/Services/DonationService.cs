using System.Diagnostics;

namespace SongGrab.UI.Services
{
    public class DonationService
    {
        private const string PayPalDonateUrl = "https://paypal.me/gabinlgn1";
        private const string GithubSponsorUrl = "https://github.com/sponsors/VOTRE_USERNAME";
        private const string BuyMeACoffeeUrl = "https://www.buymeacoffee.com/VOTRE_USERNAME";

        public void OpenPayPalDonation()
        {
            OpenUrl(PayPalDonateUrl);
        }

        public void OpenGithubSponsors()
        {
            OpenUrl(GithubSponsorUrl);
        }

        public void OpenBuyMeACoffee()
        {
            OpenUrl(BuyMeACoffeeUrl);
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
                // Gérer les erreurs d'ouverture d'URL
            }
        }
    }
} 