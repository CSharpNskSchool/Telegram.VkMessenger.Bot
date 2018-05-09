﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommunicationModels.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkNet;
using VkNet.Model.RequestParams;

namespace VkConnector.Services
{
    public class UpdatesListener : IUpdatesListener
    {
        private const int LongPoolWait = 20;
        private const int LongPoolMode = 2;
        private const int LongPoolVersion = 2;

        public async Task StartListening(SubscriptionModel subscriptionModel)
        {
            var api = new VkApi();
            await api.AuthorizeAsync(new ApiAuthParams
            {
                AccessToken = subscriptionModel.User.AccessToken
            });

            Task.Factory.StartNew(async () => await NotifyNewUpdates(subscriptionModel, api));
        }

        public bool StopListening(SubscriptionModel subscriptionModel)
        {
            throw new NotImplementedException();
        }

        private async Task NotifyNewUpdates(SubscriptionModel subscriptionModel, VkApi api)
        {
            var client = new HttpClient();
            var longPollServer = api.Messages.GetLongPollServer();
            var ts = longPollServer.Ts;

            while (true)
            {
                var updateResponse = await client
                    .GetAsync(
                        $"https://{longPollServer.Server}?act=a_check&key={longPollServer.Key}&ts={ts}&wait={LongPoolWait}&mode={LongPoolMode}&version={LongPoolVersion}");
                var jsoned = await updateResponse.Content.ReadAsStringAsync();
                var updates = JsonConvert.DeserializeObject<JObject>(jsoned);

                var longPollHistory = await api.Messages.GetLongPollHistoryAsync(new MessagesGetLongPollHistoryParams
                {
                    Ts = ts
                });

                foreach (var message in longPollHistory.Messages)
                {
                    await SendToWebHook(
                            subscriptionModel.Url,
                            new RecievedMessage(
                                chatId: message.ChatId ?? -1,
                                sender: new ExternalUser(message.UserId ?? -1),
                                isIncoming: !message.Out ?? false,
                                body: new MessageBody(message.Body)));
                }

                ts = updates["ts"].ToObject<ulong>();
            }
        }

        private async Task SendToWebHook(Uri url, RecievedMessage message)
        {
            var client = new HttpClient();
            var toSend = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

            var a = await client.PostAsync(url, toSend);

            Console.WriteLine($"\r\n\r\nПолучено новое сообщние от {message.Sender.Id}: {message.Body.Text} \r\n\r\n");
            Console.WriteLine(url);
            Console.WriteLine($"Код ответа: {a.StatusCode}");
        }
    }
}