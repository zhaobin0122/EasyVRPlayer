using NibiruTask;
using System.Runtime.InteropServices;
using UnityEngine;
using XR;

namespace Nvr.Internal
{
    public class NvrTrackedDevice : MonoBehaviour
    {
        #region Struct
        [StructLayout(LayoutKind.Sequential)]
        public struct Nibiru_ControllerStates
        {
            public uint battery; // 电量
            public uint connectStatus;//连接状态 : hmd/left/right
            public uint buttons;//手柄按键
            public uint hmdButtons;// 一体机按键：上，下，左，右，确认
            public uint touches;//手柄触摸
            public Vector2 touchpadAxis;//触摸坐标
        }
        #endregion

        public enum NibiruDeviceType
        {
            Hmd = 0,
            LeftController,
            RightController
        }

        public enum ButtonID
        {
            Trigger = 0,
            Grip = 1,
            Menu = 21,
            System = -1,
            TouchPad = 20,
            DPadUp = 5,
            DPadDown = 4,
            DPadLeft = 2,
            DPadRight = 3,
            DPadCenter = 6,
            TrackpadTouch = 7,
        }

        public NibiruDeviceType deviceType;
        Nibiru_ControllerStates _prevStates;
        Nibiru_ControllerStates _currentStates;

        NvrControllerModel controllerModel;
        NvrLaserPointer laserPointer;
        public bool isGamePad;
        public NvrLaserPointer GetLaserPointer()
        {
            return laserPointer;
        }

        public void ReloadLaserPointer(NvrLaserPointer laserPointerIn)
        {
            this.laserPointer = laserPointerIn;
            if (laserPointer != null)
            {
                laserPointer.PointerIn += PointerInEventHandler;
                laserPointer.PointerOut += PointerOutEventHandler;
            }
        }

        private void Start()
        {
            isGamePad = gameObject.name.Contains("Gamepad");
            laserPointer = GetComponent<NvrLaserPointer>();
            if (laserPointer != null)
            {
                laserPointer.PointerIn += PointerInEventHandler;
                laserPointer.PointerOut += PointerOutEventHandler;
            }

            if (!isGamePad)
            {
                controllerModel = GetComponentInChildren<NvrControllerModel>();
               
                controllerModel.gameObject.SetActive(false);
            }
#if UNITY_ANDROID
            NibiruTaskApi.deviceConnectState += OnDeviceConnectState;
            _currentStates = new Nibiru_ControllerStates();
            _prevStates = new Nibiru_ControllerStates();

            // 默认不显示
            _currentStates.connectStatus = 0;
            _prevStates.connectStatus = 0;

            _currentStates.buttons = 0;
            _currentStates.touches = 0;
            _prevStates.buttons = 0;
            _prevStates.touches = 0;
#endif
        }

        private void OnDestroy()
        {
            if (laserPointer != null)
            {
                laserPointer.PointerIn -= PointerInEventHandler;
                laserPointer.PointerOut -= PointerOutEventHandler;
            }
#if UNITY_ANDROID
            NibiruTaskApi.deviceConnectState -= OnDeviceConnectState;
#endif
        }

        private int GetNoloType()
    {
        int noloType = (int)CDevice.NOLO_TYPE.NONE;
        if (deviceType == NibiruDeviceType.LeftController)
        {
            noloType = (int)CDevice.NOLO_TYPE.LEFT;
        }
        else if (deviceType == NibiruDeviceType.RightController)
        {
            noloType = (int)CDevice.NOLO_TYPE.RIGHT;
        }
        else if (deviceType == NibiruDeviceType.Hmd)
        {
            noloType = (int)CDevice.NOLO_TYPE.HEAD;
        }
        return noloType;
    }

    public void OnDeviceConnectState(int state, CDevice device)
    {
        if (device.getType() != CDevice.DEVICE_NOLO_SIXDOF) return;
        //0=connect,1=disconnect
            Debug.Log("NvrTrackedDevice.onDeviceConnectState:" + state + "," + device.getType() + "," + device.getName() + "," + device.getMode() + "," +
                device.getisQuat());
            if(state == 0)
            {
                _currentStates.connectStatus = 1;
                NvrViewer.Instance.SwitchControllerMode(true);
            } else
            {
                _currentStates.connectStatus = 0;
            }
        }

        void PointerInEventHandler(object sender, PointerEventArgs e)
        {
            NvrControllerHelper.ControllerRaycastObject = e.target.gameObject;
            Debug.Log("PointerInEventHandler---------" + e.target.gameObject.name);
        }

        void PointerOutEventHandler(object sender, PointerEventArgs e)
        {
            NvrControllerHelper.ControllerRaycastObject = null;
            Debug.Log("PointerOutEventHandler---------" + e.target.gameObject.name);
        }

