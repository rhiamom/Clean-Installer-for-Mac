/***************************************************************************
 *   Copyright (C) 2004-2007 by Karol Rybak                                *
 *   Additional programming © 2010-2014 Mootilda                           *
 *   .NET 8 / Avalonia port © 2026 GramzeSweatshop (rhiamom@mac.com)       *
 *                                                                         *
 *   GNU GPLv2 or later, see LICENSE.                                      *
 ***************************************************************************/

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace S2;

/// <summary>
/// Path resolution for the Sims 2 user folder.
///
/// The location is no longer a single hardcoded constant. All the DDT-family Mac tools
/// (this Clean Installer, Delphy's Download Organiser, eventually SimPE) agree on one
/// shared file — <c>~/Library/Application Support/Sims2Tools/paths.json</c> — so fixing
/// the path in one app fixes it for all of them.
///
/// <see cref="UserFolder"/> resolves in this order:
///   1. The Downloads path saved in the shared registry (its parent is the user folder).
///   2. The first of the known Aspyr "Super Collection" user folders that exists
///      (App Store sandbox first, then non-sandboxed) — and that choice is written back
///      to the shared registry so the other tools converge on it.
///   3. The App Store path as a last-resort default (so behaviour matches the old port).
/// </summary>
public static class Sims2Directories
{
    private static string Home =>
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);

    /// <summary>Known Sims 2 (Aspyr Super Collection) user folders, in preference order.</summary>
    public static readonly string[] CandidateUserFolders =
    {
        // 1. Mac App Store build — sandboxed under its container.
        Path.Combine(Home,
            "Library", "Containers", "com.aspyr.sims2.appstore", "Data",
            "Library", "Application Support", "Aspyr", "The Sims 2"),
        // 2. Non-sandboxed Aspyr build.
        Path.Combine(Home,
            "Library", "Application Support", "Aspyr", "The Sims 2"),
    };

    /// <summary>Last-resort default if nothing else resolves (the App Store container).</summary>
    public static readonly string SIMS2_USER_FOLDER = CandidateUserFolders[0];

    /// <summary>The shared registry file every DDT Mac tool agrees on.</summary>
    public static string RegistryFile => Path.Combine(
        Home, "Library", "Application Support", "Sims2Tools", "paths.json");

    public static string UserFolder => ResolveUserFolder();

    public static string Downloads  => SubFolderEnsured("Downloads");
    public static string Teleport   => SubFolderEnsured("Teleport");
    public static string LotCatalog => SubFolder("LotCatalog");
    public static string SavedSims  => SubFolder("SavedSims");

    public static bool HasUserFolder =>
        !string.IsNullOrEmpty(UserFolder) && Directory.Exists(UserFolder);

    private static string ResolveUserFolder()
    {
        var doc = Load();

        // 1. Honour a saved Downloads path — its parent is the user folder.
        if (!string.IsNullOrEmpty(doc.DownloadsPath))
        {
            var parent = Path.GetDirectoryName(doc.DownloadsPath.TrimEnd(Path.DirectorySeparatorChar));
            if (!string.IsNullOrEmpty(parent) && Directory.Exists(parent))
                return parent;
        }

        // 2. Probe the known candidates; first that exists wins, and we record it.
        foreach (var candidate in CandidateUserFolders)
        {
            if (Directory.Exists(candidate))
            {
                doc.DownloadsPath = Path.Combine(candidate, "Downloads");
                doc.Candidates = CandidateUserFolders;
                doc.UpdatedBy = "CleanInstaller";
                Save(doc);
                return candidate;
            }
        }

        // 3. Last-resort default.
        return SIMS2_USER_FOLDER;
    }

    private static string SubFolder(string name) =>
        string.IsNullOrEmpty(UserFolder)
            ? string.Empty
            : Path.Combine(UserFolder, name) + Path.DirectorySeparatorChar;

    private static string SubFolderEnsured(string name)
    {
        var path = SubFolder(name);
        if (path.Length > 0 && !Directory.Exists(path))
            Directory.CreateDirectory(path);
        return path;
    }

    // ---- shared registry I/O (schema kept identical across DDT Mac tools) ----

    private static RegistryDoc Load()
    {
        try
        {
            if (File.Exists(RegistryFile))
            {
                var json = File.ReadAllText(RegistryFile);
                return JsonSerializer.Deserialize<RegistryDoc>(json) ?? new RegistryDoc();
            }
        }
        catch
        {
            // A corrupt/unreadable registry must never break path resolution.
        }
        return new RegistryDoc();
    }

    private static void Save(RegistryDoc doc)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(RegistryFile)!);
            File.WriteAllText(RegistryFile, JsonSerializer.Serialize(doc, JsonOptions));
        }
        catch
        {
            // Best-effort persistence; resolution still works this session if it fails.
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private sealed class RegistryDoc
    {
        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;

        [JsonPropertyName("downloadsPath")]
        public string? DownloadsPath { get; set; }

        [JsonPropertyName("candidates")]
        public string[]? Candidates { get; set; }

        [JsonPropertyName("updatedBy")]
        public string? UpdatedBy { get; set; }
    }
}
