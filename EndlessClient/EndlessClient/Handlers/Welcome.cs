﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EOLib;

namespace EndlessClient.Handlers
{
	enum WelcomeReply : short
	{
		RequestGranted = 1, //response from welcome_request
		WelcomeMessage = 2, //response from welcome_message
	}

	public static class Welcome
	{
		//used to signal a response was received
		private static System.Threading.ManualResetEvent response = new System.Threading.ManualResetEvent(false);

		public static bool FirstTimePlayer { get; private set; }

		private static List<string> m_news;
		public static IList<string> News
		{
			get { return m_news.AsReadOnly(); }
		}

		//Sends WELCOME_REQUEST to server
		public static bool SelectCharacter(int charID)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.Connected)
				return false;

			response.Reset();

			Packet builder = new Packet(PacketFamily.Welcome, PacketAction.Request);
			builder.AddInt(charID);

			if (!client.SendPacket(builder))
				return false;

			if (!response.WaitOne(Constants.ResponseTimeout))
				return false;
			response.Reset();

			return true;
		}

		//Sends WELCOME_MSG to server
		public static bool WelcomeMessage(int charID)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.Connected)
				return false;

			response.Reset();

			Packet builder = new Packet(PacketFamily.Welcome, PacketAction.Message);
			builder.AddThree(0x00123456); //?
			builder.AddInt(charID);

			if (!client.SendPacket(builder))
				return false;

			if (!response.WaitOne(Constants.ResponseTimeout))
				return false;
			response.Reset();

			return true;
		}
		
		//this is the handler method registered in EOClient
		public static void WelcomeResponse(Packet pkt)
		{
			//set server response to the value in the packet
			WelcomeReply reply = (WelcomeReply)pkt.GetShort();
			Player main = World.Instance.MainPlayer;
			//returning from the switch block will prompt an error back to the user for connection timeout
			//use 'return' on errors instead of break (since 'response' is never set at the end of the method)
			switch(reply)
			{
				//clicking login was successful. gets files from server.
				//response for SelectCharacter method
				case WelcomeReply.RequestGranted:
					{
						short playerID = pkt.GetShort();
						if (playerID != main.PlayerID)
							return;
						main.SetPlayerID(playerID);

						if (!main.SetActiveCharacter(pkt.GetInt()))
							return;

						short mapID = pkt.GetShort();
						byte[] mapRid = pkt.GetBytes(4);
						int mapFileSize = pkt.GetThree();
						main.ActiveCharacter.CurrentMap = mapID;
						main.ActiveCharacter.MapIsPk = mapRid[0] == 0xFF && mapRid[1] == 0x01;
						World.Instance.CheckMap(mapID, mapRid, mapFileSize);

						byte[] eifRid = pkt.GetBytes(4);
						byte[] eifLen = pkt.GetBytes(2);
						World.Instance.CheckPub(InitFileType.Item, Packet.DecodeNumber(eifRid), (short)Packet.DecodeNumber(eifLen));

						byte[] enfRid = pkt.GetBytes(4);
						byte[] enfLen = pkt.GetBytes(2);
						World.Instance.CheckPub(InitFileType.Npc, Packet.DecodeNumber(enfRid), (short)Packet.DecodeNumber(enfLen));

						byte[] esfRid = pkt.GetBytes(4);
						byte[] esfLen = pkt.GetBytes(2);
						World.Instance.CheckPub(InitFileType.Spell, Packet.DecodeNumber(esfRid), (short)Packet.DecodeNumber(esfLen));

						byte[] ecfRid = pkt.GetBytes(4);
						byte[] ecfLen = pkt.GetBytes(2);
						World.Instance.CheckPub(InitFileType.Class, Packet.DecodeNumber(ecfRid), (short)Packet.DecodeNumber(ecfLen));

						main.ActiveCharacter.Name = pkt.GetBreakString();
						main.ActiveCharacter.Title = pkt.GetBreakString();
						main.ActiveCharacter.GuildName = pkt.GetBreakString();
						main.ActiveCharacter.GuildRankStr = pkt.GetBreakString();
						main.ActiveCharacter.Class = pkt.GetChar();
						main.ActiveCharacter.PaddedGuildTag = pkt.GetFixedString(3); //padded guild tag is 3 characters
						main.ActiveCharacter.AdminLevel = (AdminLevel)pkt.GetChar();

						main.ActiveCharacter.Stats = new CharStatData()
						{
							level = pkt.GetChar(),
							exp = pkt.GetInt(),
							usage = pkt.GetInt(),

							hp = pkt.GetShort(),
							maxhp = pkt.GetShort(),
							tp = pkt.GetShort(),
							maxtp = pkt.GetShort(),
							maxsp = pkt.GetShort(),

							statpoints = pkt.GetShort(),
							skillpoints = pkt.GetShort(),
							karma = pkt.GetShort(),
							mindam = pkt.GetShort(),
							maxdam = pkt.GetShort(),
							accuracy = pkt.GetShort(),
							evade = pkt.GetShort(),
							armor = pkt.GetShort(),
							disp_str = pkt.GetShort(),
							disp_int = pkt.GetShort(),
							disp_wis = pkt.GetShort(),
							disp_agi = pkt.GetShort(),
							disp_con = pkt.GetShort(),
							disp_cha = pkt.GetShort()
						};

						for (int i = 0; i < (int)EquipLocation.PAPERDOLL_MAX; ++i )
						{
							main.ActiveCharacter.PaperDoll[i] = pkt.GetShort();
						}

						main.ActiveCharacter.GuildRankNum = pkt.GetChar();
						World.Instance.JailMap = pkt.GetShort();
						pkt.Skip(12); //i think these can safely be skipped for the moment
						FirstTimePlayer = pkt.GetChar() == 2; //signal that the player should see the "first timer" message
						if (pkt.GetByte() != 255)
							return; //error, something is off.
					}
					break;
				case WelcomeReply.WelcomeMessage:
					{
						if (pkt.GetByte() != 255) return; //error, something is off.

						//get the server's news
						m_news = new List<string>();
						for (int i = 0; i < 9; ++i )
						{
							m_news.Add(pkt.GetBreakString());
						}
						//pkt.GetByte();

						main.ActiveCharacter.Weight = pkt.GetChar();
						main.ActiveCharacter.MaxWeight = pkt.GetChar();
						while(pkt.PeekByte() != 255)
						{
							main.ActiveCharacter.Inventory.Add(new InventoryItem() { id = pkt.GetShort(), amount = pkt.GetInt() });
						}
						pkt.GetByte();

						while(pkt.PeekByte() != 255)
						{
							main.ActiveCharacter.Spells.Add(new CharacterSpell { id = pkt.GetShort(), level = pkt.GetShort() });
						}
						pkt.GetByte();

						//Get data for other characters
						int numOtherCharacters = pkt.GetChar();
						if (pkt.GetByte() != 255) return;
						for(int i = 0; i < numOtherCharacters; ++i)
						{
							string charName = pkt.GetBreakString(); 
							short playerID = pkt.GetShort();
							EndlessClient.Character newGuy = new EndlessClient.Character(playerID, null);
							newGuy.CurrentMap = pkt.GetShort();
							newGuy.X = pkt.GetShort();
							newGuy.Y = pkt.GetShort();
							newGuy.RenderData.SetDirection((EODirection)pkt.GetChar());
							pkt.GetChar(); //6 ?
							newGuy.PaddedGuildTag = pkt.GetFixedString(3);

							byte level = pkt.GetChar();
							newGuy.RenderData.SetGender(pkt.GetChar());
							newGuy.RenderData.SetHairStyle(pkt.GetChar());
							newGuy.RenderData.SetHairColor(pkt.GetChar());
							newGuy.RenderData.SetRace(pkt.GetChar());

							short maxHP = pkt.GetShort(),
								hp = pkt.GetShort(),
								maxTP = pkt.GetShort(),
								tp = pkt.GetShort();

							newGuy.Stats = new CharStatData()
							{
								level = level,
								hp = hp,
								maxhp = maxHP,
								tp = tp,
								maxtp = maxTP
							};

							newGuy.RenderData.SetBoots(pkt.GetShort());
							pkt.Skip(3 * sizeof(short)); //other paperdoll data is 0'd out
							newGuy.RenderData.SetArmor(pkt.GetShort());
							pkt.Skip(sizeof(short));
							newGuy.RenderData.SetHat(pkt.GetShort());
							newGuy.RenderData.SetShield(pkt.GetShort());
							newGuy.RenderData.SetWeapon(pkt.GetShort());

							newGuy.RenderData.SetSitting((SitState)pkt.GetChar());
							newGuy.RenderData.SetHidden(pkt.GetChar() != 0);

							if (pkt.GetByte() != 255)
								return;

							World.Instance.ActiveMapRenderer.OtherPlayers.Add(newGuy);
						}

						//get data for any npcs
						while(pkt.PeekByte() != 255)
						{
							NPC newGuy = new NPC(pkt.GetChar(), pkt.GetShort(), pkt.GetChar(), pkt.GetChar(), (EODirection)pkt.GetChar());
							World.Instance.ActiveMapRenderer.NPCs.Add(newGuy);
						}
						pkt.GetByte();

						//get data for items on map
						while(pkt.ReadPos < pkt.Length)
						{
							MapItem newItem = new MapItem()
							{
								uid = pkt.GetShort(),
								id = pkt.GetShort(),
								x = pkt.GetChar(),
								y = pkt.GetChar(),
								amount = pkt.GetThree()
							};

							World.Instance.ActiveMapRenderer.MapItems.Add(newItem);
						}
					}
					break;
			}
			response.Set();
		}
		
		//classes that have a manualresetevent in them need to be cleaned up.
		//this call should be put in the dispose method of the EOClient class
		public static void Cleanup()
		{
			response.Dispose();
		}
	}
}