using System;
using System.Linq;

namespace PKHeX.Core
{
    /// <summary> Generation 5 <see cref="PKM"/> format. </summary>
    public class PK5 : PKM, IRibbonSetEvent3, IRibbonSetEvent4, IRibbonSetUnique3, IRibbonSetUnique4, IRibbonSetCommon3, IRibbonSetCommon4
    {
        public static readonly byte[] ExtraBytes =
        {
            0x42, 0x43, 0x5E, 0x63, 0x64, 0x65, 0x66, 0x67, 0x87
        };
        public sealed override int SIZE_PARTY => PKX.SIZE_5PARTY;
        public override int SIZE_STORED => PKX.SIZE_5STORED;
        public override int Format => 5;
        public override PersonalInfo PersonalInfo => PersonalTable.B2W2.GetFormeEntry(Species, AltForm);

        public PK5(byte[] decryptedData = null, string ident = null)
        {
            Data = (byte[])(decryptedData ?? new byte[SIZE_PARTY]).Clone();
            PKMConverter.CheckEncrypted(ref Data);
            Identifier = ident;
            if (Data.Length != SIZE_PARTY)
                Array.Resize(ref Data, SIZE_PARTY);
        }
        public override PKM Clone() => new PK5(Data);

        private string GetString(int Offset, int Count) => StringConverter.GetString5(Data, Offset, Count);
        private byte[] SetString(string value, int maxLength) => StringConverter.SetString5(value, maxLength);

        // Trash Bytes
        public override byte[] Nickname_Trash { get => GetData(0x48, 22); set { if (value?.Length == 22) value.CopyTo(Data, 0x48); } }
        public override byte[] OT_Trash { get => GetData(0x68, 16); set { if (value?.Length == 16) value.CopyTo(Data, 0x68); } }

        // Future Attributes
        public override uint EncryptionConstant { get => PID; set { } }
        public override int CurrentFriendship { get => OT_Friendship; set => OT_Friendship = value; }
        public override int CurrentHandler { get => 0; set { } }
        public override int AbilityNumber { get => HiddenAbility ? 4 : 1 << PIDAbility; set { } }

        // Structure
        public override uint PID { get => BitConverter.ToUInt32(Data, 0x00); set => BitConverter.GetBytes(value).CopyTo(Data, 0x00); }
        public override ushort Sanity { get => BitConverter.ToUInt16(Data, 0x04); set => BitConverter.GetBytes(value).CopyTo(Data, 0x04); }
        public override ushort Checksum { get => BitConverter.ToUInt16(Data, 0x06); set => BitConverter.GetBytes(value).CopyTo(Data, 0x06); }

