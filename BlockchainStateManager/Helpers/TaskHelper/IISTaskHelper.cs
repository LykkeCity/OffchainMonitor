using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Helpers.TaskHelper
{
    public class IISTaskHelper
    {
        public bool Start()
        {
            //return InvokeCommand("Start-Website");
            return InvokeCommand("Start-service");
        }

        public bool Stop()
        {
            // return InvokeCommand("Stop-Website");
            return InvokeCommand("Stop-service");
        }

        // https://stackoverflow.com/questions/31626244/retrieving-the-com-class-factory-for-component-with-clsid-688eeee5-6a7e-422f-b2
        private bool InvokeCommand(string command)
        {
            PowerShell ps = PowerShell.Create();
            ps.AddCommand(command);
            // ps.AddArgument("QBitNinja");
            ps.AddArgument("W3SVC");
            var result = ps.Invoke();
            if (ps.Streams.Error.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
