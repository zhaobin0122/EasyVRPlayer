//  Copyright 2016 Nibiru. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using NibiruTask;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using XR;
/// The NvrViewer object communicates with the head-mounted display.
/// Is is repsonsible for:
/// -  Querying the device for viewing parameters
/// -  Retrieving the latest head tracking data
/// -  Providing the rendered scene to the device for distortion correction (optional)
///
/// There should only be one of these in a scene.  An instance will be generated automatically
/// by this script at runtime, or you can add one via the Editor if you wish to customize
/// its starting properties.
/// 
namespace Nvr.Internal
{
    [AddComponentMenu("NVR/NvrViewer")]
    public class NvrViewer : MonoBehaviour
    {
        // base 2.1.4.x
        public const string NVR_SDK_VERSION = "1.0.0_20200514";

        const string CORE_VERSION = "CV_1";
        // dtr or not 
        public static bool USE_DTR = true;

        private static int _texture_count = 6;

        // 头部角度限制范围
        private float[] headEulerAnglesRange = null;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("OnSceneLoaded->" + scene.name + " , Triggered=" + Triggered);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// The singleton instance of the NvrViewer class.
        public static NvrViewer Instance
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                USE_DTR = false;
                if (instance == null && !Application.isPlaying)
                {
                    Debug.Log("Create NvrViewer Instance !");
                    instance = UnityEngine.Object.FindObjectOfType<NvrViewer>();
                }
#endif
                if (instance == null)
                {
                    Debug.LogError("No NvrViewer instance found.  Ensure one exists in the scene, or call "
                        + "NvrViewer.Create() at startup to generate one.\n"
                        + "If one does exist but hasn't called Awake() yet, "
                        + "then this error is due to order-of-initialization.\n"
                        + "In that case, consider moving "
                        + "your first reference to NvrViewer.Instance to a later point in time.\n"
                        + "If exiting the scene, this indicates that the NvrViewer object has already "
                        + "been destroyed.");
                }
                return instance;
            }
        }

        private static NvrViewer instance = null;
        public NvrEye[] eyes = new NvrEye[2];
        private byte[] winTypeName = new byte[] { 110, 120, 114, 46, 78, 118, 114, 87, 105, 110, 66, 97, 115, 101 };//N/v/r/W/i/n/B/a/s/e

        /// Generate a NvrViewer instance.  Takes no action if one already exists.
        public static void Create()
        {
            if (instance == null && UnityEngine.Object.FindObjectOfType<NvrViewer>() == null)
            {
                Debug.Log("Creating NvrViewer object");
                var go = new GameObject("NvrViewer", typeof(NvrViewer));
                go.transform.localPosition = Vector3.zero;
                // sdk will be set by Awake().
            }
        }

        /// The StereoController instance attached to the main camera, or null if there is none.
        /// @note Cached for performance.
        public static NvrStereoController Controller
        {
            get
            {
                if (currentController == null)
                {
                    currentController = FindObjectOfType<NvrStereoController>();
                }
                return currentController;
            }
        }
        private static NvrStereoController currentController;


        /// Whether to draw directly to the output window (_true_), or to an offscreen buffer
        /// first and then blit (_false_). If you wish to use Deferred Rendering or any
        /// Image Effects in stereo, turn this option off.  A common symptom that indicates
        /// you should do so is when one of the eyes is spread across the entire screen.
        [SerializeField]
        private bool openEffectRender = false;

        /// <summary>
        ///  false关闭后处理，true开启后处理
        /// </summary>
        public bool EffectRender
        {
            get
            {
                return openEffectRender;
            }
            set
            {
                if (value != openEffectRender)
                {
                    openEffectRender = value;
                }
            }
        }

        /// Determine whether the scene renders in stereo or mono.
        /// _True_ means to render in stereo, and _false_ means to render in mono.
        public bool VRModeEnabled
        {
            get
            {
                return vrModeEnabled;
            }
            set
            {
                if (value != vrModeEnabled && device != null)
                {
                    device.SetVRModeEnabled(value);
                }
                vrModeEnabled = value;
            }
        }

        [SerializeField]
        private bool vrModeEnabled = true;


        /// <summary>
        /// 头瞄控制
        /// </summary>
        public HeadControl HeadControl
        {
            get
            {
                return headControlEnabled;
            }
            set
            {
                headControlEnabled = value;
                UpdateHeadControl();
            }
        }
        [SerializeField]
        private HeadControl headControlEnabled = HeadControl.GazeApplication;


        public float Duration
        {
            get
            {
                return duration;
            }
            set
            {
                duration = value;
            }
        }
        [SerializeField]
        private float duration = 2;

        NvrReticle mNxrReticle;

        public NvrReticle GetNvrReticle()
        {
            InitNvrReticleScript();
            return mNxrReticle;
        }

        public void DismissReticle()
        {
            GetNvrReticle().Dismiss();
        }

        public void ShowReticle()
        {
            GetNvrReticle().Show();
        }

        private void InitNvrReticleScript()
        {
            if (mNxrReticle == null)
            {
                mNxrReticle = FindObjectOfType<NvrReticle>();
            }
        }

        /// <summary>
        /// 显示头控
        /// </summary>
        public void ShowHeadControl()
        {
            InitNvrReticleScript();
            if (mNxrReticle != null)
            {
                mNxrReticle.HeadShow();
                Debug.Log("ShowHeadControl");
            }
        }

        /// <summary>
        /// 隐藏头控
        /// </summary>
        public void HideHeadControl()
        {
            InitNvrReticleScript();
            if (mNxrReticle != null)
            {
                mNxrReticle.HeadDismiss();
                Debug.Log("HideHeadControl");
            }
        }

        public Vector3 NoloLeftControllerOffset
        {
            get
            {
                return noloLeftControllerYOffset;
            }
            set
            {
                noloLeftControllerYOffset = value;
            }
        }

        public Vector3 NoloRightControllerOffset
        {
            get
            {
                return noloRightControllerOffset;
            }
            set
            {
                noloRightControllerOffset = value;
            }
        }

        [SerializeField]
        private Vector3 noloLeftControllerYOffset = new Vector3(-0.5f, -1, 0);

        [SerializeField]
        private Vector3 noloRightControllerOffset = new Vector3(0, -1, 0);






        public bool TrackerPosition
        {
            get
            {
                return trackerPosition;
            }
            set
            {
                trackerPosition = value;
            }
        }

        // 纹理质量
        [SerializeField]
        public TextureMSAA textureMsaa = TextureMSAA.NONE;
        public TextureMSAA TextureMSAA
        {
            get
            {
                return textureMsaa;
            }
            set
            {
                if (value != textureMsaa)
                {
                    textureMsaa = value;
                }
            }
        }

