using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public class HttpServer
{
    private HttpListener _listener;
    private bool _isRunning;

    public HttpServer(string[] prefixes)
    {
        _listener = new HttpListener();
        foreach (var prefix in prefixes)
        {
            _listener.Prefixes.Add(prefix);
        }
    }

    public async Task StartAsync(CancellationToken cts)
    {
        _isRunning = true;
        _listener.Start();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("- Webserver started...");
        Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------");
        while (!cts.IsCancellationRequested)
        {
            try 
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequestAsync(context));
            }
            catch { }
        }
        Console.WriteLine("- Webserver stopped...");
    }

    public void Stop()
    {
        _isRunning = false;
        _listener.Stop();
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        try
        {
            string path = context.Request.Url.AbsolutePath.ToLower();
            var request = context.Request;

            if (request.HttpMethod == "POST")
            {
                // Read incoming data stream
                using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string requestBody = reader.ReadToEnd();

                    // Parse POST data
                    var parsedFormData = ParseFormData(requestBody);
                    if (path.Contains("/add"))
                    {
                        // Generiere einen verfügbaren CameraIndex
                        var cam = new Camera
                        {
                            CameraIndex = await GetAvailableCameraIndex() // Verwende die Funktion hier
                        };

                        // Setze die Kameraeigenschaften aus den Formulardaten
                        if (parsedFormData.ContainsKey("camera-name"))
                        {
                            cam.CameraName = (string)parsedFormData["camera-name"];
                        }

                        if (parsedFormData.ContainsKey("username"))
                        {
                            cam.CameraUser = (string)parsedFormData["username"];
                        }

                        if (parsedFormData.ContainsKey("password"))
                        {
                            cam.CameraPass = (string)parsedFormData["password"];
                        }

                        if (parsedFormData.ContainsKey("camera-url"))
                        {
                            cam.CameraUrl = (string)parsedFormData["camera-url"];
                        }

                        // Füge die neue Kamera hinzu
                        Cameras.Add(cam.CameraIndex, cam);

                        // Weiterleitung nach dem Hinzufügen
                        context.Response.Redirect("/");
                        context.Response.OutputStream.Close();
                        return;
                    }

                    else if (path.Contains("/edit"))
                    {
                        if (parsedFormData.ContainsKey("camera-index"))
                        {
                            var cam = Cameras.Get((int)parsedFormData["camera-index"]);
                            if (cam != null)
                            {
                                // Check if "delete-camera" exists and perform delete if it's set to "on"
                                if (parsedFormData.ContainsKey("delete-camera") && (string)parsedFormData["delete-camera"] == "on")
                                {
                                    Cameras.Remove(cam.CameraIndex);
                                }
                                else
                                {
                                    // Update existing camera properties if delete is not requested
                                    if (parsedFormData.ContainsKey("camera-name"))
                                    {
                                        cam.CameraName = (string)parsedFormData["camera-name"];
                                    }

                                    if (parsedFormData.ContainsKey("camera-user"))
                                    {
                                        cam.CameraUser = (string)parsedFormData["camera-user"];
                                    }

                                    if (parsedFormData.ContainsKey("camera-pass"))
                                    {
                                        cam.CameraPass = (string)parsedFormData["camera-pass"];
                                    }

                                    if (parsedFormData.ContainsKey("camera-url"))
                                    {
                                        cam.CameraUrl = (string)parsedFormData["camera-url"];
                                    }
                                }

                                // Redirect after editing or deleting
                                context.Response.Redirect("/");
                                context.Response.OutputStream.Close();
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                if (path.Contains("/manifest.json"))
                {
                    var s = File.ReadAllText(".\\www\\manifest.json");
                    byte[] b = Encoding.UTF8.GetBytes(s);

                    context.Response.ContentType = "application/json";
                    context.Response.ContentLength64 = b.Length;
                    await context.Response.OutputStream.WriteAsync(b, 0, b.Length);
                    context.Response.OutputStream.Close();
                    return;
                }
                else if (path.Contains(".png"))
                {
                    byte[] b = File.ReadAllBytes(".\\www\\icon.png");

                    context.Response.ContentType = "image/png";
                    context.Response.ContentLength64 = b.Length;
                    await context.Response.OutputStream.WriteAsync(b, 0, b.Length);
                    context.Response.OutputStream.Close();
                    return;
                }
                else if (path.Contains("/status"))
                {
                    var s = Cameras.GetJson();
                    byte[] b = Encoding.UTF8.GetBytes(s);

                    context.Response.ContentType = "application/json";
                    context.Response.ContentLength64 = b.Length;
                    await context.Response.OutputStream.WriteAsync(b, 0, b.Length);
                    context.Response.OutputStream.Close();
                    return;
                }
                else if (path.Contains("/stream/"))
                {
                    var indexStr = path.Replace("/stream/", "");
                    if (int.TryParse(indexStr, out int index))
                    {
                        byte[] imageData = await ImageProcessing.GetStreamAsync(index);
                        if (imageData != null)
                        {
                            context.Response.ContentType = "image/jpeg";
                            context.Response.ContentLength64 = imageData.Length;
                            await context.Response.OutputStream.WriteAsync(imageData, 0, imageData.Length);
                            context.Response.OutputStream.Close();
                            return;
                        }
                    }
                }
                else if (path.Contains("/video/"))
                {
                    byte[] imageData = await ImageProcessing.GetRecording($".{path.Replace("/", "\\")}");
                    if (imageData != null)
                    {
                        context.Response.ContentType = "image/gif";
                        context.Response.ContentLength64 = imageData.Length;
                        await context.Response.OutputStream.WriteAsync(imageData, 0, imageData.Length);
                        context.Response.OutputStream.Close();
                        return;
                    }
                }

                // Handle other requests (assumed to be text/HTML)
                string responseString = await GetPageContent(context, path);
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                context.Response.ContentType = "text/html";
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async Task<int> GetAvailableCameraIndex()
    {
        int newIndex = 0;
        // Schleife, um einen nicht existierenden Index zu finden
        while (Cameras.All().Any(c => c.CameraIndex == newIndex))
        {
            newIndex++;
        }

        return newIndex;
    }



    private async Task<string> GetPageContent(HttpListenerContext context, string path)
    {
        path = path.ToLower();

        if (path == "/")
        {
            return await Dashboard.HomePage();
        }
        else if (path == "/add")
        {
            return await Dashboard.AddPage();
        }
        else if (path.Contains("/view/"))
        {
            var indexStr = path.Replace("/view/", "");
            if (int.TryParse(indexStr, out int index))
            {
                context.Response.ContentType = "text/html";
                return await Dashboard.ViewPage(index);
            }
        }
        else if (path.Contains("/edit/"))
        {
            var indexStr = path.Replace("/edit/", "");
            if (int.TryParse(indexStr, out int index))
            {
                context.Response.ContentType = "text/html";
                return await Dashboard.EditPage(index);
            }
        }
        else if (path.Contains("/stats"))
        {
            context.Response.ContentType = "text/html";
            return await Dashboard.CameraStatisticsPage();
        }
        else if (path.Contains("/logs"))
        {
            context.Response.ContentType = "text/html";
            return await Dashboard.LogPage();
        }
        return "<html><body><h1>404 Not Found</h1></body></html>";
        
    }

    private static Dictionary<string, object> ParseFormData(string formData)
    {
        var result = new Dictionary<string, object>();

        // Form-Daten splitten: 'key=value&key2=value2'
        string[] pairs = formData.Split('&');

        foreach (string pair in pairs)
        {
            // Einzelne Paare splitten in 'key' und 'value'
            string[] keyValue = pair.Split('=');

            if (keyValue.Length == 2)
            {
                string key = Uri.UnescapeDataString(keyValue[0]);
                string value = Uri.UnescapeDataString(keyValue[1]);

                // Try parsing the value to int, bool, or fallback to string
                if (int.TryParse(value, out int intValue))
                {
                    result[key] = intValue;
                }
                else if (bool.TryParse(value, out bool boolValue))
                {
                    result[key] = boolValue;
                }
                else
                {
                    result[key] = value;
                }
            }
            else if (keyValue.Length == 1)
            {
                // Falls nur ein Schlüssel ohne Wert vorhanden ist (z.B. 'key=' oder 'key')
                string key = Uri.UnescapeDataString(keyValue[0]);
                result[key] = null; // Oder lege hier einen Standardwert fest
            }
        }

        return result;
    }




}