        #region Block A
        public override int Species { get => BitConverter.ToUInt16(Data, 0x08); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x08); }
        public override int HeldItem { get => BitConverter.ToUInt16(Data, 0x0A); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x0A); }
        public override int TID { get => BitConverter.ToUInt16(Data, 0x0C); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x0C); }
        public override int SID { get => BitConverter.ToUInt16(Data, 0x0E); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x0E); }
        public override uint EXP { get => BitConverter.ToUInt32(Data, 0x10); set => BitConverter.GetBytes(value).CopyTo(Data, 0x10); }
        public override int OT_Friendship { get => Data[0x14]; set => Data[0x14] = (byte)value; }
        public override int Ability { get => Data[0x15]; set => Data[0x15] = (byte)value; }
        public override int MarkValue { get => Data[0x16]; protected set => Data[0x16] = (byte)value; }
        public override int Language { get => Data[0x17]; set => Data[0x17] = (byte)value; }
        public override int EV_HP { get => Data[0x18]; set => Data[0x18] = (byte)value; }
        public override int EV_ATK { get => Data[0x19]; set => Data[0x19] = (byte)value; }
        public override int EV_DEF { get => Data[0x1A]; set => Data[0x1A] = (byte)value; }
        public override int EV_SPE { get => Data[0x1B]; set => Data[0x1B] = (byte)value; }
        public override int EV_SPA { get => Data[0x1C]; set => Data[0x1C] = (byte)value; }
        public override int EV_SPD { get => Data[0x1D]; set => Data[0x1D] = (byte)value; }
        public override int CNT_Cool { get => Data[0x1E]; set => Data[0x1E] = (byte)value; }
        public override int CNT_Beauty { get => Data[0x1F]; set => Data[0x1F] = (byte)value; }
        public override int CNT_Cute { get => Data[0x20]; set => Data[0x20] = (byte)value; }
        public override int CNT_Smart { get => Data[0x21]; set => Data[0x21] = (byte)value; }
        public override int CNT_Tough { get => Data[0x22]; set => Data[0x22] = (byte)value; }
        public override int CNT_Sheen { get => Data[0x23]; set => Data[0x23] = (byte)value; }

        private byte RIB0 { get => Data[0x24]; set => Data[0x24] = value; } // Sinnoh 1
        private byte RIB1 { get => Data[0x25]; set => Data[0x25] = value; } // Sinnoh 2
        private byte RIB2 { get => Data[0x26]; set => Data[0x26] = value; } // Unova 1
        private byte RIB3 { get => Data[0x27]; set => Data[0x27] = value; } // Unova 2
        public bool RibbonChampionSinnoh    { get => (RIB0 & (1 << 0)) == 1 << 0; set => RIB0 = (byte)(RIB0 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool RibbonAbility           { get => (RIB0 & (1 << 1)) == 1 << 1; set => RIB0 = (byte)(RIB0 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool RibbonAbilityGreat      { get => (RIB0 & (1 << 2)) == 1 << 2; set => RIB0 = (byte)(RIB0 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool RibbonAbilityDouble     { get => (RIB0 & (1 << 3)) == 1 << 3; set => RIB0 = (byte)(RIB0 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool RibbonAbilityMulti      { get => (RIB0 & (1 << 4)) == 1 << 4; set => RIB0 = (byte)(RIB0 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool RibbonAbilityPair       { get => (RIB0 & (1 << 5)) == 1 << 5; set => RIB0 = (byte)(RIB0 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool RibbonAbilityWorld      { get => (RIB0 & (1 << 6)) == 1 << 6; set => RIB0 = (byte)(RIB0 & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool RibbonAlert             { get => (RIB0 & (1 << 7)) == 1 << 7; set => RIB0 = (byte)(RIB0 & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        public bool RibbonShock             { get => (RIB1 & (1 << 0)) == 1 << 0; set => RIB1 = (byte)(RIB1 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool RibbonDowncast          { get => (RIB1 & (1 << 1)) == 1 << 1; set => RIB1 = (byte)(RIB1 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool RibbonCareless          { get => (RIB1 & (1 << 2)) == 1 << 2; set => RIB1 = (byte)(RIB1 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool RibbonRelax             { get => (RIB1 & (1 << 3)) == 1 << 3; set => RIB1 = (byte)(RIB1 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool RibbonSnooze            { get => (RIB1 & (1 << 4)) == 1 << 4; set => RIB1 = (byte)(RIB1 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool RibbonSmile             { get => (RIB1 & (1 << 5)) == 1 << 5; set => RIB1 = (byte)(RIB1 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool RibbonGorgeous          { get => (RIB1 & (1 << 6)) == 1 << 6; set => RIB1 = (byte)(RIB1 & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool RibbonRoyal             { get => (RIB1 & (1 << 7)) == 1 << 7; set => RIB1 = (byte)(RIB1 & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        public bool RibbonGorgeousRoyal     { get => (RIB2 & (1 << 0)) == 1 << 0; set => RIB2 = (byte)(RIB2 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool RibbonFootprint         { get => (RIB2 & (1 << 1)) == 1 << 1; set => RIB2 = (byte)(RIB2 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool RibbonRecord            { get => (RIB2 & (1 << 2)) == 1 << 2; set => RIB2 = (byte)(RIB2 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool RibbonEvent             { get => (RIB2 & (1 << 3)) == 1 << 3; set => RIB2 = (byte)(RIB2 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool RibbonLegend            { get => (RIB2 & (1 << 4)) == 1 << 4; set => RIB2 = (byte)(RIB2 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool RibbonChampionWorld     { get => (RIB2 & (1 << 5)) == 1 << 5; set => RIB2 = (byte)(RIB2 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool RibbonBirthday          { get => (RIB2 & (1 << 6)) == 1 << 6; set => RIB2 = (byte)(RIB2 & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool RibbonSpecial           { get => (RIB2 & (1 << 7)) == 1 << 7; set => RIB2 = (byte)(RIB2 & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        public bool RibbonSouvenir          { get => (RIB3 & (1 << 0)) == 1 << 0; set => RIB3 = (byte)(RIB3 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool RibbonWishing           { get => (RIB3 & (1 << 1)) == 1 << 1; set => RIB3 = (byte)(RIB3 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool RibbonClassic           { get => (RIB3 & (1 << 2)) == 1 << 2; set => RIB3 = (byte)(RIB3 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool RibbonPremier           { get => (RIB3 & (1 << 3)) == 1 << 3; set => RIB3 = (byte)(RIB3 & ~(1 << 3) | (value ? 1 << 3 : 0)); }              
        public bool RIB3_4 { get => (RIB3 & (1 << 4)) == 1 << 4; set => RIB3 = (byte)(RIB3 & ~(1 << 4) | (value ? 1 << 4 : 0)); } // Unused
        public bool RIB3_5 { get => (RIB3 & (1 << 5)) == 1 << 5; set => RIB3 = (byte)(RIB3 & ~(1 << 5) | (value ? 1 << 5 : 0)); } // Unused
        public bool RIB3_6 { get => (RIB3 & (1 << 6)) == 1 << 6; set => RIB3 = (byte)(RIB3 & ~(1 << 6) | (value ? 1 << 6 : 0)); } // Unused
        public bool RIB3_7 { get => (RIB3 & (1 << 7)) == 1 << 7; set => RIB3 = (byte)(RIB3 & ~(1 << 7) | (value ? 1 << 7 : 0)); } // Unused
        #endregion

        #region Block B
        public override int Move1 { get => BitConverter.ToUInt16(Data, 0x28); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x28); }
        public override int Move2 { get => BitConverter.ToUInt16(Data, 0x2A); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x2A); }
        public override int Move3 { get => BitConverter.ToUInt16(Data, 0x2C); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x2C); }
        public override int Move4 { get => BitConverter.ToUInt16(Data, 0x2E); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x2E); }
        public override int Move1_PP { get => Data[0x30]; set => Data[0x30] = (byte)value; }
        public override int Move2_PP { get => Data[0x31]; set => Data[0x31] = (byte)value; }
        public override int Move3_PP { get => Data[0x32]; set => Data[0x32] = (byte)value; }
        public override int Move4_PP { get => Data[0x33]; set => Data[0x33] = (byte)value; }
        public override int Move1_PPUps { get => Data[0x34]; set => Data[0x34] = (byte)value; }
        public override int Move2_PPUps { get => Data[0x35]; set => Data[0x35] = (byte)value; }
        public override int Move3_PPUps { get => Data[0x36]; set => Data[0x36] = (byte)value; }
        public override int Move4_PPUps { get => Data[0x37]; set => Data[0x37] = (byte)value; }
        private uint IV32 { get => BitConverter.ToUInt32(Data, 0x38); set => BitConverter.GetBytes(value).CopyTo(Data, 0x38); }
        public override int IV_HP { get => (int)(IV32 >> 00) & 0x1F; set => IV32 = (uint)((IV32 & ~(0x1F << 00)) | (uint)((value > 31 ? 31 : value) << 00)); }
        public override int IV_ATK { get => (int)(IV32 >> 05) & 0x1F; set => IV32 = (uint)((IV32 & ~(0x1F << 05)) | (uint)((value > 31 ? 31 : value) << 05)); }
        public override int IV_DEF { get => (int)(IV32 >> 10) & 0x1F; set => IV32 = (uint)((IV32 & ~(0x1F << 10)) | (uint)((value > 31 ? 31 : value) << 10)); }
        public override int IV_SPE { get => (int)(IV32 >> 15) & 0x1F; set => IV32 = (uint)((IV32 & ~(0x1F << 15)) | (uint)((value > 31 ? 31 : value) << 15)); }
        public override int IV_SPA { get => (int)(IV32 >> 20) & 0x1F; set => IV32 = (uint)((IV32 & ~(0x1F << 20)) | (uint)((value > 31 ? 31 : value) << 20)); }
        public override int IV_SPD { get => (int)(IV32 >> 25) & 0x1F; set => IV32 = (uint)((IV32 & ~(0x1F << 25)) | (uint)((value > 31 ? 31 : value) << 25)); }
        public override bool IsEgg { get => ((IV32 >> 30) & 1) == 1; set => IV32 = (uint)((IV32 & ~0x40000000) | (uint)(value ? 0x40000000 : 0)); }
        public override bool IsNicknamed { get => ((IV32 >> 31) & 1) == 1; set => IV32 = (IV32 & 0x7FFFFFFF) | (value ? 0x80000000 : 0); }
        
        private byte RIB4 { get => Data[0x3C]; set => Data[0x3C] = value; } // Hoenn 1a
        private byte RIB5 { get => Data[0x3D]; set => Data[0x3D] = value; } // Hoenn 1b
        private byte RIB6 { get => Data[0x3E]; set => Data[0x3E] = value; } // Hoenn 2a
        private byte RIB7 { get => Data[0x3F]; set => Data[0x3F] = value; } // Hoenn 2b
        public bool RibbonG3Cool            { get => (RIB4 & (1 << 0)) == 1 << 0; set => RIB4 = (byte)(RIB4 & ~(1 << 0) | (value ? 1 << 0 : 0)); }             
        public bool RibbonG3CoolSuper       { get => (RIB4 & (1 << 1)) == 1 << 1; set => RIB4 = (byte)(RIB4 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool RibbonG3CoolHyper       { get => (RIB4 & (1 << 2)) == 1 << 2; set => RIB4 = (byte)(RIB4 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool RibbonG3CoolMaster      { get => (RIB4 & (1 << 3)) == 1 << 3; set => RIB4 = (byte)(RIB4 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool RibbonG3Beauty          { get => (RIB4 & (1 << 4)) == 1 << 4; set => RIB4 = (byte)(RIB4 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool RibbonG3BeautySuper     { get => (RIB4 & (1 << 5)) == 1 << 5; set => RIB4 = (byte)(RIB4 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool RibbonG3BeautyHyper     { get => (RIB4 & (1 << 6)) == 1 << 6; set => RIB4 = (byte)(RIB4 & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool RibbonG3BeautyMaster    { get => (RIB4 & (1 << 7)) == 1 << 7; set => RIB4 = (byte)(RIB4 & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        public bool RibbonG3Cute            { get => (RIB5 & (1 << 0)) == 1 << 0; set => RIB5 = (byte)(RIB5 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool RibbonG3CuteSuper       { get => (RIB5 & (1 << 1)) == 1 << 1; set => RIB5 = (byte)(RIB5 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool RibbonG3CuteHyper       { get => (RIB5 & (1 << 2)) == 1 << 2; set => RIB5 = (byte)(RIB5 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool RibbonG3CuteMaster      { get => (RIB5 & (1 << 3)) == 1 << 3; set => RIB5 = (byte)(RIB5 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool RibbonG3Smart           { get => (RIB5 & (1 << 4)) == 1 << 4; set => RIB5 = (byte)(RIB5 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool RibbonG3SmartSuper      { get => (RIB5 & (1 << 5)) == 1 << 5; set => RIB5 = (byte)(RIB5 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool RibbonG3SmartHyper      { get => (RIB5 & (1 << 6)) == 1 << 6; set => RIB5 = (byte)(RIB5 & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool RibbonG3SmartMaster     { get => (RIB5 & (1 << 7)) == 1 << 7; set => RIB5 = (byte)(RIB5 & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        public bool RibbonG3Tough           { get => (RIB6 & (1 << 0)) == 1 << 0; set => RIB6 = (byte)(RIB6 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool RibbonG3ToughSuper      { get => (RIB6 & (1 << 1)) == 1 << 1; set => RIB6 = (byte)(RIB6 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool RibbonG3ToughHyper      { get => (RIB6 & (1 << 2)) == 1 << 2; set => RIB6 = (byte)(RIB6 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool RibbonG3ToughMaster     { get => (RIB6 & (1 << 3)) == 1 << 3; set => RIB6 = (byte)(RIB6 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool RibbonChampionG3Hoenn   { get => (RIB6 & (1 << 4)) == 1 << 4; set => RIB6 = (byte)(RIB6 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool RibbonWinning           { get => (RIB6 & (1 << 5)) == 1 << 5; set => RIB6 = (byte)(RIB6 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool RibbonVictory           { get => (RIB6 & (1 << 6)) == 1 << 6; set => RIB6 = (byte)(RIB6 & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool RibbonArtist            { get => (RIB6 & (1 << 7)) == 1 << 7; set => RIB6 = (byte)(RIB6 & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        public bool RibbonEffort            { get => (RIB7 & (1 << 0)) == 1 << 0; set => RIB7 = (byte)(RIB7 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool RibbonChampionBattle    { get => (RIB7 & (1 << 1)) == 1 << 1; set => RIB7 = (byte)(RIB7 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool RibbonChampionRegional  { get => (RIB7 & (1 << 2)) == 1 << 2; set => RIB7 = (byte)(RIB7 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool RibbonChampionNational  { get => (RIB7 & (1 << 3)) == 1 << 3; set => RIB7 = (byte)(RIB7 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool RibbonCountry           { get => (RIB7 & (1 << 4)) == 1 << 4; set => RIB7 = (byte)(RIB7 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool RibbonNational          { get => (RIB7 & (1 << 5)) == 1 << 5; set => RIB7 = (byte)(RIB7 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool RibbonEarth             { get => (RIB7 & (1 << 6)) == 1 << 6; set => RIB7 = (byte)(RIB7 & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool RibbonWorld             { get => (RIB7 & (1 << 7)) == 1 << 7; set => RIB7 = (byte)(RIB7 & ~(1 << 7) | (value ? 1 << 7 : 0)); }

        public override bool FatefulEncounter { get => (Data[0x40] & 1) == 1; set => Data[0x40] = (byte)(Data[0x40] & ~0x01 | (value ? 1 : 0)); }
        public override int Gender { get => (Data[0x40] >> 1) & 0x3; set => Data[0x40] = (byte)(Data[0x40] & ~0x06 | (value << 1)); }
        public override int AltForm { get => Data[0x40] >> 3; set => Data[0x40] = (byte)(Data[0x40] & 0x07 | (value << 3)); }
        public override int Nature { get => Data[0x41]; set => Data[0x41] = (byte)value; }
        public bool HiddenAbility { get => (Data[0x42] & 1) == 1; set => Data[0x42] = (byte)(Data[0x42] & ~0x01 | (value ? 1 : 0)); }
        public bool NPokémon { get => (Data[0x42] & 2) == 2; set => Data[0x42] = (byte)(Data[0x42] & ~0x02 | (value ? 2 : 0)); }
        // 0x43-0x47 Unused
        #endregion

        #region Block C
        public override string Nickname { get => GetString(0x48, 22); set => SetString(value, 11).CopyTo(Data, 0x48); }
        // 0x5E unused
        public override int Version { get => Data[0x5F]; set => Data[0x5F] = (byte)value; }
        private byte RIB8 { get => Data[0x60]; set => Data[0x60] = value; } // Sinnoh 3
        private byte RIB9 { get => Data[0x61]; set => Data[0x61] = value; } // Sinnoh 4
        private byte RIBA { get => Data[0x62]; set => Data[0x62] = value; } // Sinnoh 5
        private byte RIBB { get => Data[0x63]; set => Data[0x63] = value; } // Sinnoh 6
        public bool RibbonG4Cool            { get => (RIB8 & (1 << 0)) == 1 << 0; set => RIB8 = (byte)(RIB8 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool RibbonG4CoolGreat       { get => (RIB8 & (1 << 1)) == 1 << 1; set => RIB8 = (byte)(RIB8 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool RibbonG4CoolUltra       { get => (RIB8 & (1 << 2)) == 1 << 2; set => RIB8 = (byte)(RIB8 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool RibbonG4CoolMaster      { get => (RIB8 & (1 << 3)) == 1 << 3; set => RIB8 = (byte)(RIB8 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool RibbonG4Beauty          { get => (RIB8 & (1 << 4)) == 1 << 4; set => RIB8 = (byte)(RIB8 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool RibbonG4BeautyGreat     { get => (RIB8 & (1 << 5)) == 1 << 5; set => RIB8 = (byte)(RIB8 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool RibbonG4BeautyUltra     { get => (RIB8 & (1 << 6)) == 1 << 6; set => RIB8 = (byte)(RIB8 & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool RibbonG4BeautyMaster    { get => (RIB8 & (1 << 7)) == 1 << 7; set => RIB8 = (byte)(RIB8 & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        public bool RibbonG4Cute            { get => (RIB9 & (1 << 0)) == 1 << 0; set => RIB9 = (byte)(RIB9 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool RibbonG4CuteGreat       { get => (RIB9 & (1 << 1)) == 1 << 1; set => RIB9 = (byte)(RIB9 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool RibbonG4CuteUltra       { get => (RIB9 & (1 << 2)) == 1 << 2; set => RIB9 = (byte)(RIB9 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool RibbonG4CuteMaster      { get => (RIB9 & (1 << 3)) == 1 << 3; set => RIB9 = (byte)(RIB9 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool RibbonG4Smart           { get => (RIB9 & (1 << 4)) == 1 << 4; set => RIB9 = (byte)(RIB9 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool RibbonG4SmartGreat      { get => (RIB9 & (1 << 5)) == 1 << 5; set => RIB9 = (byte)(RIB9 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool RibbonG4SmartUltra      { get => (RIB9 & (1 << 6)) == 1 << 6; set => RIB9 = (byte)(RIB9 & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool RibbonG4SmartMaster     { get => (RIB9 & (1 << 7)) == 1 << 7; set => RIB9 = (byte)(RIB9 & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        public bool RibbonG4Tough           { get => (RIBA & (1 << 0)) == 1 << 0; set => RIBA = (byte)(RIBA & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool RibbonG4ToughGreat      { get => (RIBA & (1 << 1)) == 1 << 1; set => RIBA = (byte)(RIBA & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool RibbonG4ToughUltra      { get => (RIBA & (1 << 2)) == 1 << 2; set => RIBA = (byte)(RIBA & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool RibbonG4ToughMaster     { get => (RIBA & (1 << 3)) == 1 << 3; set => RIBA = (byte)(RIBA & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool RIBA_4 { get => (RIBA & (1 << 4)) == 1 << 4; set => RIBA = (byte)(RIBA & ~(1 << 4) | (value ? 1 << 4 : 0)); } // Unused
        public bool RIBA_5 { get => (RIBA & (1 << 5)) == 1 << 5; set => RIBA = (byte)(RIBA & ~(1 << 5) | (value ? 1 << 5 : 0)); } // Unused
        public bool RIBA_6 { get => (RIBA & (1 << 6)) == 1 << 6; set => RIBA = (byte)(RIBA & ~(1 << 6) | (value ? 1 << 6 : 0)); } // Unused
        public bool RIBA_7 { get => (RIBA & (1 << 7)) == 1 << 7; set => RIBA = (byte)(RIBA & ~(1 << 7) | (value ? 1 << 7 : 0)); } // Unused
        public bool RIBB_0 { get => (RIBB & (1 << 0)) == 1 << 0; set => RIBB = (byte)(RIBB & ~(1 << 0) | (value ? 1 << 0 : 0)); } // Unused
        public bool RIBB_1 { get => (RIBB & (1 << 1)) == 1 << 1; set => RIBB = (byte)(RIBB & ~(1 << 1) | (value ? 1 << 1 : 0)); } // Unused
        public bool RIBB_2 { get => (RIBB & (1 << 2)) == 1 << 2; set => RIBB = (byte)(RIBB & ~(1 << 2) | (value ? 1 << 2 : 0)); } // Unused
        public bool RIBB_3 { get => (RIBB & (1 << 3)) == 1 << 3; set => RIBB = (byte)(RIBB & ~(1 << 3) | (value ? 1 << 3 : 0)); } // Unused
        public bool RIBB_4 { get => (RIBB & (1 << 4)) == 1 << 4; set => RIBB = (byte)(RIBB & ~(1 << 4) | (value ? 1 << 4 : 0)); } // Unused
        public bool RIBB_5 { get => (RIBB & (1 << 5)) == 1 << 5; set => RIBB = (byte)(RIBB & ~(1 << 5) | (value ? 1 << 5 : 0)); } // Unused
        public bool RIBB_6 { get => (RIBB & (1 << 6)) == 1 << 6; set => RIBB = (byte)(RIBB & ~(1 << 6) | (value ? 1 << 6 : 0)); } // Unused
        public bool RIBB_7 { get => (RIBB & (1 << 7)) == 1 << 7; set => RIBB = (byte)(RIBB & ~(1 << 7) | (value ? 1 << 7 : 0)); } // Unused
        // 0x64-0x67 Unused
        #endregion

        #region Block D
        public override string OT_Name { get => GetString(0x68, 0x16); set => SetString(value, 7).CopyTo(Data, 0x68); }
        public override int Egg_Year { get => Data[0x78]; set => Data[0x78] = (byte)value; }
        public override int Egg_Month { get => Data[0x79]; set => Data[0x79] = (byte)value; }
        public override int Egg_Day { get => Data[0x7A]; set => Data[0x7A] = (byte)value; }
        public override int Met_Year { get => Data[0x7B]; set => Data[0x7B] = (byte)value; }
        public override int Met_Month { get => Data[0x7C]; set => Data[0x7C] = (byte)value; }
        public override int Met_Day { get => Data[0x7D]; set => Data[0x7D] = (byte)value; }
        public override int Egg_Location { get => BitConverter.ToUInt16(Data, 0x7E); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x7E); }
        public override int Met_Location { get => BitConverter.ToUInt16(Data, 0x80); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x80); }
        private byte PKRS { get => Data[0x82]; set => Data[0x82] = value; }
        public override int PKRS_Days { get => PKRS & 0xF; set => PKRS = (byte)(PKRS & ~0xF | value); }
        public override int PKRS_Strain { get => PKRS >> 4; set => PKRS = (byte)(PKRS & 0xF | (value << 4)); }
        public override int Ball { get => Data[0x83]; set => Data[0x83] = (byte)value; }
        public override int Met_Level { get => Data[0x84] & ~0x80; set => Data[0x84] = (byte)((Data[0x84] & 0x80) | value); }
        public override int OT_Gender { get => Data[0x84] >> 7; set => Data[0x84] = (byte)((Data[0x84] & ~0x80) | value << 7); }
        public override int EncounterType { get => Data[0x85]; set => Data[0x85] = (byte)value; }
        // 0x86-0x87 Unused
        #endregion

        public override int Stat_Level { get => Data[0x8C]; set => Data[0x8C] = (byte)value; }
        public override int Stat_HPCurrent { get => BitConverter.ToUInt16(Data, 0x8E); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x8E); }
        public override int Stat_HPMax { get => BitConverter.ToUInt16(Data, 0x90); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x90); }
        public override int Stat_ATK { get => BitConverter.ToUInt16(Data, 0x92); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x92); }
        public override int Stat_DEF { get => BitConverter.ToUInt16(Data, 0x94); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x94); }
        public override int Stat_SPE { get => BitConverter.ToUInt16(Data, 0x96); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x96); }
        public override int Stat_SPA { get => BitConverter.ToUInt16(Data, 0x98); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x98); }
        public override int Stat_SPD { get => BitConverter.ToUInt16(Data, 0x9A); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x9A); }

        // Generated Attributes
        public override int PSV => (int)((PID >> 16 ^ PID & 0xFFFF) >> 3);
        public override int TSV => (TID ^ SID) >> 3;
        public override int Characteristic
        {
            get
            {
                // Characteristic with PID%6
                int pm6 = (int)(PID % 6); // PID MOD 6
                int maxIV = IVs.Max();
                int pm6stat = 0;

                for (int i = 0; i < 6; i++)
                {
                    pm6stat = (pm6 + i) % 6;
                    if (IVs[pm6stat] == maxIV)
                        break; // P%6 is this stat
                }
                return pm6stat * 5 + maxIV % 5;
            }
        }
        
        // Maximums
        public override int MaxMoveID => Legal.MaxMoveID_5;
        public override int MaxSpeciesID => Legal.MaxSpeciesID_5;
        public override int MaxAbilityID => Legal.MaxAbilityID_5;
        public override int MaxItemID => Legal.MaxItemID_5_B2W2;
        public override int MaxBallID => Legal.MaxBallID_5;
        public override int MaxGameID => Legal.MaxGameID_5; // B2
        public override int MaxIV => 31;
        public override int MaxEV => 255;
        public override int OTLength => 7;
        public override int NickLength => 10;

        // Methods
        protected override byte[] Encrypt()
        {
            RefreshChecksum();
            return PKX.EncryptArray45(Data);
        }

        public PK6 ConvertToPK6()
        {
            PK6 pk6 = new PK6 // Convert away!
            {
                EncryptionConstant = PID,
                Species = Species,
                TID = TID,
                SID = SID,
                EXP = EXP,
                PID = PID,
                Ability = Ability
            };

            int[] abilities = PersonalInfo.Abilities;
            int abilval = Array.IndexOf(abilities, Ability);
            if (abilval >= 0 && abilities[abilval] == abilities[2] && HiddenAbility)
                abilval = 2; // hidden ability shared with a regular ability
            if (abilval >= 0)
                pk6.AbilityNumber = 1 << abilval;
            else // Fallback (shouldn't happen)
            {
                if (HiddenAbility) pk6.AbilityNumber = 4; // Hidden, else G5 or G3/4 correlation.
                else pk6.AbilityNumber = Gen5 ? 1 << (int)(PID >> 16 & 1) : 1 << (int)(PID & 1);
            }
            pk6.Markings = Markings;
            pk6.Language = Math.Max((int)LanguageID.Japanese, Language); // Hacked or Bad IngameTrade (Japanese B/W)

            pk6.CNT_Cool = CNT_Cool;
            pk6.CNT_Beauty = CNT_Beauty;
            pk6.CNT_Cute = CNT_Cute;
            pk6.CNT_Smart = CNT_Smart;
            pk6.CNT_Tough = CNT_Tough;
            pk6.CNT_Sheen = CNT_Sheen;

            // Cap EVs
            pk6.EV_HP = EV_HP > 252 ? 252 : EV_HP;
            pk6.EV_ATK = EV_ATK > 252 ? 252 : EV_ATK;
            pk6.EV_DEF = EV_DEF > 252 ? 252 : EV_DEF;
            pk6.EV_SPA = EV_SPA > 252 ? 252 : EV_SPA;
            pk6.EV_SPD = EV_SPD > 252 ? 252 : EV_SPD;
            pk6.EV_SPE = EV_SPE > 252 ? 252 : EV_SPE;

            pk6.Move1 = Move1;
            pk6.Move2 = Move2;
            pk6.Move3 = Move3;
            pk6.Move4 = Move4;

            pk6.Move1_PPUps = Move1_PPUps;
            pk6.Move2_PPUps = Move2_PPUps;
            pk6.Move3_PPUps = Move3_PPUps;
            pk6.Move4_PPUps = Move4_PPUps;

            // Fix PP
            pk6.Move1_PP = pk6.GetMovePP(pk6.Move1, pk6.Move1_PPUps);
            pk6.Move2_PP = pk6.GetMovePP(pk6.Move2, pk6.Move2_PPUps);
            pk6.Move3_PP = pk6.GetMovePP(pk6.Move3, pk6.Move3_PPUps);
            pk6.Move4_PP = pk6.GetMovePP(pk6.Move4, pk6.Move4_PPUps);

            pk6.IV_HP = IV_HP;
            pk6.IV_ATK = IV_ATK;
            pk6.IV_DEF = IV_DEF;
            pk6.IV_SPA = IV_SPA;
            pk6.IV_SPD = IV_SPD;
            pk6.IV_SPE = IV_SPE;
            pk6.IsEgg = IsEgg;
            pk6.IsNicknamed = IsNicknamed;

            pk6.FatefulEncounter = FatefulEncounter;
            pk6.Gender = Gender;
            pk6.AltForm = AltForm;
            pk6.Nature = Nature;

            // Apply trash bytes for species name of current app language -- default to PKM's language if no match
            int curLang = PKX.GetSpeciesNameLanguage(Species, Nickname, Format);
            pk6.Nickname = PKX.GetSpeciesNameGeneration(Species, curLang < 0 ? Language : curLang, pk6.Format);
            if (IsNicknamed)
                pk6.Nickname = Nickname;

            pk6.Version = Version;

            pk6.OT_Name = OT_Name;

            // Dates are kept upon transfer
            pk6.MetDate = MetDate;
            pk6.EggMetDate = EggMetDate;

            // Locations are kept upon transfer
            pk6.Met_Location = Met_Location;
            pk6.Egg_Location = Egg_Location;

            pk6.PKRS_Strain = PKRS_Strain;
            pk6.PKRS_Days = PKRS_Days;
            pk6.Ball = Ball;

            // OT Gender & Encounter Level
            pk6.Met_Level = Met_Level;
            pk6.OT_Gender = OT_Gender;
            pk6.EncounterType = EncounterType;
            
            // Ribbon Decomposer (Contest & Battle)
            byte contestribbons = 0;
            byte battleribbons = 0;

            // Contest Ribbon Counter
            for (int i = 0; i < 8; i++) // Sinnoh 3, Hoenn 1
            {
                if ((Data[0x60] >> i & 1) == 1) contestribbons++;
                if (((Data[0x61] >> i) & 1) == 1) contestribbons++;
                if (((Data[0x3C] >> i) & 1) == 1) contestribbons++;
                if (((Data[0x3D] >> i) & 1) == 1) contestribbons++;
            }
            for (int i = 0; i < 4; i++) // Sinnoh 4, Hoenn 2
            {
                if (((Data[0x62] >> i) & 1) == 1) contestribbons++;
                if (((Data[0x3E] >> i) & 1) == 1) contestribbons++;
            }

            // Battle Ribbon Counter
            // Winning Ribbon
            if ((Data[0x3E] & 0x20) >> 5 == 1) battleribbons++;
            // Victory Ribbon
            if ((Data[0x3E] & 0x40) >> 6 == 1) battleribbons++;
            for (int i = 1; i < 7; i++)     // Sinnoh Battle Ribbons
                if (((Data[0x24] >> i) & 1) == 1) battleribbons++;

            // Fill the Ribbon Counter Bytes
            pk6.RibbonCountMemoryContest = contestribbons;
            pk6.RibbonCountMemoryBattle = battleribbons;

            // Copy Ribbons to their new locations.
            int bx30 = 0;
            // bx30 |= 0;                             // Kalos Champ - New Kalos Ribbon
            bx30 |= ((Data[0x3E] & 0x10) >> 4) << 1; // Hoenn Champion
            bx30 |= ((Data[0x24] & 0x01) >> 0) << 2; // Sinnoh Champ
            // bx30 |= 0;                             // Best Friend - New Kalos Ribbon
            // bx30 |= 0;                             // Training    - New Kalos Ribbon
            // bx30 |= 0;                             // Skillful    - New Kalos Ribbon
            // bx30 |= 0;                             // Expert      - New Kalos Ribbon
            bx30 |= ((Data[0x3F] & 0x01) >> 0) << 7; // Effort Ribbon
            pk6.Data[0x30] = (byte)bx30;

            int bx31 = 0;
            bx31 |= ((Data[0x24] & 0x80) >> 7) << 0;  // Alert
            bx31 |= ((Data[0x25] & 0x01) >> 0) << 1;  // Shock
            bx31 |= ((Data[0x25] & 0x02) >> 1) << 2;  // Downcast
            bx31 |= ((Data[0x25] & 0x04) >> 2) << 3;  // Careless
            bx31 |= ((Data[0x25] & 0x08) >> 3) << 4;  // Relax
            bx31 |= ((Data[0x25] & 0x10) >> 4) << 5;  // Snooze
            bx31 |= ((Data[0x25] & 0x20) >> 5) << 6;  // Smile
            bx31 |= ((Data[0x25] & 0x40) >> 6) << 7;  // Gorgeous
            pk6.Data[0x31] = (byte)bx31;

            int bx32 = 0;
            bx32 |= ((Data[0x25] & 0x80) >> 7) << 0;  // Royal
            bx32 |= ((Data[0x26] & 0x01) >> 0) << 1;  // Gorgeous Royal
            bx32 |= ((Data[0x3E] & 0x80) >> 7) << 2;  // Artist
            bx32 |= ((Data[0x26] & 0x02) >> 1) << 3;  // Footprint
            bx32 |= ((Data[0x26] & 0x04) >> 2) << 4;  // Record
            bx32 |= ((Data[0x26] & 0x10) >> 4) << 5;  // Legend
            bx32 |= ((Data[0x3F] & 0x10) >> 4) << 6;  // Country
            bx32 |= ((Data[0x3F] & 0x20) >> 5) << 7;  // National
            pk6.Data[0x32] = (byte)bx32;

            int bx33 = 0;
            bx33 |= ((Data[0x3F] & 0x40) >> 6) << 0;  // Earth
            bx33 |= ((Data[0x3F] & 0x80) >> 7) << 1;  // World
            bx33 |= ((Data[0x27] & 0x04) >> 2) << 2;  // Classic
            bx33 |= ((Data[0x27] & 0x08) >> 3) << 3;  // Premier
            bx33 |= ((Data[0x26] & 0x08) >> 3) << 4;  // Event
            bx33 |= ((Data[0x26] & 0x40) >> 6) << 5;  // Birthday
            bx33 |= ((Data[0x26] & 0x80) >> 7) << 6;  // Special
            bx33 |= ((Data[0x27] & 0x01) >> 0) << 7;  // Souvenir
            pk6.Data[0x33] = (byte)bx33;

            int bx34 = 0;
            bx34 |= ((Data[0x27] & 0x02) >> 1) << 0;  // Wishing Ribbon
            bx34 |= ((Data[0x3F] & 0x02) >> 1) << 1;  // Battle Champion
            bx34 |= ((Data[0x3F] & 0x04) >> 2) << 2;  // Regional Champion
            bx34 |= ((Data[0x3F] & 0x08) >> 3) << 3;  // National Champion
            bx34 |= ((Data[0x26] & 0x20) >> 5) << 4;  // World Champion
            pk6.Data[0x34] = (byte)bx34;
            
            // Write Transfer Location - location is dependent on 3DS system that transfers.
            pk6.Country = PKMConverter.Country;
            pk6.Region = PKMConverter.Region;
            pk6.ConsoleRegion = PKMConverter.ConsoleRegion;

            // Write the Memories, Friendship, and Origin!
            pk6.CurrentHandler = 1;
            pk6.HT_Name = PKMConverter.OT_Name;
            pk6.HT_Gender = PKMConverter.OT_Gender;
            pk6.Geo1_Region = PKMConverter.Region;
            pk6.Geo1_Country = PKMConverter.Country;
            pk6.HT_Intensity = 1;
            pk6.HT_Memory = 4;
            pk6.HT_Feeling = (int)(Util.Rand32() % 10);
            // When transferred, friendship gets reset.
            pk6.OT_Friendship = pk6.HT_Friendship = PersonalInfo.BaseFriendship;

            // Antishiny Mechanism
            ushort LID = (ushort)(PID & 0xFFFF);
            ushort HID = (ushort)(PID >> 0x10);

            int XOR = TID ^ SID ^ LID ^ HID;
            if (XOR >= 8 && XOR < 16) // If we get an illegal collision...
                pk6.PID ^= 0x80000000;

            // HMs are not deleted 5->6, transfer away (but fix if blank spots?)
            pk6.FixMoves();

            // Fix Name Strings
            pk6.Nickname = pk6.Nickname
                .Replace('\u2467', '\u00d7') // ×
                .Replace('\u2468', '\u00f7') // ÷
                .Replace('\u246c', '\u2026') // …

                .Replace('\u246d', '\uE08E') // ♂
                .Replace('\u246e', '\uE08F') // ♀
                .Replace('\u246f', '\uE090') // ♠
                .Replace('\u2470', '\uE091') // ♣
                .Replace('\u2471', '\uE092') // ♥
                .Replace('\u2472', '\uE093') // ♦
                .Replace('\u2473', '\uE094') // ★
                .Replace('\u2474', '\uE095') // ◎

                .Replace('\u2475', '\uE096') // ○
                .Replace('\u2476', '\uE097') // □
                .Replace('\u2477', '\uE098') // △
                .Replace('\u2478', '\uE099') // ◇
                .Replace('\u2479', '\uE09A') // ♪
                .Replace('\u247a', '\uE09B') // ☀
                .Replace('\u247b', '\uE09C') // ☁
                .Replace('\u247d', '\uE09D') // ☂
                ;

            pk6.OT_Name = pk6.OT_Name
                .Replace('\u2467', '\u00d7') // ×
                .Replace('\u2468', '\u00f7') // ÷
                .Replace('\u246c', '\u2026') // …

                .Replace('\u246d', '\uE08E') // ♂
                .Replace('\u246e', '\uE08F') // ♀
                .Replace('\u246f', '\uE090') // ♠
                .Replace('\u2470', '\uE091') // ♣
                .Replace('\u2471', '\uE092') // ♥
                .Replace('\u2472', '\uE093') // ♦
                .Replace('\u2473', '\uE094') // ★
                .Replace('\u2474', '\uE095') // ◎

                .Replace('\u2475', '\uE096') // ○
                .Replace('\u2476', '\uE097') // □
                .Replace('\u2477', '\uE098') // △
                .Replace('\u2478', '\uE099') // ◇
                .Replace('\u2479', '\uE09A') // ♪
                .Replace('\u247a', '\uE09B') // ☀
                .Replace('\u247b', '\uE09C') // ☁
                .Replace('\u247d', '\uE09D') // ☂
                ;

            // Fix Checksum
            pk6.RefreshChecksum();

            return pk6; // Done!
        }
    }
}
