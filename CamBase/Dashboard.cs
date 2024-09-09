﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


internal class Dashboard
{

    public static async Task<string> HomePage()
    {
        var _camGrids_ = "";
        foreach (Camera cam in Cameras.All())
        {
            var detection = cam.IsRecording;
            var isRecording = detection ? "<i class=\"fas fa-record-vinyl\" style=\"color: red;\"></i> Recording" : "-";
            _camGrids_ += @"
                <div class=""camera-card"">
                    <div class=""camera-image"">            <div class='connecting-overlay'>Connecting...</div><img src='/stream/" + cam.CameraIndex + @"'></div>
                    <div class=""camera-info"">
                        <span class=""camera-name"">" + cam.CameraName + @"</span>
                        <span class=""recording-status"">
                            " + isRecording + @"
                        </span>
                    </div>
                    <div class=""camera-actions"">
                        <button class=""btn btn-primary"" onclick=""editCamera(" + cam.CameraIndex + @")"">Edit</button>
                        <button class=""btn btn-secondary"" onclick=""viewCamera(" + cam.CameraIndex + @")"">View</button>
                    </div>
                </div>";
        }

        return @"
<!DOCTYPE html>
<html lang=""en"">
<head><link rel=""icon"" type=""image/png"" href=""logo.png""><meta name='theme-color' content='#333333'>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Security Camera WebUI</title>
<link rel=""manifest"" href=""manifest.json?t="+Guid.NewGuid().ToString()+@""">
          <script src='https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.3/js/all.min.js'></script>
<style>
  html, body {
    -webkit-user-select: none; /* Verhindern von Textauswahl auf iOS */
  }
        :root {
            --bg-color: #1a1a1a;
            --text-color: #ffffff;
            --primary-color: #3498db;
            --secondary-color: #2ecc71;
            --danger-color: #e74c3c;
        }

        body,html {
            font-family: Arial, sans-serif;
            background-color: var(--bg-color);
            color: var(--text-color);
            margin: 0;
            padding: 0;
        }

        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
        }

        h1 {
            text-align: center;
        }

        .camera-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
            gap: 20px;
        }

        .camera-card {
            background-color: #2c2c2c;
            border-radius: 8px;
            padding: 15px;
            display: flex;
            flex-direction: column;
        }

        .camera-image {
            width: 100%;
            height: 150px;
            background-color: #444;
            border-radius: 4px;
            margin-bottom: 10px;
            position:relative;
            overflow:hidden;
        }
        .camera-image img{
            position:absolute;
            width:100%;
            height:100%;
            z-index:10px;
        }
        

        .camera-info {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 10px;
        }

        .camera-name {
            font-weight: bold;
        }

        .recording-status {
            display: flex;
            align-items: center;
        }

        .recording-status i {
            margin-right: 5px;
        }

        .camera-actions {
            display: flex;
            justify-content: space-between;
        }

        .btn {
            padding: 8px 15px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s;
        }

        .btn-primary {
            background-color: var(--primary-color);
            color: white;
        }

        .btn-secondary {
            background-color: var(--secondary-color);
            color: white;
        }

        .btn:hover {
            opacity: 0.8;
        }

        .add-camera {
            margin-bottom: 20px; /* Abstand zwischen Button und Grid */
            text-align: left; /* Button linksbündig ausrichten */
        }

        @media (max-width: 768px) {
            .camera-grid {
                grid-template-columns: 1fr;
                height:300px;
            }
            .camera-image{
            height:200px;
            }
        }
        .navbar {
            display: flex;
            justify-content: flex-end;
            background-color: #333;
            padding: 10px;
        }

        .navbar a {
            color: white;
            padding: 14px 20px;
            text-decoration: none;
            text-align: center;
        }
        .navbar a:hover {
            background-color: #ddd;
            color: black;
        }
        .connecting-overlay {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0, 0, 0, 0.5);
            color: white;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 24px;
            z-index: 0;
        }
        .navbar .addCam{
                    background-color: var(--primary-color);
            color: white;
            border-radius:20px;
        }
/* For WebKit browsers (Chrome, Safari) */
::-webkit-scrollbar {
    width: 8px; /* Width of the scrollbar */
}

::-webkit-scrollbar-thumb {
    background-color: #444; /* Dark scrollbar color */
    border-radius: 10px; /* Rounded corners */
}

