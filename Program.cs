
using Application.OpenStatDumpApp;
using Utils.Logging;
using Microsoft.AspNetCore.Builder;
using Services.StatsStore;

internal class Program
{
    private static async Task Main()
    {
        var statsStore = new StatsStore();
        OpenStatDumpApp app = new OpenStatDumpApp(statsStore);

        WebApplication? webApp = null;
        CancellationTokenSource? cts = null;
        Task? monitoringTask = null;
        const string URL = "http://localhost:5055";

        try
        {
            app.Init();
            app.Config();

            cts = new CancellationTokenSource();

            var webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

            var builder = WebApplication.CreateBuilder(
                new WebApplicationOptions
                {
                    ContentRootPath = AppContext.BaseDirectory,
                    WebRootPath = webRoot
                });

            builder.WebHost.UseUrls(URL);

            webApp = builder.Build();

            webApp.UseDefaultFiles();
            webApp.UseStaticFiles();

            webApp.MapGet("/api/stats", IResult () =>
            {
                var stats = statsStore.GetLatest();

                if (stats is null)
                {
                    return Results.Problem(
                        "Hardware statistics are not available yet.",
                        statusCode: 503);
                }

                return Results.Json(stats);
            });

            webApp.Lifetime.ApplicationStopping.Register(
                cts.Cancel);

            await webApp.StartAsync();

            monitoringTask = Task.Run(() => app.Run(cts.Token));


            await webApp.WaitForShutdownAsync();

            cts.Cancel();
            await monitoringTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");
            Logging.CreateLog($"Error: {ex}");
        }
        finally
        {
            cts?.Cancel();

            if (monitoringTask != null)
            {
                try
                {
                    await monitoringTask;
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Monitoring stopped.");
                }
                catch (Exception ex)
                {
                    Logging.CreateLog(
                        $"ERROR::monitoring stopped: {ex}");
                }
            }

            if (webApp != null)
            {
                await webApp.DisposeAsync();
            }

            app.End();
            cts?.Dispose();
        }
    }
}
