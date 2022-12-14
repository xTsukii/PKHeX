using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PKHeX.Core
{
    /// <summary> Generation 2 <see cref="PKM"/> format. </summary>
    public class PK2 : PKM
    {
        // Internal use only
        protected internal byte[] otname;
        protected internal byte[] nick;
        public override PersonalInfo PersonalInfo => PersonalTable.C[Species];

        public byte[] OT_Name_Raw => (byte[])otname.Clone();
        public byte[] Nickname_Raw => (byte[])nick.Clone();
        public override bool Valid => Species <= 252;

        public sealed override int SIZE_PARTY => PKX.SIZE_2PARTY;
        public override int SIZE_STORED => PKX.SIZE_2STORED;
        internal const int STRLEN_J = 6;
        internal const int STRLEN_U = 11;
        private int StringLength => Japanese ? STRLEN_J : STRLEN_U;
        public override bool Korean => !Japanese && otname[0] <= 0xB;

        private string GetString(int Offset, int Count)
        {
            if (Korean)
                return StringConverter.GetString2KOR(Data, Offset, Count);
            return StringConverter.GetString1(Data, Offset, Count, Japanese);
        }

        private byte[] SetString(string value, int maxLength)
        {
            if (Korean)
                return StringConverter.SetString2KOR(value, maxLength);
            return StringConverter.SetString1(value, maxLength, Japanese);
        }

        // Trash Bytes
        public override byte[] Nickname_Trash { get => nick; set { if (value?.Length == nick.Length) nick = value; } }
        public override byte[] OT_Trash { get => otname; set { if (value?.Length == otname.Length) otname = value; } }

        public override int Format => 2;

        public override bool Japanese => otname.Length == STRLEN_J;
        public override string FileName
        {
            get
            {
                string form = AltForm > 0 ? $"-{AltForm:00}" : "";
                string star = IsShiny ? " ★" : "";
                return $"{Species:000}{form}{star} - {Nickname} - {SaveUtil.CRC16_CCITT(Encrypt()):X4}.{Extension}";
            }
        }

        public PK2(byte[] decryptedData = null, string ident = null, bool jp = false)
        {
            Data = (byte[])(decryptedData ?? new byte[SIZE_PARTY]).Clone();
            Identifier = ident;
            if (Data.Length != SIZE_PARTY)
                Array.Resize(ref Data, SIZE_PARTY);
            int strLen = jp ? STRLEN_J : STRLEN_U;
            otname = Enumerable.Repeat((byte) 0x50, strLen).ToArray();
            nick = Enumerable.Repeat((byte) 0x50, strLen).ToArray();

            IsEgg = false; // Egg data stored in Pokemon List.
        }

        public override PKM Clone()
        {
            PK2 new_pk2 = new PK2(Data, Identifier, Japanese);
            Array.Copy(otname, 0, new_pk2.otname, 0, otname.Length);
            Array.Copy(nick, 0, new_pk2.nick, 0, nick.Length);
            new_pk2.IsEgg = IsEgg;
            return new_pk2;
        }
        public override string Nickname
        {
            get
            {
                if (Korean)
                    return StringConverter.GetString2KOR(nick, 0, nick.Length);
                return StringConverter.GetString1(nick, 0, nick.Length, Japanese);
            }
            set
            {
                if (!IsNicknamed)
                    return;

                byte[] strdata = SetString(value, StringLength);
                if (nick.Any(b => b == 0) && nick[StringLength - 1] == 0x50 && Array.FindIndex(nick, b => b == 0) == strdata.Length - 1) // Handle JP Mew event with grace
                {
                    int firstInd = Array.FindIndex(nick, b => b == 0);
                    for (int i = firstInd; i < StringLength - 1; i++)
                        if (nick[i] != 0)
                            break;
                    strdata = strdata.Take(strdata.Length - 1).ToArray();
                }
                strdata.CopyTo(nick, 0);
            }
        }

        public override string OT_Name
        {
            get
            {
                if (Korean)
                    return StringConverter.GetString2KOR(otname, 0, otname.Length);
                return StringConverter.GetString1(otname, 0, otname.Length, Japanese); 
            }
            set
            {
                byte[] strdata = SetString(value, StringLength);
                if (otname.Any(b => b == 0) && otname[StringLength - 1] == 0x50 && Array.FindIndex(otname, b => b == 0) == strdata.Length - 1) // Handle JP Mew event with grace
                {
                    int firstInd = Array.FindIndex(otname, b => b == 0);
                    for (int i = firstInd; i < StringLength - 1; i++)
                        if (otname[i] != 0)
                            break;
                    strdata = strdata.Take(strdata.Length - 1).ToArray();
                }
                strdata.CopyTo(otname, 0);
            }
        }

        protected override byte[] Encrypt() => new PokemonList2(this).GetBytes();
        public override byte[] EncryptedPartyData => Encrypt();
        public override byte[] EncryptedBoxData => Encrypt();
        public override byte[] DecryptedBoxData => Encrypt();
        public override byte[] DecryptedPartyData => Encrypt();

        private bool? _isnicknamed;
        public override bool IsNicknamed
        {
            get => (bool)(_isnicknamed ?? (_isnicknamed = !nick.SequenceEqual(GetNonNickname())));
            set
            {
                _isnicknamed = value;
                if (_isnicknamed == false)
                    SetNotNicknamed();
            }
        }
        public void SetNotNicknamed() => nick = GetNonNickname().ToArray();
        private IEnumerable<byte> GetNonNickname()
        {
            var name = PKX.GetSpeciesNameGeneration(Species, GuessedLanguage(), Format);
            var bytes = SetString(name, StringLength);
            var data = bytes.Concat(Enumerable.Repeat((byte) 0x50, nick.Length - bytes.Length));
            if (!Korean)
                data = data.Select(b => (byte)(b == 0xF2 ? 0xE8 : b)); // Decimal point<->period fix
            return data;
        }
        public bool IsNicknamedBank
        {
            get
            {
                var spName = PKX.GetSpeciesNameGeneration(Species, GuessedLanguage(), Format);
                return Nickname != spName;
            }
        }
        public override int Language
        {
            get
            {
                if (Japanese)
                    return (int)LanguageID.Japanese;
                if (Korean)
                    return (int)LanguageID.Korean;
                if (StringConverter.IsG12German(otname))
                    return (int)LanguageID.German; // german
                int lang = PKX.GetSpeciesNameLanguage(Species, Nickname, Format);
                if (lang > 0)
                    return lang;
                return 0;
            }
            set { }
        }
        private int GuessedLanguage(int fallback = (int)LanguageID.English)
        {
            int lang = Language;
            if (lang > 0)
                return lang;
            if (fallback == (int)LanguageID.French || fallback == (int)LanguageID.German) // only other permitted besides English
                return fallback;
            return (int)LanguageID.English;
        }

        #region Stored Attributes
        public override int Species
        {
            get => Data[0];
            set => Data[0] = (byte)value;
        }
        public override int SpriteItem => ItemConverter.GetG4Item((byte)HeldItem);
        public override int HeldItem { get => Data[0x1]; set => Data[0x1] = (byte)value; }
        public override int Move1 { get => Data[2]; set => Data[2] = (byte)value; }
        public override int Move2 { get => Data[3]; set => Data[3] = (byte)value; }
        public override int Move3 { get => Data[4]; set => Data[4] = (byte)value; }
        public override int Move4 { get => Data[5]; set => Data[5] = (byte)value; }
        public override int TID { get => BigEndian.ToUInt16(Data, 6); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 6); }
        public override uint EXP
        {
            get => (BigEndian.ToUInt32(Data, 8) >> 8) & 0x00FFFFFF;
            set => Array.Copy(BigEndian.GetBytes((value << 8) & 0xFFFFFF00), 0, Data, 8, 3);
        }
        public override int EV_HP { get => BigEndian.ToUInt16(Data, 0xB); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0xB); }
        public override int EV_ATK { get => BigEndian.ToUInt16(Data, 0xD); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0xD); }
        public override int EV_DEF { get => BigEndian.ToUInt16(Data, 0xF); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0xF); }
        public override int EV_SPE { get => BigEndian.ToUInt16(Data, 0x11); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x11); }
        public int EV_SPC { get => BigEndian.ToUInt16(Data, 0x13); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x13); }
        public override int EV_SPA { get => EV_SPC; set => EV_SPC = value; }
        public override int EV_SPD { get => EV_SPC; set { } }
        public ushort DV16 { get => BigEndian.ToUInt16(Data, 0x15); set => BigEndian.GetBytes(value).CopyTo(Data, 0x15); }
        public override int IV_HP { get => ((IV_ATK & 1) << 3) | ((IV_DEF & 1) << 2) | ((IV_SPE & 1) << 1) | ((IV_SPC & 1) << 0); set { } }
        public override int IV_ATK { get => (DV16 >> 12) & 0xF; set => DV16 = (ushort)((DV16 & ~(0xF << 12)) | (ushort)((value > 0xF ? 0xF : value) << 12)); }
        public override int IV_DEF { get => (DV16 >> 8) & 0xF; set => DV16 = (ushort)((DV16 & ~(0xF << 8)) | (ushort)((value > 0xF ? 0xF : value) << 8)); }
        public override int IV_SPE { get => (DV16 >> 4) & 0xF; set => DV16 = (ushort)((DV16 & ~(0xF << 4)) | (ushort)((value > 0xF ? 0xF : value) << 4)); }
        public int IV_SPC { get => (DV16 >> 0) & 0xF; set => DV16 = (ushort)((DV16 & ~(0xF << 0)) | (ushort)((value > 0xF ? 0xF : value) << 0)); }
        public override int IV_SPA { get => IV_SPC; set => IV_SPC = value; }
        public override int IV_SPD { get => IV_SPC; set { } }
        public override int Move1_PP { get => Data[0x17] & 0x3F; set => Data[0x17] = (byte)((Data[0x17] & 0xC0) | Math.Min(63, value)); }
        public override int Move2_PP { get => Data[0x18] & 0x3F; set => Data[0x18] = (byte)((Data[0x18] & 0xC0) | Math.Min(63, value)); }
        public override int Move3_PP { get => Data[0x19] & 0x3F; set => Data[0x19] = (byte)((Data[0x19] & 0xC0) | Math.Min(63, value)); }
        public override int Move4_PP { get => Data[0x1A] & 0x3F; set => Data[0x1A] = (byte)((Data[0x1A] & 0xC0) | Math.Min(63, value)); }
        public override int Move1_PPUps { get => (Data[0x17] & 0xC0) >> 6; set => Data[0x17] = (byte)((Data[0x17] & 0x3F) | ((value & 0x3) << 6)); }
        public override int Move2_PPUps { get => (Data[0x18] & 0xC0) >> 6; set => Data[0x18] = (byte)((Data[0x18] & 0x3F) | ((value & 0x3) << 6)); }
        public override int Move3_PPUps { get => (Data[0x19] & 0xC0) >> 6; set => Data[0x19] = (byte)((Data[0x19] & 0x3F) | ((value & 0x3) << 6)); }
        public override int Move4_PPUps { get => (Data[0x1A] & 0xC0) >> 6; set => Data[0x1A] = (byte)((Data[0x1A] & 0x3F) | ((value & 0x3) << 6)); }
        public override int CurrentFriendship { get => Data[0x1B]; set => Data[0x1B] = (byte)value; }
        private byte PKRS { get => Data[0x1C]; set => Data[0x1C] = value; }
        public override int PKRS_Days { get => PKRS & 0xF; set => PKRS = (byte)(PKRS & ~0xF | value); }
        public override int PKRS_Strain { get => PKRS >> 4; set => PKRS = (byte)(PKRS & 0xF | value << 4); }
        // Crystal only Caught Data
        private int CaughtData { get => BigEndian.ToUInt16(Data, 0x1D); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x1D); }
        public int Met_TimeOfDay { get => (CaughtData >> 14) & 0x3; set => CaughtData = (CaughtData & 0x3FFF) | ((value & 0x3) << 14); }
        public override int Met_Level { get => (CaughtData >> 8) & 0x3F; set => CaughtData = (CaughtData & 0xC0FF) | ((value & 0x3F) << 8); }
        public override int OT_Gender { get => (CaughtData >> 7) & 1; set => CaughtData = (CaughtData & 0xFF7F) | ((value & 1) << 7); }
        public override int Met_Location { get => CaughtData & 0x7F; set => CaughtData = (CaughtData & 0xFF80) | (value & 0x7F); }
        
        public override int Stat_Level
        {
            get => Data[0x1F];
            set => Data[0x1F] = (byte)value;
        }

        #endregion

        #region Party Attributes
        public int Status_Condition { get => Data[0x20]; set => Data[0x20] = (byte)value; }

        public override int Stat_HPCurrent { get => BigEndian.ToUInt16(Data, 0x22); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x22); }
        public override int Stat_HPMax { get => BigEndian.ToUInt16(Data, 0x24); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x24); }
        public override int Stat_ATK { get => BigEndian.ToUInt16(Data, 0x26); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x26); }
        public override int Stat_DEF { get => BigEndian.ToUInt16(Data, 0x28); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x28); }
        public override int Stat_SPE { get => BigEndian.ToUInt16(Data, 0x2A); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x2A); }
        public override int Stat_SPA { get => BigEndian.ToUInt16(Data, 0x2C); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x2C); }
        public override int Stat_SPD { get => BigEndian.ToUInt16(Data, 0x2E); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x2E); }
        #endregion

        public override int GetMovePP(int move, int ppup) => Math.Min(61, base.GetMovePP(move, ppup));
        public override ushort[] GetStats(PersonalInfo p)
        {
            ushort[] Stats = new ushort[6];
            for (int i = 0; i < Stats.Length; i++)
            {
                ushort L = (ushort)Stat_Level;
                ushort B = (ushort)p.Stats[i];
                ushort I = (ushort)IVs[i];
                ushort E = // Fixed formula via http://www.smogon.com/ingame/guides/rby_gsc_stats
                    (ushort)Math.Floor(Math.Min(255, Math.Floor(Math.Sqrt(Math.Max(0, EVs[i] - 1)) + 1)) / 4.0);
                Stats[i] = (ushort)Math.Floor((2 * (B + I) + E) * L / 100.0 + 5);
            }
            Stats[0] += (ushort)(5 + Stat_Level); // HP

            return Stats;
        }

        public override bool IsEgg { get; set; }

        public override int Gender
        {
            get
            {
                int gv = PersonalInfo.Gender;
                if (gv == 255)
                    return 2;
                if (gv == 254)
                    return 1;
                if (gv == 0)
                    return 0;
                switch (gv)
                {
                    case 31:
                        return IV_ATK >= 2 ? 0 : 1;
                    case 63:
                        return IV_ATK >= 5 ? 0 : 1;
                    case 127:
                        return IV_ATK >= 8 ? 0 : 1;
                    case 191:
                        return IV_ATK >= 12 ? 0 : 1;
                }
                Debug.WriteLine($"Unknown Gender value: {gv}");
                return 0;
            }
            set { }
        }
        
        public override bool HasOriginalMetLocation => CaughtData != 0;

        #region Future, Unused Attributes
        public override bool IsGenderValid() => true; // not a separate property, derived via IVs
        public override uint EncryptionConstant { get => 0; set { } }
        public override uint PID { get => 0; set { } }
        public override int Nature { get => 0; set { } }

        public override int AltForm
        {
            get
            {
                if (Species != 201) // Unown
                    return 0;

                uint formeVal = 0;
                formeVal |= (uint)((IV_ATK & 0x6) << 5);
                formeVal |= (uint)((IV_DEF & 0x6) << 3);
                formeVal |= (uint)((IV_SPE & 0x6) << 1);
                formeVal |= (uint)((IV_SPC & 0x6) >> 1);
                return (int)(formeVal / 10);
            }
            set { }
        }

        private int HPVal => GetHiddenPowerBitVal(new[] {IV_SPC, IV_SPE, IV_DEF, IV_ATK});
        public override int HPPower => (5 * HPVal + IV_SPC % 4) / 2 + 31;
        public override int HPType
        {
            get => ((IV_ATK & 3) << 2) | (IV_DEF & 3); set
            {
                IV_DEF = ((IV_DEF >> 2) << 2) | (value & 3);
                IV_DEF = ((IV_ATK >> 2) << 2) | ((value >> 2) & 3);
            }
        }
        public override bool IsShiny => IV_DEF == 10 && IV_SPE == 10 && IV_SPC == 10 && (IV_ATK & 2) == 2;
        public override ushort Sanity { get => 0; set { } }
        public override bool ChecksumValid => true;
        public override ushort Checksum { get => 0; set { } }
        public override bool FatefulEncounter { get => false; set { } }
        public override int TSV => 0x0000;
        public override int PSV => 0xFFFF;
        public override int Characteristic => -1;
        public override int MarkValue { get => 0; protected set { } }
        public override int Ability { get => 0; set { } }
        public override int CurrentHandler { get => 0; set { } }
        public override int Egg_Location { get => 0; set { } }
        public override int OT_Friendship { get => 0; set { } }
        public override int Ball { get => 0; set { } }
        public override int Version { get => (int)GameVersion.GSC; set { } }
        public override int SID { get => 0; set { } }
        public override int CNT_Cool { get => 0; set { } }
        public override int CNT_Beauty { get => 0; set { } }
        public override int CNT_Cute { get => 0; set { } }
        public override int CNT_Smart { get => 0; set { } }
        public override int CNT_Tough { get => 0; set { } }
        public override int CNT_Sheen { get => 0; set { } }
        #endregion

        // Maximums
        public override int MaxMoveID => Legal.MaxMoveID_2;
        public override int MaxSpeciesID => Legal.MaxSpeciesID_2;
        public override int MaxAbilityID => Legal.MaxAbilityID_2;
        public override int MaxItemID => Legal.MaxItemID_2;
        public override int MaxBallID => -1;
        public override int MaxGameID => -1;
        public override int MaxIV => 15;
        public override int MaxEV => ushort.MaxValue;
        public override int OTLength => Japanese ? 5 : 7;
        public override int NickLength => Japanese ? 5 : 10;

        public PK1 ConvertToPK1()
        {
            PK1 pk1 = new PK1(null, Identifier, Japanese) {TradebackStatus = TradebackType.WasTradeback};
            Array.Copy(Data, 0x1, pk1.Data, 0x7, 0x1A);
            pk1.Species = Species; // This will take care of Typing :)
            pk1.Stat_HPCurrent = Stat_HPCurrent;
            pk1.Stat_Level = Stat_Level;
            // Status = 0
            Array.Copy(otname, 0, pk1.otname, 0, otname.Length);
            Array.Copy(nick, 0, pk1.nick, 0, nick.Length);

            int[] newMoves = pk1.Moves;
            for (int i = 0; i < 4; i++)
                if (newMoves[i] > 165) // not present in Gen 1
                    newMoves[i] = 0;
            pk1.Moves = newMoves;
            pk1.FixMoves();

            return pk1;
        }

        public PK7 ConvertToPK7()
        {
            var pk7 = new PK7
            {
                EncryptionConstant = Util.Rand32(),
                Species = Species,
                TID = TID,
                CurrentLevel = CurrentLevel,
                EXP = EXP,
                Met_Level = CurrentLevel,
                Nature = (int)(EXP % 25),
                PID = Util.Rand32(),
                Ball = 4,
                MetDate = DateTime.Now,
                Version = (int)GameVersion.SV,
                Move1 = Move1,
                Move2 = Move2,
                Move3 = Move3,
                Move4 = Move4,
                Move1_PPUps = Move1_PPUps,
                Move2_PPUps = Move2_PPUps,
                Move3_PPUps = Move3_PPUps,
                Move4_PPUps = Move4_PPUps,
                Move1_PP = Move1_PP,
                Move2_PP = Move2_PP,
                Move3_PP = Move3_PP,
                Move4_PP = Move4_PP,
                Met_Location = 30004,
                Gender = Gender,
                IsNicknamed = false,
                AltForm = AltForm,

                Country = PKMConverter.Country,
                Region = PKMConverter.Region,
                ConsoleRegion = PKMConverter.ConsoleRegion,
                CurrentHandler = 1,
                HT_Name = PKMConverter.OT_Name,
                HT_Gender = PKMConverter.OT_Gender,
                Geo1_Country = PKMConverter.Country,
                Geo1_Region = PKMConverter.Region
            };
            pk7.Language = GuessedLanguage(PKMConverter.Language);
            pk7.Nickname = PKX.GetSpeciesNameGeneration(pk7.Species, pk7.Language, pk7.Format);
            if (otname[0] == 0x5D) // Ingame Trade
            {
                var s = StringConverter.GetG1Char(0x5D, Japanese);
                pk7.OT_Name = s.Substring(0, 1) + s.Substring(1).ToLower();
            }
            pk7.OT_Friendship = pk7.HT_Friendship = PersonalTable.SM[Species].BaseFriendship;

            // IVs
            var special = Species == 151 || Species == 251;
            var new_ivs = new int[6];
            int flawless = special ? 5 : 3;
            for (var i = 0; i < new_ivs.Length; i++) new_ivs[i] = (int)(Util.Rand32() & 31);
            for (var i = 0; i < flawless; i++) new_ivs[i] = 31;
            Util.Shuffle(new_ivs);
            pk7.IVs = new_ivs;

            // Really? :(
            if (IsShiny)
                pk7.SetShinyPID();

            int abil = 2; // Hidden
            if (Legal.TransferSpeciesDefaultAbility_2.Contains(Species))
                abil = 0; // Reset
            pk7.RefreshAbility(abil); // 0/1/2 (not 1/2/4)

            if (special)
                pk7.FatefulEncounter = true;
            else if (IsNicknamedBank)
            {
                pk7.IsNicknamed = true;
                pk7.Nickname = Korean ? Nickname 
                    : StringConverter.GetG1ConvertedString(nick, Japanese);
            }
            pk7.OT_Name = Korean ? OT_Name
                : StringConverter.GetG1ConvertedString(otname, Japanese);

            pk7.TradeMemory(Bank: true); // oh no, memories on gen7 pkm

            pk7.RefreshChecksum();
            return pk7;
        }
    }

    public class PokemonList2
    {
        private const int CAPACITY_DAYCARE = 1;
        private const int CAPACITY_PARTY = 6;
        private const int CAPACITY_STORED = 20;
        private const int CAPACITY_STORED_JP = 30;

        private readonly bool Japanese;

        private int StringLength => Japanese ? PK2.STRLEN_J : PK2.STRLEN_U;

        public enum CapacityType
        {
            Daycare = CAPACITY_DAYCARE,
            Party = CAPACITY_PARTY,
            Stored = CAPACITY_STORED,
            StoredJP = CAPACITY_STORED_JP,
            Single
        }

        private static int GetEntrySize(CapacityType c) => c == CapacityType.Single || c == CapacityType.Party
            ? PKX.SIZE_2PARTY
            : PKX.SIZE_2STORED;

        private static byte GetCapacity(CapacityType c) => c == CapacityType.Single ? (byte)1 : (byte)c;

        private static byte[] GetEmptyList(CapacityType c, bool is_JP = false)
        {
            int cap = GetCapacity(c);
            return new[] { (byte)0 }.Concat(Enumerable.Repeat((byte)0xFF, cap + 1)).Concat(Enumerable.Repeat((byte)0, GetEntrySize(c) * cap)).Concat(Enumerable.Repeat((byte)0x50, (is_JP ? PK2.STRLEN_J : PK2.STRLEN_U) * 2 * cap)).ToArray();
        }

        public PokemonList2(byte[] d, CapacityType c = CapacityType.Single, bool jp = false)
        {
            Japanese = jp;
            Data = d ?? GetEmptyList(c, Japanese);
            Capacity = GetCapacity(c);
            Entry_Size = GetEntrySize(c);

            if (Data.Length != DataSize)
            {
                Array.Resize(ref Data, DataSize);
            }

            Pokemon = new PK2[Capacity];
            for (int i = 0; i < Capacity; i++)
            {
                int base_ofs = 2 + Capacity;
                byte[] dat = new byte[Entry_Size];
                byte[] otname = new byte[StringLength];
                byte[] nick = new byte[StringLength];
                Buffer.BlockCopy(Data, base_ofs + Entry_Size * i, dat, 0, Entry_Size);
                Buffer.BlockCopy(Data, base_ofs + Capacity * Entry_Size + StringLength * i, otname, 0, StringLength);
                Buffer.BlockCopy(Data, base_ofs + Capacity * Entry_Size + StringLength * (i + Capacity), nick, 0, StringLength);
                Pokemon[i] = new PK2(dat, null, jp) {IsEgg = Data[1 + i] == 0xFD, otname = otname, nick = nick};
            }
        }

        public PokemonList2(CapacityType c = CapacityType.Single, bool jp = false)
            : this(null, c, jp) => Count = 1;

        public PokemonList2(PK2 pk)
            : this(CapacityType.Single, pk.Japanese)
        {
            this[0] = pk;
            Count = 1;
        }

        private readonly byte[] Data;
        private readonly byte Capacity;
        private readonly int Entry_Size;

        public byte Count
        {
            get => Data[0];
            set => Data[0] = value > Capacity ? Capacity : value;
        }

        public readonly PK2[] Pokemon;

        public PK2 this[int i]
        {
            get
            {
                if (i > Capacity || i < 0) throw new ArgumentOutOfRangeException($"Invalid PokemonList Access: {i}");
                return Pokemon[i];
            }
            set
            {
                if (value == null) return;
                Pokemon[i] = (PK2)value.Clone();
            }
        }

        private void Update()
        {
            int count = Array.FindIndex(Pokemon, pk => pk.Species == 0);
            Count = count < 0 ? Capacity : (byte)count;
            for (int i = 0; i < Count; i++)
            {
                Data[1 + i] = Pokemon[i].IsEgg ? (byte)0xFD : (byte)Pokemon[i].Species;
                Array.Copy(Pokemon[i].Data, 0, Data, 2 + Capacity + Entry_Size * i, Entry_Size);
                Array.Copy(Pokemon[i].OT_Name_Raw, 0, Data, 2 + Capacity + Capacity * Entry_Size + StringLength * i, StringLength);
                Array.Copy(Pokemon[i].Nickname_Raw, 0, Data, 2 + Capacity + Capacity * Entry_Size + StringLength * Capacity + StringLength * i, StringLength);
            }
            Data[1 + Count] = byte.MaxValue;
        }

        public byte[] GetBytes()
        {
            Update();
            return Data;
        }

        private int DataSize => Capacity * (Entry_Size + 1 + 2 * StringLength) + 2;
        public static int GetDataLength(CapacityType c, bool jp = false) => GetCapacity(c) * (GetEntrySize(c) + 1 + 2 * (jp ? PK2.STRLEN_J : PK2.STRLEN_U)) + 2;
    }
}