#if UNITY_ANDROID
        [SerializeField]
        private bool trackerPosition = false;
#endif

        public bool InitialRecenter
        {
            get
            {
                return initialRecenter;
            }
            set
            {
                initialRecenter = value;
            }
        }

        [SerializeField]
        private bool initialRecenter = false;

        //纹理质量
        [SerializeField]
        public TextureQuality textureQuality = TextureQuality.Better;
        public TextureQuality TextureQuality
        {
            get
            {
                return textureQuality;
            }
            set
            {
                if (value != textureQuality)
                {
                    textureQuality = value;
                }
            }
        }

        [SerializeField]
        private bool requestLock = false;
        /// <summary>
        ///  在Unity渲染层面，固定头部姿态
        /// </summary>
        public bool LockHeadTracker
        {
            get
            {
                return requestLock;
            }
            set
            {
                if (value != requestLock)
                {
                    requestLock = value;
                }
            }
        }

        /// <summary>
        ///  SDK底层锁定头部姿态
        /// </summary>
        public void RequestLock()
        {
            if (device != null)
            {
                device.NLockTracker();
            }
        }

        /// <summary>
        /// SDK底层解锁头部姿态
        /// </summary>
        public void RequestUnLock()
        {
            if (device != null)
            {
                device.NUnLockTracker();
            }
        }

        [SerializeField]
        private bool distortionEnabled = true;
        public bool DistortionEnabled
        {
            get
            {
                return distortionEnabled;
            }
            set
            {
                if (value != distortionEnabled)
                {
                    distortionEnabled = value;
                }
                NvrGlobal.distortionEnabled = distortionEnabled;
            }
        }

        



 
        public void GazeApi(GazeTag tag)
        {
            GazeApi(tag, "");
        }

        private bool IsNativeGazeShow = false;
  
        /// <summary>
        ///  GazeTag.Show， GazeTag.Hide 后面的param传 "" 即可
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="param"></param>
        public void GazeApi(GazeTag tag, string param)
        {
            if (device != null)
            {
                bool rslt = device.GazeApi(tag, param);
                if (tag == GazeTag.Show)
                {
                    bool useDFT = USE_DTR && !NvrGlobal.supportDtr;
                    IsNativeGazeShow = useDFT ? true : rslt;
                }
                else if (tag == GazeTag.Hide)
                {
                    IsNativeGazeShow = false;
                }
            }
        }

        // 手柄
        public void SwitchControllerMode(bool enabled)
        {
            if (enabled)
            {
                HeadControl = HeadControl.Controller;
            }
            else
            {
                // 
                HeadControl = HeadControl.GazeApplication;
            }
        }

        /// <summary>
        /// true  强制使用白点
        /// false  使用系统自带白点
        /// </summary>
        /// <param name="enabled"></param>
        public void SwitchApplicationReticle(bool enabled)
        {
            InitNvrReticleScript();

            bool IsControllerMode = HeadControl == HeadControl.Controller;

            if (enabled)
            {
                if (mNxrReticle != null) mNxrReticle.Show();
                GazeInputModule.gazePointer = mNxrReticle;
            }
            else if (!enabled && (!NvrGlobal.isVR9Platform || IsControllerMode))
            {
                if (mNxrReticle != null)
                {
                    mNxrReticle.Dismiss();
                }
                GazeInputModule.gazePointer = null;
            }

            if (enabled)
            {
                GazeApi(GazeTag.Hide);
            }
        }



#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        /// The screen size to emulate when testing in the Unity Editor.
        public NvrProfile.ScreenSizes ScreenSize
        {
            get
            {
                return screenSize;
            }
            set
            {
                if (value != screenSize)
                {
                    screenSize = value;
                    if (device != null)
                    {
                        device.UpdateScreenData();
                    }
                }
            }
        }
        [SerializeField]
        private NvrProfile.ScreenSizes screenSize = NvrProfile.ScreenSizes.Nexus5;

        /// The viewer type to emulate when testing in the Unity Editor.
        public NvrProfile.ViewerTypes ViewerType
        {
            get
            {
                return viewerType;
            }
            set
            {
                if (value != viewerType)
                {
                    viewerType = value;
                    if (device != null)
                    {
                        device.UpdateScreenData();
                    }
                }
            }
        }
        [SerializeField]
        private NvrProfile.ViewerTypes viewerType = NvrProfile.ViewerTypes.CardboardMay2015;
