﻿using BlockchainStateManager.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Helpers.TaskHelper
{
    public class QBitninjaTaskHelper
    {
        IBlockchainStateManagerSettingsProvider settingsProvider = null;
        Process QBitNinjaProcess = null;

        public QBitninjaTaskHelper(IBlockchainStateManagerSettingsProvider _settingsProvider)
        {
            settingsProvider = _settingsProvider;
        }

        public async Task<bool> StartQBitNinjaListener()
        {
            var settings = settingsProvider.GetSettings();

            var command = settings.QBitNinjaListenerConsolePath + "\\QBitNinja.Listener.Console.exe";
            var commandParams = "--Listen";
            return await ShellHelper.PerformShellCommandAndLeave(command, commandParams, (p) => QBitNinjaProcess = p);
        }

        public void FinishQBitNinjaListener()
        {
            QBitNinjaProcess.Kill();
        }
    }
}
