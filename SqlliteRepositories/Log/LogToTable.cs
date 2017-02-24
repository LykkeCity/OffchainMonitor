using System;
using System.Threading.Tasks;
using AzureStorage;
using Common.Log;
using Microsoft.WindowsAzure.Storage.Table;
using SqlliteRepositories.Model;

namespace SqlliteRepositories.Log
{
    public class LogToTable : ILog
    {
        /*
        private readonly INoSQLTableStorage<LogEntity> _tableStorageError;
        private readonly INoSQLTableStorage<LogEntity> _tableStorageWarning;
        private readonly INoSQLTableStorage<LogEntity> _tableStorageInfo;

        public LogToTable(INoSQLTableStorage<LogEntity> tableStorageError, INoSQLTableStorage<LogEntity> tableStorageWarning, INoSQLTableStorage<LogEntity> tableStorageInfo)
        {
            _tableStorageError = tableStorageError;
            _tableStorageInfo = tableStorageInfo;
            _tableStorageWarning = tableStorageWarning;
        }
        */

        private async Task Insert(string level, string component, string process, string context, string type, string stack,
            string msg, DateTime? dateTime)
        {
            var dt = dateTime ?? DateTime.UtcNow;
            var newEntity = new LogEntity
            {
                Level = level,
                Component = component,
                Context = context,
                DateTime = dt,
                Msg = msg,
                Process = process,
                Stack = stack,
                Type = type
            };

            using (OffchainMonitorContext dbContext = new OffchainMonitorContext())
            {
                dbContext.Add(newEntity);
            }
        }

        public Task WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            return Insert("info", component, process, context, null, null, info, dateTime);
        }

        public Task WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            return Insert("warning", component, process, context, null, null, info, dateTime);
        }

        public Task WriteErrorAsync(string component, string process, string context, Exception type, DateTime? dateTime = null)
        {
            return Insert("error", component, process, context, type.GetType().ToString(), type.StackTrace, type.Message, dateTime);
        }

        public Task WriteFatalErrorAsync(string component, string process, string context, Exception type, DateTime? dateTime = null)
        {
            return Insert("fatalerror", component, process, context, type.GetType().ToString(), type.StackTrace, type.Message, dateTime);
        }

        public int Count { get { return 0; } }
    }
}
