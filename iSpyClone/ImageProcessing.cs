using ImageMagick;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
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
        using (var img = Image.FromFile(".\\nosignal.gif"))
        {
            return camera.ImageToByteArray(img);
        }
    }

    public static async Task<byte[]> GetRecording(string path)
    {
        var i = path.Replace(".\\media\\video\\", "").Replace("\\"+Path.GetFileName(path),"");

        var camera = Cameras.Get(Convert.ToInt32(i));
        try
        {
            return camera.GetRecording(path);
        }
        catch (Exception ex)
        {
            //Console.WriteLine(ex.Message);
        }
        using (var img = Image.FromFile(".\\nosignal.gif"))
        {
            return camera.ImageToByteArray(img);
        }
    }

    public static List<string> ExtractGifFramesAsBase64(string filePath)
    {
        var index = Path.GetFileNameWithoutExtension(filePath.Replace("media\\video\\", "").Replace("\\" + Path.GetFileName(filePath), ""));

        var camera = Cameras.Get(Convert.ToInt32(index));
        List<string> framesBase64 = new List<string>();

        using (Image gifImage = Image.FromFile(camera.GetRecordingFrames(filePath)))
        {
            int frameCount = gifImage.GetFrameCount(FrameDimension.Time);

            for (int i = 0; i < frameCount; i++)
            {
                gifImage.SelectActiveFrame(FrameDimension.Time, i);

                using (MemoryStream ms = new MemoryStream())
                {
                    gifImage.Save(ms, ImageFormat.Png);
                    byte[] byteImage = ms.ToArray();
                    string base64String = Convert.ToBase64String(byteImage);
                    framesBase64.Add($"data:image/png;base64,{base64String}");
                }
            }
        }

        return framesBase64;
    }

    public static async Task<List<string>> GetFramesForRecording(string recordingPath)
    {
        List<string> frameBase64List = ExtractGifFramesAsBase64(recordingPath);
        return frameBase64List;
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


    public static async Task<byte[]> ConvertGifToVideo(string gifPath)
    {
        using (Image gifImage = Image.FromFile(gifPath))
        {
            FrameDimension dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
            int frameCount = gifImage.GetFrameCount(dimension);

            // Temporären Speicher-Stream für das Video nutzen
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Temporären Pfad für die Videodatei (kann erforderlich sein, da OpenCvSharp nicht direkt mit Streams arbeitet)
                string tempFilePath = Path.GetTempFileName() + ".avi";

                // VideoWriter initialisieren
                VideoWriter writer = new VideoWriter(tempFilePath, FourCC.XVID, Program.config.FPS, new OpenCvSharp.Size(gifImage.Width, gifImage.Height));

                // Alle Frames des GIF durchlaufen und in das Video schreiben
                for (int i = 0; i < frameCount; i++)
                {
                    gifImage.SelectActiveFrame(dimension, i);
                    using (Bitmap bmp = new Bitmap(gifImage))
                    {
                        Mat mat = BitmapToMat(bmp);
                        writer.Write(mat);
                        mat.Dispose();
                    }
                }

                // VideoWriter schließen
                writer.Release();

                // Temporäre Videodatei in MemoryStream laden
                using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.CopyTo(memoryStream);
                }

                // Temporäre Datei löschen
                File.Delete(tempFilePath);

                // Byte-Array zurückgeben
                return memoryStream.ToArray();
            }
        }
    }


    // Helper-Methode zum Konvertieren einer Bitmap zu OpenCvSharp-Mat
    private static Mat BitmapToMat(Bitmap bmp)
    {
        var mat = new Mat(bmp.Height, bmp.Width, MatType.CV_8UC3);
        for (int y = 0; y < bmp.Height; y++)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                System.Drawing.Color color = bmp.GetPixel(x, y);
                mat.Set(y, x, new Vec3b(color.B, color.G, color.R));
            }
        }
        return mat;
    }

    private static async Task CreateGif(string cameraIndex, string timestamp, List<string> imageFiles)
    {
        var c = Cameras.Get(Convert.ToInt32(cameraIndex));
        string gifFolderPath = $"{c.getDir("video")}\\{timestamp}";
        if (!Directory.Exists(gifFolderPath))
        {
            Directory.CreateDirectory(gifFolderPath); // Ensure the directory exists
        }
        string gifFilePath = Path.Combine(gifFolderPath, "animated.gif");

        using (var collection = new MagickImageCollection())
        {
            foreach (var imagePath in imageFiles)
            {
                if(!DataChecker.IsFileCorrupt(imagePath)) 
                    {
                        var image = new MagickImage(imagePath);
                        image.AnimationDelay = 50; // Frame-Delay setzen (anpassbar)

                        // Reduziere die Bildgröße (optional, um die Dateigröße zu reduzieren)
                        image.Resize(500, 0); // z.B. auf 300px Breite skalieren, Höhe proportional

                        // Reduziere die Farbanzahl (z.B. auf 128 Farben, um die Dateigröße zu minimieren)
                        image.Quantize(new QuantizeSettings
                        {
                            Colors = Program.config.Quality
                        });

                        collection.Add(image);
                    }
                        File.Delete(imagePath);
            }

            // GIF optimieren (entfernt redundante Frames, reduziert die Größe weiter)
            collection.Optimize();
            gifFolderPath = $"{c.getDir("video")}\\{timestamp}";
            if (!Directory.Exists(gifFolderPath))
            {
                Directory.CreateDirectory(gifFolderPath); // Ensure the directory exists
            }
            gifFilePath = Path.Combine(gifFolderPath, "animated.gif");
            // GIF an den gewünschten Ort speichern
            collection.Write(gifFilePath);
        }
    }




    public static async Task SaveSnapshots(Camera cam, int totalframes)
    {
        int imageCount = 0;
        var d = DateTime.Now;
        await cam.AltDirCheck();
        // Safe timestamp format for folder creation
        string timestamp = d.ToString("dd-MM-yyyy_HH-mm-ss");

        // Create the path as media/images/{cam.CameraIndex}/{timestamp}
        string folderPath = Path.Combine(cam.getDir("images"), timestamp);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath); // Ensure the directory exists
        }
        List<string> imageFiles = new List<string>(); // To store captured image paths

        while (imageCount < totalframes)
        {
            folderPath = Path.Combine(cam.getDir("images"), timestamp);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath); // Ensure the directory exists
            }
            var snap = await cam.getSnapshot(); // Capture image from the camera
            var img = snap.Item1;
            if (snap.Item2)
            {
                var fileName = Path.Combine(folderPath, $"image_{imageCount + 1}.jpg");

                // Save the image as JPEG
                img.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);

                imageFiles.Add(fileName); // Add file path to the list
                imageCount++;
                await Task.Delay(Motion.Interval * 1000); // Pause for 500ms (adjust the delay as needed)
            }
        }
        await CreateGif(cam.CameraIndex.ToString(), timestamp, imageFiles);

    }
}

