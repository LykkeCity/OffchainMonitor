using Autofac;
using Autofac.Features.ResolveAnything;
using Common;
using Common.Log;
using Core.Bitcoin;
using Core.Settings;
using LkeServices;
using SqlliteRepositories;
using SqlliteRepositories.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OffchainMonitorApi.Binder
{
    public class SqlliteBinder
    {
        public ContainerBuilder Bind(BaseSettings settings)
        {
            var logToTable = new LogToTable();
            var log = new LogToTableAndConsole(logToTable, new LogToConsole());
            var ioc = new ContainerBuilder();
            InitContainer(ioc, settings, log);
            return ioc;
        }

        private void InitContainer(ContainerBuilder ioc, BaseSettings settings, ILog log)
        {
#if DEBUG
            log.WriteInfoAsync("BitcoinApi", "App start", null, $"BaseSettings : {settings.ToJson()}").Wait();
#else
            log.WriteInfoAsync("BitcoinApi", "App start", null, $"BaseSettings : private").Wait();
#endif

            ioc.RegisterInstance(log);
            ioc.RegisterInstance(settings);
            ioc.RegisterInstance(new RpcConnectionParams(settings));

            ioc.BindCommonServices();
            ioc.BindSqllite();

            // ioc.RegisterType<RetryFailedTransactionService>().As<IRetryFailedTransactionService>();

            //ioc.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
        }
    }
}
