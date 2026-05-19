#nullable disable
/***************************************************************************
 *   Copyright (C) 2004-2007 by Karol Rybak                                *
 *   http://phervers.ModTheSims.info                                       *
 *                                                                         *
 *   This program is free software; you can redistribute it and/or modify  *
 *   it under the terms of the GNU General Public License as published by  *
 *   the Free Software Foundation; either version 2 of the License, or     *
 *   (at your option) any later version.                                   *
 *                                                                         *
 *   This program is distributed in the hope that it will be useful,       *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 *   GNU General Public License for more details.                          *
 *                                                                         *
 *   You should have received a copy of the GNU General Public License     *
 *   along with this program; if not, write to the                         *
 *   Free Software Foundation, Inc.,                                       *
 *   59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.             *
 ***************************************************************************/

using System;
using System.IO;
using System.Text;

namespace Sims2Pack_Installer
{
    /// <summary>
    /// Summary description for Helpers.
    /// </summary>
    public class Helpers
    {
        public static void copyDirectory(string Src,string Dst)
        {
            String[] Files;

            if(Dst[Dst.Length-1]!=Path.DirectorySeparatorChar)
                Dst+=Path.DirectorySeparatorChar;
            if(!Directory.Exists(Dst)) Directory.CreateDirectory(Dst);
            Files=Directory.GetFileSystemEntries(Src);
            foreach(string Element in Files)
            {
                // Sub directories
                if(Directory.Exists(Element))
                    copyDirectory(Element,Dst+Path.GetFileName(Element));
                    // Files in directory
                else
                    File.Copy(Element,Dst+Path.GetFileName(Element),true);
            }
        }
        public static uint Crc24(string toHash)
        {
            uint hash = DatGen.CRCHash.GenerateCRC24_TS2(Encoding.ASCII.GetBytes(toHash.ToLower()));
            return hash | 0xFF000000;

        }
        public static uint Crc32(string toHash)
        {
            return DatGen.CRCHash.GenerateCRC32_TS2(Encoding.ASCII.GetBytes(toHash.ToLower()));
        }

        public static string PackageHash(string toHash)
        {
            uint hash = DatGen.CRCHash.GenerateCRC24_TS2(Encoding.ASCII.GetBytes(toHash.ToLower()));
            hash = hash | 0x7F000000;
            //return "#0x" + SimPe.Helper.HexString(hash) + "!";
            return "";
        }
        public static uint PackageGroupID(string toHash)
        {
            uint hash = DatGen.CRCHash.GenerateCRC24_TS2(Encoding.ASCII.GetBytes(toHash.ToLower()));
            hash = hash | 0x7F000000;
            return hash;
        }

        public static uint CalculateThumbnailInstance(uint groupID, string modelName)
        {
            string tohash = Convert.ToString(groupID) + modelName;
            //Data.ThumbnailInstance = Helpers.Crc32(tohash);
            return (uint)SimPe.Hashes.ToLong(SimPe.Hashes.Crc32.ComputeHash(SimPe.Helper.ToBytes(tohash.ToLower())));


        }
        public static string SanitizeName(string name)
        {
            name = name.Replace(" ", "_");
            string newName = "";
            for(int i=0;i<name.Length; i++)
            {
                if( char.IsLetterOrDigit(name, i))
                {

                    newName += new string(name[i],1);
                }
            }
            return newName;
        }
    }

}