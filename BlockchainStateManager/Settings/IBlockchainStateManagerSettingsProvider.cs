using Common.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Settings
{
    public interface IBlockchainStateManagerSettingsProvider : ISettingsProvider
    {
        new IBlockchainStateManagerSettings GetSettings();
    }
}
