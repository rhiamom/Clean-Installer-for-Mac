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
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;
using S2;
using SimPe.Interfaces.Files;
using SimPe.Packages;


namespace Sims2Pack_Installer
{

    #region S2Sims2Pack
    public class S2Sims2Pack
    {
        #region Data_storage
        private struct Data
        {
            public string fileName;
            public string gameVersion;
            public S2CPackageList files;
            public int firstFileOffset;
        }
        private Data dataControl;
        public string type;

        /// <summary>Enumerates the packages inside this Sims2Pack after construction.</summary>
        public S2CPackageList Items => dataControl.files;
        #endregion

        #region constructors
        public S2Sims2Pack(ArrayList files, string desc, string fileName)
        {
            FileStream fs, fsw;
            try
            {
                fsw = new FileStream(fileName, FileMode.CreateNew);
                BinaryWriter writer = new BinaryWriter(fsw);

                string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<Sims2Package type=\"Sim\">\n  <GameVersion>2141707388.153.1</GameVersion>\n";
                int offset = 0;
                for(int i = 0; i<files.Count ; i++)
                {
                    FileInfo fi = new FileInfo((string)files[i]);
                    int fSize = (int)fi.Length;

                    string sFileName = System.Web.HttpUtility.HtmlEncode(Path.GetFileName((string)files[i]));
                    xml += "  <PackagedFile>\n";
                    xml += "    <Name>" + sFileName /* Path.GetFileName((string)files[i]) */ + "</Name>\n";
                    xml += "    <Length>"+ Convert.ToString(fSize) +"</Length>\n";
                    xml += "    <Type>sim</Type>\n";
                    xml += "    <Crc>"+ MD5File((string)files[i], fSize) +"</Crc>\n" ;
                    xml += "    <Offset>"+offset+"</Offset>\n" ;
                    xml += "    <Description><![CDATA["+ ((i==0)?desc+ "\n\nSims2Pack file created with\nsims2pack.modthesims2.com":"") +"]]></Description>\n" ;
                    xml += "  </PackagedFile>\n";
                    offset += fSize;
                }
                xml += "</Sims2Package>";

                string t = "Sims2 Packager 1.0";
                writer.Write(t.ToCharArray());
                writer.Write((UInt32)(xml.Length+22));
                writer.Write(xml.ToCharArray());
                for(int i=0;i<files.Count; i++)
                {
                    fs = new FileStream((string)files[i], FileMode.Open,FileAccess.Read, FileShare.Read);
                    BinaryReader reader = new BinaryReader(fs);
                    byte[] byteArray = new byte[fs.Length];
                    byteArray = reader.ReadBytes((int)fs.Length);

                    writer.Write(byteArray);

                }
                Debug.WriteLine("Sims2Pack file successfully created: " + fileName);
            }
            catch (Exception e)
            {
                Debug.WriteLine("S2Sims2Pack ctor (create) failed: " + e.Message);
            }
        }



        public S2Sims2Pack(string fileName)
        {
            dataControl.fileName = fileName;
            type = "";
            dataControl.files = new S2CPackageList();

            FileStream stream = new FileStream(fileName, FileMode.Open,FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);

            string checkVersion=new String(reader.ReadChars(18));

            if (checkVersion!="Sims2 Packager 1.0")//make sure it is a format I can read
            {
                reader.Close();
                return ;// if not quit
            }

            dataControl.firstFileOffset = (int)reader.ReadUInt32();

            int xmlSize = dataControl.firstFileOffset - 22;
            string xml = new String(reader.ReadChars(xmlSize));

            ParseXML(xml);
            RecognizeFiles();
            stream.Close();

        }

        #endregion

        #region private_methods

        private string MD5File(string fileName, int size)
        {

            FileStream fs = new FileStream(fileName, FileMode.Open);
            BinaryReader r = new BinaryReader(fs);
            byte[] byteArray = new byte[size];
            byteArray = r.ReadBytes(size);
            MD5 fMD5 = MD5.Create();
            byte[] result = fMD5.ComputeHash( byteArray );
            fs.Close();
            return Utility.HexEncoding.ToString(result);
        }


