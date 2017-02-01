﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Localization;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Chat
{
    public class PrivateMessageTargetNotFound : InGameOnlyPacketHandler
    {
        private const int TALK_NOTFOUND = 1;

        private readonly IChatRepository _chatRepository;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IEnumerable<IChatEventNotifier> _chatEventNotifiers;

        public override PacketFamily Family => PacketFamily.Talk;

        public override PacketAction Action => PacketAction.Reply;

        public PrivateMessageTargetNotFound(IPlayerInfoProvider playerInfoProvider,
                                            IChatRepository chatRepository,
                                            ILocalizedStringFinder localizedStringFinder,
                                            IEnumerable<IChatEventNotifier> chatEventNotifiers)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
            _localizedStringFinder = localizedStringFinder;
            _chatEventNotifiers = chatEventNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var response = packet.ReadShort();
            if (response != TALK_NOTFOUND)
                return false;

            var from = packet.ReadEndString();
            from = char.ToUpper(from[0]) + from.Substring(1).ToLower();
            var sysMessage = _localizedStringFinder.GetString(EOResourceID.SYS_CHAT_PM_PLAYER_COULD_NOT_BE_FOUND);
            var message = $"{@from} {sysMessage}";

            var chatData = new ChatData(string.Empty, message, ChatIcon.Error, ChatColor.Error);
            _chatRepository.AllChat[ChatTab.System].Add(chatData);

            foreach (var notifier in _chatEventNotifiers)
                notifier.NotifyPrivateMessageRecipientNotFound(from);

            return true;
        }
    }
}
