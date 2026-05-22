using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using DatGen.DBPF.IO;
using DatGen.Types.TS2;
using Sims2Pack.Installer.Views;
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
    public string Md5 => Package.md5 ?? string.Empty;
    public string FileName => Package.fileName ?? string.Empty;
    public string Version => Package.info?.version ?? string.Empty;
    public string Author  => Package.info?.author  ?? string.Empty;
    public string Description => Package.info?.description ?? string.Empty;

    private Bitmap? _previewImage;
    private bool _previewImageLoaded;
    public Bitmap? PreviewImage
    {
        get
        {
            if (_previewImageLoaded) return _previewImage;
            _previewImageLoaded = true;
            if (Package.images is null || Package.images.Count == 0) return null;
            try
            {
                _previewImage = new Bitmap(new MemoryStream(Package.images[0]));
            }
            catch
            {
                _previewImage = null;
            }
            return _previewImage;
        }
    }

    public bool HasTextures =>
        Package.textures != null && Package.textures.Count > 0;

    private IReadOnlyList<TexturePreviewItem>? _textures;
    /// <summary>
    /// Largest mipmap of every DXT1/3/5-encoded TXTR record in the package,
    /// decoded to an Avalonia bitmap and labelled with its size and encoding.
    /// Decoded lazily on first access. Mipmaps in unsupported encodings
    /// (raw/grayscale/LIFO-referenced) are skipped silently.
    /// </summary>
    public IReadOnlyList<TexturePreviewItem> Textures
    {
        get
        {
            if (_textures != null) return _textures;
            var list = new List<TexturePreviewItem>();
            if (Package.textures != null)
            {
                foreach (PackageEntry entry in Package.textures)
                {
                    if (entry.RawData == null || entry.RawData.Length == 0)
                        continue;
                    try
                    {
                        var txtr = new TXTR();
                        txtr.Load(entry.RawData);
                        foreach (TXTRImage img in txtr.Images)
                        {
                            if (img.MipMaps == null || img.MipMaps.Count == 0)
                                continue;
                            // Mipmaps may be stored smallest-first or
                            // largest-first depending on the TXTR; pick the
                            // largest decodable mipmap by pixel count.
                            TXTRMipmap? best = null;
                            int bestArea = 0;
                            foreach (TXTRMipmap m in img.MipMaps)
                            {
                                int area = m.Width * m.Height;
                                if (area > bestArea && !m.IsLifoReference)
                                {
                                    best = m;
                                    bestArea = area;
                                }
                            }
                            if (best == null) continue;
                            byte[]? bgra = best.DecodeBgra();
                            if (bgra == null) continue;
                            Bitmap? bmp = BgraToBitmap(bgra, best.Width, best.Height);
                            if (bmp == null) continue;
                            list.Add(new TexturePreviewItem
                            {
                                Image = bmp,
                                Label = $"{best.Width}×{best.Height} · {EncodingName(best.EncodingType)}",
                            });
                        }
                    }
                    catch
                    {
                        // Skip TXTRs that fail to parse — bad/odd encodings
                        // shouldn't take the whole preview down.
                    }
                }
            }
            _textures = list;
            return _textures;
        }
    }

    private static string EncodingName(uint encodingType) => encodingType switch
    {
        4 => "DXT1",
        5 => "DXT3",
        8 => "DXT5",
        _ => $"enc {encodingType}",
    };

    private static Bitmap? BgraToBitmap(byte[] bgra, int width, int height)
    {
        if (width <= 0 || height <= 0) return null;
        if (bgra.Length < width * height * 4) return null;
        var wb = new WriteableBitmap(
            new PixelSize(width, height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Unpremul);
        using var locked = wb.Lock();
        int srcStride = width * 4;
        if (locked.RowBytes == srcStride)
        {
            Marshal.Copy(bgra, 0, locked.Address, bgra.Length);
        }
        else
        {
            for (int y = 0; y < height; y++)
            {
                Marshal.Copy(
                    bgra, y * srcStride,
                    locked.Address + y * locked.RowBytes,
                    srcStride);
            }
        }
        return wb;
    }

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
