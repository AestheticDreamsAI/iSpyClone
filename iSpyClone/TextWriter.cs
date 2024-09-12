using System.Text;
using System.Collections.Generic;

public class ConsoleEvent
{
    public string Message { get; set; }
    public string Color { get; set; }
    public DateTime Timestamp { get; set; }

    public ConsoleEvent(string message, string color)
    {
        Message = message;
        Color = color;
        Timestamp = DateTime.Now;
    }
}

public static class ColorConverter
{
    public static string ToHex(ConsoleColor color)
    {
        // Mapping von ConsoleColor zu entsprechenden Hex-Werten
        return color switch
        {
            ConsoleColor.Black => "#000000",
            ConsoleColor.DarkBlue => "#00008B",
            ConsoleColor.DarkGreen => "#006400",
            ConsoleColor.DarkCyan => "#008B8B",
            ConsoleColor.DarkRed => "#8B0000",
            ConsoleColor.DarkMagenta => "#8B008B",
            ConsoleColor.DarkYellow => "#B8860B",
            ConsoleColor.Gray => "#808080",
            ConsoleColor.DarkGray => "#A9A9A9",
            ConsoleColor.Blue => "#0000FF",
            ConsoleColor.Green => "#008000",
            ConsoleColor.Cyan => "#00FFFF",
            ConsoleColor.Red => "#FF0000",
            ConsoleColor.Magenta => "#FF00FF",
            ConsoleColor.Yellow => "#FFFF00",
            ConsoleColor.White => "#FFFFFF",
            _ => "#000000"  // Standard: Schwarz, wenn die Farbe nicht erkannt wird
        };
    }
}

public class CustomTextWriter : TextWriter
{
    private readonly TextWriter _originalOut;
    private readonly List<ConsoleEvent> _eventLog = new List<ConsoleEvent>();

    public CustomTextWriter(TextWriter originalOut)
    {
        _originalOut = originalOut;
    }

    public override Encoding Encoding => _originalOut.Encoding;

    public override void WriteLine(string message)
    {
        var m = message;
        if (!m.Contains(":"))
        {
            if (m.Contains("- ") && !m.Contains("--") && !m.Contains("__"))
            {
                m = m.Replace("- ", $"- {DateTime.Now.ToLongTimeString()}: ");
            }
        }

            // Konvertiere die Konsolenfarbe in einen HTML-Hex-Code
           var currentColor = Console.ForegroundColor;
            string colorHex = ColorConverter.ToHex(currentColor);
        if (m.Contains(":"))
        {
            _eventLog.Add(new ConsoleEvent(m.Replace("- ", $"- {DateTime.Now.ToShortDateString()} - "), colorHex));
        }
            // Schreibe die Nachricht in die Konsole
            _originalOut.WriteLine($"{message}");
        
    }

    // Methode zum Abrufen aller Ereignisse
    public List<ConsoleEvent> GetEventLog()
    {
        return _eventLog;
    }
}

