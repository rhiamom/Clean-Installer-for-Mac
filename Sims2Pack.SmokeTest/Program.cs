using System;
using System.IO;
using S2;
using Sims2Pack_Installer;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: Sims2Pack.SmokeTest <path-to.Sims2Pack>");
    return 1;
}

string path = args[0];
if (!File.Exists(path))
{
    Console.Error.WriteLine($"File not found: {path}");
    return 2;
}

Console.WriteLine($"Sims2Pack.SmokeTest");
Console.WriteLine($"  User folder:    {Sims2Directories.UserFolder}");
Console.WriteLine($"  Folder exists?  {Sims2Directories.HasUserFolder}");
Console.WriteLine($"  Downloads:      {Sims2Directories.Downloads}");
Console.WriteLine();

Config.LoadConfig();
Console.WriteLine($"  Config loaded   (exePath={Config.exePath})");
Console.WriteLine();

Console.WriteLine($"Parsing: {path}");
var pack = new S2Sims2Pack(path);

Console.WriteLine($"  Pack type:      {pack.type}");
Console.WriteLine($"  Items:          {pack.Items.Count}");
Console.WriteLine();

int i = 1;
foreach (S2CPackage p in pack.Items)
{
    string name = p.info?.name
                  ?? Path.GetFileNameWithoutExtension(p.fileName);
    string type = p.DisplayTypeName;
    string version = string.IsNullOrEmpty(p.info?.version) ? "" : $"  v{p.info.version}";
    string author  = string.IsNullOrEmpty(p.info?.author)  ? "" : $"  by {p.info.author}";
    string flags = string.Join(",",
        new[]
        {
            p.IsOverwriting ? "OVERWRITE" : null,
            p.IsBodyShopIncomplete ? "BS-INCOMPLETE" : null,
            p.IsDuplicated ? "DUPLICATE" : null,
        }.Where(s => s is not null));
    string flagSuffix = flags.Length > 0 ? $"  [{flags}]" : "";

    Console.WriteLine($"  {i,2}. {name}  ({type}){version}{author}{flagSuffix}");
    i++;
}

return 0;
