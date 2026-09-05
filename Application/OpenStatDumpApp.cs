using System;
using System.Collections.Generic;
using System.Text;
using LibreHardwareMonitor.Hardware;
using Services.HardwareFind;
using Utils.UserInput;
using Services.UserSettings;
using Services.CreateLua;
using System.Text.Json;
using System.Text.Json.Nodes;
using Utils.Logging;

namespace Application.OpenStatDumpApp
{
    internal class OpenStatDumpApp
    {
        private JsonArray allData = new();
        private UserSettings settings = new();

        private HardwareFind service = new();
        private CreateLua Lua = new();
        private Computer computer = new();

        private string userSettingsPath = "userSettings.json";

        private bool findSavedSettings;
        private bool firstLoop = true;

        public void Init()
        {
            computer.Open();

            settings.SetUserSettingsPath(userSettingsPath);

            findSavedSettings = settings.CheckForSavedSettings();

            if (findSavedSettings)
            {
                settings.LoadSettings(computer);
            } 
        }

        public void Config()
        {
            if (!findSavedSettings)
            {
                settings.ConfigSettings(computer);

                settings.SaveSettings();
            }
        }

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                allData = service.FindHardware(computer, settings.GetHardwareTypes());
                try
                {
                    if (settings.IsFileTypeLua())
                    {
                        Lua.SaveLua(settings.GetLuaPath(), allData);
                        if (firstLoop)
                        {
                            Logging.CreateLog($"SUCCESS::create/save {settings.GetLuaPath()} \n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.CreateLog($"ERROR::failed to create/save {settings.GetLuaPath()}\n {ex.ToString()} \n");
                    break;
                }

                if (settings.IsFileTypeJson())
                {
                    try
                    {
                        File.WriteAllText(settings.GetJsonPath(), allData.ToJsonString(
                            new JsonSerializerOptions
                            {
                                WriteIndented = true
                            }
                        ));
                        if (firstLoop)
                        {
                            Logging.CreateLog($"SUCCESS::create/save {settings.GetJsonPath()} \n");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.CreateLog($"ERROR::failed to create/save {settings.GetJsonPath()} \n{ex.ToString()}\n");
                        break;
                    }

                }
                Console.WriteLine("------------------------------------------");
                Console.WriteLine("OPEN-STAT-DUMP----------------------------");
                Console.WriteLine("");
                Console.WriteLine($"> running ... at interval {settings.GetInterval()}");
                if (settings.IsFileTypeLua())
                {
                    Console.WriteLine($"> Lua file saved to {settings.GetLuaPath()}");
                }
                if (settings.IsFileTypeJson())
                {
                    Console.WriteLine($"> Json file saved to {settings.GetJsonPath()}");
                }
                Console.WriteLine("");
                Console.WriteLine("** Close terminal to end the program------");
                Console.WriteLine("OPEN-STAT-DUMP----------------------------");
                Console.WriteLine("------------------------------------------");

                Thread.Sleep(settings.GetInterval());
                firstLoop = false;
            }
        }

        public void End()
        {
            computer.Close();
        }

    }
}
