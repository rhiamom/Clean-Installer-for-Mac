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
using System.Diagnostics;
using System.IO;
using System.Collections;

namespace Sims2Pack_Installer
{
    /// <summary>
    /// Summary description for myDBPF.
    /// </summary>
    public class myDBPF
    {
        public static PackageEntryCollection ReadPackage(string fileName)
        {
            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);

            byte[] contents = new byte[stream.Length];
            contents = reader.ReadBytes((int)stream.Length);

            return ReadPackage(contents);
        }
        public static PackageEntryCollection ReadPackage(byte[] contents)
        {
            PackageEntryCollection entries = new PackageEntryCollection();

            MemoryStream stream = new MemoryStream(contents);


            long sOffset = stream.Position;
            BinaryReader dbpf = null;
            dbpf = new BinaryReader(stream);
            try
            {
                string check=new String(dbpf.ReadChars(4));
                if (check!="DBPF")//make sure it is a format I can read
                {
                    throw new Exception("Unknown package format");
                }
                dbpf.BaseStream.Seek(32,SeekOrigin.Current);
                uint numFiles = dbpf.ReadUInt32();
                uint offset = dbpf.ReadUInt32();
                uint sizeIndex = dbpf.ReadUInt32();
                if (sizeIndex==0)
                {
                    return entries;
                    //throw new Exception("Package is empty");
                }
                // does package use 24 or 20 byte indexes
                bool newIndex = ((sizeIndex / numFiles)== 24) ? true:false;
                dbpf.BaseStream.Seek(offset + sOffset,SeekOrigin.Begin);
                for (uint i=0;i<numFiles;i++)// read id's for each package
                {
                    PackageEntry tFile  = new PackageEntry();
                    tFile.TypeID        = dbpf.ReadUInt32();
                    tFile.GroupID       = dbpf.ReadUInt32();
                    tFile.InstanceID    = dbpf.ReadUInt32();
                    tFile.ResourceID    = newIndex ? dbpf.ReadUInt32():0;
                    tFile.offset = dbpf.ReadUInt32();
                    tFile.size = dbpf.ReadUInt32();
                    entries.Add(tFile);
                }
                //entries.SortByOffset();
                for(int i = 0;i<entries.Count;i++)
                {
                    dbpf.BaseStream.Seek(entries[i].offset + sOffset, SeekOrigin.Begin);
                    entries[i].RawData = dbpf.ReadBytes((int)entries[i].size);
                }
                return entries;
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                return entries;
            }
        }
    }


    #region PackageEntryCollection
    public class PackageEntryCollection : CollectionBase
    {
        public PackageEntry this[int index]
        {
            get
            {
                //if(index>=List.Count)

                return ((PackageEntry)(List[index]));
            }
            set { List[index] = value; }
        }

        public int Add(PackageEntry value)
        {
            return List.Add(value);
        }

        public void Insert(int index, PackageEntry value)
        {
            List.Insert(index, value);
        }

        public void Remove(PackageEntry value)
        {
            List.Remove(value);
        }

        public bool Contains(PackageEntry value)
        {
            return List.Contains(value);
        }

        public void SortByOffset()
        {
            for (int i = List.Count; --i>=0; )
            {
                bool flipped = false;
                for (int j = 0; j<i; j++)
                {
                    //if (stopRequested) { return; }
                    if (((PackageEntry)List[j]).offset < ((PackageEntry)List[j+1]).offset)
                    {
                        object T = List[j];
                        List[j] = List[j+1];
                        List[j+1] = T;
                        flipped = true;
                    }
                    //pause(i,j);
                }
                if (!flipped) { return; }
            }
        }

    }
    #endregion

    public class PackageEntry
    {
        public uint TypeID, GroupID, InstanceID, ResourceID, offset, mOffset, size;
        public byte[] RawData;
        public string FileName;
        public DatGen.DBPF.IO.Unknown file;
    }

}