
namespace Utils.Logging;
class Logging
{
    private static string logPath = "log.txt";
    public static void CreateLog(string message)
    {
        File.AppendAllText(logPath,$"{message}");
    }


}