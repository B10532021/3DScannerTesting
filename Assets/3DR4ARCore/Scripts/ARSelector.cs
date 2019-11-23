using UnityEngine;
using UnityEngine.UI;

namespace LVonasek
{
    public class ARSelector : MonoBehaviour
    {
        public ARProvider arProvider;
        public Text infoText;

        private static bool initialized;

        private void OnApplicationPause(bool pause)
        {
            if (!initialized)
            {
                if (Application.isEditor || arProvider.RequestCameraPermission())
                {
                    arProvider.SelectAR();
                    initialized = true;
                    OnEnable();
                }
            }
            else if (pause)
            {
                new AndroidJavaObject("com.lvonasek.liboc.JNI").CallStatic("Stop");
            }
        }

        private void OnEnable()
        {
            if (initialized)
            {
                if (infoText != null)
                {
                    switch (arProvider.GetSelectedAR())
                    {
                        case ARSDK.ARCORE:
                            infoText.text = "This app will be using <b>Google ARCore</b> on this device.";
                            break;
                        case ARSDK.ARENGINE:
                            infoText.text = "This app will be using <b>Huawei AREngine</b> on this device.";
                            break;
                    }
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                new AndroidJavaObject("com.lvonasek.liboc.JNI").CallStatic("Stop");
            }
        }
    }
}
