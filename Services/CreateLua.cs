using System.Text;
using System.Text.Json.Nodes;

namespace Services.CreateLua;
class CreateLua
{
    public void SaveLua(string path, JsonArray jObject)
    {
        StringBuilder sb = new();

        sb.Append("return ");
        WriteNode(sb, jObject, 0);

        File.WriteAllText(path, sb.ToString());
    }

    private static void WriteNode(StringBuilder sb, JsonNode? node, int indent)
    {
        switch (node)
        {
            case JsonObject obj:
                WriteObject(sb, obj, indent);
                break;

            case JsonArray array:
                WriteArray (sb, array, indent);
                break;

            case JsonValue value:
                WriteValue(sb, value);
                break;

            default:
                sb.Append("nil");
                break;

        }
    }

    private static void WriteObject(StringBuilder sb, JsonObject obj, int indent)
    {
        sb.AppendLine("{");

        foreach(var pair in obj)
        {
            sb.Append(new string(' ', (indent + 1) * 4));
            sb.Append(pair.Key);
            sb.Append(" = ");

            WriteNode(sb, pair.Value, indent + 1);

            sb.AppendLine(",");
        }
        sb.Append(new string(' ', indent * 4));
        sb.Append("}");
    }

    private static void WriteArray(StringBuilder sb, JsonArray array, int indent)
    {
        sb.AppendLine("{");

        foreach (var item in array)
        {
            sb.Append(new string(' ', (indent + 1) * 4));

            WriteNode(sb, item, indent + 1);

            sb.AppendLine(",");
        }

        sb.Append(new string(' ', (indent) * 4));
        sb.Append("}");
    }

    private static void WriteValue(StringBuilder sb, JsonValue value)
    {
        object? val = value.GetValue<object>();

        switch (val)
        {
            case string s:
                sb.Append($"\"{s.Replace("\"", "\\\"")}\"");
                break;

            case bool b:
                sb.Append(b ? "true" : "false");
                break;

            case null:
                sb.Append("nil");
                break;

            default:
                sb.Append(val);
                break;
        }
    }
}