::-webkit-scrollbar-thumb:hover {
    background-color: #555; /* Lighter shade on hover */
}

::-webkit-scrollbar-track {
    background-color: #222; /* Dark background for the scrollbar track */
}

/* For Firefox */
* {
    scrollbar-width: thin; /* Thin scrollbar */
    scrollbar-color: #444 #222; /* Thumb color and track color */
}

    </style>
</head>
<body>
<div class=""navbar"">
    <a href=""../"">Home</a>
    <a href=""../add"" class='addCam'>Add Camera</a>
</div>
        <h1>" + Console.Title+@"</h1>
    <div class=""container"">
        <div class=""camera-grid"">
            " + _camGrids_ + @"
        </div> <!-- End of camera-grid -->
        
    </div>

    <script>
document.addEventListener('DOMContentLoaded', function() {
function fetchCameraStatus() {
    fetch('/status')
        .then(response => response.json())
        .then(data => {
            console.log(data);  // Um die Antwortstruktur zu prüfen
            updateCameraGrid(data);
        })
        .catch(error => console.error('Error fetching camera status:', error));
}


function updateCameraGrid(data) {
    const cameras = Object.values(data);  // Konvertiere das nummerierte Objekt in ein Array
    const container = document.querySelector('.camera-grid');
    container.innerHTML = '';  // Leere den Container, bevor neue Inhalte hinzugefügt werden

    cameras.forEach(cam => {
        const isRecording = cam.IsRecording ? 
            ""<i class=\""fas fa-record-vinyl\"" style=\""color: red;\""></i> Recording"" : ""-"";

        // Hier fügen wir einen Cache-Busting Parameter (Zeitstempel) hinzu
        const imgUrl = `/stream/${cam.CameraIndex}?t=${new Date().getTime()}`;

        const cameraCard = `
            <div class=""camera-card"">
                <div class=""camera-image"">
                    <div class='connecting-overlay'>Connecting...</div>
                    <img src='${imgUrl}' />
                </div>
                <div class=""camera-info"">
                    <span class=""camera-name"">${cam.CameraName}</span>
                    <span class=""recording-status"">${isRecording}</span>
                </div>
                <div class=""camera-actions"">
                    <button class=""btn btn-primary"" onclick=""editCamera(${cam.CameraIndex})"">Edit</button>
                    <button class=""btn btn-secondary"" onclick=""viewCamera(${cam.CameraIndex})"">View</button>
                </div>
            </div>`;
        container.innerHTML += cameraCard;
    });
}

// Automatische Aktualisierung alle 5 Sekunden
setInterval(fetchCameraStatus, 15000);

// Initialer Abruf beim Laden der Seite
fetchCameraStatus();

});

        function editCamera(id) {
 window.location.href='/edit/'+id;
        }

function viewCamera(id) {
 window.location.href='/view/'+id;
}


        function addCamera() {
            alert('Adding a new camera');
        }
    </script>
