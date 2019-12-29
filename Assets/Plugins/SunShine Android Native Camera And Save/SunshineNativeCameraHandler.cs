using UnityEngine;
public class SunshineNativeCameraHandler : MonoBehaviour
{

    private const string PACKAGE_NAME = "com.SmileSoft.unityplugin";

    private const string CAMERA_CLASS_NAME = ".CameraFragment";
    private const string CAMERA_METHOD_TAKE_IMAGE = "takePicture";
    private const string CAMERA_METHOD_TAKE_VIDEO = "takeVideo";

    private const string CAMERA_METHOD_TAKE_CALLBACK = "OnTakeIngPictureCallback";
    private const string CAPTURE_METHOD_VIDEO_CALLBACK = "OnTakingVideoCallback";


    //File Fragment
      //private const string File_FRAGMENT_CLASS_NAME = ".FileFragment";
     //private const string FOLDER_CREATE_METHOD_NAME = "CreateFolder";

     private const string File_FRAGMENT_CLASS_NAME = ".MainActivity";
     private const string FOLDER_CREATE_METHOD_NAME = "CreateFolder";


    private const string FileProviderName = "com.GameLab.ScanUI";


    public delegate void OnTakePictureCallbackHandler(bool success, string path);
    private OnTakePictureCallbackHandler _callBackCamera_Image;

    public delegate void OnTakeVideoCallbackHandler(bool success, string path);
    private OnTakeVideoCallbackHandler _callBackCamera_Video;


    public void TakePicture(string filename, OnTakePictureCallbackHandler callback)
    {
#if UNITY_ANDROID
        using (AndroidJavaObject camera = new AndroidJavaObject(PACKAGE_NAME + CAMERA_CLASS_NAME))
        {
            _callBackCamera_Image = callback;
            camera.Call(CAMERA_METHOD_TAKE_IMAGE, FileProviderName, gameObject.name, filename, CAMERA_METHOD_TAKE_CALLBACK);

        
    }
#endif
        Debug.Log("This Plugin only worked in android");

    }

    public void TakeVideo(string filename, OnTakeVideoCallbackHandler callback)
    {
#if UNITY_ANDROID
        using (AndroidJavaObject camera = new AndroidJavaObject(PACKAGE_NAME + CAMERA_CLASS_NAME))
        {
            _callBackCamera_Video = callback;
            camera.Call(CAMERA_METHOD_TAKE_VIDEO, FileProviderName, gameObject.name, filename, CAPTURE_METHOD_VIDEO_CALLBACK);

    
}
#endif
        Debug.Log("This Plugin only worked in android");
    }

    public void OnTakeIngPictureCallback(string result)
    {
#if UNITY_ANDROID
        if (_callBackCamera_Image != null)
        {
            _callBackCamera_Image.Invoke(!string.IsNullOrEmpty(result), result);
            _callBackCamera_Image = null;
        }
#endif
    }


    public void OnTakingVideoCallback(string result)
    {
#if UNITY_ANDROID
        if (_callBackCamera_Video != null)
        {
            _callBackCamera_Video.Invoke(!string.IsNullOrEmpty(result), result);
            _callBackCamera_Video = null;
        }
#endif
    }

    public string CreateFolderInAndroid(string foldername)
    {

#if UNITY_ANDROID
        using (AndroidJavaObject file_android_obj = new AndroidJavaObject(PACKAGE_NAME + File_FRAGMENT_CLASS_NAME))
        {
            string folderPath = file_android_obj.Call<string>(FOLDER_CREATE_METHOD_NAME, foldername);

            return folderPath;

        }
#endif
        Debug.Log("Native Share just work in android Platform");
        return "";

    }

}