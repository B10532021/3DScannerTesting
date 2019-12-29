using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TestCamera : MonoBehaviour
{

    [SerializeField]
    SunshineNativeCameraHandler _sunshineScript;

    [SerializeField] private string folderName;
    [SerializeField] private string imageFileName;
    [SerializeField] private string videoFileName;

    private string filePath;
    [SerializeField] private GameObject _previewObject;

    [SerializeField] private GameObject _saveBtn;

    private void Start()
    {
        _saveBtn.SetActive(false);
    }
    public void TakePicture()
    {
        _sunshineScript.TakePicture(imageFileName, (SunshineNativeCameraHandler.OnTakePictureCallbackHandler)((bool success, string path) =>
        {
            if (success)
            {
                filePath = path;
                _saveBtn.SetActive(true);
                StartCoroutine(PreviewFile(path));
            }
        }));
    }

    public void TakeVideo()
    {
        _sunshineScript.TakeVideo(videoFileName, (SunshineNativeCameraHandler.OnTakeVideoCallbackHandler)((bool success, string path) =>
        {
            if (success)
            {
                filePath = path;
                _saveBtn.SetActive(true);
                StartCoroutine(PreviewFile(path));
            }
          
        }));
    }


    IEnumerator PreviewFile(string url)
    {
       
        if (url.Contains(".mp4"))
        {
            url = "File://" + url;
            VideoPlayer videoPlayer = _previewObject.GetComponent<VideoPlayer>();
         
            videoPlayer.playOnAwake = true;
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = url;
            videoPlayer.Prepare();


            WaitForSeconds waitTime = new WaitForSeconds(1);
            while (!videoPlayer.isPrepared)
            {
                yield return waitTime;
                break;
            }

            RawImage image = _previewObject.GetComponent<RawImage>();
            image.texture = videoPlayer.texture;
            videoPlayer.Play();
           // playIcon.SetActive(true);
             while (videoPlayer.isPlaying)
             {
                 yield return null;
             }
        }
        else
        {
            Texture2D tex = LoadPNG(url);
            RawImage image = _previewObject.GetComponent<RawImage>();
            image.texture = tex;
        }

    }

    public static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }

    IEnumerator SaveFile(string CurrentFilePath, string SavedFilePath)
    {
        WWW www = new WWW("File://" + CurrentFilePath); // because Local Storage
        yield return www;   
        System.IO.File.WriteAllBytes(SavedFilePath, www.bytes);
    }

    public void SaveFile()
    {
        string new_Folder_Path = _sunshineScript.CreateFolderInAndroid(folderName);

        string storedPath;
        if (filePath.Contains(".mp4"))
             storedPath = new_Folder_Path + "/" + videoFileName;
        else
            storedPath = new_Folder_Path + "/" + imageFileName;

       StartCoroutine(SaveFile(filePath, storedPath));
    
    }
}
