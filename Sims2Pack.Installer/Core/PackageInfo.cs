#nullable disable
/***************************************************************************
 *   Copyright (C) 2004-2007 by Karol Rybak                                *
 *   http://phervers.ModTheSims.info                                       *
 *                                                                         *
 *   Additional programming:                                               *
 *   Copyright (C) 2012-2013 by Mootilda                                   *
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

// NOTE: PackageTypes enum and PackageTypesNames class are defined in
// Recognitor.cs (they were external — DatGen.DBPF.dll — in the original).
// The Recognitor.cs versions are canonical and have more enum members.

namespace Sims2Pack_Installer
{
    #region S2CIPackageInfoList
    public class S2CIPackageInfoList : CollectionBase
    {
        public S2CIPackageInfo this[int index]
        {
            get { return ((S2CIPackageInfo)(List[index])); }
            set { List[index] = value; }
        }

        public int Add(S2CIPackageInfo value)
        {
            return List.Add(value);
        }

        public void Insert(int index, S2CIPackageInfo value)
        {
            List.Insert(index, value);
        }

        public void Remove(S2CIPackageInfo value)
        {
            List.Remove(value);
        }

        public bool Contains(S2CIPackageInfo value)
        {
            return List.Contains(value);
        }

    }
    #endregion

    #region S2CIPackageInfo
    public class S2CIPackageInfo
    {
        #region Data_storage

        private string text;
        public string MD5, name, version, author, email, url, description;
        public Int32 size;
        public bool local, overwriting, missingBinaryIndex, installToTeleport;
        public PackageTypes packageType;
        #endregion

        #region constructors
        public S2CIPackageInfo(string line)
        {
            installToTeleport = false;
            MD5 = null;
            name = null;
            version = null;
            author = null;
            email = null;
            url = null;
            description = null;

            text = line;
            Tokens f = new Tokens(line, new char[] {';'});
            foreach (string item in f)
            {
                int t = item.IndexOf('=');
                if(t!=-1)
                {
                    string st = item.Substring(0,t);
                    switch(item.Substring(0,t))
                    {
                        case "md5"          : MD5 =            item.Substring(t+1);break;
                        case "size"         : size =           Convert.ToInt32(item.Substring(t+1));break;
                        case "name"         : name =           Decode(item.Substring(t+1));break;
                        case "version"      : version =        item.Substring(t+1);break;
                        case "author"       : author =         Decode(item.Substring(t+1));break;
                        case "email"        : email =          item.Substring(t+1);break;
                        case "URL"          : url =            item.Substring(t+1);break;
                        case "Description"  : description =    Decode(item.Substring(t+1));break;
                        //case "type"         : type =           item.Substring(t+1);break;

                    }
                }
            }
        }

        public S2CIPackageInfo()
        {
        }
        #endregion

        private string Decode(string text)
        {
            text = text.Replace("<br>", "\n");
            text = text.Replace("&equal;", "=");
            text = text.Replace("\\\"", "\"");
            return text.Replace("\\'", "'");
        }
        public override string ToString()
        {
            return text;
        }

        public string TypeName()
        {
            return PackageTypesNames.GetName(packageType);
        }



    }
    #endregion

    public class S2CObjectPackageInfo : S2CIPackageInfo
    {
        public bool                         _original;
        // _package was typed S2CPackage in the original. While S2CPackage is
        // still being ported (parked in _pending/), keep this as object so
        // dependent code (Recognitor) can build. Will retype once S2CPackage
        // lands in Core/.
        public object                       _package;
        public string                       _modelName;

        public uint                         _guid,
                                            _thumbID,
                                            _roomSort,
                                            _funcSort;
    }
}