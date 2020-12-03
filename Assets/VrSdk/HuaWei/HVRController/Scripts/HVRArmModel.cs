using UnityEngine;
using HVRCORE;

public class HVRArmModel
{
    public static Vector3 m_DefaultControllerPosition = new Vector3(0.22f, -0.75f, -0.26f);

    public static float m_Radius = 0.65f;

    private const float m_DeltaAlpha = 4.0f;

    private const float m_MinExtensionAngle = 0.0f;

    private const float m_MaxExtensionAngle = 60.0f;

    private const float m_ExtensionWeight = 0.4f;

    private static readonly Vector3 m_DefaultShoulderRight = new Vector3(0.155f, -0.46f, -0.03f);

    private static readonly Vector3 m_PointerOffset = new Vector3(0.0f, 0.003f, 0.68f);

    private static Vector3 m_ElbowInitialPosition = m_DefaultControllerPosition;

    private static Vector3 m_WristInitialPosition = new Vector3(0.0f, 0.0f, m_Radius); //m_Radius:Controller horizontal movement radius

    private static readonly Vector3 m_ArmExtensionOffset = new Vector3(-0.13f, 0.2f, 0.1f);

    private static HVRArmModel instance = null;

    private Vector3 m_TorsoDirection;

    private Vector3 m_HandedMultiplier;

    private Quaternion m_LastControllerRotation;

    public Posture m_ControllePos;

    public float m_AddedElbowHeight;

    public float m_AddedElbowDepth;

    public float m_PointerTiltAngle;

    public float m_FadeDistanceFromFace;

    public Handedness m_MyHandedness;

    public bool m_UseAccelerometer;

    public Vector3 m_PointerPosition;

    public Quaternion m_PointerRotation;

    public Vector3 m_WristPosition;

    public Quaternion m_WristRotation;

    public Vector3 m_ElbowPosition;

    public Quaternion m_ElbowRotation;

    public Vector3 m_ShoulderPosition;

    public Quaternion m_ShoulderRotation;

    public float m_AlphaValue;

    public enum Handedness
    {
        RIGHT_HANDED,
        LEFT_HANDED
    }

