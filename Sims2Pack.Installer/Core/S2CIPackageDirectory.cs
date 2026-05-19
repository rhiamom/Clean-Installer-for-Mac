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
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Security;

namespace Sims2Pack_Installer
{
    #region S2CIPackageDirectory
    public class S2CIPackageDirectory
    {
        #region Data_storage

        public int nFiles;
        private struct Data
        {
            public string enabledPath;
            public S2CPackageList files;
            public Hashtable packages;
        }
        private Data dataControl;
        #endregion

        public S2CIPackageDirectory(string enabledPath, bool recursive)
        {
            dataControl.packages = new Hashtable();
            dataControl.enabledPath = enabledPath;
            ReloadFiles(recursive);

            // Dispose check for dupes list
            nFiles = dataControl.files.Count;
            dataControl.packages = null;
        }

        #region public properties
        public S2CPackageList Items => dataControl.files;

        public S2CPackage this[int index]
        {
            get { return dataControl.files[index]; }
        }
        #endregion

        #region private_methods

        private byte[] RawFile(string fileName)
        {
            FileInfo fi = new FileInfo(fileName);
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            //BinaryReader r = new BinaryReader(fs);
            byte[] byteArr = new byte[fi.Length];
            fs.Read(byteArr, 0, (int)fi.Length);
            //byteArr =  r.ReadBytes((int)fi.Length);
            fs.Close();
            //r.Close();
            return byteArr;
        }

        private void AddFile(string file, bool _enabled)
        {
            S2CPackage tFile;
            FileInfo fi = new FileInfo(file);
            tFile = new S2CPackage(RawFile(file), file);

            tFile.enabled = _enabled;
            dataControl.files.Add(tFile);

            int t = dataControl.files.Count;

            // Check for duplicates

            if(dataControl.packages.Contains(tFile.md5))
            {
                ((S2CPackage)dataControl.packages[tFile.md5]).duplicated = true;
                tFile.duplicated = true;
            }
            else
            {
                dataControl.packages.Add(tFile.md5, tFile);
            }
        }

        // true => continue processing; false => don't process any more files or paths.
        private bool LoadFilesFromPath(string path, bool enabled, bool recursive)
        {
            if (!Directory.Exists(path))
                return true;

            // To test error handling:
            // path = "C:\\My Documents\\EA Games\\The Sims 2\\Downloads\\PathOrFileNameTooLong\\PathOrFileNameTooLong\\PathOrFileNameTooLong\\PathOrFileNameTooLong\\PathOrFileNameTooLong\\PathOrFileNameTooLong\\PathOrFileNameTooLong\\PathOrFileNameTooLong\\PathOrFileNameTooLong\\PathOrFileNameTooLong";
            if (path.Length >= 248)
            {
                // Legacy Windows path-length limit; macOS doesn't share it.
                // Keep the skip + continue behavior, log instead of prompting.
                Debug.WriteLine($"Path too long, skipping: {path}");
                return true;
            }

            if(recursive)
            {
                string[] dirs = Directory.GetDirectories(path);
                foreach (string dir in dirs)
                {
                    if (!LoadFilesFromPath(dir, enabled, recursive))
                        return false;
                }
            }

            string[] files = Directory.GetFiles(path, "*.package");

            foreach(string file in files)
            {
                try
                {
                    AddFile(file, true);
                    // Test error handling:
                    // throw new Exception("There was a Mootilda error in this file.");
                }
                catch (Exception e)
                {
                    // Legacy raised a Yes/No "continue?" prompt; we log and
                    // continue silently — Avalonia integration can layer
                    // user feedback on top via Config.ConfirmOverwrite-style hooks.
                    Debug.WriteLine($"Skipping {file}: {e.Message}");
                }
            }

            files = Directory.GetFiles(path, "*.off");

            foreach(string file in files)
            {
                try
                {
                    AddFile(file, false);
                    // Test error handling:
                    // throw new Exception("There was a Mootilda error in this file.");
                }
                catch (Exception e)
                {
                    // Legacy raised a Yes/No "continue?" prompt; we log and
                    // continue silently — Avalonia integration can layer
                    // user feedback on top via Config.ConfirmOverwrite-style hooks.
                    Debug.WriteLine($"Skipping {file}: {e.Message}");
                }
            }
            return true;
        }
        #endregion

        #region public_methods
        public void ReloadFiles(bool recursive)
        {
            dataControl.files = new S2CPackageList();
            LoadFilesFromPath(dataControl.enabledPath, true, recursive);
        }

        public string GetFileName(int index)
        {
            return dataControl.files[index].fileName;
        }
        #endregion

    }
    #endregion
}