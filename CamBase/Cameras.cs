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
        // JSON-Datei einlesen und in Dictionary<int, Camera> umwandeln
        list.Clear();
        if (File.Exists(filePath))
        {
            list = JsonConvert.DeserializeObject<Dictionary<int, Camera>>(File.ReadAllText(filePath));
            foreach(Camera cam in list.Values)
            {
                var dir1 = Program.manager.getDirectory() + $"\\images\\{cam.CameraIndex}";
                var dir2 = Program.manager.getDirectory() + $"\\video\\{cam.CameraIndex}";
                if (!Directory.Exists(dir1))
                {
                    Directory.CreateDirectory(dir1);
                }
                if (!Directory.Exists(dir2))
                {
                    Directory.CreateDirectory(dir2);
                }
            }
        }
        else
        {
            Console.WriteLine("JSON-Datei nicht gefunden.");
            return;
        }
    }
    public static void Save()
    {
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

