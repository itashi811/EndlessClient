﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Net
{
	public enum PacketFamily : byte
	{
		Internal = 0,
		Connection = (byte)1,
		Account = (byte)2,
		Character = (byte)3,
		Login = (byte)4,
		Welcome = (byte)5,
		Walk = (byte)6,
		Face = (byte)7,
		Chair = (byte)8,
		Emote = (byte)9,
		Attack = (byte)11,
		Spell = (byte)12,
		Shop = (byte)13,
		Item = (byte)14,
		StatSkill = (byte)16,
		Global = (byte)17,
		Talk = (byte)18,
		Warp = (byte)19,
		JukeBox = (byte)21,
		Players = (byte)22,
		Avatar = (byte)23,
		Party = (byte)24,
		Refresh = (byte)25,
		NPC = (byte)26,
		AutoRefresh = (byte)27,
		AutoRefresh2 = (byte)28,
		Appear = (byte)29,
		PaperDoll = (byte)30,
		Effect = (byte)31,
		Trade = (byte)32,
		Chest = (byte)33,
		Door = (byte)34,
		Message = (byte)35,
		Bank = (byte)36,
		Locker = (byte)37,
		Barber = (byte)38,
		Guild = (byte)39,
		Music = (byte)40,
		Sit = (byte)41,
		Recover = (byte)42,
		Board = (byte)43,
		Cast = (byte)44,
		Arena = (byte)45,
		Priest = (byte)46,
		Marriage = (byte)47,
		AdminInteract = (byte)48,
		Citizen = (byte)49,
		Quest = (byte)50,
		Book = (byte)51,
		Init = (byte)255
	}
}