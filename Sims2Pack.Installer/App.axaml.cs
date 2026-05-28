using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Sims2Pack.Installer.ViewModels;
using Sims2Pack.Installer.Views;

namespace Sims2Pack.Installer;

public partial class App : Application
{
    private IClassicDesktopStyleApplicationLifetime? _desktop;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;
            var args = desktop.Args ?? System.Array.Empty<string>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(args),
            };

            // On macOS, double-clicking a .Sims2Pack file in Finder sends an
            // Apple Event ("openDocument") rather than command-line args. Hook
            // the activation feature so the file path gets to the VM.
            // Same plumbing handles "Open With" from Finder when the app's
            // already running — we swap DataContext to a fresh VM with the
            // new file.
            if (TryGetFeature(typeof(IActivatableLifetime)) is IActivatableLifetime activatable)
                activatable.Activated += OnActivated;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnActivated(object? sender, ActivatedEventArgs e)
    {
        if (e is not FileActivatedEventArgs fileArgs || _desktop?.MainWindow is null)
            return;

        foreach (var file in fileArgs.Files)
        {
            var path = file.TryGetLocalPath();
            if (string.IsNullOrEmpty(path)) continue;
            _desktop.MainWindow.DataContext = new MainWindowViewModel(new string[] { path });
            return; // one pack at a time
        }
    }
}
