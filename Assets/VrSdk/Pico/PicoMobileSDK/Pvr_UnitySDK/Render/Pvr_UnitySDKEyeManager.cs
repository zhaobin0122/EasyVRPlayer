using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using Pvr_UnitySDKAPI;

public class Pvr_UnitySDKEyeManager : MonoBehaviour
{
    private static Pvr_UnitySDKEyeManager instance;
    public static Pvr_UnitySDKEyeManager Instance
    {
        get
        {
            if (instance == null)
            {
                PLOG.E("Pvr_UnitySDKEyeManager instance is not init yet...");
                UnityEngine.Object.FindObjectOfType<Pvr_UnitySDKEyeManager>();
            }
            return instance;
        }
    }

    /************************************    Properties  *************************************/
    #region Properties
    /// <summary>
    /// Eyebuffer Layers
    /// </summary>
    private Pvr_UnitySDKEye[] eyes = null;
    public Pvr_UnitySDKEye[] Eyes
    {
        get
        {
            if (eyes == null)
            {
                eyes = Pvr_UnitySDKEye.Instances.ToArray();
            }
            return eyes;
        }
    }

    /// <summary>
    /// Compositor Layers
    /// </summary>
    private Pvr_UnitySDKEyeOverlay[] overlays = null;
    public Pvr_UnitySDKEyeOverlay[] Overlays
    {
        get
        {
            if (overlays == null)
            {
                overlays = Pvr_UnitySDKEyeOverlay.Instances.ToArray();
            }
            return overlays;
        }
    }
    [HideInInspector]
    public Camera LeftEyeCamera;
    [HideInInspector]
    public Camera RightEyeCamera;
    /// <summary>
    /// Mono Camera(only enable when Monoscopic switch on)
    /// </summary>
	[HideInInspector]
    public Camera MonoEyeCamera;
    [HideInInspector]
    public Camera BothEyeCamera;
    /// <summary>
    /// Mono Eye RTexture ID
    /// </summary>
    private int MonoEyeTextureID = 0;

    // wait for a number of frames, because custom splash screen(2D loading) need display time when first start-up.
    private readonly int WaitSplashScreenFrames = 3;
    public bool isFirstStartup = true;
    private int frameNum = 0;

    /// <summary>
    /// Max Compositor Layers
    /// </summary>
    private int MaxCompositorLayers = 15;

    [SerializeField]
    [HideInInspector]
    private EFoveationLevel foveationLevel = EFoveationLevel.None;
    [HideInInspector]
    public EFoveationLevel FoveationLevel
    {
        get
        {
            return foveationLevel;
        }
        set
        {
            if(value != foveationLevel)
            {
                foveationLevel = value;
                if (Application.isPlaying && FFRLevelChanged != null)
                {
                    FFRLevelChanged();
                }
            }
        }
    }
    public static Action FFRLevelChanged;

    [HideInInspector]
    public Vector2 FoveationGainValue = Vector2.zero;
    [HideInInspector]
    public float FoveationAreaValue = 0.0f;
    [HideInInspector]
    public float FoveationMinimumValue = 0.0f;
    #endregion

    /************************************ Process Interface  *********************************/
    #region  Process Interface
    private void SetCameraEnableEditor()
    {
        MonoEyeCamera.enabled = !Pvr_UnitySDKManager.SDK.VRModeEnabled || Pvr_UnitySDKManager.SDK.Monoscopic;
        for (int i = 0; i < Eyes.Length; i++)
        {
            if (Eyes[i].eyeSide == Eye.LeftEye || Eyes[i].eyeSide == Eye.RightEye)
            {
                Eyes[i].eyecamera.enabled = Pvr_UnitySDKManager.SDK.VRModeEnabled;
            }
            else if (Eyes[i].eyeSide == Eye.BothEye)
            {
                Eyes[i].eyecamera.enabled = false;
            }
        }
    }
    private void SetCamerasEnableByStereoRendering()
    {
        MonoEyeCamera.enabled = Pvr_UnitySDKManager.SDK.Monoscopic && Pvr_UnitySDKManager.StereoRenderPath == StereoRenderingPathPico.MultiPass;
    }
    private void SetupMonoCamera()
    {
        transform.localPosition = Vector3.zero;
        MonoEyeCamera.aspect = Pvr_UnitySDKManager.SDK.EyesAspect;
        MonoEyeCamera.rect = new Rect(0, 0, 1, 1);
    }

