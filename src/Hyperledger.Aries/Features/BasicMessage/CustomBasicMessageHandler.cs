using System;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using Hyperledger.Aries.Extensions;
using Newtonsoft.Json.Linq;
using Hyperledger.Aries.Contracts;
using Newtonsoft.Json;
using Hyperledger.Indy.LedgerApi;
using IndyLedger = Hyperledger.Indy.LedgerApi.Ledger;

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
            var jObject = JObject.Parse(message.Content);
            if (jObject.ContainsKey("~CustomType"))
            {
                var customType = (string)jObject["~CustomType"];
                switch (customType)
                {
                    case "LedgerLookupDefinition":
                        {
                            var definitionID = (string)jObject["~DefinitionID"];
                            var req = await IndyLedger.BuildGetCredDefRequestAsync(null, definitionID);
                            var res = await IndyLedger.SubmitRequestAsync(await agentContext.Pool, req);
                            var resMessage = new BasicMessage
                            {
                                Content = res
                            };
                            return resMessage;
                        }
                    case "LedgerLookupRevocationRegistryDefinition":
                        {
                            var registryId = (string)jObject["~RegistryID"];
                            var req = await IndyLedger.BuildGetRevocRegDefRequestAsync(null, registryId);
                            var res = await IndyLedger.SubmitRequestAsync(await agentContext.Pool, req);
                            var resMessage = new BasicMessage
                            {
                                Content = res
                            };
                            return resMessage;
                        }
                    case "LedgerLookupSchema":
                        {
                            var schemaId = (string)jObject["~SchemaID"];
                            var req = await IndyLedger.BuildGetSchemaRequestAsync(null, schemaId);
                            var res = await IndyLedger.SubmitRequestAsync(await agentContext.Pool, req);
                            var resMessage = new BasicMessage
                            {
                                Content = res
                            };
                            return resMessage;
                        }
                    case "LedgerLookupRevocationRegistryDelta":
                        {
                            var revocationRegistryId = (string)jObject["~RevocationRegistryId"];
                            var from = (string)jObject["~From"];
                            var to = (string)jObject["~To"];
                            var req = await IndyLedger.BuildGetRevocRegDeltaRequestAsync(null, revocationRegistryId, Convert.ToInt64(from), Convert.ToInt64(to));
                            var res = await IndyLedger.SubmitRequestAsync(await agentContext.Pool, req);
                            var resMessage = new BasicMessage
                            {
                                Content = res
                            };
                            return resMessage;
                        }
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