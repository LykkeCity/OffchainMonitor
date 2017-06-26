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

            builder.RegisterType<SettingsProvider>().InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies).As<ISettingsProvider>();
            builder.RegisterType<DaemonHelper>().InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies).As<IDaemonHelper>();
            builder.RegisterType<BitcoinDaemonTransactionBroadcaster>().InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies).As<ITransactionBroacaster>();
            builder.RegisterType<QBitNinjaHelper>().InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies).As<IBlockchainExplorerHelper>();
            builder.RegisterType<FeeManager>().InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies).As<IFeeManager>();

            container = builder.Build();
        }
    }
}