    private void SetupUpdate()
    {
        MonoEyeCamera.fieldOfView = Pvr_UnitySDKManager.SDK.EyeVFoV;
        MonoEyeCamera.aspect = Pvr_UnitySDKManager.SDK.EyesAspect;
        MonoEyeTextureID = Pvr_UnitySDKManager.SDK.currEyeTextureIdx;
    }

    private void MonoEyeRender()
    {
        SetupUpdate();
        if (Pvr_UnitySDKManager.SDK.eyeTextures[MonoEyeTextureID] != null)
        {
            Pvr_UnitySDKManager.SDK.eyeTextures[MonoEyeTextureID].DiscardContents();
            MonoEyeCamera.targetTexture = Pvr_UnitySDKManager.SDK.eyeTextures[MonoEyeTextureID];
        }
    }
    #endregion

    /*************************************  Unity API ****************************************/
    #region Unity API
    private void Awake()
    {
        instance = this;
        if (this.MonoEyeCamera == null)
        {
            this.MonoEyeCamera = this.GetComponent<Camera>();
        }
        if (this.LeftEyeCamera == null)
        {
            this.LeftEyeCamera = this.gameObject.transform.Find("LeftEye").GetComponent<Camera>();
        }
        if (this.RightEyeCamera == null)
        {
            this.RightEyeCamera = this.gameObject.transform.Find("RightEye").GetComponent<Camera>();
        }
        if (this.BothEyeCamera == null)
        {
            this.BothEyeCamera = this.gameObject.transform.Find("BothEye").GetComponent<Camera>();
        }
        if (this.BothEyeCamera != null)
        {
            this.BothEyeCamera.transform.GetComponent<Pvr_UnitySDKEye>().eyeSide = Eye.BothEye;
        }

        Pvr_UnitySDKManager.eventEnterVRMode += SetEyeTrackingMode;
    }

    void OnEnable()
    {
        StartCoroutine("EndOfFrame");
    }

    void Start()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        if (Pvr_UnitySDKManager.StereoRenderPath == StereoRenderingPathPico.SinglePass)
        {
            Pvr_UnitySDKManager.StereoRendering.InitEye(BothEyeCamera);
        }
        SetCamerasEnableByStereoRendering();
        SetupMonoCamera();
#endif

#if UNITY_EDITOR
        SetCameraEnableEditor();
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        SetCameraEnableEditor();
#endif

        if (Pvr_UnitySDKManager.StereoRenderPath == StereoRenderingPathPico.SinglePass)
        {
            for (int i = 0; i < Eyes.Length; i++)
            {
                if (Eyes[i].isActiveAndEnabled && Eyes[i].eyeSide == Eye.BothEye)
                {
                    Eyes[i].EyeRender();
                }
            }
        }

        if (Pvr_UnitySDKManager.StereoRenderPath == StereoRenderingPathPico.MultiPass)
        {
            if (!Pvr_UnitySDKManager.SDK.Monoscopic)
            {
                // Open Stero Eye Render
                for (int i = 0; i < Eyes.Length; i++)
                {
                    if (Eyes[i].isActiveAndEnabled && Eyes[i].eyeSide != Eye.BothEye)
                    {
                        Eyes[i].EyeRender();
                    }
                }
            }
            else
            {
                // Open Mono Eye Render
                MonoEyeRender();
            }
        }
    }

    private void OnPause()
    {
        Pvr_UnitySDKManager.eventEnterVRMode -= SetEyeTrackingMode;
    }

    
    void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnPostRender()
    {
        long eventdata = Pvr_UnitySDKAPI.System.UPvr_GetEyeBufferData(Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx]);
        // eyebuffer
        Pvr_UnitySDKAPI.System.UPvr_UnityEventData(eventdata);
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.LeftEyeEndFrame);

        Pvr_UnitySDKAPI.System.UPvr_UnityEventData(eventdata);
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.RightEyeEndFrame);
    } 
#endregion

    /************************************  End Of Per Frame  *************************************/
    // for eyebuffer params
    private int eyeTextureId = 0;
    private RenderEventType eventType = RenderEventType.LeftEyeEndFrame;

    private int overlayLayerDepth = 1;
    private int underlayLayerDepth = 0;
    private bool isHeadLocked = false;
    private int layerFlags = 0;

    IEnumerator EndOfFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