</body>
</html>";
    }


    public static async Task<string> ViewPage(int index)
    {
        var camera = new Camera();
        var _recordings_ = "";
        try
        {
            camera = Cameras.Get(index);
            foreach (var f in Directory.GetDirectories($@".\media\video\{camera.CameraIndex}\", "*.*"))
            {
                var p = Path.GetFileName(f).Replace("_"," ").Replace("-",".").Split(' ');
                _recordings_ += $@"
                <li class=""recording-item"">
                    <span>{p[0]} - {p[1].Replace(".",":")} - Motion Detected</span>
                    <button class=""btn btn-secondary"" onclick=""playRecording('{camera.CameraIndex}/{Path.GetFileName(f)}')"">Play</button>
                </li>";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }


        return $@"
<!DOCTYPE html>
<html lang='en'>
<head><link rel=""icon"" type=""image/png"" href=""logo.png""><meta name='theme-color' content='#333333'>
    <meta charset='UTF-8'>
    <title>View Camera-Security Camera WebUI</title>
<link rel=""manifest"" href=""manifest.json"">
          <script src='https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.3/js/all.min.js'></script>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        :root {{--bg-color: #1a1a1a;
            --text-color: #ffffff;
            --primary-color: #3498db;
            --secondary-color: #2ecc71;
            --danger-color: #e74c3c;
            --card-bg: #2c2c2c;
        }}

        body,html {{font-family: Arial, sans-serif;
            background-color: var(--bg-color);
            color: var(--text-color);
            margin: 0;
            padding: 0;
        }}

        .container {{max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
        }}

        h1, h2 {{text-align: center;
        }}

        .live-feed {{background-color: var(--card-bg);
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 20px;
        }}

        .video-container {{position: relative;
            padding-bottom: 56.25%; /* 16:9 aspect ratio */
            height: 0;
            overflow: hidden;
        }}

        .video-container img {{position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            object-fit: cover;
            z-index:10;
        }}

        .camera-controls {{display: flex;
            justify-content: space-between;
            margin-top: 20px;
        }}

        .btn {{padding: 10px 20px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s;
        }}

        .btn-primary {{background-color: var(--primary-color);
            color: white;
        }}

        .btn-secondary {{background-color: var(--secondary-color);
            color: white;
        }}

        .btn:hover {{opacity: 0.8;
        }}

        .recordings {{background-color: var(--card-bg);
            border-radius: 8px;
            padding: 20px;
        }}

        .recording-list {{list-style-type: none;
            padding: 0;
        }}

        .recording-item {{display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 10px;
            border-bottom: 1px solid #444;
        }}

        .recording-item:last-child {{border-bottom: none;
        }}

        @media (max-width: 768px) {{
            .camera-controls {{
                flex-direction: column;
            }}
            .camera-controls .btn {{
                margin-bottom: 10px;
            }}
        }}
    h1, h2{{
text-align:center;
            
            }}
        .navbar {{display: flex;
            justify-content: flex-end;
            background-color: #333;
            padding: 10px;
        }}

        .navbar a {{color: white;
            padding: 14px 20px;
            text-decoration: none;
            text-align: center;
        }}
        .navbar a:hover {{
            background-color: #ddd;
            color: black;
        }}
        .connecting-overlay {{
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0, 0, 0, 0.5);
            color: white;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 24px;
            z-index: 0;
        }}
        .navbar .addCam{{
                    background-color: var(--primary-color);
            color: white;
            border-radius:20px;
        }}

/* For WebKit browsers (Chrome, Safari) */
::-webkit-scrollbar {{
    width: 8px; /* Width of the scrollbar */
}}

::-webkit-scrollbar-thumb {{
    background-color: #444; /* Dark scrollbar color */
    border-radius: 10px; /* Rounded corners */
}}

::-webkit-scrollbar-thumb:hover {{
    background-color: #555; /* Lighter shade on hover */
}}

::-webkit-scrollbar-track {{
    background-color: #222; /* Dark background for the scrollbar track */
}}

/* For Firefox */
* {{
    scrollbar-width: thin; /* Thin scrollbar */
    scrollbar-color: #444 #222; /* Thumb color and track color */
}}


    </style>
</head>
<body>
<div class=""navbar"">
    <a href=""../"">Home</a>
    <a href=""../add"" class='addCam'>Add Camera</a>
</div>
    <div class='container'>
        <h1>{camera.CameraName}</h1>

        <div class='live-feed'>
            <h2>Live Feed</h2>
            <div class='video-container'>
            <div class='connecting-overlay'>Connecting...</div>
                <img src='../stream/{camera.CameraIndex}' alt='Live Camera Feed' class='camera-img'>
            </div>
            <div class='camera-controls'>
                <button class='btn btn-primary' onclick='toggleRecording()' style='display:none'>
                    <i class='fas fa-record-vinyl'></i> Start Recording
                </button>
                <button class='btn btn-secondary' onclick='takePicture()' style='display:none'>
                    <i class='fas fa-camera'></i> Take Picture
                </button>
                <button id='fullscreen-btn' class='btn btn-primary'>
                    <i class='fas fa-expand'></i> Fullscreen
                </button>
            </div>
        </div>

        <div class='recordings'>
            <h2>Recordings</h2>
            <ul class='recording-list'>
 {_recordings_}
            </ul>
        </div>
    </div>


<script>
document.addEventListener('DOMContentLoaded', function() {{
    let abortController = null;
    let timeoutId = null;
    let intervalId = null;
    const cameraImg = document.querySelector('.camera-img');
    const image = document.querySelector('.video-container');
    const button = document.getElementById('fullscreen-btn');
    let isPlaying = false; // Controls whether recording is playing

    // Fullscreen toggle handler
    button.addEventListener('click', () => {{
        const requestFullscreen = image.requestFullscreen || image.mozRequestFullScreen || image.webkitRequestFullscreen || image.msRequestFullscreen;
        if (requestFullscreen) requestFullscreen.call(image);
    }});

    // Function to start live feed fetching every second
    function startLiveFeed() {{
        console.log(""Starting live feed..."");
        intervalId = setInterval(() => {{
            if (isPlaying) {{
                console.log(""Skipping live feed fetch: Recording is playing"");
                return;
            }}

            fetch(`/stream/{camera.CameraIndex}`, {{ cache: 'no-cache' }})
                .then(response => {{
                    if (!response.ok) throw new Error('Image fetch error');
                    return response.blob();
                }})
                .then(blob => {{
                    console.log(""Fetched live feed blob, size:"", blob.size);
                    if (blob.size === 0) {{
                        console.error(""Empty blob for live feed"");
                        return;
                    }}
                    const objectURL = URL.createObjectURL(blob);
                    cameraImg.src = objectURL;

                    cameraImg.onload = () => URL.revokeObjectURL(objectURL);
                }})
                .catch(error => console.error('Error fetching live camera image:', error));
        }}, 1000);
    }}

    // Function to stop live feed fetching
    function stopLiveFeed() {{
        if (intervalId) {{
            console.log(""Stopping live feed..."");
            clearInterval(intervalId);
            intervalId = null;
        }}
    }}

    // Start live feed when the page is loaded
    startLiveFeed();

    // Play specific recording by timestamp
    window.playRecording = function(timestamp) {{
  window.scrollTo({{
                top: 0,
                behavior: 'smooth'
            }});

        console.log(""Attempting to play recording for timestamp:"", timestamp);

        // Stop live feed fetching
        stopLiveFeed();

        // Cancel any ongoing recording playback
        if (abortController) {{
            console.log(""Aborting previous fetch..."");
            abortController.abort();
        }}
        if (timeoutId) clearTimeout(timeoutId);

        // Set the isPlaying flag to true to prevent live feed fetches
        isPlaying = true;

        // Fetch and play the recording
        const gifUrl = `../media/video/${{timestamp}}?t=${{Date.now()}}`;
        abortController = new AbortController();

        fetch(gifUrl, {{ cache: 'no-cache', signal: abortController.signal }})
            .then(response => {{
                if (!response.ok) throw new Error('GIF fetch error');
                return response.blob();
            }})
            .then(blob => {{
                console.log('Fetched recording blob, size:', blob.size);
                if (blob.size === 0) {{
                    throw new Error('Empty blob received');
                }}



                cameraImg.onload = () => URL.revokeObjectURL(objectURL);
setTimeout(() => {{
                const [date, time] = timestamp.replaceAll('_', ' ').replace(`{camera.CameraIndex}/`, '').split(' ');
                document.querySelector('h2').textContent = `${{date.replaceAll('-', '.')}} - ${{time.replaceAll('-', ':')}} - Motion Detection`;

setImageSrcFromBlob(cameraImg, blob);
                // Reset to live feed after 10 seconds
                timeoutId = setTimeout(() => {{
                    console.log(""Resetting to live feed..."");
                    isPlaying = false;
                    document.querySelector('h2').textContent = 'Live Feed';

                    // Restart the live feed fetching
                    startLiveFeed();
                }}, 10000);
                }}, 1000);


            }})
            .catch(error => {{
                console.error('Error fetching recording:', error);
                isPlaying = false; // Reset playing state on error
                startLiveFeed(); // Restart live feed in case of error
            }});
    }};

function setImageSrcFromBlob(imgElement, blob) {{
  const reader = new FileReader();

  reader.onloadend = function() {{
    imgElement.src = reader.result; // Set the data URI as the img src
  }};

  reader.readAsDataURL(blob);
}}
}});
</script>





</body>
</html>";

    }




    public static async Task<string> EditPage(int index)
    {
        var camera = new Camera();
        try
        {
            camera = Cameras.Get(index);
        }
        catch (Exception ex)
        {
            // Handle exception if necessary
        }

        return @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <link rel=""icon"" type=""image/png"" href=""logo.png"">
    <meta name='theme-color' content='#333333'>
    <meta charset=""UTF-8"">
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <script src='https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.3/js/all.min.js'></script>
    <style>
        :root {
            --bg-color: #1a1a1a;
            --text-color: #ffffff;
            --primary-color: #3498db;
            --secondary-color: #2ecc71;
            --danger-color: #e74c3c;
            --input-bg: #2c2c2c;
        }

        body,html {
            font-family: Arial, sans-serif;
            background-color: var(--bg-color);
            color: var(--text-color);
            margin: 0;
            padding: 0;
        }

        .container {
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
        }

        h1 {
            text-align: center;
        }

        .edit-form {
            background-color: #2c2c2c;
            border-radius: 8px;
            padding: 20px;
        }

        .form-group {
            margin-bottom: 20px;
        }

        label {
            display: block;
            margin-bottom: 5px;
        }

        input[type=""text""],
        input[type=""url""],
        select,
        textarea {
            width: 100%;
            padding: 8px;
            border: 1px solid #444;
            border-radius: 4px;
            background-color: var(--input-bg);
            color: var(--text-color);
        }

        .btn {
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s;
        }

        .btn-primary {
            background-color: var(--primary-color);
            color: white;
        }

        .btn-danger {
            background-color: var(--danger-color);
            color: white;
        }

        .btn:hover {
            opacity: 0.8;
        }

        .form-actions {
            display: flex;
            justify-content: space-between;
            margin-top: 20px;
        }

        .preview {
            margin-top: 20px;
            text-align: center;
        }

        .preview img {
            max-width: 100%;
            border-radius: 4px;
        }

        @media (max-width: 768px) {
            .form-actions {
                flex-direction: column;
            }
            .form-actions .btn {
                margin-bottom: 10px;
            }
        }
        .navbar {
            display: flex;
            justify-content: flex-end;
            background-color: #333;
            padding: 10px;
        }

        .navbar a {
            color: white;
            padding: 14px 20px;
            text-decoration: none;
            text-align: center;
        }

        .navbar a:hover {
            background-color: #ddd;
            color: black;
        }
       .navbar {display: flex;
            justify-content: flex-end;
            background-color: #333;
            padding: 10px;
        }

        .navbar a {color: white;
            padding: 14px 20px;
            text-decoration: none;
            text-align: center;
        }
        .navbar .addCam{
                    background-color: var(--primary-color);
            color: white;
            border-radius:20px;
        }
/* For WebKit browsers (Chrome, Safari) */
::-webkit-scrollbar {
    width: 8px; /* Width of the scrollbar */
}

::-webkit-scrollbar-thumb {
    background-color: #444; /* Dark scrollbar color */
    border-radius: 10px; /* Rounded corners */
}

::-webkit-scrollbar-thumb:hover {
    background-color: #555; /* Lighter shade on hover */
}

::-webkit-scrollbar-track {
    background-color: #222; /* Dark background for the scrollbar track */
}

/* For Firefox */
* {
    scrollbar-width: thin; /* Thin scrollbar */
    scrollbar-color: #444 #222; /* Thumb color and track color */
}


    </style>
</head>
<body>
    <div class=""navbar"">
        <a href=""../"">Home</a>
        <a href=""../add"" class='addCam'>Add Camera</a>
    </div>

    <div class=""container"">
        <h1>Edit Camera</h1>

        <form class=""edit-form"" method=""post"">
            <div class=""form-group"">
                <label for=""camera-name"">Camera Name</label>
                <input type=""text"" id=""camera-name"" name=""camera-name"" value=""" + camera.CameraName + @""" required oninput=""updatePreview()"">
            </div>

            <div class=""form-group"">
                <label for=""camera-user"">Username</label>
                <input type=""text"" id=""camera-user"" name=""camera-user"" value=""" + camera.CameraUser + @""" required oninput=""updatePreview()"">
            </div>

            <div class=""form-group"">
                <label for=""camera-pass"">Password</label>
                <input type=""text"" id=""camera-pass"" name=""camera-pass"" value=""" + camera.CameraPass + @""" required oninput=""updatePreview()"">
            </div>

            <div class=""form-group"">
                <label for=""camera-url"">Stream URL</label>
                <input type=""url"" id=""camera-url"" name=""camera-url"" value=""" + camera.CameraUrl + @""" required oninput=""updatePreview()"">
            </div>

            <div class=""form-group"">
                <label for=""camera-type"">Camera Type</label>
                <select id=""camera-type"" name=""camera-type"" oninput=""updatePreview()"">
                    <option value=""ip"">IP Camera</option>
                    <option value=""rtsp"">RTSP Stream</option>
                    <option value=""webcam"">Webcam</option>
                </select>
            </div>

            <div class=""form-group"">
                <label for=""recording-settings"">Recording Settings</label>
                <select id=""recording-settings"" name=""recording-settings"" oninput=""updatePreview()"">
                    <option value=""always"">Always Record</option>
                    <option value=""motion"">Record on Motion</option>
                    <option value=""scheduled"">Scheduled Recording</option>
                </select>
            </div>

            <div class=""form-group"">
                <label for=""camera-notes"">Notes</label>
                <textarea id=""camera-notes"" name=""camera-notes"" rows=""4"" oninput=""updatePreview()""></textarea>
            </div>

            <div class=""preview"">
                <h3>Camera Preview</h3>
                <img id=""camera-preview"" src=""" + camera.CameraUrl + @""" alt=""Camera Preview"">
            </div>

            <!-- Hidden checkbox for deletion -->
            <input type=""checkbox"" id=""delete-camera"" name=""delete-camera"" style=""display:none;"">
            <input type=""number"" id=""camera-index"" name=""camera-index"" value=""" + camera.CameraIndex + @""" style=""display:none;"" required>

            <div class=""form-actions"">
                <button type=""submit"" class=""btn btn-primary"">Save Changes</button>
                <button type=""button"" class=""btn btn-danger"" onclick=""confirmDelete()"">Delete Camera</button>
            </div>
        </form>
    </div>

    <script>
        function updatePreview() {
            var cameraUrl = document.getElementById('camera-url').value;
            document.getElementById('camera-preview').src = cameraUrl.replace('[username]',document.querySelector('#camera-user').value).replace('[password]',document.querySelector('#camera-pass').value);
        }
        function confirmDelete() {
            if (confirm(""Are you sure you want to delete this camera? This action cannot be undone."")) {
                // Check the hidden checkbox and submit the form
                document.getElementById('delete-camera').checked = true;
                document.querySelector('.edit-form').submit();
            }
        }
updatePreview();
    </script>
</body>
</html>";
    }




    public static async Task<string> AddPage()
    {

        return @"
<!DOCTYPE html>
<html lang=""en"">
<head><link rel=""icon"" type=""image/png"" href=""logo.png""><meta name='theme-color' content='#333333'>
    <meta charset=""UTF-8"">
<link rel=""manifest"" href=""manifest.json"">
          <script src='https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.3/js/all.min.js'></script>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        :root {
            --bg-color: #1a1a1a;
            --text-color: #ffffff;
            --primary-color: #3498db;
            --secondary-color: #2ecc71;
            --danger-color: #e74c3c;
            --input-bg: #2c2c2c;
        }

        body,html {
            font-family: Arial, sans-serif;
            background-color: var(--bg-color);
            color: var(--text-color);
            margin: 0;
            padding: 0;
        }

        .container {
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
        }

        h1 {
            text-align: center;
        }

        .edit-form {
            background-color: #2c2c2c;
            border-radius: 8px;
            padding: 20px;
        }

        .form-group {
            margin-bottom: 20px;
        }

        label {
            display: block;
            margin-bottom: 5px;
        }

        input[type=""text""],
        input[type=""url""],
        select,
        textarea {
            width: 100%;
            padding: 8px;
            border: 1px solid #444;
            border-radius: 4px;
            background-color: var(--input-bg);
            color: var(--text-color);
        }

        .btn {
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s;
        }

        .btn-primary {
            background-color: var(--primary-color);
            color: white;
        }

        .btn-danger {
            background-color: var(--danger-color);
            color: white;
        }

        .btn:hover {
            opacity: 0.8;
        }

        .form-actions {
            display: flex;
            justify-content: space-between;
            margin-top: 20px;
        }

        .preview {
            margin-top: 20px;
            text-align: center;
        }

        .preview img {
            max-width: 100%;
            border-radius: 4px;
        }

        @media (max-width: 768px) {
            .form-actions {
                flex-direction: column;
            }
            .form-actions .btn {
                margin-bottom: 10px;
            }
        }
        .navbar {
            display: flex;
            justify-content: flex-end;
            background-color: #333;
            padding: 10px;
        }

        .navbar a {
            color: white;
            padding: 14px 20px;
            text-decoration: none;
            text-align: center;
        }

        .navbar a:hover {
            background-color: #ddd;
            color: black;
        }
       .navbar {display: flex;
            justify-content: flex-end;
            background-color: #333;
            padding: 10px;
        }

        .navbar a {color: white;
            padding: 14px 20px;
            text-decoration: none;
            text-align: center;
        }
/* For WebKit browsers (Chrome, Safari) */
::-webkit-scrollbar {{
    width: 8px; /* Width of the scrollbar */
}}

::-webkit-scrollbar-thumb {{
    background-color: #444; /* Dark scrollbar color */
    border-radius: 10px; /* Rounded corners */
}}

::-webkit-scrollbar-thumb:hover {{
    background-color: #555; /* Lighter shade on hover */
}}

::-webkit-scrollbar-track {{
    background-color: #222; /* Dark background for the scrollbar track */
}}

/* For Firefox */
* {{
    scrollbar-width: thin; /* Thin scrollbar */
    scrollbar-color: #444 #222; /* Thumb color and track color */
}}


    </style>
</head>
<body>
<div class=""navbar"">
    <a href=""../"">Home</a>
    <a href=""../add"">Add Camera</a>
</div>

    <div class=""container"">
        <h1>Add Camera</h1>
        
        <form class=""edit-form"" method='post'>
            <div class=""form-group"">
                <label for=""camera-name"">Camera Name</label>
                <input type=""text"" id=""camera-name"" name=""camera-name"" value=""cam"" oninput=""updatePreview()"" required>
            </div>


            <div class=""form-group"">
                <label for=""camera-name"">Username</label>
                <input type=""text"" id=""username"" name=""username"" value=""admin"" oninput=""updatePreview()"" required>
            </div>

            <div class=""form-group"">
                <label for=""camera-name"">Password</label>
                <input type=""text"" id=""password"" name=""password"" value=""admin"" oninput=""updatePreview()"" required>
            </div>

            <div class=""form-group"">
                <label for=""camera-url"">Stream URL</label>
                <input type=""url"" id=""camera-url"" name=""camera-url"" value='http://192.168.1.XX:88/cgi-bin/CGIProxy.fcgi?cmd=snapPicture2&usr=[username]&pwd=[password]' oninput=""updatePreview()"" required>
            </div>

            <div class=""form-group"">
                <label for=""camera-type"">Camera Type</label>
                <select id=""camera-type"" name=""camera-type"">
                    <option value=""ip"">IP Camera</option>
                    <option value=""rtsp"">RTSP Stream</option>
                    <option value=""webcam"">Webcam</option>
                </select>
            </div>

            <div class=""form-group"">
                <label for=""recording-settings"">Recording Settings</label>
                <select id=""recording-settings"" name=""recording-settings"" oninput=""updatePreview()"">
                    <option value=""always"">Always Record</option>
                    <option value=""motion"">Record on Motion</option>
                    <option value=""scheduled"">Scheduled Recording</option>
                </select>
            </div>

            <div class=""form-group"">
                <label for=""camera-notes"">Notes</label>
                <textarea id=""camera-notes"" name=""camera-notes"" rows=""4"" oninput=""updatePreview()""></textarea>
            </div>

            <div class=""preview"">
                <h3>Camera Preview</h3>
                <img src='' alt=""Camera Preview"">
            </div>

            <div class=""form-actions"">
                <button type=""submit"" class=""btn btn-primary"">Save Changes</button>
                <button type=""button"" class=""btn btn-danger"" onclick=""confirmDelete()"">Delete Camera</button>
            </div>
        </form>
    </div>

    <script>
        
        function updatePreview() {
            var cameraUrl = document.getElementById('camera-url').value;
            document.getElementById('camera-preview').src = cameraUrl.replace('[username]',document.querySelector('#camera-user').value).replace('[password]',document.querySelector('#camera-pass').value);
        }
updatePreview();
    </script>
</body>
</html>";
    }



}




