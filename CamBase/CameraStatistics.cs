using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

public class CameraStatistics
{
    // Dictionary to store statistics for each camera
    private static Dictionary<int, CameraStat> cameraStats = new Dictionary<int, CameraStat>();

    // File path where statistics will be saved
    private static string statsFilePath = "camera_statistics.json";

    // Method to update statistics when a recording starts
    public static void RecordStart(Camera camera)
    {
        if (!cameraStats.ContainsKey(camera.CameraIndex))
        {
            cameraStats[camera.CameraIndex] = new CameraStat(camera.CameraName);
        }

        // Update the statistics for recording start
        cameraStats[camera.CameraIndex].TotalRecordings++;
        cameraStats[camera.CameraIndex].LastRecordingStartTime = DateTime.Now;
        //Console.WriteLine($"{camera.CameraName} - Recording started. Total recordings: {cameraStats[camera.CameraIndex].TotalRecordings}");
    }

    // Method to update statistics when a recording ends
    public static void RecordEnd(Camera camera)
    {
        if (cameraStats.ContainsKey(camera.CameraIndex))
        {
            var stat = cameraStats[camera.CameraIndex];
            TimeSpan recordingDuration = DateTime.Now - stat.LastRecordingStartTime;
            stat.TotalRecordingDuration += recordingDuration;
            stat.LastRecordingEndTime = DateTime.Now;

            //Console.WriteLine($"{camera.CameraName} - Recording ended. Total recording duration: {stat.TotalRecordingDuration.TotalMinutes} minutes.");
        }
        SaveStatistics();
    }

    // Method to update motion detections
    public static void MotionDetected(Camera camera)
    {
        if (!cameraStats.ContainsKey(camera.CameraIndex))
        {
            cameraStats[camera.CameraIndex] = new CameraStat(camera.CameraName);
        }

        // Increment the motion detection count
        cameraStats[camera.CameraIndex].TotalMotionDetections++;
        cameraStats[camera.CameraIndex].LastMotionDetectionTime = DateTime.Now;
        //Console.WriteLine($"{camera.CameraName} - Motion detected. Total motion detections: {cameraStats[camera.CameraName].TotalMotionDetections}");
    }

    // Method to get statistics for a specific camera
    public static CameraStat GetStatistics(Camera cam)
    {
        if (cameraStats.ContainsKey(cam.CameraIndex))
        {
            return cameraStats[cam.CameraIndex];
        }
        else
        {
            //Console.WriteLine($"No statistics found for camera: {cameraName}");
            return null;
        }
    }

    // Save statistics to a JSON file
    public static void SaveStatistics()
    {
        try
        {
            string json = JsonConvert.SerializeObject(cameraStats, Formatting.Indented);
            File.WriteAllText(statsFilePath, json);
            //Console.WriteLine("Statistics saved successfully.");
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Error saving statistics: {ex.Message}");
        }
    }

    // Load statistics from a JSON file
    public static void LoadStatistics()
    {
        try
        {
            if (File.Exists(statsFilePath))
            {
                string json = File.ReadAllText(statsFilePath);
                cameraStats = JsonConvert.DeserializeObject<Dictionary<int, CameraStat>>(json);
                //Console.WriteLine("Statistics loaded successfully.");
            }
            else
            {
                //Console.WriteLine("No existing statistics file found, creating a new one.");
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Error loading statistics: {ex.Message}");
        }
    }

    // Class to store statistics for an individual camera
    public class CameraStat
    {
        public string CameraName { get; set; }
        public int TotalRecordings { get; set; }
        public TimeSpan TotalRecordingDuration { get; set; }
        public DateTime LastRecordingStartTime { get; set; }
        public DateTime LastRecordingEndTime { get; set; }
        public int TotalMotionDetections { get; set; }
        public DateTime LastMotionDetectionTime { get; set; }

        public CameraStat(string cameraName)
        {
            CameraName = cameraName;
            TotalRecordings = 0;
            TotalRecordingDuration = TimeSpan.Zero;
            TotalMotionDetections = 0;
        }
    }
}
