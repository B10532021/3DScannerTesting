using UnityEngine;
using UnityEngine.SceneManagement;

namespace LVonasek
{
    public class SceneOpener : MonoBehaviour
    {
        public void OpenScene(int index)
        {
            SceneManager.LoadScene(index);
        }
    }
}
