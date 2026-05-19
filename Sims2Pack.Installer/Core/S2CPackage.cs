#nullable disable
/***************************************************************************
 *   Copyright (C) 2004 by phervers                                        *
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
using System.IO;
using System.Collections;
using DatGen.Types.TS2;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Sims2Pack_Installer
{
    using DatGen.Types.TS2;

    #region S2CPackageList
    public class S2CPackageList : CollectionBase
    {
        public S2CPackage this[int index]
        {
            get
            {
                if(index>=List.Count)
                    return null;
                return ((S2CPackage)(List[index]));
            }
            set { List[index] = value; }
        }

        public int Add(S2CPackage value)
        {
            return List.Add(value);
        }

        public void Insert(int index, S2CPackage value)
        {
            List.Insert(index, value);
        }

        public void Remove(S2CPackage value)
        {
            List.Remove(value);
        }

        public bool Contains(S2CPackage value)
        {
            return List.Contains(value);
        }

    }
    #endregion

    #region S2CPackage
    public class S2CPackage
    {
        #region data_storage

        public S2CIPackageInfo info;

        public string fileName, type, crc, md5, description, tooltip, instModeFileName;

        public int offset, size;

        public bool enabled, duplicated;
        public ArrayList images;
        public ArrayList textures;

        #endregion

        #region constructors

        public S2CPackage()
        {
            offset = 0;
            enabled = true;
            instModeFileName = null;

        }

        public S2CPackage(byte[] contents, string FileName)
        {
            offset = 0;
            fileName = FileName;
            enabled = true;
            instModeFileName = null;

            Complete(contents);
        }
        public S2CPackage(string FileName)
        {
            offset = 0;
            instModeFileName = null;
            fileName = FileName;
            enabled = true;
            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);

            byte[] contents = new byte[stream.Length];
            contents = reader.ReadBytes((int)stream.Length);
            Complete(contents);

            stream.Close();
        }
        #endregion

        #region public properties
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if (enabled == value) return;
                enabled = value;
                string newFileName = Path.ChangeExtension(
                    fileName, enabled ? ".package" : ".off");
                File.Move(fileName, newFileName);
                fileName = newFileName;
            }
        }

        // Display state surfaced as plain properties so the ViewModel can
        // translate to Avalonia colors without the model knowing about UI.
        public string DisplayTypeName
        {
            get
            {
                string s = info?.TypeName() ?? "";
                return (info != null && info.missingBinaryIndex)
                    ? "Incomplete BodyShop Project: " + s
                    : s;
            }
        }

        public bool IsBodyShopIncomplete => info != null && info.missingBinaryIndex;
        public bool IsOverwriting        => info != null && info.overwriting;
        public bool IsDuplicated         => duplicated;
        #endregion

        #region private methods
        private void ReadPackage(byte[] contents)
        {

            images      = new ArrayList();
            textures    = new ArrayList();
            tooltip = "Package: " + Path.GetFileName(fileName);

            // string tooltipAdd = "";

            PackageEntryCollection entries = myDBPF.ReadPackage(contents);

            if(entries.Count == 0)
            {
                info.packageType = PackageTypes.ptEmpty;
            }

            // A package which contains only BHAV or BCON records is a Behavior Mod
            int iBehaviorMod = 0;

            // A behavior mod with object functions is an Object Mod
            int iObjectMod = 0;

            // A package which contains only TXTR or GMDC is probably a Default Replacement
            int iDefaultReplacement = 0;

            // A package which contains just property sets (GZPS) is probably a recategorization mod
            int iCategoryMod = 0;

            // A package which contains just UIData is probably a UI mod
            int iUIMod = 0;

            // A package which contains just material overrides (MMAT) is probably a recolor
            int iMatOverride = 0;

            // Note that the order here is important:
            // A package may be reoognized as multiple types.
            // For example, a lot may be both packaged and occupied.
            // Later recognitors override earlier ones.
            BasicRecognitor[] recognitors = new BasicRecognitor[14];
            CASPackageRecognitor casRecognitor = new CASPackageRecognitor();
            int iIndex = 0;
            recognitors[iIndex++] = new CollectionRecognitor();
            recognitors[iIndex++] = new MeshRecognitor();
            recognitors[iIndex++] = new FenceRecognitor();
            recognitors[iIndex++] = new RoofRecognitor();
            recognitors[iIndex++] = new ObjectRecolorRecognitor();
            recognitors[iIndex++] = new GameObjectRecognitor();
            recognitors[iIndex++] = new CareerRecognitor();
            recognitors[iIndex++] = new FloorWallPaintRecognitor();
            recognitors[iIndex++] = casRecognitor;
            recognitors[iIndex++] = new FacialStructureRecognitor();
            recognitors[iIndex++] = new CharacterSegmentRecognitor();
            recognitors[iIndex++] = new FamilySegmentRecognitor();
            recognitors[iIndex++] = new LotSegmentRecognitor();
            recognitors[iIndex++] = new OccupiedLotRecognitor();

            bool overwriting = false;
            for(int i=0; i<entries.Count; i++)
            {
                // Necessary evil or maybe not :P
                entries[i].mOffset = (uint)offset;

                #region recognition loop
                #region switch
                switch(entries[i].TypeID)
                {

                        //Texture
                    case (uint)Types.Texture:
                        //PackageEntry ent = entries[i];
                        entries[i].FileName = (instModeFileName != null) ? instModeFileName : fileName;
                        entries[i].RawData = null;
                        textures.Add(entries[i]);

                        // ToDo: would be nice if we could specify the type of default replacement:
                        // xyBody => clothing
                        // easel-painting => painting
                        iDefaultReplacement++;
                        break;

                    case (uint)Types.GeometricData: // GMDC: Geometric Data Container
                        // ToDo: would be nice if we could specify the type of default replacement:
                        // xyArch => sim
                        iDefaultReplacement++;
                        break;

                        // Image
                    case (uint)Types.Image:
                    case (uint) Types.JPG:
                        TS2Image image = new TS2Image();
                        image.Load(entries[i].RawData);
                        if (image.Image != null)
                        {
                            if ((entries[i].InstanceID == 0x35CA0002)   // Main picture for Lot
                            /* || (entries[i].InstanceID == 0x6CD85218) */)  // Main picture for Family
                            {
                                // For lots and families, we need the main picture to be first in the list.
                                // Occupied lots will be one of the two main pictures.
                                ArrayList clone = new ArrayList();
                                clone.Add(image.Image);
                                for (int j = 0; j < images.Count; j++)
                                {
                                    Object temp = images[j];
                                    clone.Add(temp);
                                }
                                images = clone;
                            }
                            else
                                images.Add(image.Image);
                        }
                        break;

                    case 0x4F424A66:                // OBJF: Object functions
                        if (entries[i].GroupID != 0xFFFFFFFF)
                        {
                            iObjectMod++;
                        }
                        break;

                    case 0x54544142:                // TTAB: Pie menu functions
                        if (entries[i].GroupID != 0xFFFFFFFF)
                        {
                            overwriting = true;
                            iBehaviorMod++;
                            iObjectMod++;               // Doesn't preclude object mod
                        }
                        break;

                    case 0x54544173:                // TTAS: Pie menu strings
                        if (entries[i].GroupID != 0xFFFFFFFF)
                        {
                            iBehaviorMod++;             // Doesn't preclude Behavior Mod
                            iObjectMod++;               // Doesn't preclude Object Mod
                        }
                        break;


                    case 0x42484156:                // BHAV's
                        if (entries[i].GroupID != 0xFFFFFFFF)
                        {
                            overwriting = true;
                            iBehaviorMod++;
                            iObjectMod++;               // Doesn't preclude Object Mod
                        }
                        break;

                    case 0x42434F4E:                // BCON: Behavior Constant
                        if (entries[i].GroupID != 0xFFFFFFFF)
                        {
                            overwriting = true;
                            iBehaviorMod++;
                            iObjectMod++;               // Doesn't preclude Object Mod
                        }
                        break;


                        //Globals
                    case (uint)Types.Globals:
                        Global global = new Global();
                        global.Load(entries[i].RawData);
                        entries[i].file = global;
                        break;


                        // OBJD files
                    case (uint)Types.OBJD:

                        nOBJD objd = new nOBJD();
                        objd.Load(entries[i].RawData);
                        entries[i].file = objd;
                        break;


                    case (uint)Types.CatDesc:
                        iBehaviorMod++;                 // Doesn't preclude Behavior Mod
                        iObjectMod++;                   // Doesn't preclude Object Mod
                        iDefaultReplacement++;          // Doesn't preclude Default Replacement
                        iCategoryMod++;                 // Doesn't preclude Category Mod
                        iMatOverride++;                 // Doesn't preclude Object Recolor
                        iUIMod++;                       // Doesn't preclude UI Mod

                        STRL strl = new STRL();
                        strl.Load(entries[i].RawData);
                        entries[i].file = strl;
                        // Get name and description of mod from Catalog Description
                        if ((strl.Languages.Count > 0) && (null == info.name))
                        {
                            string sName = strl.Languages[0].Items[0].Value;
                            if ((null != sName) && ("" != sName))
                            {
                                info.name = sName;
                                info.description = strl.Languages[0].Items[1].Value;
                            }
                        }
                        break;

                    case (uint)Types.MatOverride:
                    case (uint)Types.FloorXML:
                    case (uint)Types.ObjectXML:
                        EXMP exmp = new EXMP();
                        exmp.Load(entries[i].RawData);
                        entries[i].file = exmp;
                        if (entries[i].TypeID == (uint)Types.MatOverride)
                            iMatOverride++;
                        break;

                    case 0xCDB467B8:                // CREG: Content Registry
                        iMatOverride++;                 // Doesn't preclude Object Recolor
                        break;

                    case 0xE86B1EEF:                // CLST: Directory of Compressed Files
                    case (uint)Types.TextLists:     // STR#: Test Lists
                        iBehaviorMod++;                 // Doesn't preclude Behavior Mod
                        iObjectMod++;                   // Doesn't preclude Object Mod
                        iDefaultReplacement++;          // Doesn't preclude Default Replacement
                        iCategoryMod++;                 // Doesn't preclude Category Mod
                        iMatOverride++;                 // Doesn't preclude Object Recolor
                        iUIMod++;                       // Doesn't preclude UI Mod
                        break;

                    case (uint)Types.VersionInfo:   // VERS: Version Information
                        iBehaviorMod++;                 // Doesn't preclude Behavior Mod
                        iObjectMod++;                   // Doesn't preclude Object Mod
                        iDefaultReplacement++;          // Doesn't preclude Default Replacement
                        iCategoryMod++;                 // Doesn't preclude Category Mod
                        iMatOverride++;                 // Doesn't preclude Object Recolor
                        iUIMod++;                       // Doesn't preclude UI Mod

                        // Get name and description of mod from version info
                        EXMP Exmp = new EXMP();
                        Exmp.Load(entries[i].RawData);
                        entries[i].file = Exmp;
                        if (null == info.name)
                        {
                            string sName = Exmp.Properties.GetByName("Name").StringValue;
                            if ((null != sName) && ("" != sName))
                            {
                                info.name = sName;
                                info.description = Exmp.Properties.GetByName("Description").StringValue;
                            }
                        }

                        break;

                    case (uint)Types.PropertySet:
                        iCategoryMod++;
                        break;

                    case 0x00000000:                // UI: UI Data
                        iUIMod++;
                        break;

                    case 0x6D619378:                // XNBG: Neighborhood Object XML
                        // Does this include all neighborhood objects?
                        break;
                }

                #endregion


                foreach(BasicRecognitor recognitor in recognitors)
                {
                    recognitor.Feed(entries[i]);
                }

                #endregion

            }

            foreach(BasicRecognitor recognitor in recognitors)
            {
                if(recognitor.isRecognized())
                {
                    info = recognitor.getInfo();
                }
            }
            info.overwriting = overwriting;

            if (overwriting)
            {
                // If a package overwrites global behavior, there's a good chance that it's a behavior mod.
                if (PackageTypes.ptUnknown == info.packageType)
                    info.packageType = PackageTypes.ptBehaviorMod;
                else if (info.missingBinaryIndex)
                {
                    // CreatureFixes from MATY looks like an incomplete CAS project, but is actually a behavior mod.
                    info.packageType = PackageTypes.ptBehaviorMod;
                    info.missingBinaryIndex = false;
                }
            }

            if (PackageTypes.ptUnknown == info.packageType)
            {
                if ((iUIMod > 0) && (iUIMod == entries.Count))
                    info.packageType = PackageTypes.ptUIMod;
                else if ((iBehaviorMod == entries.Count)
                  && (iObjectMod == entries.Count)
                  && (iDefaultReplacement == entries.Count)
                  && (iCategoryMod == entries.Count)
                  && (iMatOverride == entries.Count))
                { }   // can't tell anything about this package
                else if (iBehaviorMod == entries.Count)
                    info.packageType = PackageTypes.ptBehaviorMod;
                else if (iObjectMod == entries.Count)
                    info.packageType = PackageTypes.ptObjectMod;
                else if (iDefaultReplacement == entries.Count)
                    info.packageType = PackageTypes.ptDefaultReplacement;
                else if (iCategoryMod == entries.Count)
                    info = casRecognitor.getInfo();
                else if (iMatOverride == entries.Count)
                    info.packageType = PackageTypes.ptRecolor;
            }

            //tooltip+=tooltipAdd;
        }


        #endregion

        #region public methods

        public void MoveTo(string folder)
        {
            string newFilename  = Path.Combine(folder, Path.GetFileName(fileName));
            if (File.Exists(newFilename))
            {
                if (!Config.ConfirmOverwrite(newFilename))
                    return;
                File.Delete(newFilename);
            }
            File.Move(fileName, newFilename);
            fileName = newFilename;
        }

        public void CopyTo(string folder)
        {
            string newFilename  = Path.Combine(folder, Path.GetFileName(fileName));

            if (File.Exists(newFilename))
            {
                if (!Config.ConfirmOverwrite(newFilename))
                    return;
                File.Delete(newFilename);
            }
            File.Copy(fileName, newFilename);
            fileName = newFilename;
        }

        public void Rename(string newName)
        {
            string newFilename = Path.Combine(Path.GetDirectoryName(fileName), newName);
            if (File.Exists(newFilename))
            {
                if (!Config.ConfirmOverwrite(newFilename))
                    return;
                File.Delete(newFilename);
            }
            File.Move(fileName, newFilename);
            fileName = newFilename;
        }

        public void Remove()
        {
            File.Delete(fileName);
        }

        public void Complete(byte[] contents)
        {
//            string tCRC = Path.GetFileNameWithoutExtension(fileName);
//            if(tCRC.Length == 32)
//                crc = tCRC;
//

//            name = Path.GetFileNameWithoutExtension(fileName);


            // Calculate MD5
            MD5 fMD5 = MD5.Create();
            byte[] result = fMD5.ComputeHash( contents );
            md5 = Utility.HexEncoding.ToString(result);

            // Get info from hack DB
            info = Config.packageDescription.GetInfo(md5);


            // Read package contents, add to info
            ReadPackage(contents);

            //If still no info display filename
            if ((null == info.name) || ("" == info.name))
            {
                info.name = Path.GetFileNameWithoutExtension(fileName);
            }

        }


        // GetItem() removed: it returned a WinForms ListViewItem.
        // Display data is now exposed via DisplayTypeName / IsBodyShopIncomplete /
        // IsOverwriting / IsDuplicated; the Avalonia ViewModel maps these
        // to colors and bindings.

        #endregion

    }
    #endregion
}