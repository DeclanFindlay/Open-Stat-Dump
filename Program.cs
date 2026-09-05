using Application.OpenStatDumpApp;
using Utils.Logging;

internal class Program
{
    private static void Main()
    {
        OpenStatDumpApp app = new OpenStatDumpApp();

        try
        {
            app.Init();
            app.Config();
            app.Run();
            app.End();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");
            Logging.CreateLog($"Error: {ex}");
            app.End();
        }
    }
}