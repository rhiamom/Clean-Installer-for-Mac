# Credits & Licensing

**Clean Installer for Mac** is a macOS-native port (.NET 8 + Avalonia) of the
**Sims2Pack Clean Installer**, a tool for safely inspecting and installing Sims 2
custom-content `.Sims2Pack` and `.package` files. It is released under the
**GNU General Public License v3** (see [LICENSE](LICENSE)).

## Authorship & lineage

1. **Karol Rybak** — original author of the **Sims2Pack Clean Installer**.
   Copyright © 2004–2007. The bulk of the Core and Filetypes code derives from his work.
2. **phervers** — Copyright © 2004 (`S2CPackage.cs`).
3. **Andi8104** — Copyright © 2006 (`R_LOT.cs`).
4. **Mootilda** — additional programming, © 2010–2014 (later maintenance of the
   Windows Clean Installer).
5. **Quaxi (Ambertation)** — author of **SimPE**. The focused DBPF reader/writer and
   hash/string helpers (`Filetypes/DbpfPackage.cs`, `Filetypes/_SimPeStubs.cs`) are
   vendored from SimPE so package CRCs match the game byte-for-byte.
   <quaxi@ambertation.de> · SimPE is licensed **GPL v2 or later**.
6. **GramzeSweatshop — Catherine Gramze** — the macOS .NET 8 / Avalonia port.
   <rhiamom@mac.com> · Copyright © 2026.

## License compatibility

All upstream components are licensed **"GPL v2 or, at your option, any later version."**
That grant permits distributing the combined macOS port under **GPL v3** (this repo's
LICENSE). Original copyright notices are preserved in each source file; per-file headers
that read "GPLv2 or later" reflect each upstream author's grant, while the project as a
whole is distributed under GPL v3.

## Shared infrastructure

This port shares a Sims 2 path resolver with **Delphy's Download Organiser for Mac**:
`SimsDirectories/S2Directories.cs` reads/writes the cross-app registry at
`~/Library/Application Support/Sims2Tools/paths.json`, so both tools agree on where the
Sims 2 Downloads folder is.

## Acknowledgements

Thank you to **Karol Rybak** for the original Clean Installer (and phervers, Andi8104,
and Mootilda for their contributions), and to **Quaxi/Ambertation** for SimPE. This Mac
port exists so the Clean Installer can finally run natively on macOS.
