/***************************************************************************
 *   Copyright (C) 2004-2007 by Karol Rybak                                *
 *   Additional programming © 2010-2014 Mootilda                           *
 *   .NET 8 / Avalonia port © 2026 GramzeSweatshop (rhiamom@mac.com)       *
 *                                                                         *
 *   GNU GPLv2 or later, see LICENSE.                                      *
 ***************************************************************************/
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Sims2Pack.Installer.ViewModels;

namespace Sims2Pack.Installer.Views;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private void OnGridRowTapped(object? sender, TappedEventArgs e)
    {
        // Skip taps on the IsSelected checkbox so toggling that doesn't
        // also pop the texture window.
        if (e.Source is Visual src && src.FindAncestorOfType<CheckBox>(true) != null)
            return;

        if ((sender as DataGrid)?.SelectedItem is not PackageItemViewModel vm)
            return;
        if (!vm.HasTextures) return;

        var textures = vm.Textures;
        if (textures.Count == 0) return;

        var popup = new TexturePreviewWindow(vm.Name, textures);
        popup.ShowDialog(this);
    }
}
