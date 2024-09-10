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

using System;
using System.IO;
using Newtonsoft.Json;

public class Config
{
    public string SavingDir { get; set; } = ".\\media";

    // Method to load configuration from a file
    public static Config Load(string filePath = ".\\config.json")
    {
        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<Config>(json);
        }
        else
        {
            return new Config();
        }
    }

    // Method to save the current configuration to a file
    public void Save(string filePath = ".\\config.json")
    {
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
}

class Program
{
    private static string Name = "iSpyClone";
    private static string Version = "Preview 2";

    public static DataManager manager;
    static async Task Main(string[] args)
    {
        Config config = new Config();
        if (!File.Exists(".\\config.json"))
            config.Save();
        else config = Config.Load();
        Header(Name, Version);
        //config.SavingDir = "h:\\media";
        string directoryPath = config.SavingDir;

        manager = new DataManager(directoryPath, 5); // Überprüfung alle 5 Minuten
        string[] prefixes = { "http://*:8040/" };

        // Start HTTP server on a separate thread
        var server = new HttpServer(prefixes);

        Cameras.Load();

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
                Header(Name, Version);
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
