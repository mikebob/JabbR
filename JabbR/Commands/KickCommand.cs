﻿using System;
using JabbR.Models;

namespace JabbR.Commands
{
    [Command("kick", "Kick_CommandInfo", "user [room]", "user")]
    public class KickCommand : UserCommand
    {
        public override void Execute(CommandContext context, CallerContext callerContext, ChatUser callingUser, string[] args)
        {
            if (args.Length == 0)
            {
                throw new InvalidOperationException(LanguageResources.Kick_UserRequired);
            }

            string targetUserName = args[0];

            ChatUser targetUser = context.Repository.VerifyUser(targetUserName);

            string targetRoomName = args.Length > 1 ? args[1] : callerContext.RoomName;

            if (String.IsNullOrEmpty(targetRoomName))
            {
                throw new InvalidOperationException(LanguageResources.Kick_RoomRequired);
            }

            ChatRoom room = context.Repository.VerifyRoom(targetRoomName);

            context.Service.KickUser(callingUser, targetUser, room);

            context.NotificationService.KickUser(targetUser, room);

            context.Repository.CommitChanges();
        }
    }
}