    public static HVRArmModel Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new HVRArmModel();
            }
            m_ElbowInitialPosition = m_DefaultControllerPosition;
            m_WristInitialPosition = new Vector3(0.0f, 0.0f, m_Radius);
            return instance;
        }
    }

    public void OnInit()
    {
        m_AlphaValue = 1.0f;
        m_LastControllerRotation = new Quaternion();
        m_AddedElbowHeight = 0.0f;
        m_AddedElbowDepth = 0.0f;
        m_PointerTiltAngle = 10.0f;
        m_FadeDistanceFromFace = 0.32f;
        m_MyHandedness = Handedness.RIGHT_HANDED;
        m_UseAccelerometer = false;
        m_WristPosition = m_DefaultControllerPosition + m_WristInitialPosition;

    }

    public void OnUpdateController()
    {
        if (HVRController.m_RightController == null || !HVRController.m_RightController.IsAvailable())
        {
            if (HVRController.m_LeftController != null && HVRController.m_LeftController.IsAvailable())
            {
                HVRController.m_LeftController.GetPosture(ref m_ControllePos);
            }
            else
            {
                return;
            }
        }
        else
        {
            HVRController.m_RightController.GetPosture(ref m_ControllePos);
        }
        SelectController();
        UpdateShoulderDirection();
        UpdateControllerPosition();
        m_LastControllerRotation = m_ControllePos.rotation;
    }

    private void SelectController()
    {
        m_HandedMultiplier.Set(0, 1, 1);
        if (Application.platform == RuntimePlatform.Android)
        {
            if (HVRControllerManager.m_IsLeftHandMode)
            {
                m_HandedMultiplier.x = -1.0f;
            }
            else
            {
                m_HandedMultiplier.x = 1.0f;
            }
        }
        else
        {
            if (m_MyHandedness == Handedness.RIGHT_HANDED)
            {
                m_HandedMultiplier.x = 1.0f;
            }
            else
            {
                m_HandedMultiplier.x = -1.0f;
            }
        }
        m_ShoulderRotation = Quaternion.identity;
        m_ShoulderPosition = Vector3.Scale(m_DefaultShoulderRight, this.m_HandedMultiplier);
    }

    private Vector3 GetHeadForwardOrientation()
    {
        return HVRLayoutCore.m_CamCtrObj.localRotation * Vector3.forward;
    }

    private void UpdateShoulderDirection()
    {
        Vector3 gazeDirection = GetHeadForwardOrientation();
        gazeDirection.y = 0.0f;
        gazeDirection.Normalize();
        Quaternion rotationQuat = Quaternion.Inverse(m_LastControllerRotation) * m_ControllePos.rotation;
        float rotationDegree;
        Vector3 rotationAxis = new Vector3();
        rotationQuat.ToAngleAxis(out rotationDegree, out rotationAxis);
        float angularVelocity = Mathf.Deg2Rad * rotationDegree / Time.deltaTime;
        float gazeFilterStrength = Mathf.Clamp((angularVelocity - 0.2f) / 45.0f, 0.0f, 0.1f);
        m_TorsoDirection = Vector3.Slerp(m_TorsoDirection, gazeDirection, gazeFilterStrength);
        Quaternion gazeRotation = Quaternion.FromToRotation(Vector3.forward, m_TorsoDirection);
        m_ShoulderRotation = gazeRotation;
        m_ShoulderPosition = gazeRotation * m_ShoulderPosition;
    }

    private void GetControllerRelativeOrientation(ref Quaternion controllerRotation, ref Quaternion xyplaneRotation, ref float xRotationAngle)
    {
        controllerRotation = m_ControllePos.rotation;
        controllerRotation = Quaternion.Inverse(m_ShoulderRotation) * controllerRotation;
        Vector3 controllerForward = controllerRotation * Vector3.forward;
        xRotationAngle = 90.0f - Vector3.Angle(controllerForward, Vector3.up);
        xyplaneRotation = Quaternion.FromToRotation(Vector3.forward, controllerForward);
    }

    private float GetExtentionRitio(float xRotationAngle)
    {
        float normalizedAngle = (xRotationAngle - m_MinExtensionAngle) / (m_MaxExtensionAngle - m_MinExtensionAngle);
        return Mathf.Clamp(normalizedAngle, 0.0f, 1.0f);
    }

    private Quaternion CalculateLerpRotation(Quaternion xyplaneRotation, float extensionRatio)
    {
        float totalAngle = Quaternion.Angle(xyplaneRotation, Quaternion.identity);
        float lerpSuppresion = 1.0f - Mathf.Pow(totalAngle / 180.0f, 6);
        float lerpValue = lerpSuppresion * (0.4f + (0.6f * extensionRatio * m_ExtensionWeight));
        return Quaternion.Lerp(Quaternion.identity, xyplaneRotation, lerpValue);
    }

    private void UpdateControllerPosition()
    {
        Quaternion controllerOrientation = new Quaternion();
        Quaternion xyplaneRotation = new Quaternion();
        float xRotationAngle = 0.0f;
        GetControllerRelativeOrientation(ref controllerOrientation, ref xyplaneRotation, ref xRotationAngle);

        float extensionRatio = GetExtentionRitio(xRotationAngle);

        m_ElbowPosition = m_ElbowInitialPosition + new Vector3(0.0f, m_AddedElbowHeight, m_AddedElbowDepth);
        m_ElbowPosition = Vector3.Scale(m_ElbowPosition, m_HandedMultiplier);
        m_WristPosition = Vector3.Scale(m_WristInitialPosition, m_HandedMultiplier);
        Vector3 armExtensionOffset = Vector3.Scale(m_ArmExtensionOffset, m_HandedMultiplier);
        if (!m_UseAccelerometer)
        {
            m_ElbowPosition += armExtensionOffset * extensionRatio;
        }
        Quaternion lerpRotation = CalculateLerpRotation(xyplaneRotation, extensionRatio);
        m_ElbowRotation = m_ShoulderRotation * Quaternion.Inverse(lerpRotation) * controllerOrientation;
        m_WristRotation = m_ShoulderRotation * controllerOrientation;

        m_ElbowPosition = m_ShoulderRotation * m_ElbowPosition;
        m_WristPosition = m_ElbowPosition + (m_ElbowRotation * m_WristPosition);
    }
}
