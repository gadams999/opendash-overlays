using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using OpenDash.DiscordChatOverlay.ViewModels;

namespace OpenDash.DiscordChatOverlay;

public partial class MainWindow : Window
{
    private const int GWL_EXSTYLE     = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED    = 0x00080000;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    private IntPtr _hwnd;

    public MainWindow(OverlayViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        SourceInitialized += OnSourceInitialized;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        _hwnd = new WindowInteropHelper(this).Handle;
        ApplyClickThrough();
    }

    private void ApplyClickThrough()
    {
        int style = GetWindowLong(_hwnd, GWL_EXSTYLE);
        SetWindowLong(_hwnd, GWL_EXSTYLE, style | WS_EX_TRANSPARENT | WS_EX_LAYERED);
    }

    /// <summary>Suspends click-through so the user can interact with the window (e.g. drag to reposition).</summary>
    public void SuspendClickThrough()
    {
        if (_hwnd == IntPtr.Zero) return;
        int style = GetWindowLong(_hwnd, GWL_EXSTYLE);
        SetWindowLong(_hwnd, GWL_EXSTYLE, style & ~WS_EX_TRANSPARENT);
    }

    /// <summary>Restores click-through after a drag or interaction is complete.</summary>
    public void RestoreClickThrough()
    {
        if (_hwnd == IntPtr.Zero) return;
        ApplyClickThrough();
    }

    /// <summary>
    /// Shows the drag handle bar and suspends click-through so the overlay can be repositioned.
    /// Called when the Settings window opens.
    /// </summary>
    public void EnableDragMode()
    {
        Show();
        SuspendClickThrough();
        DragHandleBorder.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Hides the drag handle bar and restores click-through.
    /// Called when the Settings window closes.
    /// </summary>
    public void DisableDragMode()
    {
        DragHandleBorder.Visibility = Visibility.Collapsed;
        RestoreClickThrough();
    }

    private void DragHandle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();
    }
}
