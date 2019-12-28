using UnityEngine;

public class RotateModel : MonoBehaviour
{
    public float rotationSpeed = 0.02f;

    public float movementSpeed = 0.001f;

    private bool move = false;
    private bool rotate = true;
   
    void Update()
    {
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            var deltaposition = Input.GetTouch(0).deltaPosition;
            if (move)
            {
                //單點觸控， 水平上下移動 改成rotation的x, y, z移動

                Vector3 deltaXZ = -transform.right * -deltaposition.x * movementSpeed;
                Vector3 deltaXY = Vector3.down * -deltaposition.y * movementSpeed;
                Vector3 deltaTotal = deltaXZ + deltaXY;
                transform.position += deltaTotal;
            }
            else if (rotate)
            {
                //單點觸控， 水平上下旋轉
                float rotX = -deltaposition.y * rotationSpeed;
                float rotY = -deltaposition.x * rotationSpeed;

                transform.eulerAngles -= new Vector3(rotX, -rotY, 0f);
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
    }
}