#if !UNITY_EDITOR
            if (!Pvr_UnitySDKManager.SDK.isEnterVRMode)
            {
                // Call GL.clear before Enter VRMode to avoid unexpected graph breaking.
                GL.Clear(false, true, Color.black);
            }
#endif
            if (isFirstStartup && frameNum == this.WaitSplashScreenFrames)
            {
                Pvr_UnitySDKAPI.System.UPvr_RemovePlatformLogo();
                Pvr_UnitySDKAPI.System.UPvr_StartVRModel();
                isFirstStartup = false;
            }
            else if (isFirstStartup && frameNum < this.WaitSplashScreenFrames)
            {
                PLOG.I("frameNum:" + frameNum);
                frameNum++;
            }

#region Eyebuffer
#if UNITY_2018_1_OR_NEWER && !UNITY_2019_1_OR_NEWER
            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
            {
                for (int i = 0; i < Eyes.Length; i++)
                {
                    if (!Eyes[i].isActiveAndEnabled || !Eyes[i].eyecamera.enabled)
                    {
                        continue;
                    }

                    switch (Eyes[i].eyeSide)
                    {
                        case Pvr_UnitySDKAPI.Eye.LeftEye:
                            eyeTextureId = Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx];
                            eventType = RenderEventType.LeftEyeEndFrame;
                            break;
                        case Pvr_UnitySDKAPI.Eye.RightEye:
                            if (!Pvr_UnitySDKManager.SDK.Monoscopic)
                            {
                                eyeTextureId = Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx + 3];
                            }
                            else
                            {
                                eyeTextureId = Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx];
                            }
                            eventType = RenderEventType.RightEyeEndFrame;
                            break;
                        case Pvr_UnitySDKAPI.Eye.BothEye:
                            eyeTextureId = Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx];
                            eventType = RenderEventType.BothEyeEndFrame;
                            break;
                        default:
                            break;
                    }
                    
                    // eyebuffer
                    Pvr_UnitySDKAPI.System.UPvr_UnityEventData(Pvr_UnitySDKAPI.System.UPvr_GetEyeBufferData(eyeTextureId));;
			     	Pvr_UnitySDKPluginEvent.Issue(eventType);

                    Pvr_UnitySDKPluginEvent.Issue(RenderEventType.EndEye);
                }
            }
#endif
#endregion

            // Compositor Layers: if find Overlay then Open Compositor Layers feature
