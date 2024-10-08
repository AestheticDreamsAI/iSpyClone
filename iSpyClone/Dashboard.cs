﻿using AForge.Vision.Motion;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static CameraStatistics;


internal class Dashboard
{
    static string GetNav()
    {
        Dictionary<string, string> menuItems = new Dictionary<string, string>();
        menuItems.Add("Home", "../");
        menuItems.Add("Logs", "../logs");
        menuItems.Add("Statistics", "../stats");
        var items = "";
        items += $"<a href=\"#\" onclick='toggleMenu()' style='font-size:30px;display:none;text-align:right;'>☰</a>";
        foreach (var item in menuItems)
        {
            items += $"<a href=\"{item.Value}\">{item.Key}</a>";
        }
        items += $"<a href=\"../add\" class=\"addCam\">Add Camera</a>";
        return $@"
<div class='navbar'>
    <a id=""menu-icon"" class='menu-icon' onclick='toggleMenu()' style='font-size:30px;'>☰</a>
    <div id='menu-items' class='menu-items'>
{items}
    </div>
</div>

<script>
var def = null;
    function toggleMenu() {{
        var menuIcon = document.getElementById('menu-icon');
        var menu = document.getElementById('menu-items');
        const items = menu.children;
        if (menu.style.display === 'none' || menu.style.display === '') {{
            menu.style.display = 'block';
            menu.style.width='100%';
            menuIcon.style.display='none';
for (let i = 0; i < items.length; i++) {{
    items[i].style.display = 'block'; 
    items[i].style.width = '100%'; 

}}
        }} else {{
            menu.style.width='auto';
            menu.style.display = 'none';
            menuIcon.style.display='block';
for (let i = 0; i < items.length; i++) {{
if(i!==0){{
    items[i].style=def;
}}else{{
items[0].style.display='none';        
}}
}}
        }}
    }}
document.addEventListener('DOMContentLoaded', function() {{

    function checkWindowSize()
        {{ 
        var menuIcon = document.getElementById('menu-icon');
        var menuItems = document.getElementById('menu-items');
        const items = menuItems.children;
        items[0].style.display='none';    
        if(def===null)
        {{
            def = items[1].style;
        }}
        if (window.innerWidth > 700) {{
            // Show full menu, hide hamburger icon
            menuIcon.style.display = 'none';
            menuItems.style.display = 'flex';
            menuItems.style.textAlign='right';
            menuItems.style.width='auto';
for (let i = 0; i < items.length; i++) {{
if(i!==0){{
    items[i].style=def;
}}else{{
items[0].style.display='none';        
}}
}}

        }} else {{
            // Show hamburger icon, hide full menu
            menuIcon.style.display = 'block';
            menuItems.style.display = 'none'; 
        }}
    }}

    // Check window size on load
    checkWindowSize();

    // Check window size on resize
    window.onresize = checkWindowSize;
}});
</script>

";
    }
    public static async Task<string> HomePage()
    {
        var _camGrids_ = "";
        foreach (Camera cam in Cameras.All())
        {
            var detection = cam.IsRecording;
            var isRecording = detection ? "<i class=\"fas fa-record-vinyl\" style=\"color: red;\"></i> Recording" : "Inactive";
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
<link rel=""manifest"" href=""manifest.json?t="+Guid.NewGuid().ToString()+ @""">
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
 box-sizing:border-box;           

}

    </style>
</head>
<body>
" + GetNav()+@"

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
            ""<i class=\""fas fa-record-vinyl\"" style=\""color: red;\""></i> Recording"" : ""Inactive"";

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
            foreach (var f in camera.GetRecordings())
            {
                _recordings_ += $@"
                <li class=""recording-item"">
                    <span>{f.Date} - {f.Time} - Motion Detected</span>
                     <button class=""btn"" style='background-color:#444; color:white;' onclick=""Download('{f.Path}')""><i class=""fa-solid fa-download""></i></button><button class=""btn btn-primary"" onclick=""showFrames('{f.Path}')""><i class=""fa-regular fa-image""></i></button><button class=""btn btn-secondary"" onclick=""playRecording('{f.Path}')""><i class=""fa-solid fa-play""></i></button>
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
      <link
        rel=""stylesheet""
        href=""https://site-assets.fontawesome.com/releases/v6.6.0/css/all.css""
      >

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

/* Modal Styles */
.modal {{
  display: none;
  position: fixed;
  z-index: 1000;
  left: 0;
  top: 0;
  width: 100%;
  height: 100%;
  overflow: auto;
  background-color: rgba(0, 0, 0, 0.5);
}}

.modal-content {{
  background-color: var(--card-bg);
  margin: 15% auto;
  padding: 10px;
  width: 100%;
max-width:1000px;
  height:auto;
  text-align: center;
  position: relative;
            border-radius: 8px;
}}

.close {{
  color: var(--primary-color);
  font-size: 28px;
  font-weight: bold;
text-align:right;
margin-bottom:-50px;
width:100%;
z-index:1000;
margin-right:20px;
}}

.close:hover,
.close:focus {{
  color: var(--secondary-color);
  text-decoration: none;
  cursor: pointer;
}}

/* Slider Container */
#framesSlider {{
  display: flex;
  align-items: center;
  justify-content: center;
  margin-top: 20px;
}}

#framesSlider button {{
  padding: 10px 20px;
  margin: 0 10px;
  cursor: pointer;
  background-color: #3498db;
  color: white;
  border: none;
  border-radius: 5px;
  transition: background-color 0.3s ease;
}}

