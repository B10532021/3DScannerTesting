using UnityEngine;

public class RotateModel : MonoBehaviour
{
    private Vector2 lastPos;
    public float rotationSpeed = 0.02f;
    private bool move = false;
    private bool rotate = true;
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

                    transform.eulerAngles -= new Vector3(rotX, -rotY, 0f);
                    break;
            }
        }
    }
    /*void Update()
    {

        //沒有觸控  
        if (Input.touchCount <= 0)
        {
            return;
        }

        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            var deltaposition = Input.GetTouch(0).deltaPosition;
            if (move)
            {
                //單點觸控， 水平上下移動 改成rotation的x, y, z移動

                Vector3 deltaXZ = -transform.right * deltaposition.x * 0.003f;
                Vector3 deltaXY = Vector3.down * deltaposition.y * 0.003f;
                Vector3 deltaTotal = deltaXZ + deltaXY;
                transform.position += deltaTotal;
            }
            else if (rotate)
            {
                //單點觸控， 水平上下旋轉
                // LogText.text = rotateTarget.transform.position.x + "," + rotateTarget.transform.position.y + "," + rotateTarget.transform.position.z;
                float rotX = deltaposition.y * Time.deltaTime * -1;
                float rotY = deltaposition.x * Time.deltaTime * -1;

                this.transform.eulerAngles -= new Vector3(rotX, -rotY, 0f);
            }
        }
        
    }

    public void MoveMode()
    {
        move = true;
        rotate = false;
    }

    public void RotateMode()
    {
        move = false;
        rotate = true;
    }*/
}