        // Update is called once per frame
        void Update()
        {

#if UNITY_ANDROID //&& !UNITY_EDITOR
            if (!isGamePad)
            {
                // Android/NOLO
                int noloType = GetNoloType();
                int handType = noloType == (int)CDevice.NOLO_TYPE.LEFT ? (int)InteractionManager.NACTION_HAND_TYPE.HAND_LEFT : (int)InteractionManager.NACTION_HAND_TYPE.HAND_RIGHT;
                bool isConnected = false;
                if (InteractionManager.IsInteractionSDKEnabled())
                {
                    isConnected = InteractionManager.IsNoloControllerConnected(handType);
                } else
                {
                    isConnected = ControllerAndroid.isDeviceConn(noloType);
                }

                if (_currentStates.connectStatus == 0 && isConnected)
                {
                    NvrViewer.Instance.SwitchControllerMode(true);
                    _currentStates.connectStatus = 1;
                    PlayerCtrl mNxrPlayerCtrl = PlayerCtrl.Instance;
                    if (mNxrPlayerCtrl != null)
                    {
                        // 关闭3dof手柄显示
                        mNxrPlayerCtrl.GamepadEnabled = false;
                        // 关闭白点显示
                    }
                }
                else if (_currentStates.connectStatus == 1 && !isConnected)
                {
                    _currentStates.connectStatus = 0;
                }

                if (!IsConneted() && controllerModel != null && controllerModel.gameObject.activeSelf)
                {
                    controllerModel.gameObject.SetActive(false);
                    laserPointer.holder.SetActive(false);
                    NvrControllerHelper.ControllerRaycastObject = null;
                    Debug.Log("controllerModel Dismiss " + deviceType + "," + controllerModel.gameObject.activeSelf);
                }
                else if (IsConneted() && controllerModel != null && !controllerModel.gameObject.activeSelf)
                {
                    controllerModel.gameObject.SetActive(true);
                    laserPointer.holder.SetActive(true);
                    Debug.Log("controllerModel Show " + deviceType);
                }

                if (IsConneted())
                {
                    processControllerKeyEvent(noloType);
                    float[] poseData = new float[8];
                    if (InteractionManager.IsNoloControllerConnected(handType))
                    {
                        float[] pose = InteractionManager.GetControllerPose((InteractionManager.NACTION_HAND_TYPE) handType);
                        poseData[1] = pose[4];
                        poseData[2] = pose[5];
                        poseData[3] = pose[6];
                        for(int i=0; i<4; i++)
                        {
                            poseData[4 + i] = pose[i];
                        }
                    } else
                    {
                        poseData = ControllerAndroid.getCPoseEvent(noloType, 1);
                    }
                    Vector3 offset = noloType == (int)CDevice.NOLO_TYPE.LEFT ? NvrViewer.Instance.NoloLeftControllerOffset : NvrViewer.Instance.NoloRightControllerOffset;
                    transform.localPosition = new Vector3(poseData[1] + offset.x, poseData[2] + offset.y, poseData[3] + offset.z);
                    transform.localRotation = new Quaternion(poseData[4], poseData[5], poseData[6], poseData[7]);
                    // Debug.LogError("---->Position=" + transform.localPosition.ToString() + "," + transform.position.ToString());
                }
            }
#endif
        }

        private int[] lastState;
        private int[] curState;

        private void initState()
        {
            if (lastState == null)
            {
                lastState = new int[256];
                curState = new int[256];
                for (int i = 0; i < 256; i++)
                {
                    curState[i] = -1;
                    lastState[i] = -1;
                }
            }
        }