#framesSlider button:hover {{
  background-color: #2980b9;
}}

/* Styling for the image */
#framesSlider img {{
  width: 100%;  /* Maximale Breite des Bildes innerhalb des Containers */
  height: auto;     /* Proportionen beibehalten */
  border-radius: 5px;
}}

/* Media Queries for smaller screens (e.g. smartphones) */
@media (max-width: 768px) {{
  .modal-content {{
    width: 95%; /* Für kleinere Bildschirme sollte das Modal fast den gesamten Bildschirm nutzen */
  }}

  #framesSlider {{
    flex-direction: column;  /* Für schmale Bildschirme wird der Slider vertikal angeordnet */
  }}

  #framesSlider img {{
    width: 100%; /* Das Bild nimmt die gesamte Breite des Containers auf kleinen Bildschirmen ein */
  }}

  #framesSlider button {{
    padding: 15px;
    width: 80%; /* Die Buttons werden breiter und passen sich der Bildschirmbreite an */
    margin-bottom: 10px;
  }}
}}

@media (max-width: 480px) {{
  .modal-content {{
    width: 90%;
  }}

  #framesSlider {{
    flex-direction: column;
  }}

  #framesSlider button {{
    width: 100%; /* Buttons füllen den gesamten Bereich aus */
    font-size: 18px; /* Größere Schrift für bessere Lesbarkeit auf kleinen Geräten */
  }}

  #framesSlider img {{
    width: 100%;  /* Das Bild wird auf die volle Breite des Bildschirms skaliert */
    height: auto; /* Proportionen bleiben erhalten */
  }}
}}
input{{
    width: 100%;
    padding: 8px;
    border: 1px solid #444;
    border-radius: 4px;
    background-color: var(--input-bg);
    color: var(--text-color);
    font-size:16px;
}}

*{{
 box-sizing:border-box;           
}}
    </style>
</head>
<body>
" +GetNav()+$@"

