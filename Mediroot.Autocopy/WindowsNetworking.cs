using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Mediroot.Autocopy
{
    public class WindowsNetworking : IDisposable
    {
        private readonly ILog _logger;
        private string _uncPath;


        public WindowsNetworking(ILog logger)
        {
            _logger = logger;
        }

        public void Connect(string uncPath, string username, string passord)
        {
            _uncPath = uncPath;

            try
            {
                _logger.DebugFormat("Trying to connect to:{0} as: {1}", uncPath, username);
                PinvokeWindowsNetworking.connectToRemote(_uncPath, username, passord);

            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Exception occured on connecting {0}", ex);
            }
        }

        public void Dispose()
        {
            _logger.DebugFormat("Trying to close connection to :{0}", _uncPath);
            try
            {
                PinvokeWindowsNetworking.disconnectRemote(_uncPath);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Exception occured on disconnecting {0}", ex);
            }

        }
    }
}
