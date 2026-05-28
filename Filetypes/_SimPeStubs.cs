// =============================================================================
// SimPe namespace surface used by the Clean Installer (hash + string helpers).
//
// Vendored from SimPE-Fixed (SimPE.Helper.Hashes / .Helper). Produces
// byte-for-byte identical output so package CRCs match the game and the
// legacy installer DB.
//
// The DBPF reader/writer (SimPe.Packages.GeneratableFile, SimPe.Packages.File,
// SimPe.Interfaces.Files.*) lives in DbpfPackage.cs.
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

        // CRC-24 with poly 0x01864CFB, seed 0x00B704CE (OpenPGP / SimPE).
        // Matches Classless.Hasher CRCStandard.CRC24 used in SimPE.Helper.Hashes.
        public static class Crc24
        {
            public static byte[] ComputeHash(byte[] data)
            {
                uint crc = 0x00B704CE;
                const uint poly = 0x01864CFB;
                if (data != null)
                {
                    foreach (byte b in data)
                    {
                        crc ^= (uint)b << 16;
                        for (int i = 0; i < 8; i++)
                        {
                            crc <<= 1;
                            if ((crc & 0x01000000) != 0) crc ^= poly;
                        }
                    }
                }
                crc &= 0x00FFFFFF;
                return new[]
                {
                    (byte)((crc >> 16) & 0xFF),
                    (byte)((crc >> 8)  & 0xFF),
                    (byte) (crc        & 0xFF),
                };
            }
        }

        // CRC-32/BZIP2: poly 0x04C11DB7, init 0xFFFFFFFF, reflectIn/Out = false,
        // xorOut = 0. Matches the custom CRCParameters used in SimPE.Helper.Hashes.
        public static class Crc32
        {
            public static byte[] ComputeHash(byte[] data)
            {
                uint crc = 0xFFFFFFFF;
                const uint poly = 0x04C11DB7;
                if (data != null)
                {
                    foreach (byte b in data)
                    {
                        crc ^= (uint)b << 24;
                        for (int i = 0; i < 8; i++)
                        {
                            crc = (crc & 0x80000000) != 0 ? (crc << 1) ^ poly : crc << 1;
                        }
                    }
                }
                return new[]
                {
                    (byte)((crc >> 24) & 0xFF),
                    (byte)((crc >> 16) & 0xFF),
                    (byte)((crc >> 8)  & 0xFF),
                    (byte) (crc        & 0xFF),
                };
            }
        }
    }

    public static class Helper
    {
        public static byte[] ToBytes(string s) => Encoding.ASCII.GetBytes(s ?? string.Empty);
    }
}
