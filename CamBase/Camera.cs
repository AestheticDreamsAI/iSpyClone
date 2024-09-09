using AForge.Vision.Motion;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class Camera
{
    public string CameraName { get; set; }
    public int CameraIndex { get; set; }
    public string CameraUrl { get; set; }
    public string CameraUser { get; set; }
    public string CameraPass { get; set; }
    public bool IsRecording { get; set; } // Neue Eigenschaft hinzugefügt
    public MotionDetector MotionDetector { get; set; }

    public Camera Clone()
    {
        return new Camera
        {
            CameraName = this.CameraName,
            CameraIndex = this.CameraIndex,
            CameraUrl = this.CameraUrl,
            CameraUser = this.CameraUser,
            CameraPass = this.CameraPass,
            IsRecording = false,
            MotionDetector = null // Clone MotionDetector if not null
        };
    }

    public async Task<(Image,bool)> getSnapshot()
    {
        var cameraUrl = CameraUrl
            .Replace("[username]", CameraUser)
            .Replace("[password]", CameraPass);

        try
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(5); // Timeout configuration

                using (Stream stream = await client.GetStreamAsync(cameraUrl))
                {
                    return (Image.FromStream(stream),true);
                }
            }
        }
        catch (TaskCanceledException ex)
        {
        }
        catch (Exception ex)
        {
        }

        // Fallback to GIF
        try
        {
            return (Image.FromFile(".\\nosignal.gif"),false); // This will load the entire GIF including animation
        }
        catch (Exception ex)
        {
            return (null,false); // Return null if even the fallback fails
        }
    }

    public byte[] ImageToByteArray(Image image)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            // Save the image to the MemoryStream in the original format
            image.Save(ms, image.RawFormat);
            return ms.ToArray();
        }
    }
}
