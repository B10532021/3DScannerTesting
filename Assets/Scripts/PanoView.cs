using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;
using TouchMovement;
using LVonasek;

public class PanoView : MonoBehaviour
{
    private bool panoView;
    private bool move;
    private bool rotate;
    private Touch oldTouch;  //上次觸控點1(手指1)
    private Transform camTransform;
    private Transform oldTransform;

    private float xAngle;
    private float yAngle;
    private float xAngTemp;
    private float yAngTemp;


    // Start is called before the first frame update
    void Start()
    {
        panoView = false;
        move = true;
        rotate = false;
        camTransform = GetComponent<Camera>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        
        //沒有觸控  
        if (Input.touchCount <= 0)
        {
            return;
        }

        if (panoView && Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
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
            else if(rotate)
            {
                //單點觸控， 水平上下旋轉
                // LogText.text = rotateTarget.transform.position.x + "," + rotateTarget.transform.position.y + "," + rotateTarget.transform.position.z;
                float rotX = deltaposition.y * Time.deltaTime * -1;
                float rotY = deltaposition.x * Time.deltaTime * -1;

                this.transform.eulerAngles += new Vector3(rotX, -rotY, 0f);
            }
        }

    }

    public void PanoMode()
    {
        if (panoView)
        {
            panoView = false;
            GetComponent<TrackedPoseDriver>().enabled = true;
            GameObject.Find("First Person Camera").GetComponent<TrackedPoseDriver>().enabled = true;

        }
        else
        {
            panoView = true;
            GetComponent<TrackedPoseDriver>().enabled = false;
            GameObject.Find("First Person Camera").GetComponent<TrackedPoseDriver>().enabled = false;
            transform.position = Vector3.forward * 0.5f;
        }
    }

    public void ResumeTrackedMode()
    {
        panoView = false;
        GetComponent<TrackedPoseDriver>().enabled = true;
        GameObject.Find("First Person Camera").GetComponent<TrackedPoseDriver>().enabled = true;
    }
    public void MoveMode()
    {
        move = true;
        rotate = false;
    }

    public void RotateMode()
    {
        rotate = true;
        move = false;
    }

    public bool GetPanoMode()
    {
        return panoView;
    }
}
