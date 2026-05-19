using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Sims2Pack_Installer;

namespace Sims2Pack.Installer.ViewModels;

/// <summary>
/// View-model wrapper around a single <see cref="S2CPackage"/>. Translates
/// the model's plain-data display state (IsOverwriting / IsBodyShopIncomplete
/// / IsDuplicated) into Avalonia bindings so the model stays UI-free.
/// </summary>
public partial class PackageItemViewModel : ObservableObject
{
    private static readonly IBrush OverwriteBrush  = new SolidColorBrush(Color.FromRgb(255, 200, 200)); // red-ish
    private static readonly IBrush DuplicatedBrush = new SolidColorBrush(Color.FromRgb(255, 220, 230)); // pink-ish
    private static readonly IBrush IncompleteBrush = new SolidColorBrush(Color.FromRgb(200, 220, 240)); // light blue-ish
    private static readonly IBrush DefaultBrush    = Brushes.Transparent;

    public PackageItemViewModel(S2CPackage package)
    {
        Package = package;
        _isSelected = package.enabled;
    }

    public S2CPackage Package { get; }

    public string Name => Package.info?.name ?? System.IO.Path.GetFileNameWithoutExtension(Package.fileName);
    public string TypeName => Package.DisplayTypeName;
    public string Version => Package.info?.version ?? string.Empty;
    public string Author  => Package.info?.author  ?? string.Empty;
    public string Description => Package.info?.description ?? string.Empty;

    public IBrush BackgroundBrush =>
        Package.IsOverwriting      ? OverwriteBrush  :
        Package.IsDuplicated       ? DuplicatedBrush :
        Package.IsBodyShopIncomplete ? IncompleteBrush :
                                     DefaultBrush;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty(ref _isSelected, value))
                Package.enabled = value;
        }
    }
}
