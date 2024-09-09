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
            Task serverTask = Task.Run(() => server.StartAsync(cts.Token));

            Task memoryCleanerTask = Task.Run(async () => await MemoryCleaner.StartAsync(cts.Token));
            // Start motion detection on a separate thread
            Task detectionTask = Task.Run(async () => await Motion.StartDetection(cts.Token, 1));

            // Monitoring task in a separate thread
            Task FileManagerTask = Task.Run(async () => await manager.StartMonitoring(cts.Token));

            // Start RecordingMonitor to track camera inactivity
            Task monitoringTask = Task.Run(async () => await RecordingMonitor.StartMonitoring(cts));

            // Warte darauf, ob der CancellationToken ausgelöst wird oder der Benutzer das Programm manuell beendet
            await Task.WhenAny(monitoringTask, Task.Run(() => Console.ReadKey()));

            // Wenn der Task wegen Inaktivität beendet wurde, beende das Programm
            if (cts.IsCancellationRequested)
            {
                //Console.WriteLine("Zu viele Kameras inaktiv. Programm wird beendet.");
            }
            else
            {
                //Console.WriteLine("Manuelle Beendigung durch Benutzer.");
                cts.Cancel();  // Stoppe die Überwachung (cancel Task)
            }

            // Server stoppen
            server.Stop();

            // Warte auf den Abschluss aller Tasks
            try
            {
                await Task.WhenAll(serverTask, monitoringTask, detectionTask, memoryCleanerTask, FileManagerTask);
            Console.Clear();
            Header("iSpyClone", "preview 1");
            Console.WriteLine("Beenden...");
            await Task.Delay(5000);
            }
            catch (OperationCanceledException)
            {
                // Dies wird erwartet, wenn Tasks aufgrund von CancellationToken abgebrochen werden
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
