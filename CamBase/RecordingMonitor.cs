public class RecordingMonitor
{
    // Dictionary zum Speichern der letzten Aufnahmezeit für jede Kamera
    private static Dictionary<string, DateTime> lastRecordingTimes = new Dictionary<string, DateTime>();

    // Zeitspanne in Sekunden, nach der geprüft wird, ob keine Aufnahme stattgefunden hat
    public static int CheckIntervalInSeconds = 60;
    public static int MaxIdleTimeInSeconds = 300; // 5 Minuten Leerlaufzeit
    public static int MaxInactiveCameras = 2; // Anzahl der inaktiven Kameras, die das Programm beenden soll

    // Methode zum Aktualisieren der letzten Aufnahmezeit
    public static void UpdateLastRecordingTime(Camera cam)
    {
        if (lastRecordingTimes.ContainsKey(cam.CameraName))
        {
            lastRecordingTimes[cam.CameraName] = DateTime.Now;
        }
        else
        {
            lastRecordingTimes.Add(cam.CameraName, DateTime.Now);
        }
    }

    // Methode, um zu überprüfen, ob eine Kamera zu lange inaktiv war
    public static bool CheckIdleCameras()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        int inactiveCamerasCount = 0;

        foreach (var cam in Cameras.All())
        {
            if (lastRecordingTimes.ContainsKey(cam.CameraName))
            {
                var lastRecordingTime = lastRecordingTimes[cam.CameraName];
                var timeSinceLastRecording = DateTime.Now - lastRecordingTime;

                if (timeSinceLastRecording.TotalSeconds > MaxIdleTimeInSeconds)
                {
                    inactiveCamerasCount++;
                    Console.WriteLine($"- {DateTime.Now.ToLongTimeString()}: {cam.CameraName} - No recording for {Math.Round(timeSinceLastRecording.TotalMinutes)} minutes.");
                }
            }
            else
            {
                // Wenn noch keine Aufnahme gemacht wurde, zählt diese Kamera auch als inaktiv
                //Console.WriteLine($"{cam.CameraName} - Noch keine Aufnahme gemacht.");
            }

            // Programm beenden, wenn mehr als 5 Kameras inaktiv sind
            if (inactiveCamerasCount >= MaxInactiveCameras)
            {
                return true; // Gibt an, dass das Programm beendet werden soll
            }
        }
        Console.ForegroundColor = ConsoleColor.White;
        return false; // Keine Beendigung erforderlich
    }

    // Diese Methode wird als Hintergrundtask ausgeführt, um regelmäßig zu überprüfen
    public static async Task StartMonitoring(CancellationTokenSource cts,int MaxCamFails = 2) // Änderung hier: Verwende CancellationTokenSource statt CancellationToken
    {
        MaxInactiveCameras = MaxCamFails;
        Console.WriteLine("- Recording Monitor started...");
        while (!cts.Token.IsCancellationRequested)
        {
            if (CheckIdleCameras())
            {
                // Wenn 5 oder mehr Kameras inaktiv sind, beenden wir das Programm
                if (!Cameras.RecordingState())
                {
                    cts.Cancel(); // Verwende CancellationTokenSource, um die Tasks zu stoppen
                    break;
                }
            }

            await Task.Delay(CheckIntervalInSeconds * 1000); // Überprüfe alle X Sekunden
        }
        Console.WriteLine("- Recording Monitor stopped...");
    }
}
