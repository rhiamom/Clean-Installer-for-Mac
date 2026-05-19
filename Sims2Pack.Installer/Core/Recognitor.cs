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
using DatGen.Types.TS2;

namespace Sims2Pack_Installer
{

    public enum Types : long
    {
        BinaryIndex       = 0x0C560F39,
        Collection        = 0x6C4F359D,
        Texture           = 0x1C4A276C,
        Image             = 0x856DDBAC,
        JPG               = 0x0C7E9A76,
        OBJD              = 0x4F424A44,
        Globals           = 0x474C4F42,
        TextLists         = 0x53545223,
        CatDesc           = 0x43545353,
        MatDef            = 0x49596978,
        MatOverride       = 0x4C697E5A,
        ObjectXML         = 0xCCA8E925,
        FloorXML          = 0x4DCADB7E,
        PropertySet       = 0xEBCF3E27,
        TextureOverlayXML = 0x2C1FD8A1,
        MeshOverlayXML    = 0x0C1FE246,
        HairToneXML       = 0x8C1580B5,
        SkinToneXML       = 0x4C158081,
        FacialStructure   = 0xCCCEF852,
        AgeData           = 0xAC598EAC,
        FamilyInformation = 0x46414D49,
        HouseDescriptor   = 0x484F5553,
        LotDescriptor     = 0x6C589723,
        GeometricData     = 0xAC4F8687,
        GeometricNode     = 0x7BA3838C,
        ResourceNode      = 0xE519C933,
        Shape             = 0xFC6EB1F7,
        FenceXML          = 0x2CB230B8,
        RoofXML           = 0xACA8EA06,
        VersionInfo       = 0xEBFEE342
    }

    public class PackageTypesNames
    {
        static string[] names =
        {
            "Unknown",
            "Empty Package",
            "Collection",
            "Object",
            "Recolor",
            "Person",
            "Career",
            "Floor",
            "Wallpaper",
            "Terrain Paint",
            "Beard",
            "Eyeliner",
            "Lipstick",
            "Eyebrow",
            "Eyeshadow",
            "Costume Makeup",
            "Stubble",
            "Blush",
            "Makeup",
            "Eye Color",
            "Clothing",
            "Hair Color",
            "Skin Tone",
            "Glasses",
            "Face Preset",
            "Bracelet",
            "Ring",
            "Earring",
            "Necklace",
            "Accessory",
            "Pets Body Part",
            "Packaged Sim",
            "Packaged Family",
            "Packaged Lot",
            "Occupied Lot",
            "Food",
            "Packaged Sim Data",
            "Packaged Ghost",
            "Fence",
            "Roof",
            "Default Replacement",
            "Behavior Mod",
            "Object Mod",
            "Category Mod",
            "Mesh",
            "Cat",
            "Dog",
            "UI Mod"
        };
        public static string GetName(PackageTypes type)
        {
            return names[(int)type];
        }
    }

    public enum PackageTypes : int
    {
        ptUnknown,
        ptEmpty,
        ptCollection,
        ptObject,
        ptRecolor,
        ptPerson,
        ptCareer,
        ptFloor,
        ptWallpaper,
        ptTerrainPaint,
        ptBeard,
        ptEyeliner,
        ptLipstick,
        ptEyebrow,
        ptEyeShadow,
        ptCostumeMakeup,
        ptStubble,
        ptBlush,
        ptMakeup,
        ptEyeColor,
        ptClothing,
        ptHairColor,
        ptSkinTone,
        ptGlasses,
        ptFacePreset,
        ptBracelet,
        ptRing,
        ptEarring,
        ptNecklace,
        ptJewelry,
        ptPetBodyParts,
        ptPackagedSim,
        ptPackagedFamily,
        ptPackagedLot,
        ptOccupiedLot,
        ptFood,
        ptPackagedSimData,
        ptPackagedGhost,
        ptFence,
        ptRoof,
        ptDefaultReplacement,
        ptBehaviorMod,
        ptObjectMod,
        ptCategoryMod,
        ptMesh,
        ptCat,
        ptDog,
        ptUIMod
    }

    #region BasicRecognitor
    /// <summary>
    /// Base class for all recognitors
    /// </summary>
    public class BasicRecognitor
    {
        private S2CIPackageInfo pInfo;

        public BasicRecognitor()
        {
            pInfo = new S2CIPackageInfo("");
        }

        public virtual void Feed(PackageEntry entry)
        {
        }

        public virtual bool isRecognized()
        {
            return false;
        }

        public virtual S2CIPackageInfo getInfo()
        {
            return pInfo;
        }
    }
    #endregion


    #region CollectionRecognitor
    /// <summary>
    /// Recognizes collections
    /// </summary>
    public class CollectionRecognitor : BasicRecognitor
    {

        private bool _collectionFile, _family;
        private S2CIPackageInfo pInfo;

        public CollectionRecognitor()
        {
            _collectionFile = false;
            _family = false;

            pInfo = new S2CIPackageInfo("");
            pInfo.packageType = PackageTypes.ptCollection;
        }

        public override void Feed(PackageEntry entry)
        {
            if(entry.TypeID == (uint)Types.FamilyInformation)
            {
                _family = true;
            }
            if(entry.TypeID == (uint)Types.Collection)
            {
                _collectionFile = true;
            }
            if(entry.file != null)
            {
                if(entry.TypeID == (uint)Types.TextLists)
                {
                    if(entry.file is STRL)
                    {
                        STRL strl = entry.file as STRL;
                        string sName = strl.Languages[0].Items[0].Value;
                        if( (null != sName) && ("" != sName))
                            pInfo.name = sName;
                    }
                }
            }
        }

        public override bool isRecognized()
        {
            return _collectionFile && !_family;
        }

        public override S2CIPackageInfo getInfo()
        {
            return pInfo;
        }
    }
    #endregion

    #region GameObjectRecognitor
    /// <summary>
    /// Base class for all recognizer's
    /// </summary>
    public class GameObjectRecognitor : BasicRecognitor
    {

        private bool _objdFile, _catDesc;

        private S2CObjectPackageInfo pInfo;

        public GameObjectRecognitor()
        {
            _objdFile = false;
            _catDesc  = false;

            pInfo = new S2CObjectPackageInfo();

            pInfo.packageType = PackageTypes.ptObject;
        }

