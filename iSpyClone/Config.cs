using Newtonsoft.Json;

public class Config
{
    public string SavingDir { get; set; } = ".\\media";
    public int WebserverPort { get; set; } = 8040;
    public int MaxCamFails { get; set; } = 3;

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