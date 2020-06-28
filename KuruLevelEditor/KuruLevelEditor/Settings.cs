using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KuruLevelEditor
{
    static class Settings
    {
        public static string ExtractorCommand;
        public static string Input;
        public static string Output;
        public static string EmulatorCommand;
        public static bool LoadSettings()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddIniFile("config.ini", optional: false);
                var config = builder.Build();
                ExtractorCommand = config.GetSection("ROM").GetSection("ExtractorCommand").Value;
                Input = config.GetSection("ROM").GetSection("InputRom").Value;
                Output = config.GetSection("ROM").GetSection("OutputRom").Value;
                EmulatorCommand = config.GetSection("Emulator").GetSection("Command").Value;
                return true;
            }
            catch { }
            return false;
        }
    }
}