#region Compositor Layers
            int boundaryState = BoundarySystem.UPvr_GetSeeThroughState();
            if (Pvr_UnitySDKEyeOverlay.Instances.Count > 0 && boundaryState != 2)
            {
                overlayLayerDepth = 1;
                underlayLayerDepth = 0;

                Pvr_UnitySDKEyeOverlay.Instances.Sort();
                for (int i = 0; i < Overlays.Length; i++)
                {
                    if (!Overlays[i].isActiveAndEnabled) continue;
                    if (Overlays[i].layerTextures[0] == null && Overlays[i].layerTextures[1] == null) continue;
                    if (Overlays[i].layerTransform != null && !Overlays[i].layerTransform.gameObject.activeSelf) continue;
                 
                    layerFlags = 0;

                    if (Overlays[i].overlayShape == Pvr_UnitySDKEyeOverlay.OverlayShape.Quad || Overlays[i].overlayShape == Pvr_UnitySDKEyeOverlay.OverlayShape.Cylinder)
                    {
                        if (Overlays[i].overlayType == Pvr_UnitySDKEyeOverlay.OverlayType.Overlay)
                        {
                            isHeadLocked = false;
                            if (Overlays[i].layerTransform != null && Overlays[i].layerTransform.parent == this.transform)
                            {
                                isHeadLocked = true;
                            }

                            // external surface
                            if (Overlays[i].isExternalAndroidSurface)
                            {
                                layerFlags = 1;
                                this.CreateExternalSurface(Overlays[i], overlayLayerDepth);
                            }

                            Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix((int)Overlays[i].overlayType, (int)Overlays[i].overlayShape, Overlays[i].layerTextureIds[0], (int)Pvr_UnitySDKAPI.Eye.LeftEye, overlayLayerDepth, isHeadLocked, layerFlags, Overlays[i].MVMatrixs[0],
							Overlays[i].ModelScales[0], Overlays[i].ModelRotations[0], Overlays[i].ModelTranslations[0], Overlays[i].CameraRotations[0], Overlays[i].CameraTranslations[0]);

                            Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix((int)Overlays[i].overlayType, (int)Overlays[i].overlayShape, Overlays[i].layerTextureIds[1], (int)Pvr_UnitySDKAPI.Eye.RightEye, overlayLayerDepth, isHeadLocked, layerFlags, Overlays[i].MVMatrixs[1],
							Overlays[i].ModelScales[1], Overlays[i].ModelRotations[1], Overlays[i].ModelTranslations[1], Overlays[i].CameraRotations[1], Overlays[i].CameraTranslations[1]);

                            overlayLayerDepth++;
                        }
                        else if (Overlays[i].overlayType == Pvr_UnitySDKEyeOverlay.OverlayType.Underlay)
                        {
                            // external surface
                            if (Overlays[i].isExternalAndroidSurface)
                            {
                                layerFlags = 1;
                                this.CreateExternalSurface(Overlays[i], underlayLayerDepth);
                            }

                            Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix((int)Overlays[i].overlayType, (int)Overlays[i].overlayShape, Overlays[i].layerTextureIds[0], (int)Pvr_UnitySDKAPI.Eye.LeftEye, underlayLayerDepth, false, layerFlags, Overlays[i].MVMatrixs[0],
							Overlays[i].ModelScales[0], Overlays[i].ModelRotations[0], Overlays[i].ModelTranslations[0], Overlays[i].CameraRotations[0], Overlays[i].CameraTranslations[0]);

                            Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix((int)Overlays[i].overlayType, (int)Overlays[i].overlayShape, Overlays[i].layerTextureIds[1], (int)Pvr_UnitySDKAPI.Eye.RightEye, underlayLayerDepth, false, layerFlags, Overlays[i].MVMatrixs[1],
							Overlays[i].ModelScales[1], Overlays[i].ModelRotations[1], Overlays[i].ModelTranslations[1], Overlays[i].CameraRotations[1], Overlays[i].CameraTranslations[1]);

                            underlayLayerDepth++;
                        }
                    }
                    else if (Overlays[i].overlayShape == Pvr_UnitySDKEyeOverlay.OverlayShape.Equirect)
                    {
                        // external surface
                        if (Overlays[i].isExternalAndroidSurface)
                        {
                            layerFlags = 1;
                            this.CreateExternalSurface(Overlays[i], 0);
                        }

                        // 360 Overlay Equirectangular Texture
                        Pvr_UnitySDKAPI.Render.UPvr_SetupLayerData(0, (int)Pvr_UnitySDKAPI.Eye.LeftEye, Overlays[i].layerTextureIds[0], (int)Overlays[i].overlayShape, layerFlags);
                        Pvr_UnitySDKAPI.Render.UPvr_SetupLayerData(0, (int)Pvr_UnitySDKAPI.Eye.RightEye, Overlays[i].layerTextureIds[1], (int)Overlays[i].overlayShape, layerFlags);
                    }
                }
#endregion
            }

            // Begin TimeWarp
            //Pvr_UnitySDKPluginEvent.IssueWithData(RenderEventType.TimeWarp, Pvr_UnitySDKManager.SDK.RenderviewNumber);
            Pvr_UnitySDKAPI.System.UPvr_UnityEventData(Pvr_UnitySDKAPI.System.UPvr_GetEyeBufferData(0));
            Pvr_UnitySDKPluginEvent.Issue(RenderEventType.TimeWarp);
            Pvr_UnitySDKManager.SDK.currEyeTextureIdx = Pvr_UnitySDKManager.SDK.nextEyeTextureIdx;
            Pvr_UnitySDKManager.SDK.nextEyeTextureIdx = (Pvr_UnitySDKManager.SDK.nextEyeTextureIdx + 1) % 3;
        }
    }

    
    /// <summary>
    /// Create External Surface
    /// </summary>
    /// <param name="overlayInstance"></param>
    /// <param name="layerDepth"></param>
    private void CreateExternalSurface(Pvr_UnitySDKEyeOverlay overlayInstance, int layerDepth)
    {
#if (UNITY_ANDROID && !UNITY_EDITOR)
        if (overlayInstance.externalAndroidSurfaceObject == System.IntPtr.Zero)
        {          
            overlayInstance.externalAndroidSurfaceObject = Pvr_UnitySDKAPI.Render.UPvr_CreateLayerAndroidSurface((int)overlayInstance.overlayType, layerDepth);
            Debug.LogFormat("CreateExternalSurface: Overlay Type:{0}, LayerDepth:{1}, SurfaceObject:{2}", overlayInstance.overlayType, layerDepth, overlayInstance.externalAndroidSurfaceObject);

            if (overlayInstance.externalAndroidSurfaceObject != System.IntPtr.Zero)
            {
                if (overlayInstance.externalAndroidSurfaceObjectCreated != null)
                {
                    overlayInstance.externalAndroidSurfaceObjectCreated();
                }
            }
        }
#endif
    }