        public override void Feed(PackageEntry entry)
        {
            if(entry.file is nOBJD)
            {
                nOBJD objd = entry.file as nOBJD;

                if(objd.Data.Type == 5 | objd.Data.Type == 8  | objd.Data.Type == 0xB | objd.Data.Type == 0xC
                    | objd.Data.Type == 4 | objd.Data.Type == 0xA | objd.Data.Type == 9)
                {
                    _objdFile = true;
                    string sName = objd.Data.Name;
                    if (((null == pInfo.name) || ("" == pInfo.name))
                     && ((null != sName) && ("" != sName)))
                    {
                        pInfo.name = sName;
                    }
                    if(objd.Data.Flag_FuncSort != 0 && objd.Data.Flag_RoomSort != 0)
                    {
                        pInfo._guid     = objd.Data.GUID;
                        pInfo._roomSort = objd.Data.Flag_RoomSort;
                        pInfo._funcSort = objd.Data.Flag_FuncSort;
                    }
                }


            }
            if(entry.TypeID == (uint)Types.CatDesc)
            {
                if(entry.file == null)
                {
                    STRL strl = new STRL();
                    strl.Load(entry.RawData);
                    entry.file = strl;
                }
                if(entry.file is STRL)
                {
                    STRL strl = entry.file as STRL;

                    int n = strl.Languages.IndexOfCode(1);
                    if (n == -1 /* not found */)
                        n = 0;
                    if (strl.Languages.Count > n)
                    {
                        string sName = strl.Languages[n].Items[0].Value;
                        if ((null != sName) && ("" != sName))
                            pInfo.name = sName;
                        pInfo.description  =  strl.Languages[n].Items[1].Value;

                        _catDesc = true;
                    }
                }
            }
            if(entry.TypeID == (uint)Types.TextLists)
            {
                if(entry.InstanceID == 0x85)
                {
                    if(entry.file == null)
                    {
                        STRL strl = new STRL();
                        strl.Load(entry.RawData);
                        entry.file = strl;
                    }
                    if(entry.file is STRL)
                    {
                        STRL strl = entry.file as STRL;
                        int n = strl.Languages.IndexOfCode(1);
                        if (n == -1 /* not found */)
                            n = 0;
                        if (strl.Languages.Count > n)
                        {
                            (pInfo as S2CObjectPackageInfo)._modelName    =  strl.Languages[n].Items[1].Value;
                        }
                    }
                }
            }
            if(entry.file is Global)
            {
                Global global = entry.file as Global;
                switch(global.Data)
                {
                    case "Food_Globals":
                        pInfo.packageType = PackageTypes.ptFood;
                        break;
                }
            }
        }

        public override bool isRecognized()
        {
            return _objdFile && _catDesc;
        }

        public override S2CIPackageInfo getInfo()
        {
            return pInfo;
        }
    }
    #endregion

    #region ObjectRecolorRecognitor
    /// <summary>
    /// Recognitor for object recolors
    /// </summary>
    public class ObjectRecolorRecognitor : BasicRecognitor
    {

        bool _matDef, _texture, _matOverride;

        private S2CObjectPackageInfo pInfo;

        public ObjectRecolorRecognitor()
        {
            _matDef = false;
            _texture = false;
            _matOverride = false;

            pInfo = new S2CObjectPackageInfo();
            pInfo.packageType = PackageTypes.ptRecolor;

        }

        public override void Feed(PackageEntry entry)
        {
            switch(entry.TypeID)
            {
                case (uint)Types.MatDef:
                    _matDef = true;
                    break;
                case (uint)Types.MatOverride:
                    _matOverride = true;
                    if(entry.file is EXMP)
                    {
                        EXMP tEXMP = entry.file as EXMP;

                        if(tEXMP.Properties.HasProperty("objectGUID"))
                        {
                            pInfo._guid = tEXMP.Properties.GetByName("objectGUID").UInt32Value;
                        }
                    }
                    break;
                case (uint)Types.Texture:
                    _texture = true;
                    break;
            }

        }

        public override bool isRecognized()
        {
            return _matDef && _matOverride && _texture;
        }

        public override S2CIPackageInfo getInfo()
        {
            return pInfo;
        }
    }
    #endregion

    #region CareerRecognitor
    /// <summary>
    /// Recognitor for careers
    /// </summary>
    public class CareerRecognitor : BasicRecognitor
    {

        bool _objd, _globals, _catDesc;

        private S2CIPackageInfo pInfo;

        public CareerRecognitor()
        {
            _objd = false;
            _globals = false;
            _catDesc = false;

            pInfo = new S2CIPackageInfo("");
            pInfo.packageType = PackageTypes.ptCareer;
        }

        public override void Feed(PackageEntry entry)
        {

            if(entry.TypeID == (uint)Types.OBJD)
            {
                if((entry.file != null) && (entry.file is nOBJD))
                {
                    nOBJD objd = entry.file as nOBJD;
                    if(objd.Data.Type == 7)
                    {
                        _objd = true;
                    }
                }
            }
            if(entry.TypeID == (uint)Types.Globals)
            {
                if((entry.file != null) && (entry.file is Global))
                {
                    Global global = entry.file as Global;
                    if(global.Data == "JobDataGlobals")
                    {
                        _globals = true;
                    }
                }
            }
            if(entry.TypeID == (uint)Types.CatDesc)
            {
                if(entry.file == null)
                {
                    STRL strl = new STRL();
                    strl.Load(entry.RawData);
                    entry.file = strl;
                }
                if((entry.file != null) && (entry.file is STRL))
                {
                    STRL strl = entry.file as STRL;
                    int n = strl.Languages.IndexOfCode(1);
                    if (n == -1 /* not found */)
                        n = 0;
                    if (strl.Languages.Count > n)
                    {
                        string sName = strl.Languages[n].Items[0].Value;
                        if ((null != sName) && ("" != sName))
                            pInfo.name = sName;
                        _catDesc = true;
                    }

                }
            }
        }

        public override bool isRecognized()
        {
            return _objd && _globals && _catDesc;
        }

        public override S2CIPackageInfo getInfo()
        {
            return pInfo;
        }
    }
    #endregion

    #region PersonRecognitor
    /// <summary>
    /// Recognitor for sims
    /// </summary>
    public class PersonRecognitor : BasicRecognitor
    {

        bool _objd;

        private S2CIPackageInfo pInfo;

        public PersonRecognitor()
        {
            _objd = false;

            pInfo = new S2CIPackageInfo("");
            pInfo.packageType = PackageTypes.ptPerson;
            // pInfo.installToTeleport = true;
        }

        public override void Feed(PackageEntry entry)
        {
            if(entry.TypeID == (uint)Types.OBJD)
            {
                if((entry.file != null) && (entry.file is nOBJD))
                {
                    nOBJD objd = entry.file as nOBJD;
                    if(objd.Data.Type == 2)
                    {
                        _objd = true;
                    }
                }

            }
        }

        public override bool isRecognized()
        {
            return _objd;
        }

        public override S2CIPackageInfo getInfo()
        {
            return pInfo;
        }
    }
    #endregion

    #region FloorWallPaintRecognitor
    /// <summary>
    /// Recognitor for floors and wall coverings
    /// </summary>
    public class FloorWallPaintRecognitor : BasicRecognitor
    {

