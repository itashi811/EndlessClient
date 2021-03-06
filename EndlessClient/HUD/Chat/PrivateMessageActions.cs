﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Panels;
using EOLib.Domain.Chat;

namespace EndlessClient.HUD.Chat
{
    [MappedType(BaseType = typeof(IPrivateMessageActions))]
    public class PrivateMessageActions : IPrivateMessageActions
    {
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IChatProvider _chatProvider;
        private readonly IChatTypeCalculator _chatTypeCalculator;

        public PrivateMessageActions(IHudControlProvider hudControlProvider,
                                     IChatProvider chatProvider,
                                     IChatTypeCalculator chatTypeCalculator)
        {
            _hudControlProvider = hudControlProvider;
            _chatProvider = chatProvider;
            _chatTypeCalculator = chatTypeCalculator;
        }

        public string GetTargetCharacter(string localTypedText)
        {
            //todo: error status bar message if there is no text following a character name

            if (_chatTypeCalculator.CalculateChatType(localTypedText) != ChatType.PM)
                return "";

            if (CurrentTab == ChatTab.Private1)
                return _chatProvider.PMTarget1;

            if (CurrentTab == ChatTab.Private2)
                return _chatProvider.PMTarget2;

            var characterArray = localTypedText.Skip(1)
                                               .TakeWhile(x => x != ' ')
                                               .ToArray();
            var characterName = new string(characterArray);

            ChatPanel.TryStartNewPrivateChat(characterName);
            return characterName;
        }

        private ChatPanel ChatPanel => _hudControlProvider.GetComponent<ChatPanel>(HudControlIdentifier.ChatPanel);

        private ChatTab CurrentTab => ChatPanel.CurrentTab;
    }

    public interface IPrivateMessageActions
    {
        string GetTargetCharacter(string localTypedText);
    }
}