#endif

        // The VR device that will be providing input data.
        private static BaseVRDevice device;



        /// The texture that Unity renders the scene to.  After the frame has been rendered,
        /// this texture is drawn to the screen with a lens distortion correction effect.
        /// The texture size is based on the size of the screen, the lens distortion
        /// parameters, and the #StereoScreenScale factor.
        public RenderTexture GetStereoScreen(int eye)
        {
            // Don't need it except for distortion correction.
            if (!vrModeEnabled || NvrGlobal.isVR9Platform)
            {
                return null;
            }
            if (eyeStereoScreens[0] == null)
            {
                // 初始化6个纹理
                InitEyeStereoScreens();
            }

            if (Application.isEditor || (NvrViewer.USE_DTR && !NvrGlobal.supportDtr))
            {
                // DFT or Editor
                return eyeStereoScreens[0];
            }

            // 获取对应索引的纹理
            return eyeStereoScreens[eye + _current_texture_index];
        }

        //初始创建6个纹理，左右各3个 【左右左右左右】
        public RenderTexture[] eyeStereoScreens = new RenderTexture[_texture_count];

        private void InitEyeStereoScreens()
        {
            InitEyeStereoScreens(-1, -1);
        }

        //初始化
        private void InitEyeStereoScreens(int width, int height)
        {
            RealeaseEyeStereoScreens();

#if UNITY_ANDROID || UNITY_EDITOR
            bool useDFT = USE_DTR && !NvrGlobal.supportDtr;
            if (!USE_DTR || useDFT)
            {
                // 编辑器模式 or 不支持DTR的DFT模式 只生成1个纹理
                RenderTexture rendetTexture = device.CreateStereoScreen(width, height);
                if (!rendetTexture.IsCreated())
                {
                    rendetTexture.Create();
                }
                int tid = (int)rendetTexture.GetNativeTexturePtr();
                for (int i = 0; i < _texture_count; i++)
                {
                    eyeStereoScreens[i] = rendetTexture;
                    _texture_ids[i] = tid;
                }
            }
            else
            {
                for (int i = 0; i < _texture_count; i++)
                {
                    eyeStereoScreens[i] = device.CreateStereoScreen(width, height);
                    eyeStereoScreens[i].Create();
                    _texture_ids[i] = (int)eyeStereoScreens[i].GetNativeTexturePtr();
                }
            }
#endif
        }

        // 释放所有纹理
        private void RealeaseEyeStereoScreens()
        {
            for (int i = 0; i < _texture_count; i++)
            {
                if (eyeStereoScreens[i] != null)
                {
                    eyeStereoScreens[i].Release();
                    eyeStereoScreens[i] = null;
                    _texture_ids[i] = 0;
                }
            }
            Debug.Log("RealeaseEyeStereoScreens");
        }

        /// Describes the current device, including phone screen.
        public NvrProfile Profile
        {
            get
            {
                return device.Profile;
            }
        }

        /// Distinguish the stereo eyes.
        public enum Eye
        {
            Left,   /// The left eye
            Right,  /// The right eye
            Center  /// The "center" eye (unused)
        }

        /// When retrieving the #Projection and #Viewport properties, specifies
        /// whether you want the values as seen through the viewer's lenses (`Distorted`) or
        /// as if no lenses were present (`Undistorted`).
        public enum Distortion
        {
            Distorted,   /// Viewing through the lenses
            Undistorted  /// No lenses
        }

        /// The transformation of head from origin in the tracking system.
        public Pose3D HeadPose
        {
            get
            {
                return device.GetHeadPose();
            }
        }

        /// The projection matrix for a given eye.
        /// This matrix is an off-axis perspective projection with near and far
        /// clipping planes of 1m and 1000m, respectively.  The NvrEye script
        /// takes care of adjusting the matrix for its particular camera.
        public Matrix4x4 Projection(Eye eye, Distortion distortion = Distortion.Distorted)
        {
            return device.GetProjection(eye, distortion);
        }

        /// The screen space viewport that the camera for the specified eye should render into.
        /// In the _Distorted_ case, this will be either the left or right half of the `StereoScreen`
        /// render texture.  In the _Undistorted_ case, it refers to the actual rectangle on the
        /// screen that the eye can see.
        public Rect Viewport(Eye eye, Distortion distortion = Distortion.Distorted)
        {
            return device.GetViewport(eye, distortion);
        }

        private void InitDevice()
        {
            if (device != null)
            {
                device.Destroy();
            }
            // 根据当前运行场景获取对应的设备对象
            device = BaseVRDevice.GetDevice();
            device.Init();

            device.SetVRModeEnabled(vrModeEnabled);
            // 更新界面数据
            device.UpdateScreenData();

            GazeApi(GazeTag.Show, "");
            GazeApi(GazeTag.Set_Size, ((int)GazeSize.Original).ToString());
        }

        NvrInput nvrInput;
        /// @note Each scene load causes an OnDestroy of the current SDK, followed
        /// by and Awake of a new one.  That should not cause the underlying native
        /// code to hiccup.  Exception: developer may call Application.DontDestroyOnLoad
        /// on the SDK if they want it to survive across scene loads.
        void Awake()
        {
            if (instance == null)
            {
                Loom.Initialize();

                instance = this;
                if (Application.isMobilePlatform)
                {
                    Application.runInBackground = false;
                    Input.gyro.enabled = false;
                    Debug.Log("SleepTimeout:" + SleepMode.ToString());
                    if(SleepMode == SleepTimeoutMode.NEVER_SLEEP)
                    {
                        // Disable screen dimming
                        Screen.sleepTimeout = SleepTimeout.NeverSleep;
                    } else
                    {
                        Screen.sleepTimeout = SleepTimeout.SystemSetting;
                    }
                }
            }
            if (instance != this)
            {
                Debug.LogError("There must be only one NvrViewer object in a scene.");
                UnityEngine.Object.DestroyImmediate(this);
                return;
            }

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            nvrInput = new NvrInput();
            InitDevice();
            if (!NvrGlobal.supportDtr && !NvrGlobal.isVR9Platform)
            {
                // 录屏功能需要使用2个脚本 [VR9不需要]
                // 非DTR需要
                AddPrePostRenderStages();
            }

#if UNITY_ANDROID
            // 在unity20172.0f3版本使用-1相当于half vsync，所以此处设置为90fps。目前机器最高就是90
            int targetFrameRate = Application.platform == RuntimePlatform.Android ? ((int)NvrGlobal.refreshRate > 0 ? (int)NvrGlobal.refreshRate : 90) : 60;
            if (NvrGlobal.isVR9Platform)
            {
                // 参考全志官方
                targetFrameRate = 60;
            }
            Application.targetFrameRate = targetFrameRate;
#endif

            if (Application.platform != RuntimePlatform.Android)
            {
                hasEnterVRMode = true;
            }

            if (NvrGlobal.isVR9Platform || !NvrGlobal.supportDtr)
            {
                //同步帧率到显示器刷新率
                QualitySettings.vSyncCount = 1;
            }
            else
            {
                // we sync in the TimeWarp, so we don't want unity syncing elsewhere
                QualitySettings.vSyncCount = 0;
            }

//#if UNITY_ANDROID && UNITY_EDITOR
//            GraphicsDeviceType[] graphicsDeviceType = UnityEditor.PlayerSettings.GetGraphicsAPIs(UnityEditor.BuildTarget.Android);
//            // Debug.Log("GraphicsDeviceType------->" + graphicsDeviceType[0].ToString());
//            if (graphicsDeviceType[0] != GraphicsDeviceType.OpenGLES2)
//            {
//                string title = "Incompatible graphics API detected!";
//                string message = "Please set graphics API to \"OpenGL ES 2.0\" and rebuild, or Some Api may not work as expected .";
//                UnityEditor.EditorUtility.DisplayDialog(title, message, "OK");
//                Debug.LogError(title + " " + message);
//            }
//#endif

            device.AndroidLog("Welcome to use Unity NVR SDK , current SDK VERSION is " + NVR_SDK_VERSION + ", j " + NvrGlobal.jarVersion + ", s " + NvrGlobal.soVersion + ", u " + Application.unityVersion + ", fps " + Application.targetFrameRate + ", vsync " + QualitySettings.vSyncCount  + ", antiAliasing : " + QualitySettings.antiAliasing
                 + "," + CORE_VERSION);

            AddStereoControllerToCameras();
        }

        
        void Start()
        {
            //lx changed for 5.4：在Awake中执行，不然在NvrArmModel调用之前Head还没创建
            //AddStereoControllerToCameras();


            if (eyeStereoScreens[0] == null && !NvrGlobal.isVR9Platform)
            {
                // 初始化6个纹理
                InitEyeStereoScreens();
                device.SetTextureSizeNative(eyeStereoScreens[0].width, eyeStereoScreens[1].height);
            }

            /*if (ShowFPS)
            {
                Transform[] father;
                father = GetComponentsInChildren<Transform>(true);
                GameObject FPS = null;
                foreach (Transform child in father)
                {
                    if (child.gameObject.name == "NxrFPS")
                    {
                        FPS = child.gameObject;
                        break;
                    }
                }

                if (FPS != null)
                {
                    FPS.SetActive(ShowFPS);
                }
                else
                {
                    GameObject fpsGo = Instantiate(Resources.Load("Prefabs/NxrFPS")) as GameObject;
#if UNITY_ANDROID && !UNITY_EDITOR
                    fpsGo.GetComponent<NxrFPS>().enabled = false;
                    fpsGo.AddComponent<FpsStatistics>();
#else
                    fpsGo.GetComponent<NxrFPS>().enabled = true;
#endif
                }
            } */

            UpdateHeadControl();
        }

        public void UpdateHeadControl()
        {
            // 已经设置强制使用Unity白点，不做处理
            switch (HeadControl)
            {
                case HeadControl.GazeApplication:
                    SwitchApplicationReticle(true);
                    GetNvrReticle().HeadDismiss();
                    break;
                case HeadControl.GazeSystem:
                    SwitchApplicationReticle(false);
                    GetNvrReticle().HeadDismiss();
                    GazeApi(GazeTag.Show);
                    break;
                case HeadControl.Hover:
                    GetNvrReticle().HeadShow();
                    SwitchApplicationReticle(true);
                    break;
                case HeadControl.Controller:
                    SwitchApplicationReticle(false);
                    GetNvrReticle().HeadDismiss();
                    GazeApi(GazeTag.Hide);
                    break;
            }
        }

        private NvrHead head;

        public NvrHead GetHead()
        {
            
            if (head == null && Controller != null)
            {
                head = Controller.Head;
            }

            if(head == null)
            {
                head = FindObjectOfType<NvrHead>();
            }
            return head;
        }

        private void Update()
        {
            if ((NoloVR_Plugins.GetElectricity(1) != 0 || NoloVR_Plugins.GetElectricity(2) != 0))
            {
                HeadControl = HeadControl.Controller;
            }

            UpdateState();

            if (!NvrGlobal.isVR9Platform)
            {
                NvrViewer.Instance.UpdateEyeTexture();
            }

            if (GazeInputModule.gazePointer != null)
            {
                GazeInputModule.gazePointer.UpdateStatus();
            }

            if (nvrInput != null)
            {
                nvrInput.Process();
            }
        }

        public BaseVRDevice GetDevice()
        {
            return device;
        }

        public void AndroidLog(string msg)
        {
            if (device != null)
            {
                device.AndroidLog(msg);
            }
            else
            {
                Debug.Log(msg);
            }
        }

        public void UpdateHeadPose()
        {
            if (device != null && hasEnterVRMode)
            {
                device.UpdateState();
            }
        }

        public void UpdateEyeTexture()
        {
            // 更新左右眼目标纹理
            if (USE_DTR && NvrGlobal.supportDtr)
            {
                // 更换纹理索引
                SwapBuffers();

                NvrEye[] eyes = NvrViewer.Instance.eyes;
                for (int i = 0; i < 2; i++)
                {
                    NvrEye eye = eyes[i];
                    if (eye != null)
                    {
                        eye.UpdateTargetTexture();
                    }
                }

            }
        }

        void AddPrePostRenderStages()
        {
            var preRender = UnityEngine.Object.FindObjectOfType<NvrPreRender>();
            if (preRender == null)
            {
                var go = new GameObject("PreRender", typeof(NvrPreRender));
                go.SendMessage("Reset");
                go.transform.parent = transform;
                Debug.Log("Add NvrPreRender");
            }
            var postRender = UnityEngine.Object.FindObjectOfType<NvrPostRender>();
            if (postRender == null)
            {
                var go = new GameObject("PostRender", typeof(NvrPostRender));
                go.SendMessage("Reset");
                go.transform.parent = transform;
                Debug.Log("Add NvrPostRender");
            }
        }

        /// Whether the viewer's trigger was pulled. True for exactly one complete frame
        /// after each pull
        public bool Triggered { get; set; }

        public bool ProfileChanged { get; private set; }

        // Only call device.UpdateState() once per frame.
        private int updatedToFrame = 0;

        public void UpdateState()
        {
            if (updatedToFrame != Time.frameCount)
            {
                updatedToFrame = Time.frameCount;
                DispatchEvents();

                if (NvrViewer.Instance.NeedUpdateNearFar && device != null && device.nibiruVRServiceId != 0)
                {
                    float far = NvrViewer.Instance.GetCameraFar();
                    float mNear = 0.0305f;
                    if (NvrGlobal.fovNear > -1)
                    {
                        mNear = NvrGlobal.fovNear;
                    }
                    device.SetCameraNearFar(mNear, far);
                    NvrViewer.Instance.NeedUpdateNearFar = false;

                    for (int i = 0; i < 2; i++)
                    {
                        NvrEye eye = eyes[i];
                        if (eye != null)
                        {
                            if (eye.cam.farClipPlane < NvrGlobal.fovFar)
                            {
                                eye.cam.farClipPlane = NvrGlobal.fovFar;
                            }
                        }
                    }

                }

            }
        }
        
        private void DispatchEvents()
        {
            // Update flags first by copying from device and other inputs.
            if (device == null) return;
            if (Input.GetMouseButton(0) && !Triggered)
            {
                Triggered = Input.GetMouseButtonDown(0);
            }
            ProfileChanged = device.profileChanged;
            if (device.profileChanged)
            {
                if(NvrOverrideSettings.OnProfileChangedEvent != null) NvrOverrideSettings.OnProfileChangedEvent();
                device.profileChanged = false;
            }
            /**  
            // 手柄上下左右兼容处理
            float leftKeyHor = Input.GetAxis("5th axis");
            float leftKeyVer =  Input.GetAxis("6th axis");

            if (leftKeyHor == 1)
            {
                // 左
                TriggerKeyEvent(KeyCode.LeftArrow);
                TriggerJoystickEvent(16, 0);
            }
            else if (leftKeyHor == -1)
            {
                // 右
                TriggerKeyEvent(KeyCode.RightArrow);
                TriggerJoystickEvent(17, 0);
            }
            if (leftKeyVer == -1)
            {
                // 上
                TriggerKeyEvent(KeyCode.UpArrow);
                TriggerJoystickEvent(14, 0);
            }
            else if (leftKeyVer == 1)
            {
                // 下
                TriggerKeyEvent(KeyCode.DownArrow);
                TriggerJoystickEvent(15, 0);
            }

            // 左摇杆
            float leftStickHor = Input.GetAxis("joystick_Horizontal");
            float leftStickVer = Input.GetAxis("joystick_Vertical");
            if (leftStickHor != 0)
            {
                TriggerJoystickEvent(10, leftStickHor);
            }
            if (leftStickVer != 0)
            {
                TriggerJoystickEvent(11, leftStickVer);
            }
            // 右摇杆
            float rightStickHor = Input.GetAxis("3th axis");
            float rightStickVer = Input.GetAxis("4th axis");
            if (rightStickHor != 0)
            {
                TriggerJoystickEvent(12, rightStickHor);
            }
            if (rightStickVer != 0)
            {
                TriggerJoystickEvent(13, rightStickVer);
            }
            // 
          **/
        }

        /// Resets the tracker so that the user's current direction becomes forward.
        public void Recenter()
        {
            device.Recenter();
            if (GetHead() != null)
            {
                GetHead().ResetInitEulerYAngle();
            }
        }

        /// Add a StereoController to any camera that does not have a Render Texture (meaning it is
        /// rendering to the screen).
        public static void AddStereoControllerToCameras()
        {
            for (int i = 0; i < Camera.allCameras.Length; i++)
            {
                Camera camera = Camera.allCameras[i];
                Debug.Log("Check Camera : " + camera.name);
                if (
                    (camera.tag == "MainCamera" || camera.tag == "NibiruCamera")&&
                    camera.targetTexture == null &&
                    camera.GetComponent<NvrStereoController>() == null &&
                    camera.GetComponent<NvrEye>() == null &&
                    camera.GetComponent<NvrPreRender>() == null &&
                    camera.GetComponent<NvrPostRender>() == null)
                {
                    camera.gameObject.AddComponent<NvrStereoController>();
                }
            }
        }

        void OnEnable()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            // This can happen if you edit code while the editor is in Play mode.
            if (device == null)
            {
                InitDevice();
            }
