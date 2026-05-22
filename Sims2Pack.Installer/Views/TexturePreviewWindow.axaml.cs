using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;

namespace Sims2Pack.Installer.Views;

public class TexturePreviewItem
{
    public Bitmap Image { get; init; } = null!;
    public string Label { get; init; } = string.Empty;
}

public partial class TexturePreviewWindow : Window
{
    public TexturePreviewWindow() => InitializeComponent();

    public TexturePreviewWindow(string title, IReadOnlyList<TexturePreviewItem> items) : this()
    {
        Title = string.IsNullOrEmpty(title) ? "Textures" : "Textures — " + title;
        TextureList.ItemsSource = items;
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
