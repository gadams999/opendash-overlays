using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using OpenDash.OverlayCore.Services;

namespace OpenDash.OverlayCore.Settings;

/// <summary>
/// Two-column settings window with left-side navigation.
/// Overlay apps call <see cref="RegisterCategory"/> to add panels;
/// <see cref="AboutSettingsCategory"/> is auto-registered at SortOrder=999.
/// </summary>
public partial class MaterialSettingsWindow : Window
{
    private readonly ObservableCollection<ISettingsCategory> _categories = new();
    private ISettingsCategory? _currentCategory;

    /// <summary>Fired after OK or Apply saves all category values.</summary>
    public event EventHandler? SettingsApplied;

    public MaterialSettingsWindow()
    {
        InitializeComponent();
        RegisterCategory(new AboutSettingsCategory());
    }

    /// <summary>
    /// Registers a category. Categories are displayed sorted ascending by <see cref="ISettingsCategory.SortOrder"/>.
    /// </summary>
    public void RegisterCategory(ISettingsCategory category)
    {
        _categories.Add(category);

        // Keep the ListBox items sorted by SortOrder
        var sorted = _categories.OrderBy(c => c.SortOrder).ToList();
        CategoryListBox.Items.Clear();
        foreach (var cat in sorted)
        {
            var item = new ListBoxItem
            {
                Content = cat.CategoryName,
                Tag = cat
            };
            CategoryListBox.Items.Add(item);
        }

        // Select the first item if nothing is selected yet
        if (CategoryListBox.SelectedIndex < 0 && CategoryListBox.Items.Count > 0)
            CategoryListBox.SelectedIndex = 0;
    }

    // -----------------------------------------------------------------------
    // Navigation
    // -----------------------------------------------------------------------

    private void CategoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategoryListBox.SelectedItem is not ListBoxItem selected)
            return;
        if (selected.Tag is not ISettingsCategory newCategory)
            return;
        if (ReferenceEquals(newCategory, _currentCategory))
            return;

        // Save the departing category
        try { _currentCategory?.SaveValues(); }
        catch (Exception ex) { LogService.Error("Error saving category values on navigation", ex); }

        // Load the arriving category
        _currentCategory = newCategory;
        try
        {
            FrameworkElement content = newCategory.CreateContent();
            CategoryContent.Content = content;
            newCategory.LoadValues();
        }
        catch (Exception ex)
        {
            LogService.Error($"Error loading category '{newCategory.CategoryName}'", ex);
        }
    }

    private void ContentScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        if (sender is ScrollViewer sv)
        {
            sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta / 3.0);
            e.Handled = true;
        }
    }

    // -----------------------------------------------------------------------
    // Buttons
    // -----------------------------------------------------------------------

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        SaveAll();
        Close();
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        SaveAll();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void SaveAll()
    {
        // Save the current category first (its controls hold the latest values)
        try { _currentCategory?.SaveValues(); }
        catch (Exception ex) { LogService.Error("Error saving current category", ex); }

        // Then save all other categories (they may hold previously loaded values)
        foreach (var cat in _categories.Where(c => !ReferenceEquals(c, _currentCategory)))
        {
            try { cat.SaveValues(); }
            catch (Exception ex) { LogService.Error($"Error saving category '{cat.CategoryName}'", ex); }
        }

        SettingsApplied?.Invoke(this, EventArgs.Empty);
    }
}
