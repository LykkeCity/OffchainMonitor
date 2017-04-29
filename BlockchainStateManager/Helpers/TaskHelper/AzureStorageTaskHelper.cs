using BlockchainStateManager.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Helpers.TaskHelper
{
    public class AzureStorageTaskHelper
    {
        ISettingsProvider settingsProvider = null;

        public AzureStorageTaskHelper(ISettingsProvider _settingsProvider)
        {
            settingsProvider = _settingsProvider;
        }

        public bool ClearAzureTables()
        {
            var settings = settingsProvider;

            var commandName = settings.GetSettings().AzureStorageEmulatorPath + "AzureStorageEmulator";
            var commandParams = "clear table";

            return ShellHelper.PerformShellCommandAndExit(commandName, commandParams);
        }
    }
}
