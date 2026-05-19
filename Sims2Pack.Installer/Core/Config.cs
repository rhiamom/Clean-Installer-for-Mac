#nullable disable
/***************************************************************************
 *   Copyright (C) 2004-2007 by Karol Rybak                                *
 *   http://phervers.ModTheSims.info                                       *
 *                                                                         *
 *   Additional programming:                                               *
 *   Copyright (C) 2010-2013 by Mootilda                                   *
 *   http://www.modthesims.info/member.php?u=589252                        *
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
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using S2;

namespace Sims2Pack_Installer
{
    public class Config
    {
        public static bool updatingFilelist;
        public static bool AutoInstallLotFiles;     // not user-configurable in v1
        public static string exePath, tempFolder;
        public static S2CIPackageDescription packageDescription;

        /// <summary>
        /// Set by the UI layer to confirm overwriting an existing file.
        /// Called from S2CPackage MoveTo/CopyTo/Rename. Default = always
        /// overwrite (preserves the legacy "if no UI handler, just do it"
        /// behavior). The Avalonia view-model can override with an actual
        /// dialog when one is needed.
        /// </summary>
        public static Func<string, bool> ConfirmOverwrite { get; set; } = _ => true;

        public static void LoadConfig()
        {
            try
            {
                tempFolder = Path.GetTempPath() + "s2pci" + Path.DirectorySeparatorChar;
                exePath = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);

                // Sims 2 user folder location is hardcoded (immutable on Mac).
                // See SimsDirectories/S2Directories.cs.

                // Load the recognised-package database. packages.txt is the
                // bundled hash → metadata index; localpackages.txt is the
                // user's private entries written via AddToLocalDB.
                string path1 = Path.Combine(exePath, "packages.txt");
                string path2 = Path.Combine(exePath, "localpackages.txt");
                packageDescription = new S2CIPackageDescription(path1, path2);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Config.LoadConfig failed: " + e);
            }
        }
    }



    #region S2CIPackageDescription
    public class S2CIPackageDescription
    {
        #region Data_storage
        private struct Data
        {
            public string lastUpdate, localHackDBFileName;
            public Hashtable hackDb;
        }
        private Data dataControl;
        #endregion

        #region constructors

        public S2CIPackageDescription(string fileName, string privDBFilename)
        {
            dataControl.hackDb = new Hashtable();
            if(File.Exists(fileName))
            {
                S2CIPackageInfoList hackDb  = LoadFile(fileName, false);
                foreach(S2CIPackageInfo tInfo in  hackDb)
                {
                    if(tInfo.MD5 != null)
                        if(!dataControl.hackDb.Contains(tInfo.MD5))
                            dataControl.hackDb.Add(tInfo.MD5, tInfo);
                }
            }

            // local db
            dataControl.localHackDBFileName = privDBFilename;
            if(File.Exists(privDBFilename))
            {
                S2CIPackageInfoList localHackDB  = LoadFile(privDBFilename, true);
                foreach(S2CIPackageInfo tInfo in  localHackDB)
                {
                    if(tInfo.MD5 != null)
                        if(!dataControl.hackDb.Contains(tInfo.MD5))
                            dataControl.hackDb.Add(tInfo.MD5, tInfo);
                }
            }
        }


        #endregion

        #region private_methods

        #endregion

        #region public_properties

        public string lastUpdate
        {
            get
            {
                return dataControl.lastUpdate;
            }
        }
        #endregion

        #region public_methods

        #region Local DB methods
        public void SaveLocalDB()
        {
            FileStream fs = new FileStream(dataControl.localHackDBFileName, FileMode.Create, FileAccess.Write);
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.BaseStream.Seek(0, SeekOrigin.Begin);
                sw.WriteLine("Local hack db");
                IDictionaryEnumerator en = dataControl.hackDb.GetEnumerator();
                while(en.MoveNext())
                {
                    S2CIPackageInfo tInfo = (S2CIPackageInfo)en.Value;
                    if(tInfo.local)
                    {
                        sw.WriteLine(tInfo.ToString());
                    }

                }
            }

        }
        public void RemoveFromLocalDB(string md5)
        {
            if (dataControl.hackDb.Contains(md5))
            {
                dataControl.hackDb.Remove(md5);
                SaveLocalDB();
            }
        }

        public void AddToLocalDB(S2CPackage tFile)
        {
            if (tFile.md5 != null && !dataControl.hackDb.Contains(tFile.md5))
            {
                dataControl.hackDb.Add(tFile.md5, tFile.info);
                SaveLocalDB();
            }
        }
        #endregion

        public S2CIPackageInfoList LoadFile(string fileName, bool isLocal)
        {
            S2CIPackageInfoList files  = new S2CIPackageInfoList();
            if (File.Exists(fileName))
            {
                using (StreamReader sr = File.OpenText(fileName))
                {
                    string line;
                    dataControl.lastUpdate = sr.ReadLine();
                    while ((line = sr.ReadLine()) != null)
                    {
                        S2CIPackageInfo tInfo = new S2CIPackageInfo(line);
                        tInfo.local = isLocal;
                        files.Add(tInfo);
                    }
                }
            }
            return files;
        }

        public S2CIPackageInfo GetInfo(string md5)
        {
            if (dataControl.hackDb.Contains(md5))
            {
                return (S2CIPackageInfo)dataControl.hackDb[md5];
            }
            else
            {
                return new S2CIPackageInfo("");
            }


        }
        #endregion

    }
    #endregion

}