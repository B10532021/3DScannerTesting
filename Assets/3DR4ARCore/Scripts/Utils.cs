using System.IO;
using UnityEngine;

namespace LVonasek
{
    public class Utils
    {

        public static void DeleteDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                foreach (string dir in Directory.GetDirectories(directory))
                {
                    DeleteDirectory(dir);
                }
                foreach (string file in Directory.GetFiles(directory))
                {
                    File.Delete(file);
                }
                Directory.Delete(directory, false);
            }
        }

        public static void ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                        message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
}