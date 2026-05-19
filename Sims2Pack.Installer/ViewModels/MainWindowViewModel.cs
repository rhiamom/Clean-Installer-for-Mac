using System;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using S2;
using Sims2Pack_Installer;

namespace Sims2Pack.Installer.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly S2Sims2Pack? _pack;

    public MainWindowViewModel() : this(Array.Empty<string>()) { }

    public MainWindowViewModel(string[] args)
    {
        WindowTitle = "Sims2Pack Clean Installer";
        Items = new ObservableCollection<PackageItemViewModel>();

        // Initialise legacy config (loads packages.txt / localpackages.txt).
        Config.LoadConfig();

        if (args.Length == 1 && File.Exists(args[0]) &&
            string.Equals(Path.GetExtension(args[0]), ".Sims2Pack",
                StringComparison.OrdinalIgnoreCase))
        {
            _pack = new S2Sims2Pack(args[0]);
            PackName = Path.GetFileNameWithoutExtension(args[0]);
            PackType = _pack.type;

            foreach (S2CPackage p in _pack.Items)
                Items.Add(new PackageItemViewModel(p));

            if (Items.Count > 0)
                SelectedItem = Items[0];
        }
        else
        {
            PackName = "No Sims2Pack opened";
            PackType = string.Empty;
            CanInstall = false;
        }
    }

    [ObservableProperty] private string _windowTitle = string.Empty;
    [ObservableProperty] private string _packName = string.Empty;
    [ObservableProperty] private string _packType = string.Empty;
    [ObservableProperty] private bool _previewImages = true;
    [ObservableProperty] private bool _removeFurniture;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _canInstall = true;

    public ObservableCollection<PackageItemViewModel> Items { get; }

    private PackageItemViewModel? _selectedItem;
    public PackageItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                OnPropertyChanged(nameof(SelectedVersion));
                OnPropertyChanged(nameof(SelectedAuthor));
                OnPropertyChanged(nameof(SelectedDescription));
            }
        }
    }

    public string SelectedVersion => _selectedItem?.Version ?? string.Empty;
    public string SelectedAuthor  => _selectedItem?.Author  ?? string.Empty;
    public string SelectedDescription => _selectedItem?.Description ?? string.Empty;

    [RelayCommand]
    private void Install()
    {
        if (_pack is null) return;

        if (!Sims2Directories.HasUserFolder)
        {
            StatusMessage = $"Sims 2 user folder not found at {Sims2Directories.UserFolder}";
            return;
        }

        bool ok;
        if (_pack.type == "Lot")
            ok = _pack.InstallLotPackage(false);
        else
            ok = _pack.InstallNormalPackage(Sims2Directories.Downloads, true);

        StatusMessage = ok
            ? "Files successfully installed."
            : "Install failed — see debug log.";
        CanInstall = false;
    }

    [RelayCommand] private void SaveAs() { /* v1: pack creation deferred */ }
    [RelayCommand] private void Update() { /* v1: recognition-DB update deferred */ }

    [RelayCommand]
    private void Cancel()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime
                is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow?.Close();
        }
    }
}
