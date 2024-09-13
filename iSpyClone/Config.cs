using Newtonsoft.Json;

public class Config
{
    public string SavingDir { get; set; } = ".\\media";
    public int MaxSpaceUsage { get; set; } = 5;
    public int WebserverPort { get; set; } = 8040;
    public int MaxCamFails { get; set; } = 3;
    public int Quality { get; set; } = 255;
    public int MaxMemoryUsage { get; set; } = 500;
    public int FPS { get; set; } = 2;
    public int RecordingTime { get; set; } = 60;


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