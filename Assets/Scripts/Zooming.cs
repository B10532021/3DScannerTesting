namespace TouchMovement
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using LVonasek;

    public class Zooming : MonoBehaviour
    {
        public GameObject m_firstPersonCamera;
        private float zoomInCur = 0; //current zoom in distance
        private float zoomInMin = 0;
        private float zoomInMax = 8;
        private float oldDistance;
        private bool viewing;
        public Text LogText;
        // Start is called before the first frame update
        void Start()
        {
            Input.multiTouchEnabled = true;
            viewing = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.touchCount == 2)
            {
                m_firstPersonCamera.GetComponent<FollowTarget>().distanceToTarget = Zoom();

            }
        }

        public float Zoom()
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;
            if (zoomInCur >= zoomInMin && zoomInCur <= zoomInMax)
            {
                zoomInCur = zoomInCur - difference * 0.007f;
                if (zoomInCur <= zoomInMin)
                {
                    zoomInCur = zoomInMin;
                }
                else if (zoomInCur >= zoomInMax)
                {
                    zoomInCur = zoomInMax;
                }
            }
            return zoomInCur;

        }

        public void IsViewing()
        {
            if (viewing)
            {
                viewing = false;
                m_firstPersonCamera.GetComponent<FollowTarget>().distanceToTarget = oldDistance;
            }
            else
            {
                viewing = true;
                oldDistance = m_firstPersonCamera.GetComponent<FollowTarget>().distanceToTarget;
                m_firstPersonCamera.GetComponent<FollowTarget>().distanceToTarget = 2;
            }
        }

        public void ResumeRecording()
        {
            viewing = false;
            m_firstPersonCamera.GetComponent<FollowTarget>().distanceToTarget = oldDistance;
        }

        public void BackToScanning()
        {
            viewing = false;
        }
    }

}
