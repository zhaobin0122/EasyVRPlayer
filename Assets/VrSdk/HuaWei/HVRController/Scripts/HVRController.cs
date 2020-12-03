using UnityEngine;
using HVRCORE;

public class HVRController : MonoBehaviour
{

    private static readonly string TAG = "Unity_HVRController";
    private GameObject m_DefaultControllerObj;
    private GameObject m_PhoneControllerObj;
    private GameObject m_ControllerObj;

    private GameObject m_Canvas;
    private GameObject m_Line, m_Anchor;
    public static GameObject m_RightEventCamera;
    public static GameObject m_LeftEventCamera;
    private GameObject m_EventCamera = null;

    public static float m_ObjForwardDir = 0.068f;
    public static float m_ObjUpDir = 0.035f;
    public static int m_DefaultDistance = 68;

    private IController m_Controller;
    public static IController m_RightController;
    public static IController m_LeftController;

    private Quaternion m_ContrllerRotation;
    public static Quaternion m_RightContrllerRotation;
    public static Quaternion m_LeftContrllerRotation;
    private Posture m_ControllePos;
    private Vector3 m_HandleInitPosition = HVRArmModel.m_DefaultControllerPosition;
    private Vector3 m_DefaultPosition = new Vector3(1.0f, 1.0f, 1.0f);

    private bool m_IsScreenOn = true;
    private bool m_IsPhoneControllerMode = false;
    private bool m_IsRightController = true;
    private ControllerType m_ControllerType = ControllerType.Controller3DOF;

    public static float m_Radio;
    private float m_DefaultRake = 0.4f;
    private float m_InitDistance;//the default distance between controller and camera;

    private void Awake()
    {
        m_DefaultControllerObj = transform.Find("DefaultController").gameObject;
        m_PhoneControllerObj = transform.Find("PhoneController").gameObject;
        m_ControllerObj = m_DefaultControllerObj;
        if (gameObject.name.Equals("HVRLeftController"))
        {
            m_LeftEventCamera = transform.Find("EventCamera").gameObject;
            m_EventCamera = m_LeftEventCamera;
        }
        else
        {
            m_RightEventCamera = transform.Find("EventCamera").gameObject;
            m_EventCamera = m_RightEventCamera;
        }
        m_Canvas = transform.Find("Canvas").gameObject;
        m_Line = m_EventCamera.transform.Find("LineRender").gameObject;
        m_Anchor = m_EventCamera.transform.Find("Anchor").gameObject;

        if (Application.platform == RuntimePlatform.Android)
        {
            SetControllerStatus(true);
        }
    }   

