using UnityEngine;
using UnityEngine.SceneManagement;

namespace LVonasek
{
    public class AppKiller : MonoBehaviour
    {
        public int sceneToLoad = -1;

        private void OnApplicationPause(bool pause)
        {
            if (pause && (sceneToLoad == -1))
            {
                new AndroidJavaObject("com.lvonasek.liboc.JNI").CallStatic("Stop");
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (sceneToLoad == -1)
                {
                    new AndroidJavaObject("com.lvonasek.liboc.JNI").CallStatic("Stop");
                }
                else
                {
                    SceneManager.LoadScene(sceneToLoad);
                }
            }
        }
    }
}