        bool _texture, _catDesc, _matDef, _objectXML, _floorPaint;

        private S2CIPackageInfo pInfo;

        public FloorWallPaintRecognitor()
        {
            _texture    = false;
            _catDesc    = false;
            _matDef     = false;
            _objectXML  = false;
            _floorPaint = false;

            pInfo = new S2CIPackageInfo("");
        }

        public override void Feed(PackageEntry entry)
        {
            switch(entry.TypeID)
            {
                case (uint)Types.Texture:
                    _texture = true;
                    break;
                case (uint)Types.TextLists:
                    if(entry.InstanceID == 0x0000007B)
                    {
                        if(entry.file == null)
                        {
                            STRL strl = new STRL();
                            strl.Load(entry.RawData);
                            entry.file = strl;
                        }
                        if(entry.file is STRL)
                        {
                            STRL strl = entry.file as STRL;
                            int n = strl.Languages.IndexOfCode(1);
                            if (n == -1 /* not found */)
                                n = 0;
                            if (strl.Languages.Count > n)
                            {
                                Debug.Assert(strl.Languages[n].Items.Count > 0);
                                string sName = strl.Languages[n].Items[0].Value;
                                if ((null != sName) && ("" != sName))
                                    pInfo.name = sName;
                                if (strl.Languages[n].Items.Count > 1)
                                    pInfo.description = strl.Languages[n].Items[1].Value;
                                _catDesc = true;
                            }
                        }
                    }
                    break;
                case (uint)Types.MatDef:
                    _matDef = true;
                    break;
                case (uint)Types.ObjectXML:
                case (uint)Types.FloorXML:
                    if(entry.TypeID == (uint)Types.FloorXML)
                    {
                        _floorPaint = true;
                    }
                    if(entry.file is EXMP)
                    {
                        EXMP exmp = entry.file as EXMP;
                        if(exmp.Properties.HasProperty("type"))
                        {
                            _objectXML = true;
                            switch(exmp.Properties.GetByName("type").StringValue)
                            {
                                case "floor":
                                    pInfo.packageType = PackageTypes.ptFloor;
                                    break;
                                case "wall":
                                    pInfo.packageType = PackageTypes.ptWallpaper;
                                    break;
                                case "terrainPaint":
                                    pInfo.packageType = PackageTypes.ptTerrainPaint;
                                    break;
                            }
                        }
                        if(exmp.Properties.HasProperty("subsort"))
                        {
                            string subsort = exmp.Properties.GetByName("subsort").StringValue;
                            pInfo.description += "\n Catalog placement: " + subsort;
                        }
                    }


                    break;
            }

        }

        public override bool isRecognized()
        {
            return (_catDesc && _objectXML && _texture && (_matDef || _floorPaint));
        }

        public override S2CIPackageInfo getInfo()
        {
            return pInfo;
        }
    }
    #endregion

    #region CASPackageRecognitor
    /// <summary>
    /// Recognizes many types of packages
    /// </summary>
    public class CASPackageRecognitor : BasicRecognitor
    {

        bool    _binx, _texture, _matDef, _textList, _meshOverlay, _hairColor, _hairTonePS, _skinTone, _glasses,
                _bracelet, _ring, _earring, _necklace, _jewelry, _petParts,
                _beard, _eyeliner, _lipstick, _eyebrow, _eyecolor, _eyeshadow, _costumeMakeup, _stubble, _blush;
        int _propertySets, _textureOverlay;

        uint gender, age, category;

        EXMP _exmp;

        private S2CIPackageInfo pInfo;

        public CASPackageRecognitor()
        {
            _exmp = null;
            _textureOverlay = 0;
            _propertySets = 0;
            pInfo = new S2CIPackageInfo("");
        }

