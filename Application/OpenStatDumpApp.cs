using LibreHardwareMonitor.Hardware;
using Services;
using Services.CreateLua;
using Services.HardwareFind;
using Services.UserSettings;
using Services.StatsStore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Utils.Logging;
using Utils.UserInput;

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

        private readonly StatsStore statsStore;

        public OpenStatDumpApp(StatsStore statsStore)
        {
            this.statsStore = statsStore;
        }

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

        public void Run(CancellationToken token)
        {
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
            Console.WriteLine("OPEN-STAT-DUMP----------------------------");
            Console.WriteLine("------------------------------------------");

            while (!token.IsCancellationRequested)
            {
                allData = service.FindHardware(computer, settings.GetHardwareTypes());
                statsStore.Update(allData);

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
                if (token.WaitHandle.WaitOne(settings.GetInterval()))
                {
                    break;
                }

                firstLoop = false;
            }
        }

        public void End()
        {
            computer.Close();
        }

    }
}
