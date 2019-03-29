using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Linq;
using Assets.Scripts.Extensions;

// Screen Recorder will save individual images of active scene in any resolution and of a specific image format
// including raw, jpg, png, and ppm.  Raw and PPM are the fastest image formats for saving.
//
// You can compile these images into a video using ffmpeg:
// ffmpeg -i screen_3840x2160_%d.ppm -y test.avi

public class ScreenRecorder : MonoBehaviour
{
    // 4k = 3840 x 2160   1080p = 1920 x 1080

 
    public int captureScaleFactor = 10;

    private int width = 0;
    private int height = 0;

    // optional game object to hide during screenshots (usually your scene canvas hud)
    public GameObject hideGameObject;

    // optimize for many screenshots will not destroy any objects so future screenshots will be fast
    public bool optimizeForManyScreenshots = true;

    // configure with raw, jpg, png, or ppm (simple raw format)
    public enum Format { RAW, JPG, PNG, PPM };
    public Format format = Format.PPM;

    // Anzahl an Frames nach denen ein Bild gemacht werden soll.
    public int takePictureEveryXFrame = 1;

    // folder to write output (defaults to data path)
    public string folder;
    public bool capture;
    public bool grayscale;
    public string coordFilename = "carCoords.txt";

    [HideInInspector]
    public bool isGenerated = false;

    [HideInInspector]
    public string captureFolder;

    [HideInInspector]
    public int counter = 0; // image #

    // private vars for screenshot
    private Rect rect;
    private RenderTexture renderTexture;
    private Texture2D screenShot;       

    // commands
    private bool captureScreenshot = false;
    private bool captureVideo = false;

    [HideInInspector]
    public int updateCounter = 0;

    private string filePath = null;

    [HideInInspector]
    public GameObject[] ObjectsToHide;
    public bool TakePicturesOnlyWithCars;




    // create a unique filename using a one-up variable
    private string uniqueFilename(int width, int height)
    {

        // use width, height, and counter for unique file name
        var filename = string.Format("{0}/screen_{1}x{2}_{3}.{4}", captureFolder, width, height, counter.ToString("00000"), format.ToString().ToLower());

        // up counter for next call
        ++counter;

        // return unique filename
        return filename;
    }

    private void InitializeCaptureFolder()
    {
        // if folder not specified by now use a good default
        if (folder == null || folder.Length == 0)
        {
            folder = Application.dataPath;
            if (Application.isEditor)
            {
                // put screenshots in folder above asset path so unity doesn't index the files
                var stringPath = folder + "/..";
                folder = Path.GetFullPath(stringPath);
            }
            folder += "/screenshots";

            // count number of files of specified format in folder
            //string mask = string.Format("screen_{0}x{1}*.{2}", width, height, format.ToString().ToLower());
            //counter = Directory.GetFiles(folder, mask, SearchOption.TopDirectoryOnly).Length;
        }

        captureFolder = Path.Combine(folder, "capture_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));

        // make sure directoroy exists
        Directory.CreateDirectory(captureFolder);

        // Wenn nicht vorhanden den filepath holen und die Datei erzeugen
        if (filePath == null)
        {
            this.filePath = Path.Combine(captureFolder, coordFilename);

            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }
        }
    }

    public void CaptureScreenshot()
    {
        captureScreenshot = true;
    }

    void Start()
    {
        width = 192;
        height = 108;

        if (capture)
        {
            InitializeCaptureFolder();
        }
    }

