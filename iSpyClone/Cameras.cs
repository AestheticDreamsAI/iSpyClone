using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class Cameras
{
    private static Dictionary<int, Camera> list = new Dictionary<int, Camera>();
    private static string filePath = "cam.json";
    public static void Load()
    {
        // Clear the list before loading new data
        list.Clear();

        // Check if the main file exists, otherwise fallback to the backup
        string fileToLoad = File.Exists(filePath) ? filePath : (File.Exists(filePath + ".bak") ? filePath + ".bak" : null);

        if (fileToLoad == null)
        {
            Console.WriteLine("Neither the main file nor the backup file was found.");
            return;
        }

        try
        {
            // Read and deserialize the JSON file into the dictionary
            list = JsonConvert.DeserializeObject<Dictionary<int, Camera>>(File.ReadAllText(fileToLoad)) ?? new Dictionary<int, Camera>();

            // Create necessary directories for each camera
            foreach (Camera cam in list.Values)
            {
                CreateDirectoryIfNotExists(Program.manager.getDirectory() + $"\\images\\{cam.CameraIndex}");
                CreateDirectoryIfNotExists(Program.manager.getDirectory() + $"\\video\\{cam.CameraIndex}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load data: {ex.Message}");
            // Initialize list to an empty dictionary to avoid null references
            list = new Dictionary<int, Camera>();
        }
    }

    // Helper function to create directory if it doesn't exist
    private static void CreateDirectoryIfNotExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }


    public static bool RecordingState()
    {
        // Prüft, ob es Kameras gibt und ob alle inaktiv sind
        return list.Values.All(cam => cam.IsRecording);
    }

    public static void Save()
    {
        File.Copy(filePath, $"{filePath}.bak");
        Dictionary<int, Camera> l = new Dictionary<int, Camera>();
        foreach (var kvp in list)
        {
            l.Add(kvp.Key, kvp.Value.Clone());
        }

        File.WriteAllText(filePath, JsonConvert.SerializeObject(l));
        Load();
    }

    public static void Add(int index, Camera camera)
    {
        list.Add(index, camera);
        Save();
    }
    public static void Remove(int index)
    {
        var el = list.Where(x=>x.Value.CameraIndex == index).FirstOrDefault();
        if (el.Value != null)
        {
            list.Remove(el.Key);
            Save();
        }
    }

    public static Camera Get(int index)
    {
        return list[index];
    }

    public static List<Camera> All()
    {
        return list.Values.ToList();
    }

    internal static string GetJson()
    {
        Dictionary<int, Camera> l = new Dictionary<int, Camera>();
        foreach (var kvp in list)
        {
            var c = kvp.Value.Clone();
            c.IsRecording=kvp.Value.IsRecording;
            l.Add(kvp.Key, c);
        }
        return JsonConvert.SerializeObject(l);
    }
}

