using LibreHardwareMonitor.Hardware;
using System.Text.Json.Nodes;

namespace Services.HardwareFind;
class HardwareFind
{

    private JsonObject CreateSensor(ISensor sensor)
    {
        JsonObject sensorObject = [];

        sensorObject["identifier"] = sensor.Identifier.ToString();
        sensorObject["sensorName"] = sensor.Name;
        sensorObject["sensorType"] = sensor.SensorType.ToString();
        sensorObject["sensorValue"] = sensor.Value;

        return sensorObject;
    }
    
    public JsonArray FindHardware(Computer computer, List<HardwareType> type)
    {
        JsonArray hardwareArray = [];


        foreach (IHardware hardware in computer.Hardware)
        {
            if (!type.Contains(hardware.HardwareType))
                continue;

            hardware.Update();

            JsonObject hardwareObject = [];

            hardwareObject["hardwareName"] = hardware.Name;
            hardwareObject["hardwareType"] = hardware.HardwareType.ToString();

            JsonArray sensorArray = [];

            foreach (ISensor sensor in hardware.Sensors)
            {             
                sensorArray.Add(CreateSensor(sensor));
            }

            foreach (IHardware subHardware in hardware.SubHardware)
            {
                subHardware.Update();

                foreach (ISensor sensor in subHardware.Sensors)
                {
                    sensorArray.Add(CreateSensor(sensor));
                }
            }

            hardwareObject["sensors"] = sensorArray;

            hardwareArray.Add(hardwareObject);
        }

        return hardwareArray;
    }

}