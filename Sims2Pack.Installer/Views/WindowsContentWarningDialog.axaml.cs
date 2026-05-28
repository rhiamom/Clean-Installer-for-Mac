using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Sims2Pack.Installer.Views;

public class FlaggedItem
{
    public string Name { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public enum WindowsContentDialogResult
{
    Cancel,
    OverrideAccepted,
}

public partial class WindowsContentWarningDialog : Window
{
    public WindowsContentWarningDialog() => InitializeComponent();

    public WindowsContentWarningDialog(
        IReadOnlyList<FlaggedItem> hardBlock,
        IReadOnlyList<FlaggedItem> softWarn) : this()
    {
        HardBlockList.ItemsSource = hardBlock;
        SoftWarnList.ItemsSource  = softWarn;

        // Hide whichever section has no items.
        bool hasHard = hardBlock.Count > 0;
        bool hasSoft = softWarn.Count > 0;
        HardBlockHeading.IsVisible = hasHard;
        HardBlockBorder.IsVisible  = hasHard;
        SoftWarnHeading.IsVisible  = hasSoft;
        SoftWarnBorder.IsVisible   = hasSoft;

        // Hard block wins: if any hard-block items, no override path.
        if (hasHard)
        {
            CancelButton.IsVisible   = false;
            OverrideButton.IsVisible = false;
            OkButton.IsVisible       = true;
        }
        else
        {
            CancelButton.IsVisible   = true;
            OverrideButton.IsVisible = true;
            OkButton.IsVisible       = false;
        }
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)   => Close(WindowsContentDialogResult.Cancel);
    private void OnOverrideClick(object? sender, RoutedEventArgs e) => Close(WindowsContentDialogResult.OverrideAccepted);
}
