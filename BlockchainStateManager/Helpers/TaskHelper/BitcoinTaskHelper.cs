using BlockchainStateManager.Settings;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace BlockchainStateManager.Helpers.TaskHelper
{
    public class BitcoinTaskHelper
    {
        IBlockchainStateManagerSettingsProvider settingsProvider = null;

        public BitcoinTaskHelper(IBlockchainStateManagerSettingsProvider _settingsProvider)
        {
            settingsProvider = _settingsProvider;
        }
        public bool EmptyBitcoinDirectiry()
        {
            var settings = settingsProvider.GetSettings();

            var dirPath = settings.BitcoinWorkingPath + "\\regtest";
            try
            {
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, true);
                }
                Directory.CreateDirectory(dirPath);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> StartClearVersionOfBitcoinRegtest(bool doEmpty = false)
        {
            var settings = settingsProvider.GetSettings();

            if (doEmpty)
            {
                if (!EmptyBitcoinDirectiry())
                {
                    return false;
                }
            }

            var bitcoinPath = settings.BitcoinDaemonPath + "\\bitcoind.exe";

            Process bitcoinProcess = null;
            await ShellHelper.PerformShellCommandAndLeave(bitcoinPath, GetBitcoinConfParam(),
                 (p) => bitcoinProcess = p);

            int count = 0;
            var rpcClient = Helper.GetRPCClient(settings);
            while (true)
            {
                try
                {
                    await rpcClient.GetBlockCountAsync();
                    return true;
                }
                catch (Exception e)
                {
                    await Task.Delay(1000);
                    count++;
                    if (count > 30)
                    {
                        return false;
                    }
                }
            }
        }

        private bool StopBitcoinServer()
        {
            string commandName = GetBitcoinCliExecPath();
            string commandParams = GetBitcoinConfParam() + " stop";
            return ShellHelper.PerformShellCommandAndExit(commandName, commandParams);
        }

        private string GetBitcoinCliExecPath()
        {
            var settings = settingsProvider.GetSettings();

            return settings.BitcoinDaemonPath + "\\bitcoin-cli.exe";
        }

        private string GetBitcoinConfParam()
        {
            var settings = settingsProvider.GetSettings();

            return String.Format("-conf=\"{0}\"", settings.BitcoinWorkingPath + "\\bitcoin.conf");
        }
    }
}
