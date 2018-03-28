﻿using System.Threading.Tasks;
using MessageTransferBot.Data.Entities;
using MessageTransferBot.Data.Repositories;
using MessageTransferBot.Services.BotCommands;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MessageTransferBot.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly IBotService _botService;
        private readonly IBotUserRepository _botUserRepository;
        private readonly ILogger<UpdateService> _logger;

        public UpdateService(IBotService botService, IBotUserRepository botUserRepository,
            ILogger<UpdateService> logger)
        {
            _botService = botService;
            _botUserRepository = botUserRepository;
            _logger = logger;
        }

        public async Task Update(Update update)
        {
            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;
                var botUser = GetRequestingUserOrRegisterNew(message.From.Id);

                if (message.Type == MessageType.Text && message.Text.StartsWith("/"))
                {
                    _logger.LogInformation($"Получена команда из диалога {message.Chat.Id}");

                    await BotCommandsExecutor.HandleComands(_botService, botUser, message);
                }
                else
                {
                    _logger.LogInformation($"Получено сообщение из диалога {message.Chat.Id}");

                    await MessageTransfer.TransferToVk(message, botUser, _botService);
                }
            }
        }

        private BotUser GetRequestingUserOrRegisterNew(int telegramId)
        {
            var botUser = _botUserRepository.GetByTelegramId(telegramId);
            if (botUser == null)
            {
                botUser = new BotUser
                {
                    TelegramId = telegramId
                };

                _botUserRepository.Add(botUser);
            }

            return botUser;
        }
    }
}