﻿using System;
using System.Threading.Tasks;
using AgentFramework.Core.Configuration.Options;
using AgentFramework.Core.Exceptions;
using AgentFramework.Core.Models;
using AgentFramework.Core.Models.Ledger;
using AgentFramework.Core.Models.Records;
using AgentFramework.Core.Models.Wallets;
using Hyperledger.Indy.WalletApi;

namespace AgentFramework.Core.Contracts
{
    /// <summary>
    /// Provisioning Service.
    /// </summary>
    public interface IProvisioningService
    {
        /// <summary>
        /// Returns the agent provisioning record. This is a single record that contains all
        /// agent configuration parameters.
        /// </summary>
        /// <param name="wallet">The wallet.</param>
        /// <exception cref="AgentFrameworkException">Throws with ErrorCode.RecordNotFound.</exception>
        /// <returns>The provisioning record.</returns>
        Task<ProvisioningRecord> GetProvisioningAsync(Wallet wallet);

        /// <summary>
        /// Creates a wallet and provisions a new agent with the default <see cref="AgentOptions" />
        /// </summary>
        /// <returns></returns>
        Task ProvisionAgentAsync();

        /// <summary>
        /// Creates a wallet and provisions a new agent with the specified <see cref="AgentOptions" />
        /// </summary>
        /// <returns></returns>
        Task ProvisionAgentAsync(AgentOptions agentOptions);

        /// <summary>
        /// Updates the agent endpoint information.
        /// </summary>
        /// <param name="wallet">The wallet.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <returns></returns>
        Task UpdateEndpointAsync(Wallet wallet, AgentEndpoint endpoint);

        /// <summary>
        /// Accepts the transaction author agreement
        /// </summary>
        /// <param name="wallet"></param>
        /// <param name="txnAuthorAgreement"></param>
        /// <returns></returns>
        Task AcceptTxnAuthorAgreementAsync(Wallet wallet, IndyTaa txnAuthorAgreement);
    }
}