        public override void Feed(PackageEntry entry)
        {
            switch(entry.TypeID)
            {
                case (uint)Types.BinaryIndex:
                    _binx = true;
                    break;
                case (uint)Types.Texture:
                    _texture = true;
                    break;
                case (uint)Types.MatDef:
                    _matDef = true;
                    break;
                case (uint)Types.TextLists:
                    if(entry.InstanceID == 1)
                    {
                        _textList = true;

                        if(entry.file == null)
                        {
                            STRL strl = new STRL();
                            strl.Load(entry.RawData);
                            entry.file = strl;
                        }
                        if(entry.file is STRL)
                        {
                            STRL strl = entry.file as STRL;
                            int n = strl.Languages.IndexOfCode(1);
                            if (n == -1 /* not found */)
                                n = 0;
                            if ((strl.Languages.Count > n) && (strl.Languages[n].Items.Count > 0))
                            {
                                string sName = strl.Languages[n].Items[0].Value;
                                if ((null != sName) && ("" != sName))
                                    pInfo.name = sName;
                            }
                        }
                    }
                    break;
                case (uint)Types.HairToneXML:
                    _hairColor = true;
                    break;
                case (uint)Types.MeshOverlayXML:
                    _meshOverlay = true;
                    if(_exmp == null)
                    {
                        _exmp = new EXMP();
                        _exmp.Load(entry.RawData);

                        if (_exmp.Properties.HasProperty("name"))
                        {
                            string tName = _exmp.Properties.GetByName("name").StringValue;
                            if (tName.IndexOf("glasses") != -1)
                                _glasses = true;
                        }
                        if (_exmp.Properties.HasProperty("bin"))
                        {
                            // We have no documentation on the MeshOverlayXML,
                            // but assume that the BIN matches the TextureOverlayXML
                            switch (_exmp.Properties.GetByName("bin").UInt32Value)
                            {
                                case 0x00: //Special Skintone Overlay - Used to add some (gross) details to the zombie skin from Uni.
                                    _skinTone = true;
                                    break;
                                case 0x01: //Stubble, Collars from Pets
                                    _stubble = true;
                                    break;
                                case 0x02: //Beards/Mustaches
                                    _beard = true;
                                    break;
                                case 0x03: //Eyebrows
                                    _eyebrow = true;
                                    break;
                                case 0x05: //Face Paint - Used for certain special kinds of face paint, like the burn marks.
                                    _costumeMakeup = true;
                                    break;
                                case 0x06: //Eye Liner
                                    _eyeliner = true;
                                    break;
                                case 0x07: //Eye Shadow
                                    _eyeshadow = true;
                                    break;
                                case 0x08: //Blush
                                    _blush = true;
                                    break;
                                case 0x0A: //Lipstick
                                    _lipstick = true;
                                    break;
                                case 0x0B: //Eye Colors
                                    _eyecolor = true;
                                    break;
                                case 0x0C: //Pets Body Parts
                                case 0x0D: //Pets Body Parts
                                    _petParts = true;
                                    break;
                                case 0x0F: //Eye Glasses
                                    _glasses = true;
                                    break;
                                case 0x10: //Accessory - Used only on a hidden nose ring accessory.
                                    _jewelry = true;
                                    break;
                                case 0x13: //Teeth - Used for the vampire teeth from NL as well as werewolf teeth from Pets.
                                    break;
                                case 0x14: //Face Paint - Used for the majority of Maxis face paints.
                                    _costumeMakeup = true;
                                    break;
                                case 0x1E: //Zits - Used only for Maxis zits.
                                    break;
                                case 0x23: //Special Skintone Overlay - Used for the pale vampire face from NL.
                                case 0x25: //Special Skintone Overlay - Used for the Big Foot from BV.
                                case 0x28: //Special Skintone Overlay - Used for the werewolf face from Pets.
                                    _skinTone = true;
                                    break;
                                case 0x32: //Left Earring
                                case 0x33: //Right Earring
                                    _earring = true;
                                    break;
                                case 0x34: //Necklace
                                    _necklace = true;
                                    break;
                                case 0x35: //Left Bracelet
                                case 0x36: //Right Bracelet
                                    _bracelet = true;
                                    break;
                                case 0x37: //Nose Ring
                                case 0x38: //Lip Ring
                                case 0x39: //Eyebrow Ring
                                    _jewelry = true;
                                    break;
                                case 0x3A: //Left Index Finger Ring
                                case 0x3B: //Another Right Index Finger Ring of Some Type (Plumeria)
                                case 0x3C: //Right Index Finger Ring (Stone)
                                case 0x3D: //Left Pinky Ring
                                case 0x3E: //Right Pinky Ring
                                case 0x3F: //Left Thumb Ring
                                case 0x40: //Right Thumb Ring
                                    _ring = true;
                                    break;
                                case 0x46: //Special Skintone Overlay - Tans/Burns from BV/Seasons 
                                    _skinTone = true;
                                    break;
                            }
                        }
                    }
                    break;
                case (uint)Types.TextureOverlayXML:
                    if(_exmp == null)
                    {
                        _exmp = new EXMP();
                        _exmp.Load(entry.RawData);

                        // ToDo: Why are we checking the layer?  This seems like very odd logic.
                        switch(_exmp.Properties.GetByName("layer").UInt32Value)
                        {
                            case 0x0:
                                if (_exmp.Properties.GetByName("subtype").UInt32Value == 2)
                                    _lipstick = true;
                                else
                                    _eyecolor = true;
                                break;
                            case 0x14:
                                _blush = true;
                                break;
                            case 0x1E:
                                _eyeshadow = true;
                                break;
                            case 0x28:
                                _eyeliner = true;
                                break;
                            case 0x32:
                                _costumeMakeup = true;
                                break;
                            case 0x3C:
                                _stubble = true;
                                break;
                            case 0x46:
                                _beard = true;
                                break;
                            case 0x50:
                                _eyebrow = true;
                                break;
                        }
                        switch (_exmp.Properties.GetByName("Subtype").UInt32Value)
                        {
                            case 0x00: //Facial Hair (Stubble and mustaches/beards), Pets Body Parts
                                // Not specific enough; try something else
                                break;
                            case 0x01: //Eyebrows
                                _eyebrow = true;
                                break;
                            case 0x02: //Lipstick
                                _lipstick = true;
                                break;
                            case 0x03: //Eye Colors
                                _eyecolor = true;
                                break;
                            case 0x04: //Face Paint
                                _costumeMakeup = true;
                                break;
                            case 0x05: //Accessory (Pet collars, glasses, other custom accessories)
                                _jewelry = true;
                                break;
                            case 0x06: //Blush
                                _blush = true;
                                break;
                            case 0x07: //Eyeshadow, Eyeliner
                                // Not specific enough; try something else
                                break;
                            case 0x08: //Accessory (All Maxis Jewelery added with Bon Voyage)
                                _jewelry = true;
                                break;
                            case 0x0A: //Tails, Faces - Found in cat related resources for the Pet's coat (known because of an entry about coats that subtype 0x0C does not have and the abbreviation c. Example resource name: fufacefl_cbeige)
                            case 0x0B: //Faces, Tails - Found in cat related resources that controls fur texture(?) (Example resource names: fufaceaccflowingfurcards_mflowing, dubodyaccfurcards_mflowing)
                            case 0x0C: //Tails, Faces - Found in cat and dog related resources for something abbreviated with the letter m. (Example resource name: fubodysm_munderbelly) 
                                _petParts = true;
                                break;
                        }
                        switch (_exmp.Properties.GetByName("bin").UInt32Value)
                        {
                            case 0x00: //Special Skintone Overlay - Used to add some (gross) details to the zombie skin from Uni.
                                _skinTone = true;
                                break;
                            case 0x01: //Stubble, Collars from Pets
                                _stubble = true;
                                break;
                            case 0x02: //Beards/Mustaches
                                _beard = true;
                                break;
                            case 0x03: //Eyebrows
                                _eyebrow = true;
                                break;
                            case 0x05: //Face Paint - Used for certain special kinds of face paint, like the burn marks.
                                _costumeMakeup = true;
                                break;
                            case 0x06: //Eye Liner
                                _eyeliner = true;
                                break;
                            case 0x07: //Eye Shadow
                                _eyeshadow = true;
                                break;
                            case 0x08: //Blush
                                _blush = true;
                                break;
                            case 0x0A: //Lipstick
                                _lipstick = true;
                                break;
                            case 0x0B: //Eye Colors
                                _eyecolor = true;
                                break;
                            case 0x0C: //Pets Body Parts
                            case 0x0D: //Pets Body Parts
                                _petParts = true;
                                break;
                            case 0x0F: //Eye Glasses
                                _glasses = true;
                                break;
                            case 0x10: //Accessory - Used only on a hidden nose ring accessory.
                                _jewelry = true;
                                break;
                            case 0x13: //Teeth - Used for the vampire teeth from NL as well as werewolf teeth from Pets.
                                break;
                            case 0x14: //Face Paint - Used for the majority of Maxis face paints.
                                _costumeMakeup = true;
                                break;
                            case 0x1E: //Zits - Used only for Maxis zits.
                                break;
                            case 0x23: //Special Skintone Overlay - Used for the pale vampire face from NL.
                            case 0x25: //Special Skintone Overlay - Used for the Big Foot from BV.
                            case 0x28: //Special Skintone Overlay - Used for the werewolf face from Pets.
                                _skinTone = true;
                                break;
                            case 0x32: //Left Earring
                            case 0x33: //Right Earring
                                _earring = true;
                                break;
                            case 0x34: //Necklace
                                _necklace = true;
                                break;
                            case 0x35: //Left Bracelet
                            case 0x36: //Right Bracelet
                                _bracelet = true;
                                break;
                            case 0x37: //Nose Ring
                            case 0x38: //Lip Ring
                            case 0x39: //Eyebrow Ring
                                _jewelry = true;
                                break;
                            case 0x3A: //Left Index Finger Ring
                            case 0x3B: //Another Right Index Finger Ring of Some Type (Plumeria)
                            case 0x3C: //Right Index Finger Ring (Stone)
                            case 0x3D: //Left Pinky Ring
                            case 0x3E: //Right Pinky Ring
                            case 0x3F: //Left Thumb Ring
                            case 0x40: //Right Thumb Ring
                                _ring = true;
                                break;
                            case 0x46: //Special Skintone Overlay - Tans/Burns from BV/Seasons 
                                _skinTone = true;
                                break;
                        }
                    }
                    _textureOverlay++;
                    break;
                case (uint)Types.PropertySet:
                    _propertySets++;
                    // Clothing info
                    _exmp = new EXMP();
                    _exmp.Load(entry.RawData);

                    if (_exmp.Properties.HasProperty("gender"))
                        gender |= _exmp.Properties.GetByName("gender").UInt32Value;
                    if (_exmp.Properties.HasProperty("age"))
                        age |= _exmp.Properties.GetByName("age").UInt32Value;
                    if (_exmp.Properties.HasProperty("category"))
                        category |= _exmp.Properties.GetByName("category").UInt32Value;
                    if (_exmp.Properties.HasProperty("hairtone"))
                    {
                        string sHair = _exmp.Properties.GetByName("hairtone").StringValue;
                        if ("00000000-0000-0000-0000-000000000000" != sHair)
                            _hairTonePS = true;
                    }
                    break;
                case (uint)Types.SkinToneXML:
                    _skinTone = true;
                    break;
            }
        }

