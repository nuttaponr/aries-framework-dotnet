using System;
using System.Runtime.InteropServices;
using Hyperledger.Aries.Storage;
using Newtonsoft.Json;
using Hyperledger.Aries.Extensions;
namespace Hyperledger.Aries.Routing.Mediator.Storage
{
    public static class PostgresPluginLoader
    {
        static bool Loaded = false;

        [DllImport("indystrgpostgres", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int postgresstorage_init();

        [DllImport("indystrgpostgres", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int init_storagetype(string config, string credential);

        public static void LoadPostGressPlugin(WalletConfiguration config)
        {
            if (!Loaded)
            {
                Console.WriteLine(System.Environment.GetEnvironmentVariable("RUST_LOG"));
                Console.WriteLine("Initializing postgres wallet");
                var result = postgresstorage_init();
                if (result != 0 )
                {
                    Console.WriteLine("Error loading library : {0}", result);
                    throw new Exception("Error load library");
                }

                result = init_storagetype(config.StorageConfiguration.ToJson(), config.StorageCredential.ToJson());
                if (result != 0)
                {
                    Console.WriteLine("Error unable to configure postgres stg: {0}", result);
                    throw new Exception($"Error unable to configure postgres stg: { result }");
                }
            }
        }
    }
}
