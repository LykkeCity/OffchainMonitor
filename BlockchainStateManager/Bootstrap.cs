using Autofac;
using BlockchainStateManager.Helpers;
using BlockchainStateManager.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager
{
    public class Bootstrap
    {
        public static IContainer container = null;
        public static void Start()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<SettingsProvider>().As<ISettingsProvider>();
            builder.RegisterType<DaemonHelper>().As<IDaemonHelper>();
            builder.RegisterType<WalletBackendOffchainClient>().As<IOffchainClient>();
            builder.RegisterType<BitcoinDaemonTransactionBroadcaster>().As<ITransactionBroacaster>();

            container = builder.Build();
        }
    }
}