        private byte[] GenerateXML(bool bIncludeAllFiles)
        {
            string xml;
            int offset = 0;
            xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<Sims2Package type=\"" + type + "\">\n  <GameVersion>2141707388.153.1</GameVersion>\n";
            for(int i = 0;i<dataControl.files.Count;i++)
            {
                if (dataControl.files[i].enabled)
                {
                    if ((dataControl.files[i].info.installToTeleport) || bIncludeAllFiles)
                    {
                        string sFileName = System.Web.HttpUtility.HtmlEncode(dataControl.files[i].fileName);
                        xml += "  <PackagedFile>\n" +
                               "    <Name>" + sFileName /* dataControl.files[i].fileName */ + "</Name>\n" +
                               "    <Length>" + Convert.ToString(dataControl.files[i].size) + "</Length>\n" +
                               "    <Type>" + dataControl.files[i].type + "</Type>\n" +
                               "    <Crc>" + dataControl.files[i].crc + "</Crc>\n" +
                               "    <Offset>" + offset + "</Offset>\n" +
                               "    <Description><![CDATA[" + dataControl.files[i].description + "]]></Description>\n" +
                               "  </PackagedFile>\n";
                        offset += dataControl.files[i].size;
                    }
                }
            }
            xml += "</Sims2Package>";
            // XML needs to be UTF8 encoded
            UTF8Encoding utf8 = new UTF8Encoding();
            byte[] validXML = utf8.GetBytes(xml);

            return validXML;
        }


        private void RecognizeFiles()
        {
            byte[] byteArray;
            try
            {
                FileStream fs = new FileStream(dataControl.fileName, FileMode.Open, FileAccess.Read);
                BinaryReader dbpf = new BinaryReader(fs);
                dbpf.BaseStream.Seek(dataControl.firstFileOffset,SeekOrigin.Begin);
                for(int i=0;i<dataControl.files.Count;i++)
                {
                    dataControl.files[i].offset =  (int)dbpf.BaseStream.Position;
                    byteArray = new Byte[dataControl.files[i].size];
                    byteArray = dbpf.ReadBytes(dataControl.files[i].size);


                    dataControl.files[i].instModeFileName = dataControl.fileName;

                    dataControl.files[i].Complete(byteArray);
                }

                dbpf.Close();
            }
            catch (System.Exception e)
            {
                Debug.WriteLine("RecognizeFiles failed: " + e);
                return;
            }
        }


        private void ParseXML(string xml)
        {
            S2CPackage tFile = null;

            try
            {

                XmlTextReader parser = new XmlTextReader(xml, XmlNodeType.Document, null);
                while (parser.Read())
                {
                    if(parser.NodeType == XmlNodeType.Element)
                    {
                        if(parser.Name == "Sims2Package")
                        {
                            type = parser.GetAttribute("type");
                        }
                        if(parser.Name == "PackagedFile")
                        {
                            // Next File
                            if(tFile != null)
                                dataControl.files.Add(tFile);
                            tFile = new S2CPackage();
                        }
                        else
                        {
                            string  nodeName = parser.Name;
                            parser.Read();
                            switch(nodeName)
                            {
                                case "GameVersion" :
                                    dataControl.gameVersion = parser.Value;
                                    break;
                                case "Name" :
                                    tFile.fileName = parser.Value;
                                    break;
                                case "Length" :
                                    tFile.size = Convert.ToInt32(parser.Value);
                                    break;
                                case "Type" :
                                    tFile.type = parser.Value;
                                    break;
                                case "Crc" :
                                    tFile.crc = parser.Value;
                                    break;
                                case "Offset" :
                                    tFile.offset = Convert.ToInt32( parser.Value);
                                    break;
                                case "Description" :
                                    tFile.description = parser.Value;
                                    break;
                            }

                        }
                    }
                }
                dataControl.files.Add(tFile);
            }
            catch
            {
                if (null != tFile)
                    dataControl.files.Add(tFile);
            }

        }


        private void WriteToFile(string fileName, byte[] byteArray)
        {
            try
            {
                FileStream fsw = new FileStream(fileName, FileMode.CreateNew);
                BinaryWriter writer = new BinaryWriter(fsw);
                writer.Write(byteArray);
                writer.Close();
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }

        }

