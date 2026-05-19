/***************************************************************************
 *   Copyright (C) 2004-2007 by Karol Rybak                                *
 *   Additional programming © 2010-2014 Mootilda                           *
 *   .NET 8 / Avalonia port © 2026 GramzeSweatshop (rhiamom@mac.com)       *
 *                                                                         *
 *   GNU GPLv2 or later, see LICENSE.                                      *
 ***************************************************************************/

using System.IO;

namespace S2;

/// <summary>
/// Path resolution for the Sims 2 user folder. On the Mac target the install
/// path is immutable — set once by editing <see cref="SIMS2_USER_FOLDER"/>
/// and used directly. No browse dialog, no persistence.
/// </summary>
public static class Sims2Directories
{
    // The Mac App Store "The Sims 2 Super Collection" (Aspyr) sandboxes the
    // user folder under com.aspyr.sims2.appstore. Path supplied by the user.
    public static readonly string SIMS2_USER_FOLDER =
        System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
            "Library", "Containers", "com.aspyr.sims2.appstore", "Data",
            "Library", "Application Support", "Aspyr", "The Sims 2");

    public static string UserFolder => SIMS2_USER_FOLDER;

    public static string Downloads  => SubFolderEnsured("Downloads");
    public static string Teleport   => SubFolderEnsured("Teleport");
    public static string LotCatalog => SubFolder("LotCatalog");
    public static string SavedSims  => SubFolder("SavedSims");

    public static bool HasUserFolder =>
        !string.IsNullOrEmpty(UserFolder) && Directory.Exists(UserFolder);

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
}