        private void processControllerKeyEvent(int noloType)
        {
            int handType = noloType == (int)CDevice.NOLO_TYPE.LEFT ? (int)InteractionManager.NACTION_HAND_TYPE.HAND_LEFT : (int)InteractionManager.NACTION_HAND_TYPE.HAND_RIGHT;
            initState();

            _prevStates = _currentStates;
            lastState = curState;
            float[] touchInfo = new float[] { 0, CKeyEvent.ACTION_UP, 0, 0}; // type-action-x-y
            if (InteractionManager.IsInteractionSDKEnabled())
            {
                curState = InteractionManager.GetKeyAction(handType);
                touchInfo[1] = curState[CKeyEvent.KEYCODE_CONTROLLER_TOUCHPAD_TOUCH];
                if (noloType == (int)CDevice.NOLO_TYPE.LEFT) {
                    Vector3 pos = InteractionManager.TouchPadPositionLeft;
                    touchInfo[2] = pos.x;
                    touchInfo[3] = pos.y;
                }
                else
                {
                    Vector3 pos = InteractionManager.TouchPadPositionRight;
                    touchInfo[2] = pos.x;
                    touchInfo[3] = pos.y;
                }
            }
            else
            {
                touchInfo = ControllerAndroid.getTouchEvent(noloType, 1);
                curState  = ControllerAndroid.getKeyState(noloType, 1);
            }

            // N
            int btnNibiru = curState[CKeyEvent.KEYCODE_BUTTON_NIBIRU];
            int btnStart = curState[CKeyEvent.KEYCODE_BUTTON_START];
            // Side A/B
            int btnSelect = curState[CKeyEvent.KEYCODE_BUTTON_SELECT];
            // Menu
            int btnApp = curState[CKeyEvent.KEYCODE_BUTTON_APP];
            // TouchPad
            int btnCenter = curState[CKeyEvent.KEYCODE_DPAD_CENTER];
            // Trigger
            int btnR1 = curState[CKeyEvent.KEYCODE_BUTTON_R1];

            // Nolo TouchPad = Center
            // Nolo Menu = App
            // Nolo Trigger = R1
            // Nolo Side = Select
            // Debug.LogError("=======>_currentStates.buttons=" + _currentStates.buttons);
            if (touchInfo[1] == CKeyEvent.ACTION_MOVE)
            {
                _currentStates.touches |= 1 << (int)ButtonID.TrackpadTouch;
                _currentStates.touchpadAxis = new Vector2(touchInfo[2], touchInfo[3]);
            }
            else if (touchInfo[1] == CKeyEvent.ACTION_UP && ((_currentStates.touches & (1 << (int)ButtonID.TrackpadTouch)) != 0))
            {
                _currentStates.touches = 0;
                _currentStates.touchpadAxis = new Vector2(0, 0);
            }

            if (btnCenter == 0)
            {
                // down
                _currentStates.buttons |= 1 << (int)ButtonID.TouchPad;
            }
            else if (lastState[CKeyEvent.KEYCODE_DPAD_CENTER] == 0)
            {
                // up
                _currentStates.buttons -= 1 << (int)ButtonID.TouchPad;
            }

            if (btnApp == 0)
            {
                _currentStates.buttons |= 1 << (int)ButtonID.Menu;
            }
            else if (lastState[CKeyEvent.KEYCODE_BUTTON_APP] == 0)
            {
                _currentStates.buttons -= 1 << (int)ButtonID.Menu;
            }

            if (btnR1 == 0)
            {
                _currentStates.buttons |= 1 << (int)ButtonID.Trigger;
            }
            else if (lastState[CKeyEvent.KEYCODE_BUTTON_R1] == 0)
            {
                _currentStates.buttons -= 1 << (int)ButtonID.Trigger;
            }

            if (btnSelect == 0)
            {
                _currentStates.buttons |= 1 << (int)ButtonID.Grip;
            }
            else if (lastState[CKeyEvent.KEYCODE_BUTTON_SELECT] == 0)
            {
                _currentStates.buttons -= 1 << (int)ButtonID.Grip;
            }

            //Debug.LogError("=====>" + _currentStates.buttons + "->Start=" + btnStart +
            //   "->Nibiru=" + btnNibiru +
            //   "->Select=" + btnSelect +
            //   "->App=" + btnApp +
            //   "->Center=" + btnCenter +
            //    "->R1=" + btnR1);
        }


        public bool IsConneted()
        {
            return _currentStates.connectStatus == 1;
        }

        public bool GetButtonDown(ButtonID btn)
        {
            return (_currentStates.buttons & (1 << (int)btn)) != 0 && (_prevStates.buttons & (1 << (int)btn)) == 0;
        }

        public bool GetButtonUp(ButtonID btn)
        {
            return (_currentStates.buttons & (1 << (int)btn)) == 0 && (_prevStates.buttons & (1 << (int)btn)) != 0;
        }

        public bool GetButtonPressed(ButtonID btn)
        {
            return (_currentStates.buttons & (1 << (int)btn)) != 0;
        }

        public bool GetTouchPressed(ButtonID btn)
        {
            return (_currentStates.touches & (1 << (int)btn)) != 0;
        }

        public bool GetTouchDown(ButtonID btn)
        {
            return (_currentStates.touches & (1 << (int)btn)) != 0 && (_prevStates.touches & (1 << (int)btn)) == 0;
        }

        public bool GetTouchUp(ButtonID btn)
        {
            return (_currentStates.touches & (1 << (int)btn)) == 0 && (_prevStates.touches & (1 << (int)btn)) != 0;
        }

        public Vector2 GetTouchPosition(ButtonID axisIndex = ButtonID.TrackpadTouch)
        {
            if ((_currentStates.touches & (1 << (int)axisIndex)) != 0)
            {
                return new Vector2(_currentStates.touchpadAxis.x, _currentStates.touchpadAxis.y);
            }
            return Vector2.zero;
        }
    }
}