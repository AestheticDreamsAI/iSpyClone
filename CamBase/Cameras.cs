using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class Cameras
{
    private static Dictionary<int, Camera> list = null;
    private static string filePath = "cam.json";
    public static void Load()
    {
        // JSON-Datei einlesen und in Dictionary<int, Camera> umwandeln
        if (File.Exists(filePath))
        {
            list = JsonConvert.DeserializeObject<Dictionary<int, Camera>>(File.ReadAllText(filePath));
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

