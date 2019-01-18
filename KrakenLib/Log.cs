using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace theCodeJerk.apis.Kraken
{
    /// <summary>
    /// Provides static methods for logging requests/responses
    /// from the KrakenAPI for debugging.
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// Write the request and response from a KrakenAPI
        /// method to log files, if enabled.
        /// </summary>
        /// <param name="method"></param>
        public static void Write(Method method)
        {
            try
            {
                if (Enabled)
                {
                    // make sure the folder exists, if for some
                    // reason it wasn't created when logging
                    // was enabled.
                    if (Directory.Exists(FolderPath))
                    {
                        Directory.CreateDirectory(FolderPath);
                    }
                    File.WriteAllText(GetNextLogFilespec(method), 
                        string.Format("{0}\n{1}", 
                            method.Request.ToString(), 
                            method.Response.ToString())
                        );
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to write to KrakenAPI log.", ex);
            }
        }
        /// <summary>
        /// Sets/returns whether or not logging is enabled.  This is
        /// determined by the existence (or lack thereof) of a
        /// "switch" file.
        /// </summary>
        public static Boolean Enabled
        {
            get
            {
                return System.IO.Directory.Exists(FolderPath);
            }
            set
            {
                try
                {
                    if (value)
                    {
                        if (!File.Exists(SwitchFilespec))
                        {
                            File.Create(SwitchFilespec);
                        }
                        if (!Directory.Exists(FolderPath))
                        {
                            Directory.CreateDirectory(FolderPath);
                        }
                    } else
                    {
                        if (File.Exists(SwitchFilespec))
                        {
                            File.Delete(SwitchFilespec);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Can not set KrakenApi.Logger.Enabled", ex);
                }
            }
        }
        /// <summary>
        /// The fully qualified path and filespec of the switch file
        /// used to determine whether or not logging is enabled.
        /// </summary>
        private static string SwitchFilespec
        {
            get
            {
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(path, "krakenapi-log-enabled.ini");
            }
        }
        /// <summary>
        /// The folder path of the location to store the log files.
        /// </summary>
        private static String FolderPath
        {
            get
            {
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(path, "krakenapi-logfiles");
            }
        }
        /// <summary>
        /// Returns the next available filespec for writing a new log file.
        /// </summary>
        /// <returns></returns>
        private static string GetNextLogFilespec(Method method)
        {
            string folder = FolderPath;
            string filetitle = method.ApiMethodName;
            string filename = Path.Combine(folder, string.Format("{0}.txt", filetitle));
            int counter = 1;
            while (File.Exists(filename))
            {
                filename = Path.Combine(folder, string.Format("{0} ({1}).txt", filetitle, counter.ToString()));
            }
            return filename;
        }
    }
}
