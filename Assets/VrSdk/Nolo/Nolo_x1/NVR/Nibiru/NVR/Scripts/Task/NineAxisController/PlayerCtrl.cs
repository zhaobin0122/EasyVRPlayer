using Nvr.Internal;
using Oahc;
using UnityEngine;

namespace NibiruTask
{
    public class PlayerCtrl : MonoBehaviour
    {
        private static PlayerCtrl m_instance = null;

        private bool isCreateControllerHandler = false;

        public bool debugInEditor;

        [SerializeField]
        private bool gamepadEnabled = false;

        public bool controllerModelDisplay = false;

        public bool GamepadEnabled
        {
            get
            {
                return gamepadEnabled;
            }
            set
            {
                gamepadEnabled = value;
            }
        }

        public static PlayerCtrl Instance
        {
            get
            {
                return m_instance;
            }
        }

        public Vector3 HeadPosition { get; set; }

        NvrArmModel nxrArmModel;
        public Transform mTransform;
        Quaternion controllerQuat = new Quaternion(0, 0, 0, 1);

        public void OnDeviceConnectState(int state, CDevice device)
        {
            Debug.Log("NvrPlayerCtrl.onDeviceConnectState:" + (state == 0 ? " Connect " : " Disconnect ") + "," + (device == null));
            if (state == 0)
            {
                NibiruRemindBox.Instance.CalibrationDelay();
                NvrViewer.Instance.HideHeadControl();
            }
            else
            {
                NvrViewer.Instance.ShowReticle();
            }
        }

        private void Awake()
        {
            m_instance = this;
            mTransform = transform;
        }

        void Start()
        {
            HeadPosition = Vector3.zero;
            nxrArmModel = GetComponent<NvrArmModel>();
            m_instance = this;
#if UNITY_ANDROID && !UNITY_EDITOR
            ControllerAndroid.onStart();
            NibiruTaskApi.setOnDeviceListener(OnDeviceConnectState);
#endif
            if ((ControllerAndroid.isDeviceConn((int)CDevice.NOLO_TYPE.LEFT) || ControllerAndroid.isDeviceConn((int)CDevice.NOLO_TYPE.RIGHT))
            && GameObject.Find("ControllerNOLO") != null)
            {
                gamepadEnabled = false;
                Debug.Log("Check Find Nolo Controller Connected, Dismiss 3dof GamePad !!!");
            }
        }

        public bool IsQuatConn()
        {
            if (debugInEditor) return true;

            if (gamepadEnabled)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return ControllerAndroid.isQuatConn();
#endif
            }
            return false;
        }

        void Update()
        {
#if UNITY_ANDROID //&& !UNITY_EDITOR
            bool isQuatConn = IsQuatConn();
            if (debugInEditor)
            {
                isQuatConn = true;
            }

            bool isNeedShowController = isQuatConn ? controllerModelDisplay : false;

            if (!isCreateControllerHandler && isNeedShowController)
            {
                CreateControllerHandler();
                isCreateControllerHandler = true;
            }
            else if (isCreateControllerHandler && !isNeedShowController)
            {
                DestroyChild(mTransform);
                isCreateControllerHandler = false;
                debugInEditor = false;
                NvrViewer.Instance.SwitchControllerMode(false);
            }

            if(isQuatConn)
            {
                //四元素
                if (InteractionManager.IsControllerConnected())
                {
                    float[] res = InteractionManager.GetControllerPose(InteractionManager.GetHandTypeByHandMode());
                    controllerQuat.x = res[0];
                    controllerQuat.y = res[1];
                    controllerQuat.z = res[2];
                    controllerQuat.w = res[3];
                }
                else
                {
                    float[] res = ControllerAndroid.getQuat(1);
                    controllerQuat.x = res[0];
                    controllerQuat.y = res[1];
                    controllerQuat.z = res[2];
                    controllerQuat.w = res[3];
                }

                //赋值 te.q为九轴传过来的四元数信息
                mTransform.rotation = controllerQuat;
                if (nxrArmModel != null)
                {
                    float factor = 1;
                    if (InteractionManager.IsInteractionSDKEnabled())
                    {
                        factor = InteractionManager.IsLeftControllerConnected() ? -1 : 1;
                    }
                    else if (ControllerAndroid.isQuatConn())
                    {
                        factor = ControllerAndroid.getHandMode() == 0 ? 1 : -1;
                    }

                    nxrArmModel.OnControllerInputUpdated();
                    Vector3 armPos = new Vector3(nxrArmModel.ControllerPositionFromHead.x * factor, nxrArmModel.ControllerPositionFromHead.y, nxrArmModel.ControllerPositionFromHead.z);
                    mTransform.position = HeadPosition + armPos;
                }
            }
#endif

        }

