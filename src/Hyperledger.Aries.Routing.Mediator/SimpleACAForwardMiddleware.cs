using System;
using System.IO;
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

        public List<KeyValuePair<string, string>> acaList;

        public SimpleACAForwardMiddleware(
            ILogger<SimpleACAForwardMiddleware> logger,
            IEnumerable<IMessageDispatcher> messageDispatchers)
        {
            Logger = logger;
            MessageDispatchers = messageDispatchers;
            logger.LogDebug("******************* MyAgentMiddleWare ******************");
            logger.LogDebug("ACA_ROUTE = {0}", Environment.GetEnvironmentVariable("ACA_ROUTE"));
            logger.LogDebug("ACA_ENDPOINT = {0}", Environment.GetEnvironmentVariable("ACA_ENDPOINT"));
            logger.LogDebug("ACA_ENDPOINT_JSON = {0}", Path.GetFullPath(Environment.GetEnvironmentVariable("ACA_ENDPOINT_JSON")));

            using (StreamReader r = new StreamReader(Path.GetFullPath(Environment.GetEnvironmentVariable("ACA_ENDPOINT_JSON"))))
            {
                string json = r.ReadToEnd();

                this.acaList = new List<KeyValuePair<string, string>>();

                // Loading the JSON object
                JObject configObject = JObject.Parse(json);

                foreach (JProperty jProp in configObject.Properties())
                {
                    Console.WriteLine(jProp.Name + " : " + jProp.Value.ToString());
                    this.acaList.Add(new KeyValuePair<string, string>(jProp.Name, jProp.Value.ToString()));
                }
            }
        }

        public async Task OnMessageAsync(IAgentContext agentContext, UnpackedMessageContext messageContext)
        {
            if (messageContext.GetMessageType() == new ForwardMessage().Type)
            {
                ForwardMessage forwardMessage = messageContext.GetMessage<ForwardMessage>();

                //if (forwardMessage.To == Environment.GetEnvironmentVariable("ACA_ROUTE"))
                if (forwardMessage.To == this.acaList.FirstOrDefault(kvp => kvp.Key == forwardMessage.To).Key)
                {
                    //var uri = new Uri(Environment.GetEnvironmentVariable("ACA_ENDPOINT"));
                    var uri = new Uri(this.acaList.First(kvp => kvp.Key == forwardMessage.To).Value);

                    var dispatcher = GetDispatcher(uri.Scheme);

                    //byte[] wireMsg = Encoding.UTF8.GetBytes(forwardMessage.ToString());
                    await dispatcher.DispatchAsync(uri, new PackedMessageContext(forwardMessage.Message));
                }
            }
        }

        private IMessageDispatcher GetDispatcher(string scheme) => MessageDispatchers.FirstOrDefault(_ => _.TransportSchemes.Contains(scheme));

    }
}
