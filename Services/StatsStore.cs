
using System.Text.Json.Nodes;

namespace Services.StatsStore;

public class StatsStore
{
    private readonly object _lock = new();
    private JsonArray? _latest;

    public void Update(JsonArray data)
    {
        lock (_lock)
        {
            _latest = (JsonArray)data.DeepClone();
        }
    }

    public JsonArray? GetLatest()
    {
        lock (_lock)
        {
            return _latest is null
                ? null
                : (JsonArray)_latest.DeepClone();
        }
    }
}