        private NvrTrackedDevice.Nibiru_ControllerStates _prevStates;
        private NvrTrackedDevice.Nibiru_ControllerStates _currentStates;
        bool GetButtonDown(NvrTrackedDevice.ButtonID btn)
        {
            return (_currentStates.buttons & (1 << (int)btn)) != 0 && (_prevStates.buttons & (1 << (int)btn)) == 0;
        }

        bool GetButtonUp(NvrTrackedDevice.ButtonID btn)
        {
            return (_currentStates.buttons & (1 << (int)btn)) == 0 && (_prevStates.buttons & (1 << (int)btn)) != 0;
        }

        void OnApplicationPause()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            ControllerAndroid.onPause();
#endif
        }

        void OnApplicationQuit()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            ControllerAndroid.onStop();
#endif
        }

        /// <summary>
        /// 删除所有子物体
        /// </summary>
        /// <param name="_trsParent"></param>
        public void DestroyChild(Transform _trsParent)
        {
            for (int i = 0; i < _trsParent.childCount; i++)
            {
                GameObject go = _trsParent.GetChild(i).gameObject;
                Destroy(go);
            }
        }

        string Controller_Name_DEFAULT = "Handler_01";
        string Controller_Name_XIMMERSE = "Handler_03";
        string Controller_Name_CLEER = "Handler_04";

        string GetControllerName()
        {
            int deviceType = ControllerAndroid.getDeviceType();
            if (deviceType == CDevice.DEVICE_CLEER)
            {
                return Controller_Name_CLEER;
            }
            else if (deviceType == CDevice.DEVICE_XIMMERSE)
            {
                return Controller_Name_XIMMERSE;
            }

            return Controller_Name_DEFAULT;
        }


        //创建手柄
        public void CreateControllerHandler()
        {
            if (InteractionManager.IsInteractionSDKEnabled() && InteractionManager.IsSupportControllerModel())
            {
                Debug.Log("CreateControllerModel.Controller3Dof");
                CreateControllerModel("Controller3Dof", InteractionManager.GetControllerConfig());
                return;
            }

            string name = GetControllerName();
            Debug.Log("CreateControllerHandler." + name);
            DestroyChild(mTransform);
            GameObject handlerPrefabs = Resources.Load<GameObject>(string.Concat("Controller/", name));
            GameObject objHandler = Instantiate(handlerPrefabs);
            objHandler.transform.parent = mTransform;
            objHandler.transform.localPosition = new Vector3(0, 0, 0);
            objHandler.transform.localRotation = new Quaternion(0, 0, 0, 1);
            objHandler.transform.localScale = Vector3.one / 2;
            NvrTrackedDevice trackedDevice = GetComponent<NvrTrackedDevice>();
            if (trackedDevice != null)
            {
               trackedDevice.ReloadLaserPointer(objHandler.GetComponent<NvrLaserPointer>());
            }
            //close
            NvrViewer.Instance.SwitchControllerMode(true);
            Debug.Log("HideGaze.ForceUseReticle");
        }

        NvrTrackedDevice trackedDevice = null;
        public NvrLaserPointer GetControllerLaser()
        {
            if (trackedDevice == null)
            {
                trackedDevice = GetComponent<NvrTrackedDevice>();
            }

            if (trackedDevice == null)
            {
                return null;
            }

            if (trackedDevice.GetLaserPointer() == null)
            {
                return null;
            }

            return trackedDevice.GetLaserPointer();
        }

        public GameObject GetControllerLaserDot()
        {
            if (trackedDevice == null)
            {
                trackedDevice = GetComponent<NvrTrackedDevice>();
            }

            if(trackedDevice == null)
            {
                return null;
            }

            if(trackedDevice.GetLaserPointer() == null)
            {
                return null;
            }

            return trackedDevice.GetLaserPointer().GetLosDot();
        }

        public void ChangeControllerDisplay(bool show)
        {
            controllerModelDisplay = show;
        }

        public void CreateControllerModel(string objName, InteractionManager.ControllerConfig mControllerConfig)
        {
            string objPath = mControllerConfig.objPath;
            if (objPath == null) return;

            DestroyChild(mTransform);

            GameObject go = new GameObject(objName);
            NvrLaserPointer mNxrLaserPointer = go.AddComponent<NvrLaserPointer>();
            go.transform.SetParent(transform);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = new Vector3(0, 0, 0);
            go.transform.localRotation = new Quaternion(0, 0, 0, 1);

            GameObject modelGO = new GameObject("model");
            modelGO.transform.SetParent(go.transform);
            modelGO.transform.localScale = new Vector3(mControllerConfig.modelScale[0]
                , mControllerConfig.modelScale[1], mControllerConfig.modelScale[2]);
            modelGO.transform.localRotation = Quaternion.Euler(mControllerConfig.modelRotation[0],
                mControllerConfig.modelRotation[1], mControllerConfig.modelRotation[2]);
            modelGO.transform.localPosition = new Vector3(0, 0, 0.2f);

            //  string objPath = "/system/etc/Objs/housing_bott.obj";
            if (Application.isEditor)
            {
                objPath = Application.dataPath + "/NVR/Resources/Controller/Objs/controller_model.obj";
            }
            Debug.Log("objPath=" + objPath);

            ObjModelLoader mObjModelLoader = GetComponent<ObjModelLoader>();
            if (mObjModelLoader == null)
            {
                gameObject.AddComponent<ObjMaterial>();
                mObjModelLoader = gameObject.AddComponent<ObjModelLoader>();
            }
            mObjModelLoader.LoadObjFile(objPath, modelGO.transform);

            GameObject powerGO = new GameObject("Power");
            powerGO.transform.SetParent(go.transform);

            MeshRenderer powerMeshRenderer = powerGO.AddComponent<MeshRenderer>();
            Mesh quadMesh = new Mesh();
            quadMesh.name = "QUAD";
            float quadSize = 0.5f;
            quadMesh.vertices = new Vector3[] {
                new Vector3(-1 * quadSize, -1* quadSize, 0),
                new Vector3(-1* quadSize, 1* quadSize, 0),
                new Vector3(1* quadSize, 1* quadSize, 0),
                new Vector3(1* quadSize, -1* quadSize, 0) };
            quadMesh.uv = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0)
           };
            int[] triangles = { 0, 1, 2, 0, 2, 3 };
            quadMesh.triangles = triangles;

            powerGO.AddComponent<MeshFilter>().mesh = quadMesh;
            powerGO.AddComponent<MeshCollider>();
            powerGO.AddComponent<NibiruControllerPower>();

            powerGO.transform.localPosition = new Vector3(mControllerConfig.batteryPosition[0], mControllerConfig.batteryPosition[1]
                , mControllerConfig.batteryPosition[2]);
            powerGO.transform.localRotation = Quaternion.Euler(mControllerConfig.batteryRotation[0], mControllerConfig.batteryRotation[1]
                , mControllerConfig.batteryRotation[2]);
            powerGO.transform.localScale = new Vector3(mControllerConfig.batteryScale[0], mControllerConfig.batteryScale[1]
                , mControllerConfig.batteryScale[2]);

            // 射线起点
            mNxrLaserPointer.SetHolderLocalPosition(new Vector3(mControllerConfig.rayStartPosition[0], mControllerConfig.rayStartPosition[1],
                mControllerConfig.rayStartPosition[2]));

            NvrTrackedDevice trackedDevice = GetComponent<NvrTrackedDevice>();
            if (trackedDevice != null)
            {
                trackedDevice.ReloadLaserPointer(mNxrLaserPointer);
            }
            //close
            NvrViewer.Instance.SwitchControllerMode(true);
        }

    }


}