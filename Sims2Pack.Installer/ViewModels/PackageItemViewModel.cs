/***************************************************************************
 *   Copyright (C) 2004-2007 by Karol Rybak                                *
 *   Additional programming © 2010-2014 Mootilda                           *
 *   .NET 8 / Avalonia port © 2026 GramzeSweatshop (rhiamom@mac.com)       *
 *                                                                         *
 *   GNU GPLv2 or later, see LICENSE.                                      *
 ***************************************************************************/
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

    public PackageItemViewModel(S2CPackage package, S2Sims2Pack? parentPack = null)
    {
        Package = package;
        _parentPack = parentPack;
        _isSelected = package.enabled;
    }

    public S2CPackage Package { get; }
    private readonly S2Sims2Pack? _parentPack;

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
        (Package.textures != null && Package.textures.Count > 0) ||
        (Package.txmts != null && Package.txmts.Count > 0 && _parentPack != null);

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
                        // myDBPF.ReadPackage gives us raw bytes — for QFS-
                        // compressed packages (those with a CLST entry), the
                        // TXTR body needs decompressing before TXTR.Load can
                        // parse it.
                        byte[] body = entry.RawData;
                        if (SimPe.Packages.GeneratableFile.LooksQfsCompressed(body))
                            body = SimPe.Packages.GeneratableFile.DecompressQfs(body);
                        var txtr = new TXTR();
                        txtr.Load(body);
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
            // Fall back for repositoried recolors: package has no own TXTR
            // but does have TXMTs that reference textures in sibling packages
            // (e.g. the 60 Steadfast stair recolors that share a base TXTR
            // from HW_stairsSteadfast__DEFAULT.package and only differ in
            // material color tint).
            if (list.Count == 0)
                AddRepositoriedPreviews(list);
            _textures = list;
            return _textures;
        }
    }

    private void AddRepositoriedPreviews(List<TexturePreviewItem> list)
    {
        if (_parentPack == null || Package.txmts == null) return;
        foreach (PackageEntry txmtEntry in Package.txmts)
        {
            if (txmtEntry.RawData == null || txmtEntry.RawData.Length == 0) continue;
            try
            {
                byte[] txmtBody = txmtEntry.RawData;
                if (SimPe.Packages.GeneratableFile.LooksQfsCompressed(txmtBody))
                    txmtBody = SimPe.Packages.GeneratableFile.DecompressQfs(txmtBody);
                var txmt = new TXMT();
                txmt.Load(txmtBody);
                if (txmt.Textures == null || txmt.Textures.Count == 0) continue;

                var tint = ExtractTint(txmt);
                string baseTextureName = txmt.Textures[0];
                if (string.IsNullOrEmpty(baseTextureName)) continue;

                PackageEntry? baseTxtr = FindTxtrByName(baseTextureName);
                if (baseTxtr == null) continue;

                byte[] txtrBody = baseTxtr.RawData;
                if (txtrBody == null || txtrBody.Length == 0) continue;
                if (SimPe.Packages.GeneratableFile.LooksQfsCompressed(txtrBody))
                    txtrBody = SimPe.Packages.GeneratableFile.DecompressQfs(txtrBody);

                var txtrParsed = new TXTR();
                txtrParsed.Load(txtrBody);
                foreach (TXTRImage img in txtrParsed.Images)
                {
                    if (img.MipMaps == null || img.MipMaps.Count == 0) continue;
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
                    ApplyTint(bgra, tint);
                    Bitmap? bmp = BgraToBitmap(bgra, best.Width, best.Height);
                    if (bmp == null) continue;

                    string tintLabel = tint.HasValue
                        ? $" · tint {tint.Value.r:F2}/{tint.Value.g:F2}/{tint.Value.b:F2}"
                        : "";
                    list.Add(new TexturePreviewItem
                    {
                        Image = bmp,
                        Label = $"{best.Width}×{best.Height} · {EncodingName(best.EncodingType)} · {baseTextureName}{tintLabel}",
                    });
                }
            }
            catch
            {
                // One bad TXMT shouldn't kill the preview for the package.
            }
        }
    }

    private PackageEntry? FindTxtrByName(string textureName)
    {
        // Strip the optional Sims 2 forced-group prefix "##0xHHHHHHHH!" or
        // "#0xHHHHHHHH!". What's left is the bare texture name. Sims 2
        // convention is that TXMTs reference textures by bare name, but the
        // TXTR resource's internal filename has a "_txtr" suffix and its
        // InstanceID is the CRC24 hash of THAT suffixed name. So we try
        // both forms.
        string bare = textureName.Trim();
        int bang = bare.LastIndexOf('!');
        if (bang >= 0 && bang < bare.Length - 1)
            bare = bare.Substring(bang + 1);
        bare = bare.ToLowerInvariant();
        string suffixed = bare.EndsWith("_txtr") ? bare : bare + "_txtr";

        uint hash24 = InstanceHash24(bare);
        uint hash24Suffixed = InstanceHash24(suffixed);

        foreach (S2CPackage sib in _parentPack!.Items)
        {
            if (sib.textures == null) continue;
            foreach (PackageEntry e in sib.textures)
            {
                if (e.InstanceID == hash24 || e.InstanceID == hash24Suffixed)
                    return e;
            }
        }

        // Hash lookup didn't match — fall back to scanning each sibling
        // TXTR's internal Filename. Slower but catches name-hash schemes the
        // suffixed-CRC24 doesn't cover.
        foreach (S2CPackage sib in _parentPack.Items)
        {
            if (sib.textures == null) continue;
            foreach (PackageEntry e in sib.textures)
            {
                if (e.RawData == null || e.RawData.Length == 0) continue;
                try
                {
                    byte[] body = e.RawData;
                    if (SimPe.Packages.GeneratableFile.LooksQfsCompressed(body))
                        body = SimPe.Packages.GeneratableFile.DecompressQfs(body);
                    var t = new TXTR();
                    t.Load(body);
                    string? fn = t.Filename?.Trim().ToLowerInvariant();
                    if (fn != null && fn == bare)
                        return e;
                }
                catch { }
            }
        }
        return null;
    }

    // Sims 2 InstanceHash: CRC24 of the lowercase name, OR'd with 0xFF000000.
    private static uint InstanceHash24(string name)
    {
        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(name);
        byte[] crc = SimPe.Hashes.Crc24.ComputeHash(bytes);
        return ((uint)crc[0] << 16) | ((uint)crc[1] << 8) | crc[2] | 0xFF000000;
    }

    private static (float r, float g, float b)? ExtractTint(TXMT txmt)
    {
        // Sims 2 TXMT material params are stored as string=string pairs.
        // Common keys: stdMatDiffCoef (diffuse multiplier — the tint we
        // want), stdMatEnvCubeCoef, stdMatSpecCoef. We look for the first
        // key that contains "diff" or "tint" or "color" with a parseable
        // 3- or 4-float value.
        if (txmt.Properties == null) return null;
        foreach (TxmtProperty p in txmt.Properties)
        {
            string name = (p.Name ?? "").ToLowerInvariant();
            if (!(name.Contains("diff") || name.Contains("tint") || name.Contains("color")))
                continue;
            var parsed = TryParseFloatTriplet(p.Value);
            if (parsed.HasValue) return parsed;
        }
        return null;
    }

    private static (float r, float g, float b)? TryParseFloatTriplet(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        var parts = value.Split(new[] { ',', ' ', '\t' },
            System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3) return null;
        if (!float.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float r))
            return null;
        if (!float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float g))
            return null;
        if (!float.TryParse(parts[2], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float b))
            return null;
        // Values >1 (e.g. 0..255 form) get normalized.
        float max = System.Math.Max(r, System.Math.Max(g, b));
        if (max > 1.5f) { r /= 255f; g /= 255f; b /= 255f; }
        // Clamp 0..1
        r = System.Math.Clamp(r, 0f, 1f);
        g = System.Math.Clamp(g, 0f, 1f);
        b = System.Math.Clamp(b, 0f, 1f);
        return (r, g, b);
    }

    private static void ApplyTint(byte[] bgra, (float r, float g, float b)? tint)
    {
        if (!tint.HasValue) return;
        var (r, g, bChan) = tint.Value;
        for (int i = 0; i + 3 < bgra.Length; i += 4)
        {
            bgra[i]     = (byte)(bgra[i]     * bChan);  // B
            bgra[i + 1] = (byte)(bgra[i + 1] * g);      // G
            bgra[i + 2] = (byte)(bgra[i + 2] * r);      // R
            // alpha (i+3) untouched
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