    public void TakePicture(string carCoordLine)
    {
        captureScreenshot = false;

        // Nur Bilder von Scenen machen, in denen auch ein Auto zu sehen ist.
        if (this.TakePicturesOnlyWithCars && carCoordLine.IsNullOrEmpty())
        {
            return;
        }

        // hide optional game object if set
        if (hideGameObject != null) hideGameObject.SetActive(false);
        foreach(GameObject objectToHide in ObjectsToHide)
        {
            objectToHide.SetActive(false);
        }

        // create screenshot objects if needed
        if (renderTexture == null)
        {
            // creates off-screen render texture that can rendered into
            rect = new Rect(0, 0, width, height);
            //rect = new Rect(0, 0, preCaptureWidth, preCaptureHeight);

            renderTexture = new RenderTexture(width, height, 24);
            screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            //renderTexture = new RenderTexture(preCaptureWidth, preCaptureHeight, 24);
            //screenShot = new Texture2D(preCaptureWidth, preCaptureHeight, TextureFormat.RGB24, false);
        }

        // get main camera and manually render scene into rt
        Camera camera = this.GetComponent<Camera>(); // NOTE: added because there was no reference to camera in original script; must add this script to Camera
        camera.aspect = 16f / 9f;
        camera.targetTexture = renderTexture;
        camera.Render();

        //if (grayscale)
        //{
        //    renderTexture = Image2Grayscale.ApplyGrayScaleShader(renderTexture);
        //}

        // read pixels will read from the currently active render texture so make our offscreen 
        // render texture active and then read the pixels
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(rect, 0, 0);
        //TextureScale.Bilinear(screenShot, captureWidth, captureHeight);
        if (grayscale)
        {
            screenShot = Image2Grayscale.ConvertToGrayscale(screenShot);
        }

        // reset active camera texture and render texture
        camera.targetTexture = null;
        RenderTexture.active = null;

        // get our unique filename
        string filename = uniqueFilename(width, height);

        // pull in our file header/data bytes for the specified image format (has to be done from main thread)
        byte[] fileHeader = null;
        byte[] fileData = null;
        if (format == Format.RAW)
        {
            fileData = screenShot.GetRawTextureData();
        }
        else if (format == Format.PNG)
        {
            fileData = screenShot.EncodeToPNG();
        }
        else if (format == Format.JPG)
        {
            fileData = screenShot.EncodeToJPG();
        }
        else // ppm
        {
            // create a file header for ppm formatted file
            string headerStr = string.Format("P6\n{0} {1}\n255\n", width, height);
            fileHeader = System.Text.Encoding.ASCII.GetBytes(headerStr);
            fileData = screenShot.GetRawTextureData();
        }

        // create new thread to save the image to file (only operation that can be done in background)
        new System.Threading.Thread(() =>
        {
            // create file and write optional header with image bytes
            var f = System.IO.File.Create(filename);
            if (fileHeader != null) f.Write(fileHeader, 0, fileHeader.Length);
            f.Write(fileData, 0, fileData.Length);
            f.Close();
            Debug.Log(string.Format("Wrote screenshot {0} of size {1}", filename, fileData.Length));

            // Die Coordinaten in die Datei schreiben.
            using (StreamWriter writer = File.AppendText(filePath))
            {
                writer.WriteLine(carCoordLine);
                writer.Close();
            }
        }).Start();

        // unhide optional game object if set
        if (hideGameObject != null) hideGameObject.SetActive(true);
        foreach (GameObject objectToHide in ObjectsToHide)
        {
            objectToHide.SetActive(true);
        }

        // cleanup if needed
        if (optimizeForManyScreenshots == false)
        {
            Destroy(renderTexture);
            renderTexture = null;
            screenShot = null;
        }
    }

    public static Texture2D Resize(Texture2D source, int newWidth, int newHeight)
    {
        source.filterMode = FilterMode.Point;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Point;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D nTex = new Texture2D(newWidth, newHeight);
        nTex.ReadPixels(new Rect(0, 0, newWidth, newWidth), 0, 0);
        nTex.Apply();
        RenderTexture.active = null;
        return nTex;

    }

    //#region OnApplicationQuit
    ///// <summary>
    ///// Wenn die Anwendung geschlossen wird, die letzte Zeile aus den carCoords.txt löschen
    ///// diese wird durch File.WriteLine erzeugt, aber nicht befüllt, wodurch eine eine Zeile 
    ///// mehr geschrieben wird, als Bilder vorhanden sind.
    ///// </summary>
    //void OnApplicationQuit()
    //{
    //    if (this.filePath != null)
    //    {
    //        string[] lines = File.ReadAllLines(filePath);
    //        File.WriteAllLines(filePath, lines.Take(lines.Length - 1).ToArray());
    //    }
    //}
    //#endregion
}