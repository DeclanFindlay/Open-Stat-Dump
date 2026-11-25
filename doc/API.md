# Open-Stat-Dump API Documentation

This document describes the HTTP API and data formats implemented by Open-Stat-Dump.

Repository: https://github.com/DeclanFindlay/Open-Stat-Dump

## 1. Overview

Open-Stat-Dump is a Windows console application that reads hardware sensor information through LibreHardwareMonitorLib. It periodically collects the hardware selected in the application's configuration and makes the latest snapshot available to other software.

There are three ways to consume the collected data:

| Interface | What it provides | When to use it |
|---|---|---|
| HTTP API | Latest in-memory hardware snapshot as JSON | Applications that can make HTTP requests |
| JSON file | Periodically written JSON snapshot | Applications that read files |
| Lua file | Periodically written Lua table | Lua-based applications and widgets |

The HTTP API does not read the JSON output file. It reads the in-memory `StatsStore`, so JSON file output can be disabled while the HTTP API remains available.

## 2. Requirements and startup

1. Start Open-Stat-Dump.
2. Complete its configuration prompts, or choose to load an existing `userSettings.json`.
3. Enable the hardware categories your application needs (CPU, GPU, memory and/or network).
4. Wait for the first monitoring iteration to populate the statistics store.

Open-Stat-Dump starts its HTTP server before starting the monitoring task. A request made before the first snapshot exists can therefore receive HTTP `503`; retry after a short delay.

The project targets .NET 10 and uses LibreHardwareMonitorLib 0.9.6. The README notes that administrator privileges may be needed for some hardware sensors.

## 3. HTTP API

### Base address

The current implementation binds the web server to:

```text
http://localhost:5055
```

### `GET /api/stats`

Returns the latest available hardware snapshot.

```http
GET http://localhost:5055/api/stats
Accept: application/json
```

Successful response:

- Status: `200 OK`
- Content type: JSON
- Body: a JSON array of hardware objects

If the statistics store has not yet received its first snapshot:

- Status: `503 Service Unavailable`
- Body: ASP.NET Core Problem Details JSON, with the detail/message:
  `Hardware statistics are not available yet.`

There are no other API routes implemented in the current `Program.cs`.

## 4. Response structure

The response is a JSON array. Each item represents a selected hardware device.

### Hardware object

| Property | Type | Description |
|---|---|---|
| `hardwareName` | string | Name reported for the hardware |
| `hardwareType` | string | LibreHardwareMonitor hardware type name |
| `sensors` | array | Sensors collected for this hardware device |

### Sensor object

| Property | Type | Description |
|---|---|---|
| `identifier` | string | Sensor identifier reported by LibreHardwareMonitor |
| `sensorName` | string | Sensor display name |
| `sensorType` | string | LibreHardwareMonitor sensor type name |
| `sensorValue` | number or null | Current sensor reading |

### Example response

The following is an illustrative shortened response. Hardware names, sensor names, identifiers and values vary by machine and configuration.

```json
[
  {
    "hardwareName": "12th Gen Intel Core i5-12400F",
    "hardwareType": "Cpu",
    "sensors": [
      {
        "identifier": "/intelcpu/0/temperature/8",
        "sensorName": "CPU Package",
        "sensorType": "Temperature",
        "sensorValue": 48
      },
      {
        "identifier": "/intelcpu/0/load/0",
        "sensorName": "CPU Total",
        "sensorType": "Load",
        "sensorValue": 25.4
      }
    ]
  },
  {
    "hardwareName": "NVIDIA GeForce RTX 3060",
    "hardwareType": "GpuNvidia",
    "sensors": [
      {
        "identifier": "/gpu-nvidia/0/temperature/0",
        "sensorName": "GPU Core",
        "sensorType": "Temperature",
        "sensorValue": 43
      }
    ]
  }
]
```

## 5. Hardware selection and sensor availability

The application configuration currently allows the user to select:

- CPU
- GPU
- Memory
- Network

GPU selection enables the AMD, Intel and NVIDIA GPU hardware types. The application only includes hardware types selected in the configuration.

The actual sensors depend on the hardware, drivers, LibreHardwareMonitor support and permissions. A consumer must not assume that every machine returns the same hardware devices or sensors, or that a particular sensor name is always present.

`HardwareFind` collects sensors from each selected hardware device and its immediate subhardware. Sensor values can be unavailable; consumers should handle null values.

The `hardwareType` and `sensorType` values are string representations of LibreHardwareMonitor enums. Examples of hardware type names include `Cpu`, `GpuNvidia`, `GpuAmd`, `GpuIntel`, `Memory` and `Network`. Sensor type names can include `Temperature`, `Load`, `Clock`, `Voltage`, `Power`, `Fan`, `Data`, `SmallData` and `Throughput`.

## 6. Connecting from C#

This example uses `HttpClient` and `System.Text.Json` to retrieve the endpoint and enumerate its hardware and sensors.

```csharp
using System.Net;
using System.Net.Http;
using System.Text.Json;

using var http = new HttpClient
{
    BaseAddress = new Uri("http://localhost:5055")
};

try
{
    using HttpResponseMessage response =
        await http.GetAsync("/api/stats");

    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
    {
        Console.WriteLine("Open-Stat-Dump has not produced its first snapshot yet.");
        return;
    }

    response.EnsureSuccessStatusCode();

    await using Stream stream =
        await response.Content.ReadAsStreamAsync();

    using JsonDocument document =
        await JsonDocument.ParseAsync(stream);

    foreach (JsonElement hardware in document.RootElement.EnumerateArray())
    {
        string? hardwareName =
            hardware.GetProperty("hardwareName").GetString();

        string? hardwareType =
            hardware.GetProperty("hardwareType").GetString();

        Console.WriteLine($"{hardwareName} ({hardwareType})");

        foreach (JsonElement sensor in hardware.GetProperty("sensors").EnumerateArray())
        {
            string? sensorName =
                sensor.GetProperty("sensorName").GetString();

            string? sensorType =
                sensor.GetProperty("sensorType").GetString();

            JsonElement valueElement =
                sensor.GetProperty("sensorValue");

            string value = valueElement.ValueKind == JsonValueKind.Number
                ? valueElement.GetDouble().ToString()
                : "N/A";

            Console.WriteLine($"  {sensorName}: {value} ({sensorType})");
        }
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Could not connect to Open-Stat-Dump: {ex.Message}");
}
catch (JsonException ex)
{
    Console.WriteLine($"The API returned invalid or unexpected JSON: {ex.Message}");
}
```