        private string GenderString( uint uGender)
        {
            string sGender = "";

            if ((uGender & 0x1) == 0x1)
                sGender += " Female";
            if ((uGender & 0x2) == 0x2)
                sGender += ((sGender.Length > 1) ? "," : "") + " Male";
            return sGender;
        }

        private string AgeString( uint uAge)
        {
            string sAge = "";

            if ((uAge & 0x00000020) == 0x00000020)
                sAge += " Baby";
            if ((uAge & 0x00000001) == 0x00000001)
                sAge += ((sAge.Length > 1) ? "," : "") + " Toddler";
            if ((uAge & 0x00000002) == 0x00000002)
                sAge += ((sAge.Length > 1) ? "," : "") + " Child";
            if ((uAge & 0x00000004) == 0x00000004)
                sAge += ((sAge.Length > 1) ? "," : "") + " Teen";
            if ((uAge & 0x00000040) == 0x00000040)
                sAge += ((sAge.Length > 1) ? "," : "") + " Young Adult";
            if ((uAge & 0x00000008) == 0x00000008)
                sAge += ((sAge.Length > 1) ? "," : "") + " Adult";
            if ((uAge & 0x00000010) == 0x00000010)
                sAge += ((sAge.Length > 1) ? "," : "") + " Elder";
            return sAge;
        }

        private string CategoryString(uint uCategory)
        {
            string sCategory = "";

            if ((uCategory & 0x7) == 0x7)
                sCategory += " Everyday";
            if ((uCategory & 0x8) == 0x8)
                sCategory += ((sCategory.Length > 1) ? "," : "") + " Swimwear";
            if ((uCategory & 0x10) == 0x10)
                sCategory += ((sCategory.Length > 1) ? "," : "") + " PJs";
            if ((uCategory & 0x20) == 0x20)
                sCategory += ((sCategory.Length > 1) ? "," : "") + " Formal";
            if ((uCategory & 0x40) == 0x40)
                sCategory += ((sCategory.Length > 1) ? "," : "") + " Undies";
            /* if ((uCategory & 0x80) == 0x80)
                sCategory += ((sCategory.Length > 1) ? "," : "") + " Skintone"; */
            if ((uCategory & 0x100) == 0x100)
                sCategory += ((sCategory.Length > 1) ? "," : "") + " Pregnant";
            if ((uCategory & 0x200) == 0x200)
                sCategory += ((sCategory.Length > 1) ? "," : "") + " Athletic";
            /* if ((uCategory & 0x400) == 0x400)
                sCategory += ((sCategory.Length > 1) ? "," : "") + " Try On"; */
            /* if ((uCategory & 0x800) == 0x800)
                sCategory += ((sCategory.Length > 1) ? "," : "") + " Naked Overlay"; */
            if ((uCategory & 0x1000) == 0x1000)
                sCategory += ((sCategory.Length > 1) ? "," : "") + " Outerwear";
            return sCategory;
        }

        public override bool isRecognized()
        {
            string sPropertyDescription =
                "Gender:" + GenderString(gender) + "\n" +
                "Categories:" + CategoryString(category) + "\n" +
                "Age:" + AgeString(age);

            pInfo.missingBinaryIndex = !_binx;

            if (_beard)
            {
                pInfo.packageType = PackageTypes.ptBeard;
                return true;
            }
            if (_eyeliner)
            {
                pInfo.packageType = PackageTypes.ptEyeliner;
                return true;
            }
            if (_lipstick)
            {
                pInfo.packageType = PackageTypes.ptLipstick;
                return true;
            }
            if (_eyebrow)
            {
                pInfo.packageType = PackageTypes.ptEyebrow;
                return true;
            }
            if (_eyeshadow)
            {
                pInfo.packageType = PackageTypes.ptEyeShadow;
                return true;
            }
            if (_costumeMakeup)
            {
                pInfo.packageType = PackageTypes.ptCostumeMakeup;
                return true;
            }
            if (_stubble)
            {
                pInfo.packageType = PackageTypes.ptStubble;
                return true;
            }
            if (_blush)
            {
                pInfo.packageType = PackageTypes.ptBlush;
                return true;
            }
            if (_petParts)
            {
                pInfo.packageType = PackageTypes.ptPetBodyParts;
                return true;
            }
            if (_bracelet)
            {
                pInfo.packageType = PackageTypes.ptBracelet;
                return true;
            }
            if (_ring)
            {
                pInfo.packageType = PackageTypes.ptRing;
                return true;
            }
            if (_earring)
            {
                pInfo.packageType = PackageTypes.ptEarring;
                return true;
            }
            if (_necklace)
            {
                pInfo.packageType = PackageTypes.ptNecklace;
                return true;
            }
            // Hair color
            if (_hairColor /* && _texture */ && _matDef && (_propertySets > 0))
            {
                // Don't need texture, since may be linking to Maxis texture
                pInfo.packageType = PackageTypes.ptHairColor;
                return true;
            }
            // Skin Tone
            if (_skinTone && _texture && _matDef && (_propertySets > 1))
            {
                pInfo.packageType = PackageTypes.ptSkinTone;
                return true;
            }
            // Glasses
            if (_glasses /* && _texture */ && _matDef && _meshOverlay && (_propertySets == 0))
            {
                // Don't need texture, since may be linking to Maxis texture
                pInfo.packageType = PackageTypes.ptGlasses;
                return true;
            }
            if (_jewelry)
            {
                pInfo.packageType = PackageTypes.ptJewelry;
                return true;
            }
            // Eye color
            if (_eyecolor && _texture && _matDef && (_textureOverlay == 1) && (_propertySets == 0))
            {
                pInfo.packageType = PackageTypes.ptEyeColor;
                return true;
            }
            // Makeup
            if (_texture && _matDef && _textList && (_textureOverlay > 1) && (_propertySets == 0))
            {
                pInfo.packageType = PackageTypes.ptMakeup;
                return true;
            }
            // Clothing
            if (/*_texture && */ _matDef && (_propertySets > 0))
            {
                // Don't need texture, since may be linking to Maxis texture
                pInfo.description = "Clothing\n" + sPropertyDescription;
                pInfo.packageType = PackageTypes.ptClothing;
                return true;
            }
            // Potential Category Mod
            if (_propertySets > 0)
            {
                pInfo.missingBinaryIndex = false;
                pInfo.description = "Category Mod";
                // ToDo: are there other types of category mods that we can identify?
                if (_hairTonePS)
                    pInfo.description += ": Hair Color";
                pInfo.description += "\n" + sPropertyDescription;
                pInfo.packageType = PackageTypes.ptCategoryMod;

                // This is not a sufficient test, so return false.
                // Later tests can determine whether this is actually a category mod and ask for the pInfo.
                return false;
            }
            return false;
        }

