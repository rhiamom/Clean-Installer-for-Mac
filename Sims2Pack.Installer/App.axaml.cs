using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Sims2Pack.Installer.ViewModels;
using Sims2Pack.Installer.Views;

namespace Sims2Pack.Installer;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var args = desktop.Args ?? System.Array.Empty<string>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(args),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
