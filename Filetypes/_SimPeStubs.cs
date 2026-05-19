// =============================================================================
// TEMPORARY SCAFFOLDING — replace with real SimPE port.
//
// This file declares just enough of the SimPe namespace surface that the rest
// of the solution compiles. None of the methods do anything real:
//   * Hash methods return empty/zero — package CRCs will be wrong.
//   * LoadFromStream throws — opening any actual .Sims2Pack will fail.
//   * GeneratableFile.FindFile/Read return null — Lot manipulation is dead.
//
// The UI builds and renders on top of this, so the layout and asset wiring
// can be visually verified without doing the full SimPE decoupling work.
//
// To replace: port (or vendor + strip WinForms from) SimPE.Helper/Hashes,
// SimPE.Interfaces.Files, SimPE.Packages, then delete this file.
// See the deferred-coupling notes in the initial commit's message.
// =============================================================================
using System;
using System.IO;
using System.Text;

namespace SimPe
{
    public static class Hashes
    {
        public static long ToLong(byte[] bytes)
        {
            if (bytes is null || bytes.Length == 0) return 0;
            long r = 0;
            for (int i = 0; i < bytes.Length && i < 8; i++) r = (r << 8) | bytes[i];
            return r;
        }

        public static class Crc24
        {
            public static byte[] ComputeHash(byte[] data) => new byte[3];
        }

        public static class Crc32
        {
            public static byte[] ComputeHash(byte[] data) => new byte[4];
        }
    }

    public static class Helper
    {
        public static byte[] ToBytes(string s) => Encoding.ASCII.GetBytes(s ?? string.Empty);
    }
}

namespace SimPe.Interfaces.Files
{
    public interface IPackedFileDescriptor
    {
        void SetUserData(byte[] data, bool compressed);
    }

    public interface IPackedFile
    {
        byte[] UncompressedData { get; }
    }

    public interface IPackageFile
    {
        IPackedFile Read(IPackedFileDescriptor pfd);
        IPackedFileDescriptor FindFile(uint typeId, uint groupId, uint instanceHi, uint instanceLo);
    }
}

namespace SimPe.Packages
{
    using SimPe.Interfaces.Files;

    public class GeneratableFile : IPackageFile
    {
        public IPackedFile Read(IPackedFileDescriptor pfd) => null!;
        public IPackedFileDescriptor FindFile(uint typeId, uint groupId, uint instanceHi, uint instanceLo) => null!;
        public MemoryStream Build() => new MemoryStream();
        public void Close() { }
    }

    public static class File
    {
        public static GeneratableFile LoadFromStream(BinaryReader br) =>
            throw new NotImplementedException("SimPE.Packages.File.LoadFromStream not yet ported");
    }
}
