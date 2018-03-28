﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MessageTransferBot.Data.Entities;

namespace MessageTransferBot.Data.Repositories
{
    public class BotUserRepository : IBotUserRepository
    {
        private static readonly ConcurrentDictionary<int, BotUser> users;

        static BotUserRepository()
        {
            users = new ConcurrentDictionary<int, BotUser>();
        }

        public IEnumerable<BotUser> Users => users.Values;

        public BotUser Get(int id)
        {
            users.TryGetValue(id, out var botUser);
            return botUser;
        }

        public BotUser GetByTelegramId(int telegramId)
        {
            var botUser = users.Values.FirstOrDefault(user => user.TelegramId == telegramId);

            return botUser;
        }

        public void Add(BotUser botUser)
        {
            users.TryAdd(botUser.TelegramId, botUser);
        }
    }
}