using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Routing.Mediator;
using Hyperledger.Aries.Features.BasicMessage;
using Hyperledger.Aries.Routing.Mediator.Storage;

namespace MediatorAgentService
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAriesFramework(builder =>
            {
                builder.RegisterMediatorAgent(options =>
                {
                    #region Required configuration parameters
                    // Agent endpoint. Use fully qualified endpoint.
                    options.EndpointUri = System.Environment.GetEnvironmentVariable("Agent_EndpointUri");
                    // The path to the genesis transaction file.
                    options.GenesisFilename = Path.GetFullPath(System.Environment.GetEnvironmentVariable("Agent_GenesisFilename"));
                    #endregion

                    #region Optional configuration parameters
                    // The identifier of the wallet
                    options.WalletConfiguration.Id = System.Environment.GetEnvironmentVariable("Agent_WalletConfiguration_Id");

                    if  (System.Environment.GetEnvironmentVariable("StorageType") == "postgres" )
                    {
                        options.WalletConfiguration.StorageType = "postgres_storage";
                        options.WalletConfiguration.StorageConfiguration.Url = System.Environment.GetEnvironmentVariable("Agent_StorageConfiguration_Url");
                        options.WalletConfiguration.StorageConfiguration.WalletScheme = System.Environment.GetEnvironmentVariable("WalletScheme"); // "MultiWalletSingleTable", "MultiWalletMultiTable"
                        options.WalletConfiguration.StorageConfiguration.Tls = System.Environment.GetEnvironmentVariable("Agent_StorageConfiguration_Tls");
                        options.WalletConfiguration.StorageConfiguration.MaxConnections = Int32.Parse(System.Environment.GetEnvironmentVariable("Agent_StorageConfiguration_MaxConnections"));

                        options.WalletConfiguration.StorageCredential.Account = System.Environment.GetEnvironmentVariable("Agent_StorageConfiguration_Account");
                        options.WalletConfiguration.StorageCredential.Password = System.Environment.GetEnvironmentVariable("Agent_StorageConfiguration_Password");
                        options.WalletConfiguration.StorageCredential.AdminAccount = System.Environment.GetEnvironmentVariable("Agent_StorageConfiguration_AdminAccount");
                        options.WalletConfiguration.StorageCredential.AdminPassword = System.Environment.GetEnvironmentVariable("Agent_StorageConfiguration_AdminPassword");
                    }

                    // Secret key used to open the wallet.
                    options.WalletCredentials.Key = System.Environment.GetEnvironmentVariable("Agent_WalletCredentials_Key");
                    options.WalletCredentials.StorageCredentials = options.WalletConfiguration.StorageCredential;


                    if  (System.Environment.GetEnvironmentVariable("StorageType") == "postgres" ) {
                        PostgresPluginLoader.LoadPostGressPlugin(options.WalletConfiguration);
                    }

                    #endregion
                });
                builder.Services.AddSingleton<IAgentMiddleware, SimpleACAForwardMiddleware>();
                builder.Services.AddMessageHandler<CustomBasicMessageHandler>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAriesFramework();
            app.UseMediatorDiscovery();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