<div id=""framesModal"" class=""modal"" style=""display:none;"">
  <div class=""modal-content"">
    <h2 class=""close""><i class=""fa-regular fa-circle-xmark"" style='font-size:30px;'></i></h2>
    <h2 id='modal-title'>-</h2>
    <div id=""framesSlider"">
      <button id=""prevFrame"">Back</button>
      <img id=""frameImage"" src="""" />
      <button id=""nextFrame"">Next</button>
    </div>
  </div>
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
            <div><input type='text' oninput='Search(this)' placeholder='Search all recordings ...'></div>
            <ul class='recording-list'>
 {_recordings_}
            </ul>
        </div>
    </div>


<script>
    let recordingsArray = [];
document.addEventListener('DOMContentLoaded', function() {{
    let abortController = null;
    let timeoutId = null;
    let intervalId = null;
    const cameraImg = document.querySelector('.camera-img');
    const image = document.querySelector('.video-container');
    const button = document.getElementById('fullscreen-btn');
    let isPlaying = false; // Controls whether recording is playing
let frames = []; // Array, um die Frames zu speichern
let currentFrameIndex = 0; // Aktueller Index des Frames

      const recordings = document.querySelectorAll('.recording-item');
        recordings.forEach(function (recording) {{
            recordingsArray.push({{
                html: recording.outerHTML, // Speichere das HTML des Listenelements
                text: recording.innerText.toLowerCase() // Text für die Suche speichern
            }});
        }});

    window.Search = function(input) {{
        const searchTerm = input.value.toLowerCase();
        const filteredRecordings = recordingsArray.filter(function (recording) {{
            return recording.text.includes(searchTerm);
        }});
        // UI neu rendern basierend auf dem Filter
        const recordingList = document.querySelector('.recording-list');
        if (searchTerm === '') {{
            recordingList.innerHTML = '';
            recordingsArray.forEach(function (recording) {{
                recordingList.innerHTML += recording.html;
            }});
            return;
        }}
        recordingList.innerHTML = ''; // Bestehende Elemente leeren

        // Neue, gefilterte Listenelemente einsetzen
        filteredRecordings.forEach(function (recording) {{
            recordingList.innerHTML += recording.html;
        }});
    }}

function formatDateTime(input) {{
  var p = input.split('/')[1].split('_');
  var date = p[0].replaceAll('-','.');
  var time = p[1].replaceAll('-',':');
    return `${{date}} - ${{time}}<br>Motion Detected`;
}}

// Modal open/close logic
window.Download = function(recordingPath)
{{
        const download = `/media/download/${{recordingPath}}?t=${{Date.now()}}`;
            // Zeige das Modal
            const modal = document.getElementById('framesModal');
            modal.style.display = ""block"";
            document.querySelector('.modal-content').style.height='100px';
    document.getElementById('prevFrame').style.display = ""none"";
    document.getElementById('nextFrame').style.display = ""none"";
    document.querySelector('#modal-title').innerHTML='Download will start soon...';
document.getElementById('frameImage').src='data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==';
setTimeout(function() {{
    window.location.href = window.location.origin + download;
    modal.style.display='none';
}}, 2000);
}}

window.showFrames = function(recordingPath) {{
    const framesUrl = `../media/frames/${{recordingPath}}?t=${{Date.now()}}`;
            // Zeige das Modal
            const modal = document.getElementById('framesModal');
modal.style.height='100%';
            modal.style.display = ""block"";
            document.querySelector('.modal-content').style.height='auto';
    document.getElementById('prevFrame').style.display = ""none"";
    document.getElementById('nextFrame').style.display = ""none"";
    document.querySelector('#modal-title').innerHTML='loading ...';
document.getElementById('frameImage').src='data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==';
    fetch(framesUrl)
        .then(response => response.json())
        .then(data => {{
            frames = data; // Speichere die geladenen Frames im Array
            currentFrameIndex = 0; // Setze den Frame-Index auf 0 (erster Frame)
            updateFrame(); // Zeige den ersten Frame an

            // Slider-Buttons anzeigen
    document.querySelector('#modal-title').innerHTML=formatDateTime(recordingPath);
            document.getElementById('prevFrame').style.display = ""inline-block"";
            document.getElementById('nextFrame').style.display = ""inline-block"";


        }})
        .catch(error => console.error('Error fetching frames:', error));
}}


// Funktion zum Abspielen einer Aufnahme (GIF) ohne Slider
window.playRecording = function(recordingPath) {{
    const gifUrl = `../media/video/${{recordingPath}}?t=${{Date.now()}}`;
            const modal = document.getElementById('framesModal');
            modal.style.display = ""block"";
            document.querySelector('.modal-content').style.height='auto';
modal.style.height='100%';
    document.querySelector('#modal-title').innerHTML='loading ...';
    // Slider-Buttons ausblenden

    document.getElementById('prevFrame').style.display = ""none"";
    document.getElementById('nextFrame').style.display = ""none"";
document.getElementById('frameImage').src='data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==';
    // Lade das GIF und setze es als Quelle für das Bild im Modal
    fetch(gifUrl, {{ cache: 'no-cache' }})
        .then(response => {{
            if (!response.ok) {{
                throw new Error('Error fetching GIF');
            }}
            return response.blob();
        }})
        .then(blob => {{
            const objectURL = URL.createObjectURL(blob);
    document.querySelector('#modal-title').innerHTML=formatDateTime(recordingPath);
setImageSrcFromBlob(document.getElementById('frameImage'),blob);
        }})
        .catch(error => {{
            console.error('Error fetching GIF:', error);
        }});
}};


// Funktion zum Aktualisieren des angezeigten Frames
function updateFrame() {{
    if (frames.length > 0) {{
        document.getElementById('frameImage').src = frames[currentFrameIndex]; // Setze den aktuellen Frame als Bildquelle
    }}
}}

// Funktion, um zum nächsten Frame zu wechseln
document.getElementById('nextFrame').onclick = function() {{
    if (currentFrameIndex < frames.length - 1) {{
        currentFrameIndex++; // Erhöhe den Index
        updateFrame(); // Aktualisiere das Bild
    }}
}};

// Funktion, um zum vorherigen Frame zu wechseln
document.getElementById('prevFrame').onclick = function() {{
    if (currentFrameIndex > 0) {{
        currentFrameIndex--; // Verringere den Index
        updateFrame(); // Aktualisiere das Bild
    }}
}};

// Modal schließen, wenn man auf das X klickt
document.querySelector("".close"").onclick = function() {{
    document.getElementById('framesModal').style.display = ""none"";
}};

// Modal schließen, wenn man außerhalb klickt
window.onclick = function(event) {{
    const modal = document.getElementById('framesModal');
    if (event.target == modal) {{
        modal.style.display = ""none"";
    }}
}};


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
                    
                    setImageSrcFromBlob(cameraImg,blob);

                   //cameraImg.onload = () => URL.revokeObjectURL(objectURL);
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
font-size:16px;
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

*{
 box-sizing:border-box;           
}
    </style>
</head>
<body>
" + GetNav()+$@"

    <div class=""container"">
        <h1>Edit Camera</h1>

        <form class=""edit-form"" method=""post"">
            <div class=""form-group"">
                <label for=""camera-name"">Camera Name</label>
                <input type=""text"" id=""camera-name"" name=""camera-name"" value=""" + camera.CameraName + @""" required oninput=""updatePreview()"">
            </div>

            <div class=""form-group"">
                <label for=""username"">Username</label>
                <input type=""text"" id=""username"" name=""camera-user"" value=""" + camera.CameraUser + @""" required oninput=""updatePreview()"">
            </div>

            <div class=""form-group"">
                <label for=""password"">Password</label>
                <input type=""text"" id=""password"" name=""camera-pass"" value=""" + camera.CameraPass + @""" required oninput=""updatePreview()"">
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
document.addEventListener('DOMContentLoaded', function() {
  // Your code here will run once the DOM is fully loaded
  console.log('DOM fully loaded and parsed');
updatePreview();
});
        function updatePreview() {
            var cameraUrl = document.getElementById('camera-url').value;
            document.getElementById('camera-preview').src = cameraUrl.replace('[username]',document.querySelector('#username').value).replace('[password]',document.querySelector('#password').value);
        }
        function confirmDelete() {
            if (confirm(""Are you sure you want to delete this camera? This action cannot be undone."")) {
                // Check the hidden checkbox and submit the form
                document.getElementById('delete-camera').checked = true;
                document.querySelector('.edit-form').submit();
            }
        }
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
            font-size:16px;
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

*{
 box-sizing:border-box;           
}
        .navbar .addCam{
                    background-color: var(--primary-color);
            color: white;
            border-radius:20px;
        }
    </style>
</head>
<body>
" + GetNav()+@"

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
                <img src='' id='camera-preview' alt=""Camera Preview"">
            </div>

            <div class=""form-actions"">
                <button type=""submit"" class=""btn btn-primary"">Save</button>
            </div>
        </form>
    </div>

    <script>
        function updatePreview() {
            var cameraUrl = document.getElementById('camera-url').value;
            document.getElementById('camera-preview').src = cameraUrl.replace('[username]',document.querySelector('#username').value).replace('[password]',document.querySelector('#password').value);
        }
        document.addEventListener('DOMContentLoaded', function() {
  // Your code here will run once the DOM is fully loaded
  console.log('DOM fully loaded and parsed');
updatePreview();
});
    </script>
</body>
</html>";
    }

    public static async Task<string> CameraStatisticsPage()
    {
        // Load saved statistics (if available)

        // Get the maximum size allocated for storage (in bytes) from Program.manager
        long totalStorageInBytes = Program.manager.getMaxSize();

        // Calculate used storage by summing the size of all files in the media directory
        string mediaDirectory = Program.manager.getDirectory();
        long usedStorageInBytes = Program.manager.CalculateDirectorySize(Cameras.All());

        // Calculate the percentage of storage used
        double storageUsedPercentage = (double)usedStorageInBytes / totalStorageInBytes * 100;

        // Convert total and used storage to human-readable format
        string usedStorageFormatted = FormatSize(usedStorageInBytes);
        string totalStorageFormatted = FormatSize(totalStorageInBytes);

        // Generate camera statistics
        var _camStats_ = "";
        var i = 0;
        foreach (Camera cam in Cameras.All())
        {
            // Fetch stats for the current camera
            var stats = CameraStatistics.GetStatistics(cam);

            // Fallback if no stats are available
            var totalRecordings = stats != null ? stats.TotalRecordings.ToString() : "0";
            var totalMotionDetections = stats != null ? stats.TotalMotionDetections.ToString() : "0";
            var totalRecordingDuration = stats != null ? stats.TotalRecordingDuration.TotalMinutes.ToString("F2") + " mins" : "0 mins";

            // Build the HTML for each camera's statistics
            var m = "style='margin-top:60px;'";
            if (i != 0)
            {
                m = "";
            }
            _camStats_ += $@"
        <div class=""camera-stats-card"" {m}>
            <h3>{cam.CameraName}</h3>
            <p><strong>Total Recordings:</strong> {totalRecordings}</p>
            <p><strong>Total Motion Detections:</strong> {totalMotionDetections}</p>
            <p><strong>Total Recording Duration:</strong> {totalRecordingDuration}</p>
        </div>";
            i++;
        }

        // Return the complete HTML page with consistent design
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Camera Statistics</title>
    <link rel=""icon"" type=""image/png"" href=""logo.png"">
    <meta name='theme-color' content='#333333'>
    <link rel=""manifest"" href=""manifest.json?t={Guid.NewGuid()}"">
    <script src='https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.3/js/all.min.js'></script>
    <style>
        :root {{
            --bg-color: #1a1a1a;
            --text-color: #ffffff;
            --primary-color: #3498db;
            --secondary-color: #2ecc71;
            --danger-color: #e74c3c;
        }}
        body, html {{
            font-family: Arial, sans-serif;
            background-color: var(--bg-color);
            color: var(--text-color);
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
        }}
        h1 {{
            text-align: center;
        }}
        .camera-stats-card {{
            background-color: #2c2c2c;
            border-radius: 8px;
            padding: 15px;
            margin-bottom: 20px;
        }}
        .camera-stats-card h3 {{
            margin-top: 0;
            color: var(--primary-color);
        }}
        .navbar {{
            display: flex;
            justify-content: flex-end;
            background-color: #333;
            padding: 10px;
        }}
        .navbar a {{
            color: white;
            padding: 14px 20px;
            text-decoration: none;
            text-align: center;
        }}
        .navbar a:hover {{
            background-color: #ddd;
            color: black;
        }}
        .navbar .addCam {{
            background-color: var(--primary-color);
            color: white;
            border-radius: 20px;
        }}
        .progress-bar-container {{
            width: 100%;
            background-color: #444;
            border-radius: 5px;
            margin-bottom: 20px;
            position: relative; /* Add this to ensure child elements position properly */
        }}
        .progress-bar {{
            width: {storageUsedPercentage:F2}%;
            height: 25px;
            background-color: {GetProgressBarColor(storageUsedPercentage)};
            border-radius: 5px;
            text-align: center;
            line-height: 25px; /* Vertically center text */
            color: white;
            position: absolute; /* Position the progress bar inside the container */
        }}
        .progress-text {{
            position: absolute;
            width: 100%;
            height: 25px;
            top: 0;
            left: 0;
            text-align: center;
            color: white;
            line-height: 25px;
        }}
        .progressbar-border{{
            border:2px solid var(--primary-color);
            height:25px;
            border-radius:5px ;
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
*{{
 box-sizing:border-box;           
}}
    </style>
</head>
<body>
{GetNav()}
    <div class=""container"">
        <h1>Camera Statistics</h1>
        
        <!-- Progress Bar for Storage -->
        <div class='progressbar-border'>
        <div class=""progress-bar-container"">
            <div class=""progress-bar""></div>
            <div class=""progress-text"">{storageUsedPercentage:F2}% used ({usedStorageFormatted} of {totalStorageFormatted})</div>
        </div>
        </div>

        {_camStats_}
    </div>
</body>
</html>";
    }

    public static async Task<string> LogPage()
    {
        // Erstelle das HTML für die Logs
        var logsHtml = "        <div class=\"log-card\">";
        var i = 0;
        foreach (var log in Program.logWriter.GetEventLog())
        {

            // Erzeuge HTML für jedes Log-Event
            logsHtml += $@"
            <p style=""color: {log.Color};"">{log.Message}</p>";
        }
        logsHtml += "</div>";
        // Rückgabe des vollständigen HTML mit CSS, das zum Rest der Seite passt
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Event Logs</title>
    <link rel=""icon"" type=""image/png"" href=""logo.png"">
    <meta name='theme-color' content='#333333'>
    <link rel=""manifest"" href=""manifest.json?t={Guid.NewGuid()}"">
    <script src='https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.3/js/all.min.js'></script>
    <style>
        :root {{
            --bg-color: #1a1a1a;
            --text-color: #ffffff;
            --primary-color: #3498db;
            --secondary-color: #2ecc71;
            --danger-color: #e74c3c;
        }}
        body, html {{
            font-family: Arial, sans-serif;
            background-color: var(--bg-color);
            color: var(--text-color);
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
        }}
        h1 {{
            text-align: center;
        }}
        .log-card {{
            background-color: #2c2c2c;
            border-radius: 8px;
            padding: 15px;
            margin-bottom: 20px;
        }}
        .log-card h3 {{
            margin-top: 0;
            color: var(--primary-color);
        }}
        .navbar {{
            display: flex;
            justify-content: flex-end;
            background-color: #333;
            padding: 10px;
        }}
        .navbar a {{
            color: white;
            padding: 14px 20px;
            text-decoration: none;
            text-align: center;
        }}
        .navbar a:hover {{
            background-color: #ddd;
            color: black;
        }}
        .navbar .addCam {{
            background-color: var(--primary-color);
            color: white;
            border-radius: 20px;
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
*{{
 box-sizing:border-box;           
}}
    </style>
</head>
<body>
{GetNav()}
    <div class=""container"">
        <h1>Event Logs</h1>
        
        <!-- Event Logs Display -->
        {logsHtml}
    </div>
</body>
</html>";
    }


    // Function to determine progress bar color based on percentage
    private static string GetProgressBarColor(double percentage)
    {
        if (percentage < 50)
            return "#2ecc71"; // green
        else if (percentage < 75)
            return "#f1c40f"; // yellow
        else
            return "#e74c3c"; // red
    }


    public static string FormatSize(long bytes)
    {
        const long KB = 1024;
        const long MB = KB * 1024;
        const long GB = MB * 1024;
        const long TB = GB * 1024;

        if (bytes < KB)
            return $"{bytes} B"; // Bytes
        else if (bytes < MB)
            return $"{(bytes / (double)KB):F2} KB"; // Kilobytes
        else if (bytes < GB)
            return $"{(bytes / (double)MB):F2} MB"; // Megabytes
        else if (bytes < TB)
            return $"{(bytes / (double)GB):F2} GB"; // Gigabytes
        else
            return $"{(bytes / (double)TB):F2} TB"; // Terabytes
    }


}




