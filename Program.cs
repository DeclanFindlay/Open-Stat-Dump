using LibreHardwareMonitor.Hardware;
using Services.HardwareFind;
using Utils.UserInput;
using Models.UserSettings;
using Services.CreateLua;
using System.Text.Json;
using System.Text.Json.Nodes;

internal class Program
{
    private static void Main()
    {
        JsonArray allData;
        JsonObject userSettings;

        UserSettings input = new();

        string jsonExtension = ".json";
        string luaExtension = ".lua";

        List <HardwareType> hardType = new()
        {
            HardwareType.Cpu,
            HardwareType.GpuAmd,
            HardwareType.GpuIntel,
            HardwareType.GpuNvidia,
            HardwareType.Network,
            HardwareType.Memory

        };
        HardwareFind service = new();
        CreateLua Lua = new();

        Computer computer = new()
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsNetworkEnabled = true,
            IsMemoryEnabled = true
        };
        computer.Open();
        
        try
        {
            if (File.Exists("userSettings.json"))
            {
                input.LoadSettings = UserInput.GetBoolInput("Load Settings: (true/false):\n");
            }
        }catch(Exception ex)
        {
            Console.WriteLine("ERROR::faild to open userSettings.json");
            File.WriteAllText("log.txt", ex.ToString());
            Console.WriteLine("CHECK::log.txt for information about the error");
        }


        if (input.LoadSettings)
        {
            userSettings = JsonNode.Parse(File.ReadAllText("userSettings.json"))!.AsObject();

            input.FileTypeLua = userSettings["FileTypeLua"]!.GetValue<bool>();
            input.FileNameLua = userSettings["FileNameLua"]!.GetValue<string>();
            input.PathLua = userSettings["PathLua"]!.GetValue<string>();

            input.FileTypeJson = userSettings["FileTypeJson"]!.GetValue<bool>();
            input.FileNameJson = userSettings["FileNameJson"]!.GetValue<string>();
            input.PathJson = userSettings["PathJson"]!.GetValue<string>();

            input.Interval = userSettings["Interval"]!.GetValue<int>();

        }
        else
        {

            Console.WriteLine("Configure Settings:\n");

            input.FileTypeLua = UserInput.GetBoolInput("Do you want a .lua file (true/false):\n");
            if (input.FileTypeLua)
            {
                input.FileNameLua = UserInput.GetStringInput("Enter the name of the lua file:\n" +
                "** Make sure not to add the .lua Extension this will be done automatically");
                input.PathLua = UserInput.GetStringInput($"Set the file path for {input.FileNameLua} file:\n" +
                $"** End path with: \\ ");
                Console.WriteLine($"Path set: {input.PathLua + input.FileNameLua + luaExtension}");
            }
            else
            {
                input.FileNameLua = "";
                input.PathLua = "";
            }
            input.FileTypeJson = UserInput.GetBoolInput("Do you want a .json file (true/false):\n");
            if (input.FileTypeJson)
            {
                input.FileNameJson = UserInput.GetStringInput("Enter the name of the json file:\n" +
                "** Make sure not to add the .json Extension this will be done automatically");
                input.PathJson = UserInput.GetStringInput($"Set the file path for {input.FileNameJson} file:\n" +
                    $"** End path with: \\ ");
                Console.WriteLine($"Path set: {input.PathJson + input.FileNameJson + jsonExtension}");
            }
            else
            {
                input.FileNameJson = "";
                input.PathJson = "";
            }
            input.Interval = UserInput.GetIntInput("Enter update interval (1000 = 1 second):\n" +
                "** Make sure not to set the interval bellow 1000");
            
            input.SaveSettings = UserInput.GetBoolInput("Do you want to save these settings (true/false):\n");

            if (input.SaveSettings)
            {
                userSettings = new();

                userSettings["FileTypeLua"] = input.FileTypeLua;
                userSettings["FileNameLua"] = input.FileNameLua;
                userSettings["PathLua"] = input.PathLua;

                userSettings["FileTypeJson"] = input.FileTypeJson;
                userSettings["FileNameJson"] = input.FileNameJson;
                userSettings["PathJson"] = input.PathJson;

                userSettings["Interval"] = input.Interval;

                userSettings["SaveSettings"] = input.SaveSettings;
                try
                {
                    File.WriteAllText("userSettings.json" , userSettings.ToJsonString(
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }
                    ));
                }catch(Exception ex)
                {
                    Console.WriteLine("ERROR::failed to create userSettings.json");
                    File.WriteAllText("log.txt", ex.ToString());
                    Console.WriteLine("CHECK::log.txt for information about the error");
                }

            }
        }

        while (true)
        {
            Console.Clear();
            allData = service.FindHardware(computer, hardType);
            try
            {
                if (input.FileTypeLua)
                {
                    Lua.SaveLua(input.PathLua + input.FileNameLua + luaExtension, allData);
                }
            }catch(Exception ex)
            {
                File.WriteAllText("log.txt", ex.ToString());
                Console.WriteLine($"ERROR::failed to create {input.FileNameLua + luaExtension} file");
                Console.WriteLine("CHECK::log.txt for information about the error");
            }

            if (input.FileTypeJson)
            {
                try
                {
                    File.WriteAllText(input.PathJson + input.FileNameJson + jsonExtension, allData.ToJsonString(
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }
                    ));
                }
                catch (Exception ex)
                {
                    File.WriteAllText("log.txt", ex.ToString());
                    Console.WriteLine($"ERROR::failed to create {input.FileNameJson + jsonExtension} file");
                    Console.WriteLine("CHECK::log.txt for information about the error");
                    break;
                }

            }
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("OPEN-STAT-DUMP----------------------------");
            Console.WriteLine("");
            Console.WriteLine($"> running ... at interval {input.Interval}");
            if (input.FileTypeLua)
            {
                Console.WriteLine($"> Lua file saved to {input.PathLua + input.FileNameLua + luaExtension}");
            }
            if (input.FileTypeJson == true)
            {
                Console.WriteLine($"> Json file saved to {input.PathJson + input.FileNameJson + jsonExtension}");
            }
            Console.WriteLine("");
            Console.WriteLine("** Close terminal to end the program------");
            Console.WriteLine("OPEN-STAT-DUMP----------------------------");
            Console.WriteLine("------------------------------------------");

            Thread.Sleep(input.Interval);
        }

    }

}