        public override S2CIPackageInfo getInfo()
        {
            return pInfo;
        }
    }
    #endregion

    #region FacialStructureRecognitor
    /// <summary>
    /// Recognitor for facial structures
    /// </summary>
    public class FacialStructureRecognitor : BasicRecognitor
    {

        bool _facialStructure, _objd;

        private S2CIPackageInfo pInfo;

        public FacialStructureRecognitor()
        {
            _facialStructure = false;
            _objd = false;
            pInfo = new S2CIPackageInfo("");
            pInfo.packageType = PackageTypes.ptFacePreset;
            pInfo.description = "Faces of sims";
            // pInfo.installToTeleport = true;
        }

        public override void Feed(PackageEntry entry)
        {
            if(entry.file is nOBJD)
            {
                nOBJD tObjd = entry.file as nOBJD;
                if(tObjd.Data.Type == 2)
                {
                    _objd = true;
                }
            }
            if(entry.TypeID == (uint)Types.FacialStructure)
            {
                _facialStructure = true;
            }
        }

        public override bool isRecognized()
        {

            return _facialStructure && !_objd;
        }

        public override S2CIPackageInfo getInfo()
        {
            return pInfo;
        }
    }
    #endregion

    #region CharacterSegmentRecognitor
    /// <summary>
    /// Recognitor for characters
    /// </summary>
    public class CharacterSegmentRecognitor : BasicRecognitor
    {
        bool _objd, _ageData, _catDesc, _sprite, _cat, _dog;

        EXMP _exmp;

        private S2CIPackageInfo pInfo;

        public CharacterSegmentRecognitor()
        {
            _objd       = false;
            _ageData    = false;
            _catDesc    = false;
            _sprite     = false;
            _cat        = false;
            _dog        = false;
            pInfo = new S2CIPackageInfo("");
            pInfo.packageType = PackageTypes.ptPackagedSim;
            pInfo.installToTeleport = true;

        }

        public override void Feed(PackageEntry entry)
        {
            if(entry.file is nOBJD)
            {
                nOBJD tObjd = entry.file as nOBJD;
                if(tObjd.Data.Type == 2)
                {
                    _objd = true;
                }
                if(tObjd.Data.Type == 0x0011)
                {
                    _sprite = true;
                }
            }
            if(entry.TypeID == (uint)Types.AgeData)
            {
                _ageData = true;
                _exmp = new EXMP();
                _exmp.Load(entry.RawData);
                if (_exmp.Properties.HasProperty("species"))
                {
                    uint species = _exmp.Properties.GetByName("species").UInt32Value;
                    switch (species)
                    {
                        case 1:     // human
                            break;
                        case 4:     // dog
                        case 6:     // dog
                            _dog = true;
                            break;
                        case 8:     // cat
                            _cat = true;
                            break;
                    }
                }

            }
            if(entry.TypeID == (uint)Types.CatDesc)
            {
                if(entry.file == null)
                {
                    STRL strl = new STRL();
                    strl.Load(entry.RawData);
                    entry.file = strl;
                }
                if(entry.file is STRL)
                {
                    STRL strl = entry.file as STRL;
                    int n = strl.Languages.IndexOfCode(1);
                    if (n == -1 /* not found */)
                        n = 0;
                    if (strl.Languages.Count > n)
                    {
                        Debug.Assert(strl.Languages[n].Items.Count > 0);
                        string sName = strl.Languages[n].Items[0].Value;
                        if ((null != sName) && ("" != sName))
                            pInfo.name = sName;
                        if (strl.Languages[n].Items.Count > 1)
                            pInfo.description = strl.Languages[n].Items[1].Value;
                        if (strl.Languages[n].Items.Count > 2)
                            pInfo.name += " " + strl.Languages[n].Items[2].Value;
                        _catDesc = true;
                    }
                }
            }
        }

        public override bool isRecognized()
        {
            if(_objd && _ageData && _catDesc)
                return true;

            if (_objd && _catDesc)
            {
                pInfo.packageType = PackageTypes.ptPackagedSimData;
                return true;
            }

            if(_sprite && _catDesc)
            {
                pInfo.packageType = PackageTypes.ptPackagedGhost;
                return true;
            }

            if (_cat)
            {
                pInfo.packageType = PackageTypes.ptCat;
                return true;
            }
            if (_dog)
            {
                pInfo.packageType = PackageTypes.ptDog;
                return true;
            }

            return false;
        }

        public override S2CIPackageInfo getInfo()
        {
            return pInfo;
        }
    }
    #endregion

    #region FamilySegmentRecognitor
    /// <summary>
    /// Recognitor for families
    /// </summary>
    public class FamilySegmentRecognitor : BasicRecognitor
    {

        private S2CIPackageInfo pInfo;

        bool _family;
        uint uInst = 0xFFFFFFFF;

        public FamilySegmentRecognitor()
        {
            _family = false;
            pInfo = new S2CIPackageInfo("");
            pInfo.packageType = PackageTypes.ptPackagedFamily;
            pInfo.installToTeleport = true;
        }

