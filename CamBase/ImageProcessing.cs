using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    public class ImageProcessing
    {
    public static async Task<byte[]> GetStreamAsync(int index)
    {
        var camera = new Camera();
        try
        {
            camera = Cameras.Get(index);
            var snap = await camera.getSnapshot();
            if (snap.Item2)
            {
                return camera.ImageToByteArray(snap.Item1);
            }
        }
        catch (Exception ex)
        {

        }
        return camera.ImageToByteArray(Image.FromFile(".\\nosignal.gif"));
    }

    public static async Task<byte[]> GetRecording(string path)
    {
        var camera = new Camera();
        try
        {
            var f = Directory.GetFiles(path, "*.*");
            var t = f.FirstOrDefault();
            return camera.ImageToByteArray(Image.FromFile(t));
        }
        catch (Exception ex)
        {
            //Console.WriteLine(ex.Message);
        }
        return camera.ImageToByteArray(Image.FromFile(".\\nosignal.gif"));
    }

    public static Bitmap ResizeImage(Image image, int width, int height)
    {
        Rectangle destRect = new Rectangle(0, 0, width, height);
        Bitmap bitmap = new Bitmap(width, height);
        bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        using ImageAttributes imageAttributes = new ImageAttributes();
        imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
        return bitmap;
    }
    private static async Task CreateGif(string cameraIndex, string timestamp, List<string> imageFiles)
    {
        string gifFolderPath = Path.Combine("media", "video", cameraIndex, timestamp);
        Directory.CreateDirectory(gifFolderPath); // Sicherstellen, dass das Verzeichnis existiert

        string gifFilePath = Path.Combine(gifFolderPath, "animated.gif");

        using (var collection = new MagickImageCollection())
        {
            foreach (var imagePath in imageFiles)
            {
                var image = new MagickImage(imagePath);
                image.AnimationDelay = 50; // Frame-Delay setzen (anpassbar)

                // Reduziere die Bildgröße (optional, um die Dateigröße zu reduzieren)
                image.Resize(300, 0); // z.B. auf 300px Breite skalieren, Höhe proportional

                // Reduziere die Farbanzahl (z.B. auf 128 Farben, um die Dateigröße zu minimieren)
                image.Quantize(new QuantizeSettings
                {
                    Colors = 128
                });

                collection.Add(image);
            }

            // GIF optimieren (entfernt redundante Frames, reduziert die Größe weiter)
            collection.Optimize();

            // GIF an den gewünschten Ort speichern
            collection.Write(gifFilePath);
        }
    }


    public static async Task SaveSnapshots(Camera cam, int v)
    {
        int imageCount = 0;
        var d = DateTime.Now;

        // Safe timestamp format for folder creation
        string timestamp = d.ToString("dd-MM-yyyy_HH-mm-ss");

        // Create the path as media/images/{cam.CameraIndex}/{timestamp}
        string folderPath = Path.Combine("media", "images", cam.CameraIndex.ToString(), timestamp);
        Directory.CreateDirectory(folderPath); // Ensure the directory exists

        List<string> imageFiles = new List<string>(); // To store captured image paths

        while (imageCount < v)
        {
            var snap = await cam.getSnapshot(); // Capture image from the camera
            var img = snap.Item1;
            if (snap.Item2)
            {
                var fileName = Path.Combine(folderPath, $"image_{imageCount + 1}.jpg");

                // Save the image as JPEG
                img.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);

                imageFiles.Add(fileName); // Add file path to the list
                imageCount++;
                await Task.Delay(Motion.Interval* 1000); // Pause for 500ms (adjust the delay as needed)
            }
        }
        await CreateGif(cam.CameraIndex.ToString(), timestamp, imageFiles);

    }
}

