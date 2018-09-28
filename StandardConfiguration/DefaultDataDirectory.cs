using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StandardConfiguration
{
    public class DefaultDataDirectory
    {
        public static string GetDirectory(string appDirectory, string subDirectory, bool createIfNotExists = true)
        {
            string directory = null;
            var home = Environment.GetEnvironmentVariable("HOME");
            var localAppData = Environment.GetEnvironmentVariable("APPDATA");
            if (!string.IsNullOrEmpty(home) && string.IsNullOrEmpty(localAppData))
            {
                directory = home;
                directory = Path.Combine(directory, "." + appDirectory.ToLowerInvariant());
            }
            else
            {
                if(!string.IsNullOrEmpty(localAppData))
                {
                    directory = localAppData;
                    directory = Path.Combine(directory, appDirectory);
                }
                else if(createIfNotExists)
                {
                    throw new DirectoryNotFoundException("Could not find suitable datadir environment variables HOME or APPDATA are not set");
                }
                else
                    return string.Empty;
            }

            if(createIfNotExists)
            {
                if(!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                directory = Path.Combine(directory, subDirectory);
                if(!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            else
            {
                directory = Path.Combine(directory, subDirectory);
            }
            return directory;
        }
    }
}