    private void ActivateAvailableController() {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (HVRPluginCore.IsPhoneController())
            {
                HVRLogCore.LOGI(TAG, "on phonecontroller mode");
                m_IsPhoneControllerMode = true;
                m_DefaultControllerObj.SetActive(false);
                m_PhoneControllerObj.SetActive(true);
                m_ControllerObj = m_PhoneControllerObj;
            }
            else
            {
                m_IsPhoneControllerMode = false;
                HVRLogCore.LOGI(TAG, "on controller mode");
                m_DefaultControllerObj.SetActive(true);
                m_PhoneControllerObj.SetActive(false);
                m_ControllerObj = m_DefaultControllerObj;
            }
        }
    }

    private void GetControllerHandle() {
        IControllerHandle ControllerHandle = HvrApi.GetControllerHandle();
        if (ControllerHandle == null)
        {
            HVRLogCore.LOGW(TAG, "ControllerHandle is null");
            return;
        }
        int[] indices = ControllerHandle.GetValidIndices();
        if (Application.platform == RuntimePlatform.Android)
        {
            if (gameObject.name.Equals("HVRLeftController"))
            {
                HVRLogCore.LOGI(TAG, "current is left controller");
                if (indices.Length >= 3)
                {
                    m_LeftController = ControllerHandle.GetControllerByIndex(indices[2]);
                }
                m_Controller = m_LeftController;
                m_IsRightController = false;
                if (m_LeftController == null || !m_LeftController.IsAvailable())
                {
                    HVRLogCore.LOGI(TAG, "left controller is not available");
                    gameObject.SetActive(false);
                }
            }
            else
            {
                HVRLogCore.LOGI(TAG, "current is right controller");
                m_RightController = ControllerHandle.GetControllerByIndex(indices[1]);
                m_Controller = m_RightController;
                m_IsRightController = true;
            }
        }
        else
        {
            m_Controller = ControllerHandle.GetControllerByIndex(indices[1]);
            m_RightController = m_Controller;
            m_LeftController = m_Controller;
        }
    }

    void Start()
    {
        ControllerPositionInit();
        ActivateAvailableController();
        GetControllerHandle();

        if (m_Controller != null)
        {
            m_ControllerType = m_Controller.GetControllerType();
            HVRLogCore.LOGI(TAG, "mControllerType: " + m_ControllerType);
        }        
    }

    private void ControllerPositionInit()
    {
        if (HVRArmModel.Instance != null)
        {
            HVRArmModel.Instance.OnInit(); //Controller position initialization
        }
        m_HandleInitPosition = HVRArmModel.m_DefaultControllerPosition + new Vector3(0.0f, 0.0f, HVRArmModel.m_Radius);
        if (HVRControllerManager.m_IsLeftHandMode || gameObject.name.Equals("HVRLeftController"))
        {
            HVRLogCore.LOGW(TAG, "m_HandleInitPosition.x *= -1");
            m_HandleInitPosition.x *= -1;
        }
        else
        {
            HVRLogCore.LOGW(TAG, "m_HandleInitPosition.x *= 1");
            m_HandleInitPosition.x *= 1;
        }
        transform.position = m_HandleInitPosition + HVRLayoutCore.m_CamCtrObj.position;
        m_InitDistance = Vector3.Distance(transform.position, HVRLayoutCore.m_CamCtrObj.position);
    }

    private void SetControllerStatus(bool isActive)
    {
        m_ControllerObj.SetActive(isActive);
        m_Line.SetActive(isActive);
        m_Anchor.SetActive(isActive);
        m_Canvas.SetActive(isActive);
    }


    void Update()
    {
        if (m_Controller == null)
        {
            HVRLogCore.LOGW(TAG, "m_Controller is null");
            return;
        }
        bool isControllerDataValid = false;

        ControllerStatus controllerStatus = m_Controller.GetControllerStatus();

        switch (controllerStatus)
        {
            case ControllerStatus.ControllerStatusDisconnected:
                HVRLogCore.LOGW(TAG, "Controller Disconnected");
                break;
            case ControllerStatus.ControllerStatusScanning:
                HVRLogCore.LOGI(TAG, "Controller Scanning");
                break;
            case ControllerStatus.ControllerStatusConnecting:
                HVRLogCore.LOGI(TAG, "Controller Connecting");
                break;
            case ControllerStatus.ControllerStatusConnected:
                isControllerDataValid = true;
                break;
            case ControllerStatus.ControllerStatusError:
                break;
        }

        if (!isControllerDataValid)
        {
            return;
        }
        UpdateControllerMode();
        UpdateControllerPos();
    }

    private void UpdateControllerMode()
    {
        if (m_IsPhoneControllerMode && Application.platform == RuntimePlatform.Android)
        {
            if (!m_IsScreenOn && HVRPluginCore.IsScreenOn())
            {
                HVRLogCore.LOGI(TAG, "switch to screen on");
                m_IsScreenOn = true;
                SetControllerStatus(true);
            }
            else if (m_IsScreenOn && !HVRPluginCore.IsScreenOn())
            {
                HVRLogCore.LOGI(TAG, "switch to screen off");
                m_IsScreenOn = false;
                SetControllerStatus(false);
            }
            if (HVRPluginCore.IsSwitchToController())
            {
                m_IsScreenOn = false;
                m_IsPhoneControllerMode = false;
                HVRLogCore.LOGI(TAG, "switch to controller mode");
                m_PhoneControllerObj.SetActive(false);
                m_ControllerObj = m_DefaultControllerObj;
                SetControllerStatus(true);
            }
        }
    }

    private void UpdateControllerPos()
    {
        m_Controller.GetPosture(ref m_ControllePos);
        m_ContrllerRotation = m_ControllePos.rotation;
        transform.localRotation = m_ContrllerRotation;
        if (!m_IsRightController)
        {
            m_LeftContrllerRotation = m_ContrllerRotation;
        }
        else
        {
            m_RightContrllerRotation = m_ContrllerRotation;
        }

        m_EventCamera.transform.localRotation = Quaternion.identity;

        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            m_EventCamera.transform.position = HVRLayoutCore.m_CamCtrObj.position;
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            m_EventCamera.transform.localPosition = Vector3.zero;
        }

        if (m_ControllerType == ControllerType.Controller3DOF)
        {
            if (HVRArmModel.Instance != null)
            {
                HVRArmModel.Instance.OnUpdateController();
                transform.parent.position = HVRLayoutCore.m_CamCtrObj.position;
                transform.localPosition = HVRArmModel.Instance.m_WristPosition; //Controller follows the main camera movement                
            }
            m_Radio = ((1 - m_DefaultRake * m_InitDistance) + m_DefaultRake * Vector3.Distance(transform.position, HVRLayoutCore.m_CamCtrObj.position));
            transform.localScale = Vector3.one * m_Radio;
        }
        else
        {
            transform.localPosition = m_ControllePos.position;
        }
    }
}