#endif
            device.OnPause(false);

            if (!NvrGlobal.isVR9Platform)
            {
                StartCoroutine("EndOfFrame");
            }
        }

        void OnDisable()
        {
            device.OnPause(true);
            StopCoroutine("EndOfFrame");
            Debug.Log("NvrViewer->OnDisable");
        }

        void OnApplicationPause(bool pause)
        {
            Debug.Log("NvrViewer->OnApplicationPause," + pause + ", hasEnterVRMode=" + hasEnterVRMode);
            // 首次不执行
            if (hasEnterVRMode)
            {
                device.OnApplicationPause(pause);
            }
        }

        void OnApplicationFocus(bool focus)
        {
            Debug.Log("NvrViewer->OnApplicationFocus," + focus);
            device.OnFocus(focus);
        }

        void OnApplicationQuit()
        {
            StopAllCoroutines();
            device.OnApplicationQuit();

            if(NvrOverrideSettings.OnApplicationQuitEvent != null)
            {
                NvrOverrideSettings.OnApplicationQuitEvent();
            }

            Debug.Log("NvrViewer->OnApplicationQuit");
#if UNITY_ANDROID && !UNITY_EDITOR
            System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
        }

        public void AppQuit()
        {
            device.AppQuit();
        }

        void OnDestroy()
        {
            VRModeEnabled = false;
            if (nvrInput != null)
            {
                nvrInput.Reset();
            }
            InteractionManager.Reset();
            if (device != null)
            {
                device.Destroy();
            }
            if (instance == this)
            {
                instance = null;
            }
            Debug.Log("NvrViewer->OnDestroy");
        }

        private bool hasEnterVRMode = false;
        public void EnterVRMode()
        {
            if (device != null)
            {
                if (!hasEnterVRMode)
                {
                    hasEnterVRMode = true;
                    device.EnterVRMode();
                    if (!NvrGlobal.isVR9Platform && NvrGlobal.supportDtr)
                    {
                        InitNvrReticleScript();
                        UpdateHeadControl();
                    }
                    else
                    {
                        Debug.Log(NvrGlobal.isVR9Platform + "-----Failed, supportDtr=" + NvrGlobal.supportDtr);
                    }
                }
            }
        }

        // 处理来自Android的调用  
        public void ResetHeadTrackerFromAndroid()
        {
            if (instance != null && device != null)
            {
                Recenter();
            }
            NibiruRemindBox.Instance.ReleaseDestory();
        }

        void OnVolumnUp()
        {
            if(nvrInput != null)
            {
                nvrInput.OnChangeKeyEvent(CKeyEvent.KEYCODE_VOLUME_DOWN, CKeyEvent.ACTION_DOWN);
            }
        }

        void OnVolumnDown()
        {
            if (nvrInput != null)
            {
                nvrInput.OnChangeKeyEvent(CKeyEvent.KEYCODE_VOLUME_UP, CKeyEvent.ACTION_DOWN);
            }
        }

        void OnKeyDown(string keyCode)
        {
            Debug.Log("OnKeyDown=" + keyCode);
            if (keyCode == NvrGlobal.KeyEvent_KEYCODE_ALT_LEFT)
            {
                if (nvrInput != null)
                {
                    nvrInput.OnChangeKeyEvent(CKeyEvent.KEYCODE_NF_1, CKeyEvent.ACTION_DOWN);
                }
            }
            else if (keyCode == NvrGlobal.KeyEvent_KEYCODE_MEDIA_RECORD)
            {
                if (nvrInput != null)
                {
                    nvrInput.OnChangeKeyEvent(CKeyEvent.KEYCODE_NF_2, CKeyEvent.ACTION_DOWN);
                }
            }
        }

        void OnKeyUp(string keyCode)
        {
            Debug.Log("OnKeyUp=" + keyCode);
            if (keyCode == NvrGlobal.KeyEvent_KEYCODE_ALT_LEFT)
            {
                if (nvrInput != null)
                {
                    nvrInput.OnChangeKeyEvent(CKeyEvent.KEYCODE_NF_1, CKeyEvent.ACTION_UP);
                }
            }
            else if (keyCode == NvrGlobal.KeyEvent_KEYCODE_MEDIA_RECORD)
            {
                if (nvrInput != null)
                {
                    nvrInput.OnChangeKeyEvent(CKeyEvent.KEYCODE_NF_2, CKeyEvent.ACTION_UP);
                }
            }
        }

        void OnActivityPause()
        {
            Debug.Log("OnActivityPause");
        }

        void OnActivityResume()
        {
            Debug.Log("OnActivityResume");
        }

        /// <summary>
        ///  系统分屏接口 1=系统分屏，0=应用分屏
        /// </summary>
        public void SetSystemVRMode(int flag)
        {
            device.NSetSystemVRMode(flag);
        }

        private int[] _texture_ids = new int[_texture_count];
        private int _current_texture_index, _next_texture_index;
        public bool SwapBuffers()
        {
            bool ret = true;
            for (int i = 0; i < _texture_count; i++)
            {
                if (!eyeStereoScreens[i].IsCreated())
                {
                    eyeStereoScreens[i].Create();
                    _texture_ids[i] = (int)eyeStereoScreens[i].GetNativeTexturePtr();
                    ret = false;
                }
            }

            _current_texture_index = _next_texture_index;
            _next_texture_index = (_next_texture_index + 2) % _texture_count;
            return ret;
        }

        public int GetEyeTextureId(int eye)
        {
            return _texture_ids[_current_texture_index + (int)eye];
        }

        public int GetTimeWarpViewNum()
        {
            return device.GetTimewarpViewNumber();
        }

        public List<GameObject> GetAllObjectsInScene()
        {
            GameObject[] pAllObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
            List<GameObject> pReturn = new List<GameObject>();
            foreach (GameObject pObject in pAllObjects)
            {
                if (pObject == null || !pObject.activeInHierarchy || pObject.hideFlags == HideFlags.NotEditable || pObject.hideFlags == HideFlags.HideAndDontSave)
                {
                    continue;
                }
                pReturn.Add(pObject);
            }
            return pReturn;
        }

        public Texture2D createTexture2D(RenderTexture renderTexture)
        {
            int width = renderTexture.width;
            int height = renderTexture.height;
            Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture2D.Apply();
            return texture2D;
        }

        private int frameCount = 0;
        private static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        IEnumerator EndOfFrame()
        {
            while (true)
            {
                yield return waitForEndOfFrame;
                if (USE_DTR && !hasEnterVRMode)
                {
                    Debug.Log("EndOfFrame->hasEnterRMode false");
                    // Call GL.clear before Enter VRMode to avoid unexpected graph breaking.
                    GL.Clear(false, true, Color.black);
                }
                else
                {
                    frameCount++;
                    if (USE_DTR && NvrGlobal.supportDtr)
                    {
                        NvrPluginEvent.IssueWithData(RenderEventType.TimeWarp, NvrViewer.Instance.GetTimeWarpViewNum());
                        // Debug.Log("TimeWrap." + frameCount);
                    }
                }
            }
        }

        public int GetFrameId()
        {
            return frameCount;
        }


       /* private void TriggerJoystickEvent(int index, float axisValue)
        {
            if (joystickListeners == null)
            {
                List<GameObject> allObject = GetAllObjectsInScene();
                joystickListeners = new List<INibiruJoystickListener>();
                foreach (GameObject obj in allObject)
                {
                    Component[] joystickcomps = obj.GetComponents(typeof(INibiruJoystickListener));

                    if (joystickcomps != null)
                    {
                        INibiruJoystickListener[] listeners = new INibiruJoystickListener[joystickcomps.Length];

                        for (int p = 0; p < joystickcomps.Length; p++)
                        {
                            listeners[p] = (INibiruJoystickListener)joystickcomps[p];
                        }
                        // INibiruJoystickListener
                        notifyJoystickPressed(listeners, index, axisValue);
                        foreach (Component cp in joystickcomps)
                        {
                            joystickListeners.Add((INibiruJoystickListener)cp);
                        }
                    }
                }
            }
            else
            {
                notifyJoystickPressed(joystickListeners.ToArray(), index, axisValue);
            }
        }

        private void notifyJoystickPressed(INibiruJoystickListener[] comps, int index, float axisValue)
        {
            if (comps == null) return;
            for (int i = 0; i < comps.Length; i++)
            {
                INibiruJoystickListener joystickListener = (INibiruJoystickListener)comps[i];
                if (joystickListener == null) continue;
                switch (index)
                {
                    case 0:
                        // l1
                        joystickListener.OnPressL1();
                        break;
                    case 1:
                        // l2
                        joystickListener.OnPressL2();
                        break;
                    case 2:
                        // r1
                        joystickListener.OnPressR1();
                        break;
                    case 3:
                        // r2
                        joystickListener.OnPressR2();
                        break;
                    case 4:
                        // select
                        joystickListener.OnPressSelect();
                        break;
                    case 5:
                        // start
                        joystickListener.OnPressStart();
                        break;
                    case 6:
                        // x
                        joystickListener.OnPressX();
                        break;
                    case 7:
                        // y
                        joystickListener.OnPressY();
                        break;
                    case 8:
                        // a
                        joystickListener.OnPressA();
                        break;
                    case 9:
                        // b
                        joystickListener.OnPressB();
                        break;
                    case 10:
                        // leftstickx
                        joystickListener.OnLeftStickX(axisValue);
                        break;
                    case 11:
                        // leftsticky
                        joystickListener.OnLeftStickY(axisValue);
                        break;
                    case 12:
                        // rightstickx
                        joystickListener.OnRightStickX(axisValue);
                        break;
                    case 13:
                        // rightsticky
                        joystickListener.OnRightStickY(axisValue);
                        break;
                    case 14:
                        // dpad-up
                        joystickListener.OnPressDpadUp();
                        break;
                    case 15:
                        // dpad-down
                        joystickListener.OnPressDpadDown();
                        break;
                    case 16:
                        // dpad-left
                        //joystickListener.OnPressDpadLeft();
                        joystickListener.OnPressDpadRight();
                        break;
                    case 17:
                        // dpad-right
                        //joystickListener.OnPressDpadRight();
                        joystickListener.OnPressDpadLeft();
                        break;
                    case 18:
                        joystickListener.OnLeftStickDown();
                        break;
                    case 19:
                        joystickListener.OnRightStickDown();
                        break;
                }

            }
        } */


        private float mFar = -1;
        private bool needUpdateNearFar = false;
        public void UpateCameraFar(float far)
        {
            mFar = far;
            needUpdateNearFar = true;
            NvrGlobal.fovFar = far;
            if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                // 编辑器及时生效
                Camera.main.farClipPlane = far;
            }
        }

        public float GetCameraFar()
        {
            return mFar;
        }

        public bool NeedUpdateNearFar
        {
            get
            {
                return needUpdateNearFar;
            }
            set
            {
                if (value != needUpdateNearFar)
                {
                    needUpdateNearFar = value;
                }
            }
        }


        private float oldFov = -1;

        private Matrix4x4[] eyeOriginalProjection = null;

        /// <summary>
        ///  检查初始是否需要更新相机投影矩阵
        /// </summary>
        /// <param name="eye"></param>
        public void UpdateEyeCameraProjection(Eye eye)
        {
            if (oldFov != -1 && eye == Eye.Right)
            {
                UpdateCameraFov(oldFov);
            }

            if (!Application.isEditor && device != null && eye == Eye.Right)
            {

                if (mFar > 0)
                {
                    float mNear = 0.0305f;
                    if (NvrGlobal.fovNear > -1)
                    {
                        mNear = NvrGlobal.fovNear;
                    }
                    // Debug.Log("new near : " + mNear + "," + NvrGlobal.fovNear+ ",new far : " + mFar + "," + NvrGlobal.fovFar);

                    // 更新camera  near far
                    float fovLeft = mNear * Mathf.Tan(-Profile.viewer.maxFOV.outer * Mathf.Deg2Rad);
                    float fovTop = mNear * Mathf.Tan(Profile.viewer.maxFOV.upper * Mathf.Deg2Rad);
                    float fovRight = mNear * Mathf.Tan(Profile.viewer.maxFOV.inner * Mathf.Deg2Rad);
                    float fovBottom = mNear * Mathf.Tan(-Profile.viewer.maxFOV.lower * Mathf.Deg2Rad);

                    //Debug.Log("fov : " +fovLeft+","+fovRight+","+fovTop+","+fovBottom);

                    Matrix4x4 eyeProjection = BaseVRDevice.MakeProjection(fovLeft, fovTop, fovRight, fovBottom, mNear, mFar);
                    for (int i = 0; i < 2; i++)
                    {
                        NvrEye mEye = eyes[i];
                        if (mEye != null)
                        {
                            mEye.cam.projectionMatrix = eyeProjection;
                        }
                    }

                }
            }
        }

        public void ResetCameraFov()
        {
            for (int i = 0; i < 2; i++)
            {
                if (eyeOriginalProjection == null || eyeOriginalProjection[i] == null) return;
                NvrEye eye = eyes[i];
                if (eye != null)
                {
                    eye.cam.projectionMatrix = eyeOriginalProjection[i];
                }
            }
            oldFov = -1;
        }

        /// <summary>
        /// 
        ///  fov范围[40~90]
        /// </summary>
        /// <param name="fov"></param>
        public void UpdateCameraFov(float fov)
        {
            if (fov > 90) fov = 90;
            if (fov < 5) fov = 5;
            // cache������͸�Ӿ���
            if (eyeOriginalProjection == null && eyes[0] != null && eyes[1] != null)
            {
                eyeOriginalProjection = new Matrix4x4[2];
                eyeOriginalProjection[0] = eyes[0].cam.projectionMatrix;
                eyeOriginalProjection[1] = eyes[1].cam.projectionMatrix;
            }
            oldFov = fov;
            float near = NvrGlobal.fovNear > 0 ? NvrGlobal.fovNear : 0.0305f;
            float far = NvrGlobal.fovFar > 0 ? NvrGlobal.fovFar : 2000;
            far = far > 100 ? far : 2000;
            float fovLeft = near * Mathf.Tan(-fov * Mathf.Deg2Rad);
            float fovTop = near * Mathf.Tan(fov * Mathf.Deg2Rad);
            float fovRight = near * Mathf.Tan(fov * Mathf.Deg2Rad);
            float fovBottom = near * Mathf.Tan(-fov * Mathf.Deg2Rad);
            Matrix4x4 eyeProjection = BaseVRDevice.MakeProjection(fovLeft, fovTop, fovRight, fovBottom, near, far);
            if (device != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    NvrEye eye = eyes[i];
                    if (eye != null)
                    {
                        eye.cam.projectionMatrix = eyeProjection;
                    }
                }
            }
        }

        /// <summary>
        ///  水平方向头部转动限制在固定范围 【 中间角度为0，左侧为负值，右侧为正值 】
        /// </summary>
        /// <param name="minRange"></param>
        /// <param name="maxRange"></param>
        public void SetHorizontalAngleRange(float minRange, float maxRange)
        {
            if (headEulerAnglesRange == null)
            {
                headEulerAnglesRange = new float[] { 0, 360, 0, 360 };
            }
            headEulerAnglesRange[0] = minRange + 360;
            headEulerAnglesRange[1] = maxRange;
        }

        /// <summary>
        ///  垂直方向头部转动限制在固定范围 【 中间角度为0，上面为负值，下面为正值 】
        /// </summary>
        /// <param name="minRange"></param>
        /// <param name="maxRange"></param>
        public void SetVerticalAngleRange(float minRange, float maxRange)
        {
            if (headEulerAnglesRange == null)
            {
                headEulerAnglesRange = new float[] { 0, 360, 0, 360 };
            }
            headEulerAnglesRange[2] = minRange + 360;
            headEulerAnglesRange[3] = maxRange;
        }

        /// <summary>
        ///  移除头部转动的限制
        /// </summary>
        public void RemoveAngleLimit()
        {
            headEulerAnglesRange = null;
        }

        public float[] GetHeadEulerAnglesRange()
        {
            return headEulerAnglesRange;
        }

        /// <summary>
        ///  获取androidSD卡路径（/storage/emulated/0）
        /// </summary>
        /// <returns>exp: /storage/emulated/0</returns>
        public string GetStoragePath()
        {
            return device.GetStoragePath();
        }

        public void SetIsKeepScreenOn(bool keep)
        {
            device.SetIsKeepScreenOn(keep);
        }

        private float defaultIpd = -1;
        private float userIpd = -1;
        /// <summary>
        ///  0.064
        /// </summary>
        /// <param name="ipd"></param>
        public void SetIpd(float ipd)
        {
            if (defaultIpd < 0)
            {
                defaultIpd = GetIpd();
            }
            NvrGlobal.dftProfileParams[0] = ipd; //0.063f;
            userIpd = ipd;
            device.SetIpd(ipd);
            device.UpdateScreenData();
        }

        public void ResetIpd()
        {
            if (defaultIpd < 0) return;
            SetIpd(defaultIpd);
        }

        public float GetIpd()
        {
            if (userIpd > 0) return userIpd;

            return eyes[0] == null ? 0.060f : 2 * Math.Abs(eyes[0].GetComponent<Camera>().transform.localPosition.x);
        }

        void onHandleAndroidMsg(string msgContent)
        {
            // msgId_msgContent
            string[] msgArr = msgContent.Split('_');
            int msgId = int.Parse(msgArr[0]);
            string msgData = msgArr[1];

            if ((MSG_ID)msgId == MSG_ID.MSG_onServerApiReady)
            {
                Loom.QueueOnMainThread((param) =>
                {
                    bool isReady = int.Parse((string)param) == 1;
                    if (NibiruTaskApi.serverApiReady != null)
                    {
                        NibiruTaskApi.serverApiReady(isReady);
                    }
                }, msgData);
            }
            else if ((MSG_ID)msgId == MSG_ID.MSG_onSysSleepApiReady)
            {
                Loom.QueueOnMainThread((param) =>
                {
                    bool isReady = int.Parse((string)param) == 1;
                    if (NibiruTaskApi.sysSleepApiReady != null)
                    {
                        NibiruTaskApi.sysSleepApiReady(isReady);
                    }
                }, msgData);
            }
            else if ((MSG_ID)msgId == MSG_ID.MSG_onInteractionDeviceConnectEvent)
            {
                Loom.QueueOnMainThread((param) =>
                {
                    InteractionManager.OnDeviceConnectState((string)param);
                }, msgContent);
            }
            else if ((MSG_ID)msgId == MSG_ID.MSG_onInteractionKeyEvent)
            {
                Loom.QueueOnMainThread((param) =>
                {
                    InteractionManager.OnCKeyEvent((string)param);
                }, msgContent);
            }
            else if ((MSG_ID)msgId == MSG_ID.MSG_onInteractionTouchEvent)
            {
                Loom.QueueOnMainThread((param) =>
                {
                    InteractionManager.OnCTouchEvent((string)param);
                }, msgContent);
            }
        }
        public void TurnOff()
        {
            device.TurnOff();
        }

        public void Reboot()
        {
            device.Reboot();
        }

        /// <summary>
        /// 
        ///  在Awake之后调用
        /// </summary>
        /// <returns></returns>
        public NibiruService GetNibiruService()
        {
            return device.GetNibiruService();
        }
		
	    // 休眠
        public enum SleepTimeoutMode
        {
            NEVER_SLEEP, SYSTEM_SETTING
        }
        [SerializeField]
        public SleepTimeoutMode sleepTimeoutMode = SleepTimeoutMode.NEVER_SLEEP;

        [SerializeField]
        public ControllerSupportMode controllerSupportMode = ControllerSupportMode.NONE;

        public SleepTimeoutMode SleepMode
        {
            get
            {
                return sleepTimeoutMode;
            }
            set
            {
                if (value != sleepTimeoutMode)
                {
                    sleepTimeoutMode = value;
                }
            }
        }

        public Camera GetMainCamera()
        {
            return Controller.cam;
        }

        public Camera GetLeftEyeCamera()
        {
            return Controller.Eyes[(int)Eye.Left].cam;
        }

        public Camera GetRightEyeCamera()
        {
            return Controller.Eyes[(int)Eye.Right].cam;
        }

        public Quaternion GetCameraQuaternion()
        {
            return GetHead().transform.rotation;
        }
 
    }
}