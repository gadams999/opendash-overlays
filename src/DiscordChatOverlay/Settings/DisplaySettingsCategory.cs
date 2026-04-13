using System.Windows;
using System.Windows.Controls;
using RadioButton = System.Windows.Controls.RadioButton;
using Orientation = System.Windows.Controls.Orientation;
using OpenDash.DiscordChatOverlay.Models;
using OpenDash.OverlayCore.Settings;

namespace OpenDash.DiscordChatOverlay.Settings;

public class DisplaySettingsCategory : ISettingsCategory
{
    private readonly AppSettings _settings;

    private RadioButton? _speakersOnlyRadio;
    private RadioButton? _allMembersRadio;
    private Slider?      _graceSlider;
    private TextBlock?   _graceValueLabel;
    private Slider?      _debounceSlider;
    private TextBlock?   _debounceValueLabel;

    public string CategoryName => "Display";
    public int SortOrder       => 20;

    public DisplaySettingsCategory(AppSettings settings)
    {
        _settings = settings;
    }

    public FrameworkElement CreateContent()
    {
        var panel = new StackPanel { Margin = new Thickness(16) };

        // ── Display mode ───────────────────────────────────────────────────

        panel.Children.Add(new TextBlock
        {
            Text       = "Display mode",
            FontSize   = 14,
            FontWeight = FontWeights.SemiBold,
            Margin     = new Thickness(0, 0, 0, 8)
        });

        _speakersOnlyRadio = new RadioButton
        {
            Content   = "Speakers only (active + fading speakers)",
            GroupName = "DisplayMode",
            Margin    = new Thickness(0, 0, 0, 4),
            IsChecked = _settings.DisplayMode == DisplayMode.SpeakersOnly
        };
        panel.Children.Add(_speakersOnlyRadio);

        _allMembersRadio = new RadioButton
        {
            Content   = "All members (speakers first, then silent)",
            GroupName = "DisplayMode",
            Margin    = new Thickness(0, 0, 0, 16),
            IsChecked = _settings.DisplayMode == DisplayMode.AllMembers
        };
        panel.Children.Add(_allMembersRadio);

        // ── Grace period (fade duration) ───────────────────────────────────

        var graceRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };
        graceRow.Children.Add(new TextBlock
        {
            Text     = "Fade duration (seconds)",
            FontSize = 14,
            Width    = 200,
            VerticalAlignment = VerticalAlignment.Center
        });
        _graceValueLabel = new TextBlock
        {
            Text     = _settings.GracePeriodSeconds.ToString("F1"),
            FontSize = 14,
            Width    = 40,
            VerticalAlignment = VerticalAlignment.Center
        };
        graceRow.Children.Add(_graceValueLabel);
        panel.Children.Add(graceRow);

        _graceSlider = new Slider
        {
            Minimum      = 0.0,
            Maximum      = 2.0,
            Value        = _settings.GracePeriodSeconds,
            TickFrequency= 0.1,
            SmallChange  = 0.1,
            LargeChange  = 0.5,
            Margin       = new Thickness(0, 0, 0, 16)
        };
        _graceSlider.ValueChanged += (_, e) =>
        {
            if (_graceValueLabel != null)
                _graceValueLabel.Text = e.NewValue.ToString("F1");
        };
        panel.Children.Add(_graceSlider);

        // ── Debounce threshold (noise gate) ────────────────────────────────

        var debounceRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };
        debounceRow.Children.Add(new TextBlock
        {
            Text     = "Noise gate (ms, 0 = disabled)",
            FontSize = 14,
            Width    = 200,
            VerticalAlignment = VerticalAlignment.Center
        });
        _debounceValueLabel = new TextBlock
        {
            Text     = _settings.DebounceThresholdMs.ToString(),
            FontSize = 14,
            Width    = 40,
            VerticalAlignment = VerticalAlignment.Center
        };
        debounceRow.Children.Add(_debounceValueLabel);
        panel.Children.Add(debounceRow);

        _debounceSlider = new Slider
        {
            Minimum      = 0,
            Maximum      = 1000,
            Value        = _settings.DebounceThresholdMs,
            TickFrequency= 50,
            SmallChange  = 10,
            LargeChange  = 100,
            Margin       = new Thickness(0, 0, 0, 8)
        };
        _debounceSlider.ValueChanged += (_, e) =>
        {
            if (_debounceValueLabel != null)
                _debounceValueLabel.Text = ((int)e.NewValue).ToString();
        };
        panel.Children.Add(_debounceSlider);

        return panel;
    }

    public void SaveValues()
    {
        if (_speakersOnlyRadio?.IsChecked == true)
            _settings.DisplayMode = DisplayMode.SpeakersOnly;
        else
            _settings.DisplayMode = DisplayMode.AllMembers;

        if (_graceSlider != null)
            _settings.GracePeriodSeconds = _graceSlider.Value;

        if (_debounceSlider != null)
            _settings.DebounceThresholdMs = (int)_debounceSlider.Value;

        _settings.Save();
    }

    public void LoadValues()
    {
        if (_speakersOnlyRadio != null)
            _speakersOnlyRadio.IsChecked = _settings.DisplayMode == DisplayMode.SpeakersOnly;
        if (_allMembersRadio != null)
            _allMembersRadio.IsChecked = _settings.DisplayMode == DisplayMode.AllMembers;

        if (_graceSlider != null)
        {
            _graceSlider.Value = _settings.GracePeriodSeconds;
            if (_graceValueLabel != null)
                _graceValueLabel.Text = _settings.GracePeriodSeconds.ToString("F1");
        }

        if (_debounceSlider != null)
        {
            _debounceSlider.Value = _settings.DebounceThresholdMs;
            if (_debounceValueLabel != null)
                _debounceValueLabel.Text = _settings.DebounceThresholdMs.ToString();
        }
    }
}
