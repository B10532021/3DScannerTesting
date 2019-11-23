using UnityEngine;

namespace LVonasek
{
    public class ObjectThrower : MonoBehaviour
    {
        public GameObject mesh;

        void Start()
        {
            mesh.SetActive(false);
        }

        void Update()
        {
            //get input
            bool throwIt = false;
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    throwIt = true;
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                throwIt = true;
            }

            //create and throw object
            if (throwIt)
            {
                GameObject go = Instantiate(mesh);
                go.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
                go.SetActive(true);
                go.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 500.0f, ForceMode.Force);
            }
        }
    }
}