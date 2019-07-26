using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsServiceWrapper.Services
{
    public interface IConfigReader
    {
        IDictionary<string, string> ConfigValues {get;}

        bool ReadConfig(string pathToConfig);

        string GetValue(string key);

        void LoadEnvironmentVariablesFromConfig();
    }
}
