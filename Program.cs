using LibreHardwareMonitor.Hardware;
using Services.HardwareFind;
using Utils.UserInput;
using Models.UserSettings;
using Services.CreateLua;
using System.Text.Json;
using System.Text.Json.Nodes;
using Utils.Logging;

internal class Program
{
    private static void Main()
    {
        JsonArray allData;
        JsonObject userSettings;
        UserSettings input = new();
        List<HardwareType> hardType = new();
        HardwareFind service = new();
        CreateLua Lua = new();
        Computer computer = new();
        computer.Open();

        string jsonExtension = ".json";
        string luaExtension = ".lua";
        string userSettingsPath = "userSettings.json";
        bool runloop = true;
        bool firstLoop = true;
        
        if (File.Exists(userSettingsPath))
        {
            input.LoadSettings = UserInput.GetBoolInput("Load Settings: (true/false):\n");
        }

        if (input.LoadSettings)
        {
            try
            {
                Console.WriteLine("Loading ...");
                userSettings = JsonNode.Parse(File.ReadAllText(userSettingsPath))!.AsObject();
                input.FileTypeLua = userSettings["FileTypeLua"]!.GetValue<bool>();
                input.FileNameLua = userSettings["FileNameLua"]!.GetValue<string>();
                input.PathLua = userSettings["PathLua"]!.GetValue<string>();

                input.FileTypeJson = userSettings["FileTypeJson"]!.GetValue<bool>();
                input.FileNameJson = userSettings["FileNameJson"]!.GetValue<string>();
                input.PathJson = userSettings["PathJson"]!.GetValue<string>();

                input.HardwareTypeCPU = userSettings["HardwareTypeCPU"]!.GetValue<bool>();
                if (input.HardwareTypeCPU)
                {
                    computer.IsCpuEnabled = true;
                    hardType.Add(HardwareType.Cpu);
                }
                input.HardwareTypeGPU = userSettings["HardwareTypeGPU"]!.GetValue<bool>();
                if (input.HardwareTypeGPU)
                {
                    computer.IsGpuEnabled = true;

                    hardType.Add(HardwareType.GpuAmd);
                    hardType.Add(HardwareType.GpuIntel);
                    hardType.Add(HardwareType.GpuNvidia);
                }
                input.HardwareTypeMemory = userSettings["HardwareTypeMemory"]!.GetValue<bool>();
                if (input.HardwareTypeMemory)
                {
                    computer.IsMemoryEnabled = true;
                    hardType.Add(HardwareType.Memory);
                }
                input.HardwareTypeNetwork = userSettings["HardwareTypeNetwork"]!.GetValue<bool>();
                if (input.HardwareTypeNetwork)
                {
                    computer.IsNetworkEnabled = true;
                    hardType.Add(HardwareType.Network);
                }

                input.Interval = userSettings["Interval"]!.GetValue<int>();
                Logging.CreateLog($"SUCCESS::parse/load {userSettingsPath} to json object\n");
            }
            catch (Exception ex)
            {
                Logging.CreateLog($"ERROR::faild to parse/load {userSettingsPath} to json object\n{ex.ToString()}\n");
                runloop = false;
            }
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

            input.HardwareTypeCPU = UserInput.GetBoolInput("Do you want CPU stats (true/false):\n");
            if (input.HardwareTypeCPU)
            {
                computer.IsCpuEnabled = true;
                hardType.Add(HardwareType.Cpu);
            }

            input.HardwareTypeGPU = UserInput.GetBoolInput("Do you want GPU stats (true/false):\n");
            if (input.HardwareTypeGPU)
            {
                computer.IsGpuEnabled = true;
                
                hardType.Add(HardwareType.GpuAmd);
                hardType.Add(HardwareType.GpuIntel);
                hardType.Add(HardwareType.GpuNvidia);
            }

            input.HardwareTypeMemory = UserInput.GetBoolInput("Do you want Memory stats(RAM) (true/false):\n");
            if (input.HardwareTypeMemory)
            {
                computer.IsMemoryEnabled = true;
                hardType.Add(HardwareType.Memory);
            }
            
            input.HardwareTypeNetwork = UserInput.GetBoolInput("Do you want Network stats (true/false):\n");
            if (input.HardwareTypeNetwork)
            {
                computer.IsNetworkEnabled = true;
                hardType.Add(HardwareType.Network);
            }
            
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

                userSettings["HardwareTypeCPU"] = input.HardwareTypeCPU;
                userSettings["HardwareTypeGPU"] = input.HardwareTypeGPU;
                userSettings["HardwareTypeMemory"] = input.HardwareTypeMemory;
                userSettings["HardwareTypeNetwork"] = input.HardwareTypeNetwork;

                userSettings["SaveSettings"] = input.SaveSettings;
                try
                {
                    File.WriteAllText(userSettingsPath, userSettings.ToJsonString(
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }
                    ));
                    Logging.CreateLog($"SUCCESS::Save {userSettingsPath}\n");
                }
                catch(Exception ex)
                {
                    Logging.CreateLog($"ERROR::failed to create/Save {userSettingsPath}\n {ex.ToString()} \n");
                    runloop = false;
                }

            }
        }
        if (runloop)
        {
            while (true)
            {
                Console.Clear();
                allData = service.FindHardware(computer, hardType);
                try
                {
                    if (input.FileTypeLua)
                    {
                        Lua.SaveLua(input.PathLua + input.FileNameLua + luaExtension, allData);
                        if (firstLoop)
                        {
                            Logging.CreateLog($"SUCCESS::create/save {input.FileNameLua + luaExtension} \n");
                        }
                    }
                }catch(Exception ex)
                {
                    Logging.CreateLog($"ERROR::failed to create/save {input.FileNameLua + luaExtension}\n {ex.ToString()} \n");
                    break;
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
                        if (firstLoop)
                        {
                            Logging.CreateLog($"SUCCESS::create/save {input.FileNameJson + jsonExtension} \n");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.CreateLog($"ERROR::failed to create/save {input.FileNameJson + jsonExtension} \n{ex.ToString()}\n");
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
                firstLoop = false;
            }
        }
        else
        {
            Console.WriteLine("ERROR::check log.txt");
        }


    }

}