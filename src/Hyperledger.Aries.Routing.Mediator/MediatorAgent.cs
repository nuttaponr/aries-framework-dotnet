﻿using System;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.BasicMessage;

namespace Hyperledger.Aries.Routing
{
    public class DefaultMediatorAgent : AgentBase
    {
        public DefaultMediatorAgent(IServiceProvider provider) : base(provider)
        {
        }

        protected override void ConfigureHandlers()
        {
            AddConnectionHandler();
            AddHandler<MediatorForwardHandler>();
            AddHandler<DefaultStoreBackupHandler>();
            AddHandler<RetrieveBackupHandler>();
            AddHandler<RoutingInboxHandler>();
            AddHandler<CustomBasicMessageHandler>();
        }
    }
}
