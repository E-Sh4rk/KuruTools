using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        public static bool Paradise { get; set; }

        public static bool LoadSettings()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddIniFile("config.ini", optional: false);
                var config = builder.Build();
                ExtractorCommand = config.GetSection("ROM").GetSection("ExtractorCommand").Value;
                Input = config.GetSection("ROM").GetSection("InputRom").Value;
                Output = config.GetSection("ROM").GetSection("OutputRom").Value;
                EmulatorCommand = config.GetSection("Emulator").GetSection("Command").Value;
                string name = GetNameOfROM();
                if (name == "KURUPARA")
                    Paradise = true;
                else if (name == "KURURIN")
                    Paradise = false;
                else return false;
                return true;
            }
            catch { }
            return false;
        }

        public static string RunExtractor(string additionalArgs)
        {
            string escapedInput = Path.GetFullPath(Input).Escape();
            string escapedOutput = Path.GetFullPath(Output).Escape();
            string escapedWorkspace = Levels.LEVELS_DIR.Escape();
            string args = $"--input \"{escapedInput}\" --output \"{escapedOutput}\" --workspace \"{escapedWorkspace}\" {additionalArgs}";
            string cmd = ExtractorCommand.Replace("%ARGS%", args);
            return cmd.RunCommand();
        }
        public static string RunEmulator()
        {
            string escapedROM = Path.GetFullPath(Output).Escape();
            string args = $"\"{escapedROM}\"";
            string cmd = EmulatorCommand.Replace("%ROM%", args);
            return cmd.RunCommand();
        }
        public static string GetNameOfROM()
        {
            string escapedInput = Path.GetFullPath(Input).Escape();
            string args = $"--input \"{escapedInput}\" --identify-only";
            string cmd = ExtractorCommand.Replace("%ARGS%", args);
            return cmd.RunCommand().Trim(new char[] { ' ', '\r', '\n' });
        }
    }
}
