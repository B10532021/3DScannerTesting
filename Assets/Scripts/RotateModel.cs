using UnityEngine;

public class RotateModel : MonoBehaviour
{
    private Vector2 lastPos;
    public float rotationSpeed = 0.02f;
    void Update()
    {
        // Handle a single touch
        if (Input.touchCount == 1)
        {
            var touch = Input.GetTouch(0);

            var deltaposition = Input.GetTouch(0).deltaPosition;
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // store the initial touch position
                    lastPos = touch.position;
                    break;

                case TouchPhase.Moved:
                    float rotX = deltaposition.y * rotationSpeed * -1;
                    float rotY = deltaposition.x * rotationSpeed * -1;

                    transform.eulerAngles += new Vector3(rotX, rotY, 0f);
                    break;
            }
        }
    }
}