        private bool ShowFileWarningDialog(string fileName)
        {
            if (!System.IO.File.Exists(fileName))
                return true;
            if (Config.ConfirmOverwrite(fileName))
            {
                System.IO.File.Delete(fileName);
                return true;
            }
            return false;
        }

        #endregion

        #region public_properties

        public S2CPackage this[int index]
        {
            get
            {
                return dataControl.files[index];
            }
        }
        public int numFiles
        {
            get
            {
                return 0;
            }
        }
        #endregion

        #region public_methods

        public void SaveAs(string fileName)
        {
            byte[] xml = GenerateXML(true);
            FileStream fsw=null, fs = null;
            try
            {
                fsw = new FileStream(fileName, FileMode.CreateNew);
                BinaryWriter writer = new BinaryWriter(fsw);
                string t = "Sims2 Packager 1.0";
                writer.Write(t.ToCharArray());
                writer.Write((UInt32)(xml.Length + 22));
                writer.Write(xml);

                fs = new FileStream(dataControl.fileName, FileMode.Open, FileAccess.Read);
                BinaryReader dbpf = new BinaryReader(fs);
                dbpf.BaseStream.Seek(dataControl.firstFileOffset,SeekOrigin.Begin);

                for(int i=0 ; i<dataControl.files.Count ; i++)
                {
                    if(dataControl.files[i].enabled)
                    {
                        dbpf.BaseStream.Seek(dataControl.files[i].offset, SeekOrigin.Begin);
                        byte[] byteArray = new Byte[dataControl.files[i].size];
                        byteArray = dbpf.ReadBytes(dataControl.files[i].size);
                        writer.Write(byteArray);
                    }
                }
                fsw.Close();
                fs.Close();

            }
            catch
            {
                if(fsw!=null)
                    fsw.Close();
                if(fs!=null)
                    fs.Close();
                Debug.WriteLine("Saving failed");
            }
        }


        private byte[] RemoveFurniture(byte[] byteArray)
        {
            // Find and modify LotDescriptor
            MemoryStream MR = null;
            BinaryReader BR = null;
            try
            {
                MR = new MemoryStream(byteArray);
                BR = new BinaryReader(MR);
                GeneratableFile LotPack = SimPe.Packages.File.LoadFromStream(BR);
                IPackedFileDescriptor IPFD = LotPack.FindFile(0x6C589723, 0, 0xFFFFFFFF, 0);
                R_LOT Res = new R_LOT(LotPack, IPFD);
                uint U0 = Res.U0;
                if ((U0 & 0x08) == 0)
                {
                    U0 |= 0x08;
                    Res.U0 = U0;
                    MemoryStream MW = LotPack.Build();
                    LotPack.Close();
                    byteArray = MW.ToArray();
                    MW.Close();
                }
            }
            catch
            {
                Debug.WriteLine("Unable to remove furniture");
            }
            if (null != BR)
                BR.Close();
            if (null != MR)
                MR.Close();
            return byteArray;
        }


        public bool InstallLotPackage()
        {
            return InstallLotPackage(false);
        }


