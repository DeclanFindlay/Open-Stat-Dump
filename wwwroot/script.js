
const dashboard = document.getElementById("dashboard");
const statusElement = document.getElementById("status");

let updateInterval = 1000;

function formatSensorValue(sensor) {
    const value = sensor.sensorValue;

    if (value == null || !Number.isFinite(Number(value))) {
        return "N/A";
    }

    const number = Number(value);

    switch (sensor.sensorType.toLowerCase()) {
        case "load":
        case "control":
            return `${number.toFixed(1)}%`;

        case "temperature":
            return `${number.toFixed(1)} °C`;

        case "clock":
            return `${number.toFixed(0)} MHz`;

        case "fan":
            return `${number.toFixed(0)} RPM`;

        case "power":
            return `${number.toFixed(1)} W`;

        case "voltage":
            return `${number.toFixed(2)} V`;

        case "data":
            return `${number.toFixed(1)} GB`;

        case "small data":
            return `${number.toFixed(0)} MB`;

        default:
            return String(value);
    }
}

function createSensorRow(sensor) {
    const row = document.createElement("div");
    row.className = "sensor-row";

    const details = document.createElement("div");
    details.className = "sensor-details";

    const name = document.createElement("span");
    name.className = "sensor-name";
    name.textContent = sensor.sensorName;

    const type = document.createElement("span");
    type.className = "sensor-type";
    type.textContent = sensor.sensorType;

    const value = document.createElement("span");
    value.className = "sensor-value";
    value.textContent = formatSensorValue(sensor);

    details.append(name, type);
    row.append(details, value);

    // Only show a bar for percentage-like sensor types.
    if (["load", "control"].includes(
        sensor.sensorType.toLowerCase()
    )) {
        const numericValue = Number(sensor.sensorValue);

        if (Number.isFinite(numericValue)) {
            const bar = document.createElement("div");
            bar.className = "sensor-bar";

            const fill = document.createElement("div");
            fill.className = "sensor-bar-fill";
            fill.style.width =
                `${Math.max(0, Math.min(100, numericValue))}%`;

            bar.append(fill);
            row.append(bar);
        }
    }

    return row;
}

function createHardwareCard(hardware) {
    const card = document.createElement("section");
    card.className = "hardware-card";

    const header = document.createElement("div");
    header.className = "hardware-header";

    const name = document.createElement("h2");
    name.textContent = hardware.hardwareName;

    const type = document.createElement("span");
    type.className = "hardware-type";
    type.textContent = hardware.hardwareType;

    header.append(name, type);
    card.append(header);

    const sensors = Array.isArray(hardware.sensors)
        ? hardware.sensors
        : [];

    const sensorList = document.createElement("div");
    sensorList.className = "sensor-list";

    for (const sensor of sensors) {
        sensorList.append(createSensorRow(sensor));
    }

    card.append(sensorList);

    return card;
}

function renderDashboard(hardwareList) {
    const fragment = document.createDocumentFragment();

    for (const hardware of hardwareList) {
        fragment.append(createHardwareCard(hardware));
    }

    dashboard.replaceChildren(fragment);
}

async function updateDashboard() {
    try {
        const response = await fetch("/api/stats");

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const data = await response.json();

        if (!Array.isArray(data)) {
            throw new Error("Expected an array of hardware data");
        }

        renderDashboard(data);

        statusElement.textContent =
            `Connected · Updated ${new Date().toLocaleTimeString()}`;
    } catch (error) {
        statusElement.textContent = "Unable to retrieve system data";
        console.error("Dashboard update failed:", error);
    }
}

updateDashboard();
setInterval(updateDashboard, updateInterval);
