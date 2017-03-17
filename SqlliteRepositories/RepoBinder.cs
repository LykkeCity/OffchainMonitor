using Autofac;
using Core.Repositories.Settings;
using Core.Repositories.Transactions;
using SqlliteRepositories.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SqlliteRepositories
{
    public static class RepoBinder
    {
        public static void BindSqllite(this ContainerBuilder ioc)
        {
            ioc.RegisterType<SettingsRepository>().As<ISettingsRepository>();
            ioc.RegisterType<DummyBroadcastedTransactionRepository>().As<IBroadcastedTransactionRepository>();
        }
    }
}
