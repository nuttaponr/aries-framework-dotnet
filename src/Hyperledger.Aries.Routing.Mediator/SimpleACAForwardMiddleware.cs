using System;
using System.Text;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.Routing;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Hyperledger.Aries.Utils;
using Hyperledger.Indy.WalletApi;
using Microsoft.Extensions.Logging;
namespace Hyperledger.Aries.Routing.Mediator
{
    public class SimpleACAForwardMiddleware : IAgentMiddleware
    {

        private HttpMessageDispatcher messageDispatcher;

        protected readonly IEnumerable<IMessageDispatcher> MessageDispatchers;

        protected readonly ILogger<SimpleACAForwardMiddleware> Logger;

        public SimpleACAForwardMiddleware(
            ILogger<SimpleACAForwardMiddleware> logger,
            IEnumerable<IMessageDispatcher> messageDispatchers)
        {
            Logger = logger;
            MessageDispatchers = messageDispatchers;
            logger.LogDebug("******************* MyAgentMiddleWare ******************");
            logger.LogDebug("ACA_ROUTE = {0}", Environment.GetEnvironmentVariable("ACA_ROUTE"));
            logger.LogDebug("ACA_ENDPOINT = {0}", Environment.GetEnvironmentVariable("ACA_ENDPOINT"));
        }

        public async Task OnMessageAsync(IAgentContext agentContext, UnpackedMessageContext messageContext)
        {
            if (messageContext.GetMessageType() == new ForwardMessage().Type)
            {
                ForwardMessage forwardMessage = messageContext.GetMessage<ForwardMessage>();

                if (forwardMessage.To == Environment.GetEnvironmentVariable("ACA_ROUTE"))
                {
                    var uri = new Uri(Environment.GetEnvironmentVariable("ACA_ENDPOINT"));

                    var dispatcher = GetDispatcher(uri.Scheme);

                    //byte[] wireMsg = Encoding.UTF8.GetBytes(forwardMessage.ToString());
                    await dispatcher.DispatchAsync(uri, new PackedMessageContext(forwardMessage.Message));
                }
            }
        }

        private IMessageDispatcher GetDispatcher(string scheme) => MessageDispatchers.FirstOrDefault(_ => _.TransportSchemes.Contains(scheme));

    }
}
