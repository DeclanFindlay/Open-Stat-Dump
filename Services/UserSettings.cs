using LibreHardwareMonitor.Hardware;
using Models.UserSettingsModel;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;
using Utils.UserInput;

namespace Services.UserSettings;

internal class UserSettings
{
    private JsonObject userSettings = new();

    private UserSettingsModel settings = new();

    private List<HardwareType> hardType = new();

    readonly private string luaExtension = ".lua";
    readonly private string jsonExtension = ".json";

    public void SetUserSettingsPath(string path)
    {
        settings.UserSettingsPath = path;
    }

    public string GetLuaPath()
    {
        return settings.PathLua + settings.FileNameLua + luaExtension;
    }

    public string GetJsonPath()
    {
        return settings.PathJson + settings.FileNameJson + jsonExtension;
    }

    public bool IsFileTypeLua()
    {
        return settings.FileTypeLua;
    }

    public bool IsFileTypeJson()
    {
        return settings.FileTypeJson;
    }

    public List<HardwareType> GetHardwareTypes()
    {
        return hardType;
    }

    public int GetInterval()
    {
        return settings.Interval;
    }

    public bool CheckForSavedSettings()
    {
        if (File.Exists(settings.UserSettingsPath))
        {
            settings.LoadSettings = UserInput.GetBoolInput("Load Settings: (true/false):\n");
        }

        return settings.LoadSettings;
         
    }

    public void LoadSettings(Computer computer)
    {
        Console.WriteLine("Loading ...");
        userSettings = JsonNode.Parse(File.ReadAllText(settings.UserSettingsPath))!.AsObject();
        settings.FileTypeLua = userSettings["FileTypeLua"]!.GetValue<bool>();
        settings.FileNameLua = userSettings["FileNameLua"]!.GetValue<string>();
        settings.PathLua = userSettings["PathLua"]!.GetValue<string>();

        settings.FileTypeJson = userSettings["FileTypeJson"]!.GetValue<bool>();
        settings.FileNameJson = userSettings["FileNameJson"]!.GetValue<string>();
        settings.PathJson = userSettings["PathJson"]!.GetValue<string>();

        settings.HardwareTypeCPU = userSettings["HardwareTypeCPU"]!.GetValue<bool>();
        if (settings.HardwareTypeCPU)
        {
            computer.IsCpuEnabled = true;
            hardType.Add(HardwareType.Cpu);
        }
        settings.HardwareTypeGPU = userSettings["HardwareTypeGPU"]!.GetValue<bool>();
        if (settings.HardwareTypeGPU)
        {
            computer.IsGpuEnabled = true;

            hardType.Add(HardwareType.GpuAmd);
            hardType.Add(HardwareType.GpuIntel);
            hardType.Add(HardwareType.GpuNvidia);
        }
        settings.HardwareTypeMemory = userSettings["HardwareTypeMemory"]!.GetValue<bool>();
        if (settings.HardwareTypeMemory)
        {
            computer.IsMemoryEnabled = true;
            hardType.Add(HardwareType.Memory);
        }
        settings.HardwareTypeNetwork = userSettings["HardwareTypeNetwork"]!.GetValue<bool>();
        if (settings.HardwareTypeNetwork)
        {
            computer.IsNetworkEnabled = true;
            hardType.Add(HardwareType.Network);
        }

        settings.Interval = userSettings["Interval"]!.GetValue<int>();
    }

    public void ConfigSettings(Computer computer)
    {
        if (!settings.LoadSettings)
        {
            Console.WriteLine("Configure Settings:\n");

            settings.FileTypeLua = UserInput.GetBoolInput("Do you want a .lua file (true/false):\n");
            if (settings.FileTypeLua)
            {
                settings.FileNameLua = UserInput.GetStringInput("Enter the name of the lua file:\n" +
                "** Make sure not to add the .lua Extension this will be done automatically");
                settings.PathLua = UserInput.GetStringInput($"Set the file path for {settings.FileNameLua} file:\n" +
                $"** End path with: \\ ");
                Console.WriteLine($"Path set: {settings.PathLua + settings.FileNameLua + luaExtension}");
            }
            else
            {
                settings.FileNameLua = "";
                settings.PathLua = "";
            }
            settings.FileTypeJson = UserInput.GetBoolInput("Do you want a .json file (true/false):\n");
            if (settings.FileTypeJson)
            {
                settings.FileNameJson = UserInput.GetStringInput("Enter the name of the json file:\n" +
                "** Make sure not to add the .json Extension this will be done automatically");
                settings.PathJson = UserInput.GetStringInput($"Set the file path for {settings.FileNameJson} file:\n" +
                    $"** End path with: \\ ");
                Console.WriteLine($"Path set: {settings.PathJson + settings.FileNameJson + jsonExtension}");
            }
            else
            {
                settings.FileNameJson = "";
                settings.PathJson = "";
            }
            settings.Interval = UserInput.GetIntInput("Enter update interval (1000 = 1 second):\n" +
                "** Make sure not to set the interval bellow 1000");

            settings.HardwareTypeCPU = UserInput.GetBoolInput("Do you want CPU stats (true/false):\n");
            if (settings.HardwareTypeCPU)
            {
                computer.IsCpuEnabled = true;
                hardType.Add(HardwareType.Cpu);
            }

            settings.HardwareTypeGPU = UserInput.GetBoolInput("Do you want GPU stats (true/false):\n");
            if (settings.HardwareTypeGPU)
            {
                computer.IsGpuEnabled = true;

                hardType.Add(HardwareType.GpuAmd);
                hardType.Add(HardwareType.GpuIntel);
                hardType.Add(HardwareType.GpuNvidia);
            }

            settings.HardwareTypeMemory = UserInput.GetBoolInput("Do you want Memory stats(RAM) (true/false):\n");
            if (settings.HardwareTypeMemory)
            {
                computer.IsMemoryEnabled = true;
                hardType.Add(HardwareType.Memory);
            }

            settings.HardwareTypeNetwork = UserInput.GetBoolInput("Do you want Network stats (true/false):\n");
            if (settings.HardwareTypeNetwork)
            {
                computer.IsNetworkEnabled = true;
                hardType.Add(HardwareType.Network);
            }

            settings.SaveSettings = UserInput.GetBoolInput("Do you want to save these settings (true/false):\n");
        }
    }

    public void SaveSettings()
    {
        userSettings["FileTypeLua"] = settings.FileTypeLua;
        userSettings["FileNameLua"] = settings.FileNameLua;
        userSettings["PathLua"] = settings.PathLua;

        userSettings["FileTypeJson"] = settings.FileTypeJson;
        userSettings["FileNameJson"] = settings.FileNameJson;
        userSettings["PathJson"] = settings.PathJson;

        userSettings["Interval"] = settings.Interval;

        userSettings["HardwareTypeCPU"] = settings.HardwareTypeCPU;
        userSettings["HardwareTypeGPU"] = settings.HardwareTypeGPU;
        userSettings["HardwareTypeMemory"] = settings.HardwareTypeMemory;
        userSettings["HardwareTypeNetwork"] = settings.HardwareTypeNetwork;

        File.WriteAllText(settings.UserSettingsPath, userSettings.ToJsonString(
            new JsonSerializerOptions
            {
                WriteIndented = true
            }
        ));
    }

    public bool ShouldSaveSettings()
    {
        return settings.SaveSettings;
    }
}
