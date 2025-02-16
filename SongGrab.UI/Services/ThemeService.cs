using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using MaterialDesignColors;

namespace SongGrab.UI.Services
{
    public class ThemeService
    {
        private readonly ResourceDictionary _themeColors;

        public ThemeService()
        {
            _themeColors = new ResourceDictionary();

            // Couleurs principales - Un dégradé de bleu profond à turquoise
            var primaryBase = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1E3D59");      // Bleu profond
            var primaryLight = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2B5876");     // Bleu océan
            var primaryAccent = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#17C3B2");    // Turquoise vif
            var primaryDark = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#152F45");      // Bleu nuit

            // Couleurs secondaires - Tons chauds pour le contraste
            var secondaryBase = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF6B6B");    // Corail vif
            var secondaryLight = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA07A");   // Pêche
            var secondaryDark = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E64C4C");    // Rouge profond

            // Couleurs neutres
            var surfaceColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F7F9FC");     // Gris très clair
            var backgroundColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF");   // Blanc pur
            var textPrimary = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2C3E50");      // Gris foncé
            var textSecondary = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#607D8B");    // Gris bleuté

            // Couleurs d'accentuation
            var successColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4CAF50");     // Vert
            var warningColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFC107");     // Jaune
            var errorColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF5252");       // Rouge

            // Application des couleurs principales
            _themeColors.Add("MaterialDesignPaper", new SolidColorBrush(backgroundColor));
            _themeColors.Add("MaterialDesignBackground", new SolidColorBrush(surfaceColor));
            _themeColors.Add("PrimaryHueLightBrush", new SolidColorBrush(primaryLight));
            _themeColors.Add("PrimaryHueMidBrush", new SolidColorBrush(primaryBase));
            _themeColors.Add("PrimaryHueDarkBrush", new SolidColorBrush(primaryDark));
            _themeColors.Add("MaterialDesignBody", new SolidColorBrush(textPrimary));
            _themeColors.Add("MaterialDesignBodyLight", new SolidColorBrush(textSecondary));

            // Application des couleurs secondaires
            _themeColors.Add("SecondaryHueLightBrush", new SolidColorBrush(secondaryLight));
            _themeColors.Add("SecondaryHueMidBrush", new SolidColorBrush(secondaryBase));
            _themeColors.Add("SecondaryHueDarkBrush", new SolidColorBrush(secondaryDark));

            // Couleurs d'accentuation
            _themeColors.Add("MaterialDesignValidationErrorBrush", new SolidColorBrush(errorColor));
            _themeColors.Add("MaterialDesignSelection", new SolidColorBrush(primaryAccent));

            // Couleurs pour les états
            _themeColors.Add("SuccessBrush", new SolidColorBrush(successColor));
            _themeColors.Add("WarningBrush", new SolidColorBrush(warningColor));
            _themeColors.Add("ErrorBrush", new SolidColorBrush(errorColor));

            // Appliquer les couleurs
            ApplyTheme();
        }

        public void SetTheme(bool isDark)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            var app = System.Windows.Application.Current;
            if (app == null) return;

            // Appliquer les couleurs
            foreach (var key in _themeColors.Keys)
            {
                if (app.Resources.Contains(key))
                {
                    app.Resources[key] = _themeColors[key];
                }
                else
                {
                    app.Resources.Add(key, _themeColors[key]);
                }
            }

            // Configurer le thème MaterialDesign
            var bundledTheme = app.Resources.MergedDictionaries
                .OfType<BundledTheme>()
                .FirstOrDefault();

            if (bundledTheme != null)
            {
                bundledTheme.BaseTheme = BaseTheme.Light;
                bundledTheme.PrimaryColor = PrimaryColor.Blue;
                bundledTheme.SecondaryColor = SecondaryColor.DeepOrange;
            }

            // Forcer la mise à jour de l'interface
            foreach (Window window in app.Windows)
            {
                window.UpdateLayout();
            }
        }
    }
} 