For a desktop application, reuse an `HttpClient` instance rather than creating a new one for every polling request.

## 7. Connecting from JavaScript

A web page served by Open-Stat-Dump can call the endpoint using a relative URL. This is how the bundled dashboard works.

```javascript
async function getHardwareStats() {
    const response = await fetch("/api/stats");

    if (response.status === 503) {
        throw new Error("Hardware statistics are not available yet.");
    }

    if (!response.ok) {
        throw new Error(`HTTP error: ${response.status}`);
    }

    return await response.json();
}

async function updateDashboardData() {
    try {
        const hardware = await getHardwareStats();

        for (const device of hardware) {
            console.log(device.hardwareName, device.hardwareType);

            for (const sensor of device.sensors) {
                const value = sensor.sensorValue ?? "N/A";
                console.log(sensor.sensorName, value, sensor.sensorType);
            }
        }
    } catch (error) {
        console.error("Unable to retrieve Open-Stat-Dump data:", error);
    }
}

updateDashboardData();
```

### Browser applications hosted elsewhere

The current project does not configure CORS. A browser page served from another origin (different scheme, host or port) may therefore be blocked from calling this API by the browser's same-origin policy.

CORS must be deliberately configured in the Open-Stat-Dump server if cross-origin browser access is required. CORS is not an authentication or access-control security feature.

Non-browser clients such as desktop applications and server-side programs are not subject to browser CORS enforcement.

## 8. Connecting from Python

This example uses only Python's standard library:

```python
import json
from urllib.request import urlopen
from urllib.error import HTTPError, URLError

url = "http://localhost:5055/api/stats"

try:
    with urlopen(url, timeout=5) as response:
        hardware_list = json.loads(response.read().decode("utf-8"))

    for device in hardware_list:
        print(f"{device['hardwareName']} ({device['hardwareType']})")

        for sensor in device["sensors"]:
            value = sensor.get("sensorValue")
            if value is None:
                value = "N/A"

            print(f"  {sensor['sensorName']}: {value} ({sensor['sensorType']})")

except HTTPError as error:
    if error.code == 503:
        print("Open-Stat-Dump has not produced its first snapshot yet.")
    else:
        print(f"HTTP error: {error.code}")

except URLError as error:
    print(f"Could not connect to Open-Stat-Dump: {error.reason}")
```

## 9. Polling for updated readings

The API returns the latest snapshot stored by the monitoring loop. It does not trigger a new hardware read when a client makes a request.

The monitoring interval is configured in Open-Stat-Dump in milliseconds. The configuration prompt asks for an interval of at least 1000 ms (one second).

Clients can poll `GET /api/stats` periodically. For example, a dashboard might poll every second:

```javascript
async function pollStats() {
    try {
        const response = await fetch("/api/stats");

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const stats = await response.json();
        // Update your application using stats.
    } catch (error) {
        console.error(error);
    }
}

pollStats();
setInterval(pollStats, 1000);
```

Choose a polling interval appropriate for your application. Polling faster than Open-Stat-Dump's monitoring interval can return the same snapshot more than once. Avoid starting a new request if the previous poll is still in progress in applications where requests may take longer than the polling interval.

## 10. Network access and security

The current server URL is hard-coded in `Program.cs` as `http://localhost:5055`.

This is intended for applications running on the same computer. `localhost` refers to the computer on which the client application itself is running. A client on another device cannot use its own `localhost` address to reach the Open-Stat-Dump computer.

The current API implementation does not add authentication or authorization, and it uses HTTP rather than HTTPS. Do not expose it to untrusted networks or the public internet. Supporting LAN access should involve an intentional host-binding, firewall and security design, not simply forwarding the port.

## 11. JSON and Lua file outputs

The HTTP endpoint is independent of the optional file outputs. The application can also write:

- JSON: the `JsonArray` snapshot is written as indented JSON.
- Lua: the snapshot is converted into a Lua table and written with a leading `return`.

The output paths, file names, enabled output types, selected hardware categories and update interval are configured through the console prompts or loaded from `userSettings.json`.

Use the HTTP API when your application can make local HTTP requests. Use JSON or Lua file output when the consuming application is designed to read files or when that integration is more convenient.

## 12. Current API limitations

The following describes the uploaded source version:

- Only `GET /api/stats` is mapped.
- The endpoint returns the complete latest snapshot; there are no per-device or per-sensor routes.
- There are no query parameters or filtering options.
- There is no authentication or authorization.
- CORS is not configured.
- The host and port are hard-coded to `http://localhost:5055`.
- The API has no version prefix.
- The first request can receive `503` until the first monitoring snapshot is stored.
- Hardware and sensor availability varies by system.

## 13. Project references

- Repository: https://github.com/DeclanFindlay/Open-Stat-Dump
- LibreHardwareMonitor: https://github.com/LibreHardwareMonitor/LibreHardwareMonitor
- Beyond All Reason widget example: https://github.com/DeclanFindlay/Open-Stat-Dump-BAR_Widget
