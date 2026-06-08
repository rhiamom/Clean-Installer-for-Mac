/***************************************************************************
 *   Copyright (C) 2004-2007 by Karol Rybak                                *
 *   Additional programming © 2010-2014 Mootilda                           *
 *   .NET 8 / Avalonia port © 2026 GramzeSweatshop (rhiamom@mac.com)       *
 *                                                                         *
 *   GNU GPLv2 or later, see LICENSE.                                      *
 ***************************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using S2;
using Sims2Pack.Installer.Views;
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
                Items.Add(new PackageItemViewModel(p, _pack));

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

    private InstallMode _selectedInstallMode = InstallMode.Everything;
    public InstallMode SelectedInstallMode
    {
        get => _selectedInstallMode;
        set
        {
            if (SetProperty(ref _selectedInstallMode, value))
            {
                OnPropertyChanged(nameof(IsEverythingMode));
                OnPropertyChanged(nameof(IsHouseOnlyMode));
                OnPropertyChanged(nameof(IsHouseAndFamilyMode));
                OnPropertyChanged(nameof(IsHouseWithoutHacksMode));
                ApplyInstallModeSelection();
            }
        }
    }

    public bool IsEverythingMode        => SelectedInstallMode == InstallMode.Everything;
    public bool IsHouseOnlyMode         => SelectedInstallMode == InstallMode.HouseOnly;
    public bool IsHouseAndFamilyMode    => SelectedInstallMode == InstallMode.HouseAndFamily;
    public bool IsHouseWithoutHacksMode => SelectedInstallMode == InstallMode.HouseWithoutHacks;

    [RelayCommand]
    private void SetInstallMode(InstallMode mode) => SelectedInstallMode = mode;

    // Mirrors Mootilda's four pictureBox click handlers (WinForm.cs ~919-1016):
    // the install-mode icons are bulk-selection helpers, not separate install
    // code paths. They flip Package.enabled on every item; InstallLotPackage
    // then skips items where !enabled.
    private void ApplyInstallModeSelection()
    {
        foreach (var item in Items)
        {
            item.IsSelected = SelectedInstallMode switch
            {
                InstallMode.HouseOnly         => item.Package.type == "Lot",
                InstallMode.HouseAndFamily    => item.Package.type is "Lot" or "Family" or "Person",
                InstallMode.HouseWithoutHacks => !(item.Package.info?.overwriting ?? false),
                _                             => true,
            };
        }
    }

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
                OnPropertyChanged(nameof(SelectedPreviewImage));
            }
        }
    }

    public string SelectedVersion => _selectedItem?.Version ?? string.Empty;
    public string SelectedAuthor  => _selectedItem?.Author  ?? string.Empty;
    public string SelectedDescription => _selectedItem?.Description ?? string.Empty;
    public Avalonia.Media.Imaging.Bitmap? SelectedPreviewImage => _selectedItem?.PreviewImage;

    [RelayCommand]
    private async Task Install()
    {
        if (_pack is null) return;

        if (!Sims2Directories.HasUserFolder)
        {
            StatusMessage = $"Sims 2 user folder not found at {Sims2Directories.UserFolder}";
            return;
        }

        // Safety check: detect items that reference Sims 2 EP/SP content
        // not natively on Mac. Hard-block tier (FT/AL/M&G/H&M) always aborts;
        // soft-warn tier (K&B/IKEA/Celebrations/Teen Style — extracted by
        // Huge Lunatic into standalone packages) lets the user override if
        // they have HL's extractions installed.
        var flags = _pack.DetectWindowsOnlyContent();
        if (flags.Count > 0)
        {
            var hardItems = new List<FlaggedItem>();
            var softItems = new List<FlaggedItem>();
            foreach (var f in flags)
            {
                var ui = new FlaggedItem
                {
                    Name   = f.Item.info?.name ?? Path.GetFileNameWithoutExtension(f.Item.fileName),
                    Reason = f.Reason,
                };
                if (f.Tier == S2Sims2Pack.ContentTier.HardBlock) hardItems.Add(ui);
                else                                             softItems.Add(ui);
            }

            if (Avalonia.Application.Current?.ApplicationLifetime
                    is IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow is not null)
            {
                var dialog = new WindowsContentWarningDialog(hardItems, softItems);
                var result = await dialog.ShowDialog<WindowsContentDialogResult>(desktop.MainWindow);
                if (hardItems.Count > 0)
                {
                    StatusMessage = "Install blocked — pack requires Sims 2 content not available on Mac.";
                    return;
                }
                if (result != WindowsContentDialogResult.OverrideAccepted)
                {
                    StatusMessage = "Install cancelled.";
                    return;
                }
                // Soft-warn override accepted — proceed.
            }
            else
            {
                // No window available somehow — fail safe by blocking.
                StatusMessage = "Install blocked — Windows-only content detected.";
                return;
            }
        }

        // Install-mode filtering happens via ApplyInstallModeSelection(),
        // which has already flipped Package.enabled on each item per the
        // selected mode. InstallLotPackage's loop skips !enabled items.
        //
        // Lot and Sim packs both install through the Teleport-import path: the
        // lot/sim record goes to Teleport as .Sims2Tmp with a .Sims2Import
        // descriptor, and on its next launch the game moves it into
        // Downloads/{crc}.package and registers it in ContentRegistry (verified
        // to match Aspyr's own installer). Lots then appear in the lots/houses
        // bin; downloaded Sims (BodyShop projects) appear in BodyShop. The
        // bundled CC parts drop straight into Downloads. Everything else is
        // loose CC and goes to Downloads directly.
        bool ok = (_pack.type == "Lot" || _pack.type == "Sim")
            ? _pack.InstallLotPackage(RemoveFurniture)
            : _pack.InstallNormalPackage(Sims2Directories.Downloads, true);

        StatusMessage = ok
            ? "Files successfully installed."
            : "Install failed — see debug log.";
        CanInstall = false;
    }

    // Mirrors Mootilda's buttonSave_Click (WinForm.cs ~662): show a save
    // dialog, write the pack via S2Sims2Pack.SaveAs which only emits
    // currently-enabled items, and atomically swap with a .bkp backup if the
    // target already exists. Reload-after-save is skipped — we just update
    // status and the displayed pack name.
    [RelayCommand]
    private async Task SaveAs()
    {
        if (_pack is null) return;

        if (Avalonia.Application.Current?.ApplicationLifetime
                is not IClassicDesktopStyleApplicationLifetime desktop
            || desktop.MainWindow is null)
        {
            return;
        }

        var sims2PackType = new FilePickerFileType("Sims2Pack")
        {
            Patterns = new[] { "*.Sims2Pack" },
        };

        var picked = await desktop.MainWindow.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title              = "Save As",
                SuggestedFileName  = PackName,
                DefaultExtension   = "Sims2Pack",
                FileTypeChoices    = new[] { sims2PackType },
            });
        if (picked?.TryGetLocalPath() is not string targetPath) return;

        // S2Sims2Pack.SaveAs uses FileMode.CreateNew, so it throws if the
        // target exists. Mirror Mootilda's temp-then-swap dance.
        try
        {
            if (File.Exists(targetPath))
            {
                string tempPath = Path.Combine(
                    Path.GetDirectoryName(targetPath) ?? Path.GetTempPath(),
                    Path.GetFileNameWithoutExtension(targetPath) + ".tmp.Sims2Pack");
                if (File.Exists(tempPath)) File.Delete(tempPath);

                _pack.SaveAs(tempPath);

                string backupPath = Path.ChangeExtension(targetPath, ".bkp");
                if (File.Exists(backupPath)) File.Delete(backupPath);
                File.Move(targetPath, backupPath);
                File.Move(tempPath, targetPath);
            }
            else
            {
                _pack.SaveAs(targetPath);
            }

            PackName      = Path.GetFileNameWithoutExtension(targetPath);
            StatusMessage = $"Saved to {targetPath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
        }
    }
    // Mootilda's Update fetched the hack-recognition DB from a long-dead
    // ModTheSims URL. Our version just opens the GitHub releases page in the
    // user's default browser so they can grab a newer installer build.
    [RelayCommand]
    private void Update()
    {
        const string releasesUrl = "https://github.com/rhiamom/Clean-Installer-for-Mac/releases";
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName        = releasesUrl,
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not open browser: {ex.Message}";
        }
    }

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
