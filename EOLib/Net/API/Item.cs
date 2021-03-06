﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Domain.Map;
using EOLib.IO;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    public struct LevelUpStats
    {
        private readonly int exp;
        private readonly byte level;
        private readonly short stat, skill, maxhp, maxtp, maxsp;

        public int Exp => exp;
        public byte Level => level;
        public short StatPoints => stat;
        public short SkillPoints => skill;
        public short MaxHP => maxhp;
        public short MaxTP => maxtp;
        public short MaxSP => maxsp;

        internal LevelUpStats(OldPacket pkt, bool includeExp)
        {
            //includeExp will be false when leveling up from NPC, true from EXPReward
            //NPC handler happens slightly differently
            exp = includeExp ? pkt.GetInt() : 0;
            level = pkt.GetChar();
            stat = pkt.GetShort();
            skill = pkt.GetShort();
            maxhp = pkt.GetShort();
            maxtp = pkt.GetShort();
            maxsp = pkt.GetShort();
        }
    }

    public struct ItemUseData
    {
        public struct CureCurseStats
        {
            private readonly short m_maxhp, m_maxtp;
            private readonly short m_str, m_int, m_wis, m_agi, m_con, m_cha;
            private readonly short m_mindam, m_maxdam, m_acc, m_evade, m_armor;

            public short MaxHP => m_maxhp;
            public short MaxTP => m_maxtp;
            public short Str => m_str;
            public short Int => m_int;
            public short Wis => m_wis;
            public short Agi => m_agi;
            public short Con => m_con;
            public short Cha => m_cha;
            public short MinDam => m_mindam;
            public short MaxDam => m_maxdam;
            public short Accuracy => m_acc;
            public short Evade => m_evade;
            public short Armor => m_armor;

            internal CureCurseStats(OldPacket pkt)
            {
                m_maxhp = pkt.GetShort();
                m_maxtp = pkt.GetShort();
                m_str = pkt.GetShort();
                m_int = pkt.GetShort();
                m_wis = pkt.GetShort();
                m_agi = pkt.GetShort();
                m_con = pkt.GetShort();
                m_cha = pkt.GetShort();
                m_mindam = pkt.GetShort();
                m_maxdam = pkt.GetShort();
                m_acc = pkt.GetShort();
                m_evade = pkt.GetShort();
                m_armor = pkt.GetShort();
            }
        }

        //in every packet
        private readonly ItemType type;
        private readonly short itemID;
        private readonly int characterAmount;
        private readonly byte weight, maxWeight;
        public ItemType Type => type;
        public short ItemID => itemID;
        public int CharacterAmount => characterAmount;
        public byte Weight => weight;
        public byte MaxWeight => maxWeight;

        //heal type
        private readonly int hpGain;
        private readonly short hp, tp;
        public int HPGain => hpGain;
        public short HP => hp;
        public short TP => tp;

        //hairdye type
        private readonly byte hairColor;
        public byte HairColor => hairColor;

        //effect potion type
        private readonly short effect;
        public short EffectID => effect;

        //curecurse type
        private readonly CureCurseStats? curecurse_stats;
        public CureCurseStats CureStats => curecurse_stats ?? new CureCurseStats();

        //expreward type
        private readonly LevelUpStats? expreward_stats;
        public LevelUpStats RewardStats => expreward_stats ?? new LevelUpStats();

        internal ItemUseData(OldPacket pkt)
        {
            type = (ItemType)pkt.GetChar();
            itemID = pkt.GetShort();
            characterAmount = pkt.GetInt();
            weight = pkt.GetChar();
            maxWeight = pkt.GetChar();

            hpGain = hp = tp = 0;
            hairColor = 0;
            effect = 0;

            curecurse_stats = null;
            expreward_stats = null;

            //format differs based on item type
            //(keeping this in order with how eoserv ITEM_USE handler is ordered
            switch (type)
            {
                case ItemType.Teleport: /*Warp packet handles the rest!*/ break;
                case ItemType.Heal:
                    {
                        hpGain = pkt.GetInt();
                        hp = pkt.GetShort();
                        tp = pkt.GetShort();
                    }
                    break;
                case ItemType.HairDye:
                    {
                        hairColor = pkt.GetChar();
                    }
                    break;
                case ItemType.Beer: /*No additional data*/ break;
                case ItemType.EffectPotion:
                    {
                        effect = pkt.GetShort();
                    }
                    break;
                case ItemType.CureCurse:
                    {
                        curecurse_stats = new CureCurseStats(pkt);
                    }
                    break;
                case ItemType.EXPReward:
                    {
                        //note: server packets may be incorrect at this point (src/handlers/Item.cpp) because of unused builder in eoserv
                        //note: server also sends an ITEM_ACCEPT packet to surrounding players on level-up?
                        expreward_stats = new LevelUpStats(pkt, true);
                    }
                    break;
            }
        }
    }

    public delegate void PlayerItemDropEvent(int characterAmount, byte weight, byte maxWeight, OldMapItem item);
    public delegate void RemoveMapItemEvent(short itemUID);
    public delegate void JunkItemEvent(short itemID, int numJunked, int numRemaining, byte weight, byte maxWeight);
    public delegate void UseItemEvent(ItemUseData data);
    public delegate void ItemChangeEvent(bool newItemObtained, short id, int amount, byte weight);

    partial class PacketAPI
    {
        /// <summary>
        /// Occurs when any player drops an item - if characterAmount == -1, this means the item was dropped by a player other than MainPlayer
        /// </summary>
        public event PlayerItemDropEvent OnDropItem;
        public event RemoveMapItemEvent OnRemoveItemFromMap;
        public event JunkItemEvent OnJunkItem;
        public event UseItemEvent OnUseItem;
        public event ItemChangeEvent OnItemChange;

        private void _createItemMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Drop), _handleItemDrop, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Add), _handleItemAdd, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Remove), _handleItemRemove, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Junk), _handleItemJunk, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Reply), _handleItemReply, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Obtain), _handleItemObtain, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Kick), _handleItemKick, true);
            //todo: handle ITEM_ACCEPT (ExpReward item type) (I think it shows the level up animation?)
        }

        /// <summary>
        /// Pick up the item with the specified UID
        /// </summary>
        public bool GetItem(short uid)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Item, PacketAction.Get);
            pkt.AddShort(uid);

            return m_client.SendPacket(pkt);
        }

        public bool DropItem(short id, int amount, byte x = 255, byte y = 255) //255 means use character's current location
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Item, PacketAction.Drop);
            pkt.AddShort(id);
            pkt.AddInt(amount);
            if (x == 255 && y == 255)
            {
                pkt.AddByte(x);
                pkt.AddByte(y);
            }
            else
            {
                pkt.AddChar(x);
                pkt.AddChar(y);
            }

            return m_client.SendPacket(pkt);
        }

        public bool JunkItem(short id, int amount)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Item, PacketAction.Junk);
            pkt.AddShort(id);
            pkt.AddInt(amount);

            return m_client.SendPacket(pkt);
        }

        public bool UseItem(short itemID)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Item, PacketAction.Use);
            pkt.AddShort(itemID);

            return m_client.SendPacket(pkt);
        }
        
        private void _handleItemDrop(OldPacket pkt)
        {
            if (OnDropItem == null) return;
            short _id = pkt.GetShort();
            int _amount = pkt.GetThree();
            int characterAmount = pkt.GetInt(); //amount remaining for the character
            OldMapItem item = new OldMapItem
            {
                ItemID = _id,
                Amount = _amount,
                UniqueID = pkt.GetShort(),
                X = pkt.GetChar(),
                Y = pkt.GetChar(),
                //turn off drop protection since main player dropped it
                DropTime = DateTime.Now.AddSeconds(-5),
                IsNPCDrop = false,
                OwningPlayerID = 0 //id of 0 means the currently logged in player owns it
            };
            byte characterWeight = pkt.GetChar(), characterMaxWeight = pkt.GetChar(); //character adjusted weights
            
            OnDropItem(characterAmount, characterWeight, characterMaxWeight, item);
        }

        private void _handleItemAdd(OldPacket pkt)
        {
            if (OnDropItem == null) return;
            OldMapItem item = new OldMapItem
            {
                ItemID = pkt.GetShort(),
                UniqueID = pkt.GetShort(),
                Amount = pkt.GetThree(),
                X = pkt.GetChar(),
                Y = pkt.GetChar(),
                DropTime = DateTime.Now,
                IsNPCDrop = false,
                OwningPlayerID = -1 //another player dropped. drop protection says "Item protected" w/o player name
            };
            OnDropItem(-1, 0, 0, item);
        }

        private void _handleItemRemove(OldPacket pkt)
        {
            if (OnRemoveItemFromMap != null)
                OnRemoveItemFromMap(pkt.GetShort());
        }

        private void _handleItemJunk(OldPacket pkt)
        {
            short id = pkt.GetShort();
            int amountRemoved = pkt.GetThree();//don't really care - just math it
            int amountRemaining = pkt.GetInt();
            byte weight = pkt.GetChar();
            byte maxWeight = pkt.GetChar();

            if (OnJunkItem != null)
                OnJunkItem(id, amountRemoved, amountRemaining, weight, maxWeight);
        }

        private void _handleItemReply(OldPacket pkt)
        {
            if (OnUseItem != null)
                OnUseItem(new ItemUseData(pkt));
        }

        private void _handleItemObtain(OldPacket pkt)
        {
            if (OnItemChange == null) return;

            short id = pkt.GetShort();
            int amount = pkt.GetThree();
            byte weight = pkt.GetChar();

            OnItemChange(true, id, amount, weight);
        }

        private void _handleItemKick(OldPacket pkt)
        {
            if (OnItemChange == null) return;

            short id = pkt.GetShort();
            int amount = pkt.GetThree();
            byte weight = pkt.GetChar();

            OnItemChange(false, id, amount, weight);
        }
    }
}
