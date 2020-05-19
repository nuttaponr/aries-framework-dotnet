using System;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using Hyperledger.Aries.Extensions;
using Newtonsoft.Json.Linq;
using Hyperledger.Aries.Contracts;

namespace Hyperledger.Aries.Features.BasicMessage
{
    /// <summary>
    /// Default basic message handler
    /// </summary>
    /// <seealso cref="MessageHandlerBase{BasicMessage}" />
    public class CustomBasicMessageHandler : MessageHandlerBase<BasicMessage>
    {
        private readonly IWalletRecordService _recordService;
        private readonly ILedgerService _ledgerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBasicMessageHandler"/> class.
        /// </summary>
        /// <param name="recordService">The record service.</param>
        public CustomBasicMessageHandler(IWalletRecordService recordService, ILedgerService ledgerService)
        {
            _recordService = recordService;
            _ledgerService = ledgerService;
        }

        /// <summary>
        /// Processes the incoming <see cref="AgentMessage" />
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="agentContext">The message agentContext.</param>
        /// <param name="messageContext">The message context.</param>
        /// <returns></returns>
        protected override async Task<AgentMessage> ProcessAsync(BasicMessage message, IAgentContext agentContext, UnpackedMessageContext messageContext)
        {
            Console.WriteLine("ProcessAsync of CustomBasicMessageHandler");

            var jObject = JObject.Parse(message.Content);
            if ( jObject.ContainsKey("~CustomType") ) {
                var customType = (string)jObject["~CustomType"];
                switch (customType)
                {
                    case "LedgerLookupDefinition":
                        var definitionID = (string)jObject["DefinitionID"];
                        var res = await _ledgerService.LookupDefinitionAsync(agentContext, definitionID);
                        var resMessage = new BasicMessage
                        {
                            Content = res.ObjectJson
                        };
                        return resMessage;
                }
            }
            
            var record = new BasicMessageRecord
            {
                Id = Guid.NewGuid().ToString(),
                ConnectionId = messageContext.Connection.Id,
                Text = message.Content,
                SentTime = DateTime.TryParse(message.SentTime, out var dateTime) ? dateTime : DateTime.UtcNow,
                Direction = MessageDirection.Incoming
            };
            await _recordService.AddAsync(agentContext.Wallet, record);
            messageContext.ContextRecord = record;

            return null;
        }
    }
}