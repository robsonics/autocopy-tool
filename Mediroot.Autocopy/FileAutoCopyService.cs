using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Core;

namespace Mediroot.Autocopy
{
    public class FileAutoCopyService
    {
        private readonly ILog _logger;
        private string _dst;
        private FileSystemWatcher _watcher;
        private string _src;
        private object _copyLock = new object();
        public FileAutoCopyService(ILog logger)
        {
            _logger = logger;
        }

        public static bool IsFileReady(String sFilename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(sFilename, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    if (inputStream.Length > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Start(string src, string dst)
        {
            _logger.Info("Started FileAutoCopyServic.");
            _dst = dst;
            _src = src;
            _watcher = new FileSystemWatcher();
            _watcher.Path = src;
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Filter = "*.xml";
            _watcher.Changed += new FileSystemEventHandler(OnChanged);
            _watcher.EnableRaisingEvents = true;
        }

        public static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            lock (_copyLock)
            {
                if (e.ChangeType != WatcherChangeTypes.Created && e.ChangeType != WatcherChangeTypes.Changed) return;
                if (!e.Name.StartsWith("L") || !IsFileReady(e.FullPath)) return;
                _logger.Info(string.Format("Start processing file {0}.", e.Name));
                var folder = RemoveDiacritics(DateTime.Now.ToString("MMMM_yyyy", new CultureInfo("pl-PL")));
                var directory = Path.Combine(_dst, folder);
                _logger.Info(string.Format("Checking if directory exist: {0}.", directory));
                _logger.InfoFormat("Acting as: {0}", WindowsIdentity.GetCurrent().Name);
                using (var wn = new WindowsNetworking(_logger))
                {
                    wn.Connect(@"\\192.168.1.200\publiczny\ewus",@"WORKGROUP\Robert","malgosia");

                    if (!Directory.Exists(directory))
                    {
                        _logger.Info(string.Format("Creating: {0}.", directory));
                        Directory.CreateDirectory(directory);
                    }
                    else
                    {
                        _logger.Info(string.Format("{0} exist.", directory));
                    }
                    var destFileName = Path.Combine(directory, e.Name);
                    if (File.Exists(destFileName))
                    {
                        _logger.Info("Skipping copying, file already exist.");
                        return;
                    }
                    _logger.Info(string.Format("Start coping {0}", e.Name));

                    File.Copy(e.FullPath, destFileName);
                    _logger.Info(string.Format("Finish coping {0}", e.Name));
                }
                
            }
        }

        public void Stop()
        {
            _logger.Info("Stoping service");
        }
    }
}
