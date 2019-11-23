using UnityEngine;

namespace LVonasek
{
    public class Background : MonoBehaviour
    {
        void Update()
        {
            transform.position = Camera.main.transform.position + Camera.main.transform.forward * 100.0f;
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.up, -Camera.main.transform.forward);
            transform.localScale = Vector3.one * 15.0f;
        }
    }
}