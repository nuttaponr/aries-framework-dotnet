using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Extensions;
using Hyperledger.Aries.Features.Routing;
using Hyperledger.Aries.Routing;
using Hyperledger.Aries.Storage;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Hyperledger.Aries.Routing
{
    
    /// <summary>
    /// Mediator Forward Handler
    /// </summary>
    public class MediatorForwardHandler : MessageHandlerBase<ForwardMessage>
    {
        private readonly IWalletRecordService recordService;
        private readonly IWalletService walletService;
        private readonly IRoutingStore routingStore;
        private readonly IEventAggregator eventAggregator;
        private string inboxNotificationEndpoint;
        private string apiKey;
        private Boolean notifyEdgeClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediatorForwardHandler"/> class.
        /// </summary>
        /// <param name="recordService"></param>
        /// <param name="walletService"></param>
        /// <param name="routingStore"></param>
        /// <param name="eventAggregator"></param>
        public MediatorForwardHandler(
            IWalletRecordService recordService,
            IWalletService walletService,
            IRoutingStore routingStore,
            IEventAggregator eventAggregator)
        {
            this.recordService = recordService;
            this.walletService = walletService;
            this.routingStore = routingStore;
            this.eventAggregator = eventAggregator;
            
            if (Environment.GetEnvironmentVariable("Inbox_Notification_Endpoint") != null && Environment.GetEnvironmentVariable("Inbox_Notification_Endpoint").Length >0)
            {
                notifyEdgeClient = true;
                inboxNotificationEndpoint = Environment.GetEnvironmentVariable("Inbox_Notification_Endpoint");
                apiKey = Environment.GetEnvironmentVariable("Inbox_Notification_ApiKey");
            }
        }

        /// <inheritdoc />
        protected override async Task<AgentMessage> ProcessAsync(ForwardMessage message, IAgentContext agentContext, UnpackedMessageContext messageContext)
        {
            var inboxId = await routingStore.FindRouteAsync(message.To);
            var inboxRecord = await recordService.GetAsync<InboxRecord>(agentContext.Wallet, inboxId);

            var edgeWallet = await walletService.GetWalletAsync(inboxRecord.WalletConfiguration, inboxRecord.WalletCredentials);

            var inboxItemRecord = new InboxItemRecord { ItemData = message.Message.ToJson(), Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds() };
            await recordService.AddAsync(edgeWallet, inboxItemRecord);

            await NotifyEdge(inboxId);

            eventAggregator.Publish(new InboxItemEvent
            {
                InboxId = inboxId,
                ItemId = inboxItemRecord.Id
            });

            return null;
        }

        private async Task<Boolean> NotifyEdge(string inboxId)
        {
            if (notifyEdgeClient)
            {

                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                var client = new System.Net.Http.HttpClient(httpClientHandler);

                HttpContent content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("request_id", inboxId),
                    new KeyValuePair<string, string>("topic", "notification"),
                    new KeyValuePair<string, string>("body", "Message from mediator")
                });

                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                content.Headers.Add("x-api-key", apiKey);

                var response = await client.PostAsync(inboxNotificationEndpoint, content);
              

                if (response.IsSuccessStatusCode)
                {
                    string respContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Return from notify {respContent}");
                    return true;
                }
                else
                {
                    Console.WriteLine("Failed to notify.");
                    return false;
                }
            }
            return true;
        }
    }
}