#region EyeTrack  
    [HideInInspector]
    public bool trackEyes = false;
    [HideInInspector]
    public Vector3 eyePoint;
    private EyeTrackingData eyePoseData;

    public bool SetEyeTrackingMode()
    {
        int trackingMode = Pvr_UnitySDKAPI.System.UPvr_GetTrackingMode();
        bool supportEyeTracking = (trackingMode & (int)Pvr_UnitySDKAPI.TrackingMode.PVR_TRACKING_MODE_EYE) != 0;
        bool result = false;

        if (trackEyes && supportEyeTracking)
        {
            result = Pvr_UnitySDKAPI.System.UPvr_setTrackingMode((int)Pvr_UnitySDKAPI.TrackingMode.PVR_TRACKING_MODE_POSITION | (int)Pvr_UnitySDKAPI.TrackingMode.PVR_TRACKING_MODE_EYE);
        }
        Debug.Log("SetEyeTrackingMode trackEyes " + trackEyes + " supportEyeTracking " + supportEyeTracking + " result " + result);
        return result;
    }

    public Vector3 GetEyeTrackingPos()
    {
        if (!Pvr_UnitySDKEyeManager.Instance.trackEyes)
            return Vector3.zero;

        EyeDeviceInfo info = GetDeviceInfo();

        Vector3 frustumSize = Vector3.zero;
        frustumSize.x = 0.5f * (info.targetFrustumLeft.right - info.targetFrustumLeft.left);
        frustumSize.y = 0.5f * (info.targetFrustumLeft.top - info.targetFrustumLeft.bottom);
        frustumSize.z = info.targetFrustumLeft.near;

        bool result = Pvr_UnitySDKAPI.System.UPvr_getEyeTrackingData(ref eyePoseData);
        if (!result)
        {
            PLOG.E("UPvr_getEyeTrackingData failed " + result);
            return Vector3.zero;
        }

        var combinedDirection = Vector3.zero;
        if ((eyePoseData.combinedEyePoseStatus & (int)pvrEyePoseStatus.kGazeVectorValid) != 0)
            combinedDirection = eyePoseData.combinedEyeGazeVector;

        if (combinedDirection.sqrMagnitude > 0f)
        {
            combinedDirection.Normalize();
            
            float denominator = Vector3.Dot(combinedDirection, Vector3.forward);
            if (denominator > float.Epsilon)
            {
                eyePoint = combinedDirection * frustumSize.z / denominator;
                eyePoint.x /= frustumSize.x; // [-1..1]
                eyePoint.y /= frustumSize.y; // [-1..1]
            }
        }
        return eyePoint;
    }

    private EyeDeviceInfo GetDeviceInfo()
    {
        EyeDeviceInfo info;
        info.targetFrustumLeft.left = -0.0428f;
        info.targetFrustumLeft.right = 0.0428f;
        info.targetFrustumLeft.top = 0.0428f;
        info.targetFrustumLeft.bottom = -0.0428f;
        info.targetFrustumLeft.near = 0.0508f;
        info.targetFrustumLeft.far = 100f;
        info.targetFrustumRight.left = -0.0428f;
        info.targetFrustumRight.right = 0.0428f;
        info.targetFrustumRight.top = 0.0428f;
        info.targetFrustumRight.bottom = -0.0428f;
        info.targetFrustumRight.near = 0.0508f;
        info.targetFrustumRight.far = 100f;

        return info;
    }
#endregion
}