        public bool InstallLotPackage(bool bRemoveFurniture)
        {
            byte[] byteArray;
            try
            {
                string targetFileName = "";

                FileStream fs = new FileStream(dataControl.fileName, FileMode.Open, FileAccess.Read);
                BinaryReader dbpf = new BinaryReader(fs);

                //Create import file

                targetFileName = Sims2Directories.Teleport + dataControl.files[0].crc + ".Sims2Import";

                FileStream fsw = new FileStream(targetFileName, FileMode.CreateNew);
                BinaryWriter writer = new BinaryWriter(fsw);

                string t1 = "Sims2 Packager 1.0";
                writer.Write(t1.ToCharArray());

                byte[] xml = GenerateXML(false);
                UInt32 nOffset = (UInt32)xml.Length + 22;
                writer.Write(nOffset);

                writer.Write(xml);
                writer.Close();

                //End of import creation

                //Seek to first file

                dbpf.BaseStream.Seek(dataControl.firstFileOffset,SeekOrigin.Begin);

                //Process every file

                for(int i=0;i<dataControl.files.Count;i++)
                {

                    if(dataControl.files[i].enabled)
                    {
                        //Add info to local db

                        DateTime d1 = DateTime.Now;
                        string desc = dataControl.files[i].description==""?"":dataControl.files[i].description+"<br><br>";
                        desc += "Filename: " + dataControl.fileName + "<br>";
                        desc += "Installed on: " + d1.ToShortTimeString() +"   "+  d1.ToLongDateString();
                        dataControl.files[i].description = desc;
                        Config.packageDescription.AddToLocalDB(dataControl.files[i]);

                        //Info added

                        //Read file into byte[]

                        dbpf.BaseStream.Seek(dataControl.files[i].offset, SeekOrigin.Begin);
                        byteArray = new Byte[dataControl.files[i].size];
                        byteArray = dbpf.ReadBytes(dataControl.files[i].size);

                        if (bRemoveFurniture && (dataControl.files[i].type == "Lot"))
                            byteArray = RemoveFurniture(byteArray);

                        //Determine target directory

                        if (dataControl.files[i].info.installToTeleport)
                            targetFileName = Sims2Directories.Teleport + dataControl.files[i].crc + ".Sims2Tmp";
                        else
                            switch (dataControl.files[i].type)
                            {
                                case "Lot":
                                case "Family":
                                case "Person":
                                    // I believe that this code is unreachable 
                                    // because these files should all have installToTeleport
                                    targetFileName = Sims2Directories.Teleport + dataControl.files[i].crc + ".Sims2Tmp";
                                    break;
                                default:
                                    // Legacy code consulted S2PCI.ini [SubFolders] to map each
                                    // file type to a per-type subfolder under Downloads. v1
                                    // drops the .ini and installs everything to Downloads root.
                                    targetFileName = Sims2Directories.Downloads + dataControl.files[i].fileName;
                                    break;


                            }
                        if (ShowFileWarningDialog(targetFileName))
                            WriteToFile(targetFileName, byteArray);
                    }
                    else
                    {
                        dbpf.BaseStream.Seek(dataControl.files[i].size,SeekOrigin.Current);
                    }
                }

                dbpf.Close();
            }
            catch(System.Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            return true;
        }


        public bool InstallNormalPackage(string folder, bool addToDB)
        {
            try
            {
                string targetFileName = "";

                FileStream fs = new FileStream(dataControl.fileName, FileMode.Open, FileAccess.Read);
                BinaryReader dbpf = new BinaryReader(fs);

                //Seek to first file

                dbpf.BaseStream.Seek(dataControl.firstFileOffset,SeekOrigin.Begin);

                //Process every file

                for(int i=0;i<dataControl.files.Count;i++)
                {


                    if(dataControl.files[i].enabled)
                    {
                        //Add info to local db

                        if(addToDB)
                        {
                            DateTime d1 = DateTime.Now;
                            string desc = dataControl.files[i].description==""?"":dataControl.files[i].description+"<br><br>";
                            desc += "Filename: " + dataControl.fileName + "<br>";
                            desc += "Installed on: " + d1.ToShortTimeString() +"   "+  d1.ToLongDateString();
                            dataControl.files[i].description = desc;
                            Config.packageDescription.AddToLocalDB(dataControl.files[i]);
                            //Info added
                        }



                        //Read file into byte[]

                        dbpf.BaseStream.Seek(dataControl.files[i].offset, SeekOrigin.Begin);
                        byte[] byteArray = new Byte[dataControl.files[i].size];
                        byteArray = dbpf.ReadBytes(dataControl.files[i].size);

                        //Determine target directory

                        targetFileName = folder + dataControl.files[i].fileName;

                        if(ShowFileWarningDialog(targetFileName))
                            WriteToFile(targetFileName, byteArray);
                    }
                    else
                    {
                        dbpf.BaseStream.Seek(dataControl.files[i].size,SeekOrigin.Current);
                    }
                }

            }
            catch (System.Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public bool InstallPackage(bool defaultInstall, string folder, string ext, bool createImport,
            bool bRemoveFurniture)
        {
            byte[] byteArray;
            try
            {
                string targetFileName = "";
                string downloads = folder;
                if (0 == string.Compare(ext, "Sims2Tmp"))
                {
                    // When CC is placed into the Teleport folder with a .Sims2Tmp extension,
                    // the game destroys all of the identifying information in the file name.
                    // CC is much better in the Downloads folder with a .package extension.
                    string dir = Path.GetDirectoryName(folder);
                    string subdir = Path.GetFileName(dir);
                    dir = Path.GetDirectoryName(dir);
                    if (0 == string.Compare(subdir, "Teleport"))
                        downloads = dir + Path.DirectorySeparatorChar + "Downloads" + Path.DirectorySeparatorChar;
                }
                FileStream fs = new FileStream(dataControl.fileName, FileMode.Open, FileAccess.Read);
                BinaryReader dbpf = new BinaryReader(fs);
                if(createImport)
                {
                    targetFileName = folder + dataControl.files[0].crc + ".Sims2Import";

                    FileStream fsw = new FileStream(targetFileName, FileMode.CreateNew);
                    BinaryWriter writer = new BinaryWriter(fsw);

                    string t1 = "Sims2 Packager 1.0";
                    writer.Write(t1.ToCharArray());

                    byte[] xml = GenerateXML(false);
                    UInt32 nOffset = (UInt32)xml.Length + 22;
                    writer.Write(nOffset);

                    writer.Write(xml);

                }

                dbpf.BaseStream.Seek(dataControl.firstFileOffset,SeekOrigin.Begin);

                for(int i=0;i<dataControl.files.Count;i++)
                {
                    if(dataControl.files[i].enabled)
                    {
                        // Add info to local db
                        DateTime d1 = DateTime.Now;
                        string desc = dataControl.files[i].description==""?"":dataControl.files[i].description+"<br><br>";
                        desc += "Filename: " + dataControl.fileName + "<br>";
                        desc += "Installed on: " + d1.ToShortTimeString() +"   "+  d1.ToLongDateString();
                        dataControl.files[i].description = desc;
                        Config.packageDescription.AddToLocalDB(dataControl.files[i]);

                        // Info added

                        dbpf.BaseStream.Seek(  dataControl.files[i].offset, SeekOrigin.Begin);
                        byteArray = new Byte[dataControl.files[i].size];
                        byteArray = dbpf.ReadBytes(dataControl.files[i].size);

                        if (bRemoveFurniture && (dataControl.files[i].type == "Lot"))
                            byteArray = RemoveFurniture(byteArray);

                        if (defaultInstall)
                        {
                            // This code is unreachable.
                            switch (dataControl.files[i].type)
                            {
                                case "SimSkin":
                                    targetFileName = Sims2Directories.Downloads + dataControl.files[i].fileName + ".package";
                                    break;
                                default:
                                    targetFileName = folder + dataControl.files[i].crc + "." + ext;
                                    break;

                            }
                        }
                        else
                        {
                            if (dataControl.files[i].info.installToTeleport)
                                targetFileName = folder + dataControl.files[i].crc + "." + ext;
                            else
                                switch (dataControl.files[i].type)
                                {
                                    case "Lot":
                                    case "Family":
                                    case "Person":
                                        // I believe that this code is unreachable 
                                        // because these files should all have installToTeleport
                                        targetFileName = folder + dataControl.files[i].crc + "." + ext;
                                        break;
                                    default:
                                        if (0 == string.Compare(ext, "Sims2Tmp"))
                                            targetFileName = downloads + dataControl.files[i].fileName;
                                        else
                                            targetFileName = folder + Path.ChangeExtension(dataControl.files[i].fileName, ext);
                                        break;
                                }
                        }
                        if (ShowFileWarningDialog(targetFileName))
                            WriteToFile(targetFileName, byteArray);
                    }
                    else
                    {
                        dbpf.BaseStream.Seek(dataControl.files[i].size,SeekOrigin.Current);
                    }
                }

                dbpf.Close();
            }
            catch(System.Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            return true;
        }


        #endregion

    }
    #endregion
}