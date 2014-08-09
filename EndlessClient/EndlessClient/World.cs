﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using EOLib.Data;

namespace EndlessClient
{
	public class WorldLoadException : Exception
	{
		public WorldLoadException(string msg)
			: base(msg)
		{

		}
	}

	//singleton pattern: provides global access to data files and network connection
	//	without allowing for instantiation outside of the class or inheriting from it
	public sealed class World
	{
		/*** STATIC MEMBERS AND SUCH FOR THE SINGLETON PATTERN ***/
		private static World inst = null;
		private static readonly object locker = new object();

		public static bool Initialized { get; private set; }

		public static World Instance
		{
			get
			{
				lock (locker)
				{
					if (inst == null)
						inst = new World();
					return inst;
				}
			}
		}

		private World() //don't allow construction of the world using 'new'
		{
			TryLoadItems();
			TryLoadNPCs();
			TryLoadSpells();
			TryLoadClasses();
			
			//initial capacity of 32: most players won't travel between too many maps in a gaming session
			MapCache = new Dictionary<int, EOLib.MapFile>(32);
			m_player = new Player();
			m_client = new EOClient();
			World.Initialized = true;
		}

		/*** Instance Properties and such ***/

		public short JailMap { get; set; }

		//this is an int for the map id since there are multiple maps
		public int NeedMap { get; set; }
		public bool NeedEIF { get; set; }
		public bool NeedENF { get; set; }
		public bool NeedESF { get; set; }
		public bool NeedECF { get; set; }

		private ItemFile m_items;
		public ItemFile EIF
		{
			get { return m_items; }
		}

		private NPCFile m_npcs;
		public NPCFile ENF
		{
			get { return m_npcs; }
		}

		private SpellFile m_spells;
		public SpellFile ESF
		{
			get { return m_spells; }
		}

		private ClassFile m_classes;
		public ClassFile ECF
		{
			get { return m_classes; }
		}

		/// <summary>
		/// Stores a list of MapFiles paired with/accessible by their IDs
		/// </summary>
		public Dictionary<int, EOLib.MapFile> MapCache { get; private set; }

		/// <summary>
		/// Returns a MapFile for the map the MainPlayer is on
		/// </summary>
		public EOLib.MapFile ActiveMap
		{
			get
			{
				if (!MapCache.ContainsKey(MainPlayer.ActiveCharacter.CurrentMap))
					return null;
				return MapCache[MainPlayer.ActiveCharacter.CurrentMap];
			}
		}

		private EOMapRenderer m_mapRender = null;
		/// <summary>
		/// Returns a map rendering object encapsulating the map the MainPlayer is on
		/// </summary>
		public EOMapRenderer ActiveMapRenderer
		{
			get
			{
				if (m_mapRender == null || m_mapRender.MapRef.MapID != MainPlayer.ActiveCharacter.CurrentMap)
					m_mapRender = new EOMapRenderer(ActiveMap);
				return m_mapRender;
			}
		}

		/*** Other things that should be singleton ***/

		private readonly Player m_player;
		public Player MainPlayer
		{
			get { return m_player; }
		}
		
		private readonly AsyncClient m_client;
		public AsyncClient Client
		{
			get { return m_client; }
		}

		/*** Functions for loading/checking the different pub/map files ***/

		//tries to load the map that MainPlayer.ActiveCharacter is hanging out on
		public bool TryLoadMap(int mapID = -1)
		{
			try
			{
				if (mapID < 0)
					mapID = MainPlayer.ActiveCharacter.CurrentMap;

				string mapFile = Path.Combine("maps", string.Format("{0,5:D5}.emf", mapID));

				MapCache.Add(mapID, new EOLib.MapFile(mapFile));
			}
			catch
			{
				return false;
			}

			return true;
		}

		public bool TryLoadItems(string fileName = null)
		{
			try
			{
				if (string.IsNullOrEmpty(fileName))
					m_items = new ItemFile();
				else
					m_items = new ItemFile(fileName);
			}
			catch
			{
				m_items = null;
				return false;
			}

			return true;
		}

		public bool TryLoadNPCs(string fileName = null)
		{
			try
			{
				if (string.IsNullOrEmpty(fileName))
					m_npcs = new NPCFile();
				else
					m_npcs = new NPCFile(fileName);
			}
			catch
			{
				m_npcs = null;
				return false;
			}

			return true;
		}

		public bool TryLoadSpells(string fileName = null)
		{
			try
			{
				if (string.IsNullOrEmpty(fileName))
					m_spells = new SpellFile();
				else
					m_spells = new SpellFile(fileName);
			}
			catch
			{
				m_spells = null;
				return false;
			}

			return true;
		}

		public bool TryLoadClasses(string fileName = null)
		{
			try
			{
				if (string.IsNullOrEmpty(fileName))
					m_classes = new ClassFile();
				else
					m_classes = new ClassFile(fileName);
			}
			catch
			{
				m_classes = null;
				return false;
			}

			return true;
		}

		public void CheckMap(int mapID, byte[] mapRid, int mapFileSize)
		{
			NeedMap = -1;

			string mapFile = string.Format("maps\\{0,5:D5}.emf", mapID);
			if (!Directory.Exists("maps") || !File.Exists(mapFile))
			{
				Directory.CreateDirectory("maps");
				NeedMap = mapID;
				return;
			}

			//try to load the map if it isn't cached. on failure, set needmap
			if (!MapCache.ContainsKey(mapID))
				NeedMap = TryLoadMap(mapID) ? -1 : mapID;

			//on success of file load, check the rid and the size of the file
			if (MapCache.ContainsKey(mapID))
			{
				for (int i = 0; i < 4; ++i)
				{
					if(MapCache[mapID].Rid[i] != mapRid[i])
					{
						NeedMap = mapID;
						break;
					}
				}

				if (NeedMap == -1 && MapCache[mapID].FileSize != mapFileSize)
					NeedMap = mapID;
			}
		}

		public void CheckPub(Handlers.InitFileType file, int rid, short len)
		{
			string fName = "pub\\";
			if (!Directory.Exists(fName))
				Directory.CreateDirectory(fName);

			switch (file)
			{
				case Handlers.InitFileType.Item:
					NeedEIF = !TryLoadItems();
					if(EIF != null)
						NeedEIF = rid != EIF.Rid || len != EIF.Len;
					break;
				case Handlers.InitFileType.Npc:
					NeedENF = !TryLoadNPCs();
					if (ENF != null)
						NeedENF = rid != ENF.Rid || len != ENF.Len;
					break;
				case Handlers.InitFileType.Spell:
					NeedESF = !TryLoadSpells();
					if (ESF != null)
						NeedESF = rid != ESF.Rid || len != ESF.Len;
					break;
				case Handlers.InitFileType.Class:
					NeedECF = !TryLoadClasses();
					if (ECF != null)
						NeedECF = rid != ECF.Rid || len != ECF.Len;
					break;
				default:
					return;
			}
		}
	}
}