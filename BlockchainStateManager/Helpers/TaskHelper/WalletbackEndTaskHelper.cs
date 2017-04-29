using BlockchainStateManager.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Helpers.TaskHelper
{
    public class WalletbackEndTaskHelper
    {
        ISettingsProvider settingsProvider = null;
        Process WalletBackendProcess = null;

        public WalletbackEndTaskHelper(ISettingsProvider _settingsProvider)
        {
            settingsProvider = _settingsProvider;
        }
        public async Task<bool> StartWalletBackend()
        {
            var settings = settingsProvider.GetSettings();
            var command = settings.WalletBackendExecutablePath + "\\ServiceLykkeWallet.exe";
            return await ShellHelper.PerformShellCommandAndLeave(command, null, (p) => WalletBackendProcess = p,
            settings.WalletBackendExecutablePath, null);
        }

        public void FinishWalletBackend()
        {
            WalletBackendProcess.Kill();
        }
    }
}
