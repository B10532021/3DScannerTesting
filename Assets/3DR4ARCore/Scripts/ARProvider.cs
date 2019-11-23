#if HAS_GOOGLE_ARCORE
using GoogleARCore;
using GoogleARCoreInternal;
using System;
#endif
#if HAS_HUAWEI_ARENGINE
using Common;
using HuaweiARInternal;
using HuaweiARUnitySDK;
#endif

using UnityEngine;

namespace LVonasek
{
    public enum ARSDK
    {
        NONE,
        ARCORE,
        ARENGINE,
    };

    public struct ARState
    {
        public int frameHandle;
        public int sessionHandle;
        public bool tracked;
    }

    public class ARProvider : MonoBehaviour
    {
        private const string ENGINE_ID = "3DR4ARCORE_Engine";

        public ARSDK preferedSDK;

        public GameObject ARCorePrefab;
        public GameObject AREnginePrefab;
        public GameObject ARLight;
        private GameObject ARObject;

        private void Start()
        {
            ARSDK selectedSDK = GetSelectedAR();
            if (selectedSDK == ARSDK.NONE)
            {
                selectedSDK = preferedSDK;
            }

#if HAS_HUAWEI_ARENGINE
            if (selectedSDK == ARSDK.ARENGINE)
            {
                ARObject = Instantiate(AREnginePrefab);
                try
                {
                    ARObject.GetComponent<SessionComponent>().OnApplicationPause(false);
                } catch (Exception e)
                {
                }

            }
#endif
#if HAS_GOOGLE_ARCORE
            if (selectedSDK == ARSDK.ARCORE)
            {
                ARObject = ARCorePrefab;
            }
#endif

            ARLight.transform.parent = GetCamera().transform;
        }

        public Camera GetCamera()
        {
            return ARObject.GetComponentInChildren<Camera>();
        }

        public int GetMode()
        {
            if (GetSelectedAR() == ARSDK.ARENGINE)
            {
                return 1;
            }
            return 0;
        }

        public ARSDK GetSelectedAR()
        {
            return (ARSDK)PlayerPrefs.GetInt(ENGINE_ID, (int)ARSDK.ARCORE);
        }
        
        public ARState GetState()
        {
            ARState state = new ARState
            {
                frameHandle = 0,
                sessionHandle = 0,
                tracked = false
            };
 #if HAS_GOOGLE_ARCORE
        
            if (GetSelectedAR() == ARSDK.ARCORE)
            {
                NativeSession native = LifecycleManager.Instance.NativeSession;
                if (native != null)
                {
                    state = new ARState
                    {
                        frameHandle = native.FrameHandle.ToInt32(),
                        sessionHandle = native.SessionHandle.ToInt32(),
                        tracked = LifecycleManager.Instance.SessionStatus == SessionStatus.Tracking
                    };
                }
            }
#endif

#if HAS_HUAWEI_ARENGINE
            if (GetSelectedAR() == ARSDK.ARENGINE)
            {
                NDKSession native = ARSessionManager.Instance.m_ndkSession;
                if (native != null)
                {
                    state = new ARState
                    {
                        frameHandle = native.FrameHandle.ToInt32(),
                        sessionHandle = native.SessionHandle.ToInt32(),
                        tracked = ARFrame.GetTrackingState() == ARTrackable.TrackingState.TRACKING
                    };
                }
            }
#endif

            return state;
        }

        public void PauseSession()
        {
#if HAS_GOOGLE_ARCORE
            if (GetSelectedAR() == ARSDK.ARCORE)
                LifecycleManager.Instance.DisableSession();
#endif
#if HAS_HUAWEI_ARENGINE
            if (GetSelectedAR() == ARSDK.ARENGINE)
                ARSessionManager.Instance.Pause();
#endif
        }

        public void ResumeSession()
        {
#if HAS_GOOGLE_ARCORE
            if (GetSelectedAR() == ARSDK.ARCORE)
                LifecycleManager.Instance.EnableSession();
#endif
#if HAS_HUAWEI_ARENGINE
            if (GetSelectedAR() == ARSDK.ARENGINE)
                ARSessionManager.Instance.Resume();
#endif
        }

        public bool RequestCameraPermission()
        {
#if HAS_GOOGLE_ARCORE
            const string ANDROID_CAMERA_PERMISSION_NAME = "android.permission.CAMERA";
            if (AndroidPermissionsManager.IsPermissionGranted(ANDROID_CAMERA_PERMISSION_NAME))
            {
                return true;
            }
            else
            {
                AndroidPermissionsManager.RequestPermission(ANDROID_CAMERA_PERMISSION_NAME);
            }
#endif
            return false;
        }

        public void SelectAR()
        {
            if ((preferedSDK == ARSDK.ARENGINE) && IsAREngineSupported())
            {
                PlayerPrefs.SetInt(ENGINE_ID, (int)ARSDK.ARENGINE);
            }
            if (preferedSDK == ARSDK.ARCORE)
            {
                PlayerPrefs.SetInt(ENGINE_ID, (int)ARSDK.ARCORE);
            }
            PlayerPrefs.Save();
        }

        private bool IsAREngineSupported()
        {
#if HAS_HUAWEI_ARENGINE
            try
            {
                AREnginesAvaliblity ability = AREnginesSelector.Instance.CheckDeviceExecuteAbility();
                if ((AREnginesAvaliblity.HUAWEI_AR_ENGINE & ability) != 0)
                {
                    AREnginesSelector.Instance.SetAREngine(AREnginesType.HUAWEI_AR_ENGINE);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }

            GameObject go = null;
            try
            {
                go = Instantiate(AREnginePrefab);
                ARSession.CreateSession();
                ARSession.Config(go.GetComponent<SessionComponent>().Config);
                ARSession.Resume();
                ARSession.SetCameraTextureNameAuto();
                ARSession.SetDisplayGeometry(Screen.width, Screen.height);
                ARSession.Pause();
                ARSession.Stop();
                Destroy(go);
                return true;
            }
            catch (Exception e)
            {
            }
            if (go != null)
            {
                Destroy(go);
            }
#endif
            return false;
        }
    }
}