        public override void Feed(PackageEntry entry)
        {
            if(entry.TypeID == (uint)Types.FamilyInformation)
            {
                _family = true;
                uInst = entry.InstanceID;   // Family and STRL instance numbers will match
            }
            if(entry.TypeID == (uint)Types.TextLists)
            {
                if(entry.file == null)
                {
                    STRL strl = new STRL();
                    strl.Load(entry.RawData);
                    entry.file = strl;
                }
                if(entry.file is STRL)
                {
                    bool bSTRLValid = false;
                    if (0xFFFFFFFF == uInst)
                    {
                        // If we don't know the correct family instance number,
                        // just check whether it's likely to be the correct one
                        if (entry.InstanceID <= 0x00000008)
                            bSTRLValid = true;
                    }
                    else
                    {
                        // If we know the correct family instance number,
                        // check whether the STRL instance number matches.
                        if (entry.InstanceID == uInst)
                            bSTRLValid = true;
                    }
                    if (bSTRLValid)
                    {
                        STRL strl = entry.file as STRL;
                        int n = strl.Languages.IndexOfCode(1);
                        if (n == -1 /* not found */)
                            n = 0;
                        if (strl.Languages.Count > n)
                        {
                            Debug.Assert(strl.Languages[n].Items.Count > 0);
                            string sName = strl.Languages[n].Items[0].Value;
                            if ((null != sName) && ("" != sName))
                                pInfo.name = sName;
                            if (strl.Languages[n].Items.Count > 1)
                                pInfo.description = strl.Languages[n].Items[1].Value;
                        }
                    }
                }
            }
        }

        public override bool isRecognized()
        {
            return _family;
        }

        public override S2CIPackageInfo getInfo()
        {
            return pInfo;
        }
    }
    #endregion

    #region LotSegmentRecognitor
    /// <summary>
    /// Recognitor for lots
    /// </summary>
    public class LotSegmentRecognitor : BasicRecognitor
    {


        bool _lotInfo;

        private S2CIPackageInfo pInfo;

        public LotSegmentRecognitor()
        {
            _lotInfo = false;
            pInfo = new S2CIPackageInfo("");
            pInfo.packageType = PackageTypes.ptPackagedLot;
            pInfo.installToTeleport = true;
        }

        public override void Feed(PackageEntry entry)
        {
            if(entry.TypeID == (uint)Types.HouseDescriptor)
            {
                _lotInfo = true;
            }
            else if (entry.TypeID == (uint)Types.VersionInfo)
            {
                VersionRecognizer v = new VersionRecognizer();
                pInfo.version = v.VersionString(entry);
            }
            else if (entry.TypeID == (uint)Types.TextLists)
            {
                if(entry.InstanceID == 0x00000A46)
                {
                    if(entry.file == null)
                    {
                        STRL strl = new STRL();
                        strl.Load(entry.RawData);
                        entry.file = strl;
                    }
                    if(entry.file is STRL)
                    {
                        STRL strl = entry.file as STRL;
                        int n = strl.Languages.IndexOfCode(1);
                        if (n == -1 /* not found */)
                            n = 0;
                        if (strl.Languages.Count > n)
                        {
                            Debug.Assert(strl.Languages[n].Items.Count > 0);
                            string sName = strl.Languages[n].Items[0].Value;
                            if ((null != sName) && ("" != sName))
                                pInfo.name = sName;
                            if (strl.Languages[n].Items.Count > 1)
                                pInfo.description = strl.Languages[n].Items[1].Value;
                        }
                    }
                }
            }


        }

        public override bool isRecognized()
        {
            return _lotInfo;
        }

        public override S2CIPackageInfo getInfo()
        {
            return pInfo;
        }
    }
    #endregion

    #region OccupiedLotRecognitor
    /// <summary>
    /// Recognitor for occupied lots (lot with family)
    /// </summary>
    public class OccupiedLotRecognitor : BasicRecognitor
    {

        bool _lotInfo, _family;
        uint uInst = 0xFFFFFFFF;

        private S2CIPackageInfo pInfo;

        public OccupiedLotRecognitor()
        {
            _lotInfo = false;
            _family = false;
            pInfo = new S2CIPackageInfo("");
            pInfo.packageType = PackageTypes.ptOccupiedLot;
            pInfo.installToTeleport = true;
        }

        public override void Feed(PackageEntry entry)
        {
            if (entry.TypeID == (uint)Types.HouseDescriptor)
            {
                _lotInfo = true;
            }
            if (entry.TypeID == (uint)Types.FamilyInformation)
            {
                _family = true;
                uInst = entry.InstanceID;   // Family and STRL instance numbers will match
            }
            else if (entry.TypeID == (uint)Types.VersionInfo)
            {
                VersionRecognizer v = new VersionRecognizer();
                pInfo.version = v.VersionString(entry);
            }
            else if (entry.TypeID == (uint)Types.TextLists)
            {
                if (entry.file == null)
                {
                    STRL strl = new STRL();
                    strl.Load(entry.RawData);
                    entry.file = strl;
                }
                if (entry.file is STRL)
                {
                    bool bSTRLValid = false;
                    if (entry.InstanceID == 0x00000A46)
                        bSTRLValid = true;
                    else if (0xFFFFFFFF == uInst)
                    {
                        // If we don't know the correct family instance number,
                        // just check whether it's likely to be the correct one
                        if (entry.InstanceID <= 0x00000008)
                            bSTRLValid = true;
                    }
                    else
                    {
                        // If we know the correct family instance number,
                        // check whether the STRL instance number matches.
                        if (entry.InstanceID == uInst)
                            bSTRLValid = true;
                    }
                    if (bSTRLValid)
                    {
                        STRL strl = entry.file as STRL;
                        int n = strl.Languages.IndexOfCode(1);
                        if (n == -1 /* not found */)
                            n = 0;
                        if (strl.Languages.Count > n)
                        {
                            Debug.Assert(strl.Languages[n].Items.Count > 0);
                            string sName = strl.Languages[n].Items[0].Value;
                            if ((null != sName) && ("" != sName))
                                pInfo.name = sName;
                            if (strl.Languages[n].Items.Count > 1)
                                pInfo.description = strl.Languages[n].Items[1].Value;
                        }
                    }
                }
            }
        }

        public override bool isRecognized()
        {
            return _lotInfo && _family;
        }

        public override S2CIPackageInfo getInfo()
        {
            return pInfo;
        }
    }
    #endregion

    #region FenceRecognitor
    /// <summary>
    /// Recognitor for fences and half-walls
    /// </summary>
    public class FenceRecognitor : BasicRecognitor
    {
        bool _fence;

        private S2CIPackageInfo pInfo;

        public FenceRecognitor()
        {
            _fence = false;

            pInfo = new S2CObjectPackageInfo();
            pInfo.packageType = PackageTypes.ptFence;
        }

