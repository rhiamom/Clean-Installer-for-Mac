#nullable disable
/***************************************************************************
 *   Copyright (C) 2006 by Andi8104                                        *
 *   Andi8104@arcor.de                                                     *
 *                                                                         *
 *   Additional programming:                                               *
 *   Copyright (C) 2007-2013 by Mootilda                                   *
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
using System.IO;
using SimPe.Interfaces.Files;


namespace Sims2Pack_Installer
{
    public class R_LOT
    {
        private IPackedFileDescriptor PFD;
        private byte[] Data;
        private const int iLeadingZeros = 64;
        private int iWidthIndex = 66;
        private int iWidth = 0;
        private int iHeightIndex = 70;
        private int iHeight = 0;
        private byte bType = 0xFF;
        private int iU10Index = 75;
        private int iU0Index = 77;
        private byte bU10 = 0xFF;
        private byte bU11 = 0xFF;
        private uint uU0 = 0xFFFFFFFF;
        private string sLotName = null;
        private string sLotDesc = null;
        private const int iLotTilesPerNeighborhoodTile = 10;

        public R_LOT(IPackageFile LotPackage, IPackedFileDescriptor Descriptor)
        {
            PFD = Descriptor;
            IPackedFile PF = LotPackage.Read(PFD);
            Data = PF.UncompressedData;

            MemoryStream M = new MemoryStream(Data);
            BinaryReader BR = new BinaryReader(M);
            for (int i = 0; i < iLeadingZeros; i++)
            {
                byte bDummy = BR.ReadByte();
                Debug.Assert(bDummy == 0);
            }

            // ToDo: Is this a version number?
            ushort uVersion = BR.ReadUInt16();
            Debug.Assert((uVersion == 6)    // ToDo: Determine whether other values are known and handled correctly
                      || (uVersion == 7)    // Bon Voyage
                      || (uVersion == 8)    // Free Time
                      || (uVersion == 11)   // Apartment Life
            );
            int iIndex = iLeadingZeros + 2 /* uDumm */;

            Debug.Assert(iWidthIndex == iIndex);
            iWidthIndex = iIndex;
            iWidth = BR.ReadInt32();
            iIndex += 4;

            Debug.Assert(iHeightIndex == iIndex);
            iHeightIndex = iIndex;
            iHeight = BR.ReadInt32();
            iIndex += 4;

            bType = BR.ReadByte();
            Debug.Assert((bType == 0)   // Residential
                      || (bType == 1)   // Community
                      || (bType == 2)   // University: Dorm
                      || (bType == 3)   // University: Greek House
                      || (bType == 4)   // University: Secret Society
                      || (bType == 5)   // Bon Voyage: Hotel
                      || (bType == 6)   // Bon Voyage: Hidden Vacation Lot
                      || (bType == 7)   // FreeTime: Hidden Hobby Lot
                      || (bType == 8)   // Apartment Life: Apartment Building
                      || (bType == 9)   // Apartment Life: Occupied Apartment
                      || (bType == 10)  // Apartment Life: Hidden Lot (Witches)
            );
            iIndex++;

            Debug.Assert(iU10Index == iIndex);
            iU10Index = iIndex;
            bU10 = BR.ReadByte();
            Debug.Assert(bU10 < 0x10);
            iIndex++;

            bU11 = BR.ReadByte();
            Debug.Assert(bU11 < 4);
            iIndex++;

            Debug.Assert(iU0Index == iIndex);
            iU0Index = iIndex;
            uU0 = BR.ReadUInt32();
            iIndex += 4;

            sLotName = BR.ReadString();
            iIndex += sLotName.Length + 1;

            sLotDesc = BR.ReadString();
            iIndex += sLotDesc.Length + 1;
        }

        private void ReplaceUInt(uint u, int iIndex)
        {
            byte[] BA = new byte[4];
            BinaryWriter BW = new BinaryWriter(new MemoryStream(BA));
            BW.Write(u);
            Array.Copy(BA, 0, Data, iIndex, 4);
            PFD.SetUserData(Data, true);
        }

        private void ReplaceInt(int i, int iIndex)
        {
            byte[] BA = new byte[4];
            BinaryWriter BW = new BinaryWriter(new MemoryStream(BA));
            BW.Write(i);
            Array.Copy(BA, 0, Data, iIndex, 4);
            PFD.SetUserData(Data, true);
        }

        private void ReplaceWidth(int i)
        {
            iWidth = i;
            ReplaceInt(iWidth, iWidthIndex);
        }

        private void ReplaceHeight(int i)
        {
            iHeight = i;
            ReplaceInt(iHeight, iHeightIndex);
        }

        public int Width
        {
            get
            {
                return iWidth * iLotTilesPerNeighborhoodTile;
            }
            set
            {
                if (this.Width != value)
                {
                    ReplaceWidth(value / iLotTilesPerNeighborhoodTile);
                }
            }
        }

        public int Height
        {
            get
            {
                return iHeight * iLotTilesPerNeighborhoodTile;
            }
            set
            {
                if (this.Height != value)
                {
                    ReplaceHeight(value / iLotTilesPerNeighborhoodTile);
                }
            }
        }

        public byte LotType
        {
            get
            {
                return bType;
            }
        }

        public bool Occupied
        {
            get
            {
                return false;
            }
        }

        public uint U0
        {
            get
            {
                return uU0;
            }
            set
            {
                if (uU0 != value)
                {
                    uU0 = value;
                    ReplaceUInt(value, iU0Index);
                }
            }
        }

        public byte U10
        {
            get
            {
                return bU10;
            }
            set
            {
                Debug.Assert(bU10 == Data[iU10Index]);
                Data[iU10Index] = bU10 = value;
                PFD.SetUserData(Data, true);
            }
        }

        public byte U11
        {
            get
            {
                return bU11;
            }
        }

        public string LotName
        {
            get
            {
                return sLotName;
            }
        }

        public override String ToString()
        {
            return sLotName;
        }

        public string LotDesc
        {
            get
            {
                return sLotDesc;
            }
            set
            {
                Debug.Fail("You have reached unreachable code!");
                return;
            }
        }
    }
}
