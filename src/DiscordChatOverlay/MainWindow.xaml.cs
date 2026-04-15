using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using OpenDash.DiscordChatOverlay.ViewModels;

namespace OpenDash.DiscordChatOverlay;

public partial class MainWindow : Window
{
    private const int GWL_EXSTYLE      = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED    = 0x00080000;

    // Use the Ptr variants — on a 64-bit process SetWindowLong (32-bit) can corrupt
    // WPF's internal WndProc chain which also uses SetWindowLongPtr (GWLP_WNDPROC).
    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint GetWindowLongPtr(IntPtr hwnd, int index);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetWindowLongPtr(IntPtr hwnd, int index, nint newStyle);

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
        nint style = GetWindowLongPtr(_hwnd, GWL_EXSTYLE);
        SetWindowLongPtr(_hwnd, GWL_EXSTYLE, style | WS_EX_TRANSPARENT | WS_EX_LAYERED);
    }

    /// <summary>Suspends click-through so the user can interact with the window (e.g. drag to reposition).</summary>
    public void SuspendClickThrough()
    {
        if (_hwnd == IntPtr.Zero) return;
        nint style = GetWindowLongPtr(_hwnd, GWL_EXSTYLE);
        SetWindowLongPtr(_hwnd, GWL_EXSTYLE, style & ~WS_EX_TRANSPARENT);
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