        public override void Feed(PackageEntry entry)
        {
            switch (entry.TypeID)
            {
                case (uint)Types.FenceXML:
                    _fence = true;
                    break;
                case (uint)Types.TextLists:
                    if (entry.InstanceID == 0x0000007B)
                    {
                        if (entry.file == null)
                        {
                            STRL strl = new STRL();
                            strl.Load(entry.RawData);
                            entry.file = strl;
                        }
                        if (entry.file is STRL)
                        {
                            STRL strl = entry.file as STRL;
                            int n = strl.Languages.IndexOfCode(1);
                            if (n == -1 /* not found */)
                                n = 0;
                            if (strl.Languages.Count > n)
                            {
                                Debug.Assert(strl.Languages[n].Items.Count > 0);
                                string sName = strl.Languages[n].Items[0].Value;
                                if ((null != sName) && ("" != sName))
                                    pInfo.name = sName;
                                if (strl.Languages[n].Items.Count > 1)
                                    pInfo.description = strl.Languages[n].Items[1].Value;
                            }
                        }
                    }
                    break;
            }
        }

        public override bool isRecognized()
        {
            return _fence;
        }

        public override S2CIPackageInfo getInfo()
        {
            return pInfo;
        }
    }
    #endregion

    #region RoofRecognitor
    /// <summary>
    /// Recognitor for roofs
    /// </summary>
    public class RoofRecognitor : BasicRecognitor
    {
        bool _roof;

        private S2CIPackageInfo pInfo;

        public RoofRecognitor()
        {
            _roof = false;

            pInfo = new S2CObjectPackageInfo();
            pInfo.packageType = PackageTypes.ptRoof;
        }

        public override void Feed(PackageEntry entry)
        {
            switch (entry.TypeID)
            {
                case (uint)Types.RoofXML:
                    _roof = true;
                    break;
                case (uint)Types.TextLists:
                    if(entry.InstanceID == 0x0000007B)
                    {
                        if(entry.file == null)
                        {
                            STRL strl = new STRL();
                            strl.Load(entry.RawData);
                            entry.file = strl;
                        }
                        if(entry.file is STRL)
                        {
                            STRL strl = entry.file as STRL;
                            int n = strl.Languages.IndexOfCode(1);
                            if (n == -1 /* not found */)
                                n = 0;
                            if (strl.Languages.Count > n)
                            {
                                Debug.Assert(strl.Languages[n].Items.Count > 0);
                                string sName = strl.Languages[n].Items[0].Value;
                                if ((null != sName) && ("" != sName))
                                    pInfo.name = sName;
                                if (strl.Languages[n].Items.Count > 1)
                                    pInfo.description = strl.Languages[n].Items[1].Value;
                            }
                        }
                    }
                    break;
            }
        }

        public override bool isRecognized()
        {
            return _roof;
        }

        public override S2CIPackageInfo getInfo()
        {
            return pInfo;
        }
    }
    #endregion

    #region MeshRecognitor
    /// <summary>
    /// Recognitor for meshes
    /// </summary>
    public class MeshRecognitor : BasicRecognitor
    {
        bool _geometricData, _geometricNode, _resourceNode, _shape;

        private S2CIPackageInfo pInfo;

        public MeshRecognitor()
        {
            _geometricData = false;
            _geometricNode = false;
            _resourceNode = false;
            _shape = false;

            pInfo = new S2CIPackageInfo("");
            pInfo.packageType = PackageTypes.ptMesh;
        }

        public override void Feed(PackageEntry entry)
        {
            switch (entry.TypeID)
            {
                case (uint)Types.GeometricData:
                    _geometricData = true;
                    break;
                case (uint)Types.GeometricNode:
                    _geometricNode = true;
                    break;
                case (uint)Types.ResourceNode:
                    _resourceNode = true;
                    break;
                case (uint)Types.Shape:
                    _shape = true;
                    break;
            }
        }

        public override bool isRecognized()
        {
            // ToDo: is there anything else which uniquely identifies a mesh?
            if (_geometricData && _geometricNode && _resourceNode && _shape)
                return true;
            return false;
        }

        public override S2CIPackageInfo getInfo()
        {
            return pInfo;
        }
    }
    #endregion

    #region NewRecognitor
    /// <summary>
    /// Class for new recognitor
    /// </summary>
    public class NewRecognitor : BasicRecognitor
    {


        private S2CIPackageInfo pInfo;

        public NewRecognitor()
        {
            pInfo = new S2CIPackageInfo("");
        }

        public override void Feed(PackageEntry entry)
        {
        }

        public override bool isRecognized()
        {
            return false;
        }

        public override S2CIPackageInfo getInfo()
        {
            return pInfo;
        }
    }
    #endregion

    #region VersionRecognitor
    public class VersionRecognizer
    {
        private string[] sVersionStrings = 
        {
            /*  0 */ "The Sims 2",
            /*  1 */ "The Sims 2 University",
            /*  2 */ "The Sims 2 Nightlife",
            /*  3 */ "The Sims 2 Open For Business",
            /*  4 */ "The Sims 2 Family Fun Stuff",
            /*  5 */ "The Sims 2 Glamour Life Stuff",
            /*  6 */ "The Sims 2 Pets",
            /*  7 */ "The Sims 2 Seasons",
            /*  8 */ "The Sims 2 Celebration! Stuff",
            /*  9 */ "The Sims™ 2 H&M® Fashion Stuff",
            /* 10 */ "The Sims™ 2 Bon Voyage",
            /* 11 */ "The Sims™ 2 Teen Style Stuff",
            /* 12 */ "The Sims™ 2 Store",
            /* 13 */ "The Sims™ 2 FreeTime",
            /* 14 */ "The Sims 2 Kitchen and Bath Stuff",
            /* 15 */ "The Sims 2 Ikea Stuff",
            /* 16 */ "The Sims™ 2 Apartment Life",
            /* 17 */ "The Sims™ 2 Mansion and Garden Stuff"
        };

        public VersionRecognizer()
        {
        }

        public string VersionString(PackageEntry entry)
        {
            string sVersionString = "";

            EXMP exmp = new EXMP();
            exmp.Load(entry.RawData);
            if (exmp.Properties.HasProperty("1"))
            {
                string sVersionNumber = exmp.Properties.GetByName("1").StringValue;
                // Format 1 has format "1.<expansion pack number>.<other version information>"
                char[] delimiter = new char[] {'.'};
                string[] sNumber = sVersionNumber.Split(delimiter);
                if (sNumber[0] == "1")
                {
                    try
                    {
                        uint uVersion = System.Convert.ToUInt32(sNumber[1]);
                        sVersionString = sVersionStrings[uVersion];
                    } 
                    catch
                    {
                        sVersionString = sVersionNumber;
                    }
                }
                else
                    sVersionString = sVersionNumber;
            }
            else if (exmp.Properties.HasProperty("0"))
                sVersionString = exmp.Properties.GetByName("0").StringValue;

            return sVersionString;
        }
    }
    #endregion


}