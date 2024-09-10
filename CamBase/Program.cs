using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using CamBase;
using Colorful;
using Newtonsoft.Json;
using Console = System.Console;

class Program
{
    static async Task Main(string[] args)
    {
        Header("iSpyClone", "preview 1");

        string directoryPath = @".\media\";
        Cameras.Load();

        DataManager manager = new DataManager(directoryPath, 5); // Überprüfung alle 5 Minuten
        string[] prefixes = { "http://*:8040/" };

        // Start HTTP server on a separate thread
        var server = new HttpServer(prefixes);
        using (CancellationTokenSource cts = new CancellationTokenSource())
        {
            // Fange das Ctrl-C oder Schließen der Konsole ab
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;  // Verhindere das sofortige Schließen der Konsole
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("- Stop signal received. Stopping all Services...");
                cts.Cancel();      // Stoppe alle Tasks sauber
            };

            Task serverTask = Task.Run(() => server.StartAsync(cts.Token));
            Task memoryCleanerTask = Task.Run(async () => await MemoryCleaner.StartAsync(cts.Token));
            Task detectionTask = Task.Run(async () => await Motion.StartDetection(cts.Token, 1));
            Task FileManagerTask = Task.Run(async () => await manager.StartMonitoring(cts.Token));
            Task monitoringTask = Task.Run(async () => await RecordingMonitor.StartMonitoring(cts));

            CameraStatistics.LoadStatistics();

            // Warte darauf, ob der Benutzer das Programm manuell beendet
            await Task.WhenAny(monitoringTask);

            if (cts.IsCancellationRequested)
            {

            }

            CameraStatistics.SaveStatistics();
            server.Stop();
            await Task.Delay(2000);
            try
            {
                await Task.WhenAll(serverTask, monitoringTask, detectionTask, memoryCleanerTask, FileManagerTask);
                Console.Clear();
                Header("iSpyClone", "preview 1");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Exiting...");
                await Task.Delay(2000);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Tasks wurden abgebrochen.");
            }
        }

        Environment.Exit(0);
    }

    private static void Header(string title, string ver)
    {
        Console.Title = title;
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine(iSpyClone.Properties.Resources.header2.Replace("[version]", ver.ToUpper()));
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------");
    }
}
