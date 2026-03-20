using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using OpenDash.OverlayCore.Services;

namespace OpenDash.OverlayCore.Settings;

/// <summary>
/// Built-in About category. Auto-registered by <see cref="MaterialSettingsWindow"/> at SortOrder=999.
/// </summary>
public sealed class AboutSettingsCategory : ISettingsCategory
{
    public string CategoryName => "About";
    public int SortOrder => 999;

    public FrameworkElement CreateContent()
    {
        var panel = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };

        // App name
        var appName = new TextBlock
        {
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 8)
        };
        appName.SetResourceReference(TextBlock.ForegroundProperty, "ThemeForeground");
        appName.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding()
        {
            Source = Assembly.GetEntryAssembly()?.GetName().Name ?? "WheelOverlay"
        });
        appName.Text = Assembly.GetEntryAssembly()?.GetName().Name ?? "WheelOverlay";
        panel.Children.Add(appName);

        // Version
        var version = Assembly.GetEntryAssembly()?.GetName().Version;
        var versionString = version != null
            ? $"Version {version.Major}.{version.Minor}.{version.Build}"
            : "Version unknown";

        var versionText = new TextBlock
        {
            Text = versionString,
            FontSize = 14,
            Margin = new Thickness(0, 0, 0, 16)
        };
        versionText.SetResourceReference(TextBlock.ForegroundProperty, "ThemeForeground");
        panel.Children.Add(versionText);

        // GitHub link
        var linkText = new TextBlock { Margin = new Thickness(0, 0, 0, 16) };
        linkText.SetResourceReference(TextBlock.ForegroundProperty, "ThemeForeground");

        var hyperlink = new Hyperlink
        {
            NavigateUri = new Uri("https://github.com/gadams999/wheel-overlay")
        };
        hyperlink.SetResourceReference(Hyperlink.ForegroundProperty, "ThemeAccent");
        hyperlink.Inlines.Add("GitHub Repository");
        hyperlink.RequestNavigate += OnHyperlinkNavigate;
        linkText.Inlines.Add(hyperlink);
        panel.Children.Add(linkText);

        return panel;
    }

    private static void OnHyperlinkNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
        catch (Exception ex)
        {
            LogService.Error("Failed to open GitHub link", ex);
        }
    }

    public void SaveValues() { /* no-op */ }
    public void LoadValues() { /* no-op */ }
}
