using UnityEngine;
using UnityEngine.UI;
using HVRCORE;

public class HVRHelpMessage : MonoBehaviour
{
    private static readonly string TAG = "Unity_HVRHelpMessage";
    private Transform m_helpMessage, m_trigger, m_volume, m_back, m_home, m_confirm;
    private Text m_trigger_text, m_volume_text, m_back_short_text, m_back_long_text, m_home_short_text, m_home_long_text, m_confirm_text;

    private CanvasGroup m_CanvasGroup;

    private float m_DefaultAlpha = 0;
    //private float currentAlpha;

    private float m_angle;
    private float m_angleFront_x_min = 280;
    private float m_angleFront_x_max = 320;
    private float m_angleFront_z_min = -30;
    private float m_angleFront_z_max = 70;

    public static string m_trigger_msg = null;
    public static string m_volume_msg = null;
    public static string m_back_short_msg = null;
    public static string m_back_long_msg = null;
    public static string m_home_short_msg = null;
    public static string m_home_long_msg = null;
    public static string m_confirm_msg = null;

    public static int m_FontSize = 18;
    public static Color m_ImageColor = new Color(0.1f, 0.1f, 0.1f, 1);
    public static Color m_ArrowColor = new Color(0.1f, 0.1f, 0.1f, 1);
    public static Color m_TextColor = new Color(0.9f, 0.9f, 0.9f, 1);

    private string m_TriggerMsg = "确认（短按）";
    private string m_VolumeMsg = "短按：音量调节";
    private string m_BasckShortMsg = "返回上一级（短按）";
    private string m_BackLongMsg = "返回主页（长按）";
    private string m_HomeShortMsg = "返回主页（短按）";
    private string m_HomeLongMsg = "视角及射线置中（长按）";
    private string m_ConfirmMsg = "确认（短按）";

    private string m_PhoneVolumeMsg = null;
    private string m_PhoneBasckShortMsg = null;
    private string m_PhoneBackLongMsg = null;
    private string m_PhoneHomeShortMsg = null;
    private string m_PhoneHomeLongMsg = null;
    private string m_PhoneConfirmMsg = null;

    private static bool isConfirmTrue = false;
    private static bool isVolumeTrue = false;

    private Transform m_phoneHelpMessage;
    private Transform m_controllerHelpMessage;

    private bool m_IsPhoneControllerMode = false;
    private bool m_IsRightController = true;

    public static void IsShowConfirmMsg(bool isShow)
    {
        isConfirmTrue = isShow;
    }
    public static void IsShowVolumeMsg(bool isShow)
    {
        isVolumeTrue = isShow;
    }
    private void Awake()
    {
        m_controllerHelpMessage = transform.Find("ControllerHelpMessage");
        m_phoneHelpMessage = transform.Find("PhoneHelpMessage");
        m_helpMessage = m_controllerHelpMessage;
    }
    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (HVRPluginCore.IsPhoneController())
            {
                HVRLogCore.LOGI(TAG, "show phoneController help msg");
                m_IsPhoneControllerMode = true;
                m_phoneHelpMessage.gameObject.SetActive(true);
                m_controllerHelpMessage.gameObject.SetActive(false);
                m_helpMessage = m_phoneHelpMessage;
                InitPhoneControllerMsg();
            }
            else
            {
                HVRLogCore.LOGI(TAG, "show controller help msg");
                m_IsPhoneControllerMode = false;
                m_controllerHelpMessage.gameObject.SetActive(true);
                m_phoneHelpMessage.gameObject.SetActive(false);
                m_helpMessage = m_controllerHelpMessage;
                InitDefaultControllerMsg();
            }
        }
        else
        {
            InitDefaultControllerMsg();
        }
        if (transform.parent.name == "HVRLeftController")
        {
            m_IsRightController = false;
        }
        else
        {
            m_IsRightController = true;
        }
    }

    private void FindObj()
    {
        m_CanvasGroup = m_helpMessage.GetComponent<CanvasGroup>();
        m_CanvasGroup.alpha = m_DefaultAlpha;

        m_volume = m_helpMessage.transform.Find("Volume");
        m_back = m_helpMessage.transform.Find("Back");
        m_home = m_helpMessage.transform.Find("Home");
        m_confirm = m_helpMessage.transform.Find("Confirm");

        m_volume_text = m_volume.Find("Text").GetComponent<Text>();
        m_back_short_text = m_back.Find("short/Text").GetComponent<Text>();
        m_back_long_text = m_back.Find("long/Text").GetComponent<Text>();
        m_home_short_text = m_home.Find("short/Text").GetComponent<Text>();
        m_home_long_text = m_home.Find("long/Text").GetComponent<Text>();
        m_confirm_text = m_confirm.Find("Text").GetComponent<Text>();
    }

    private void InitPhoneControllerMsg()
    {

        FindObj();

        m_PhoneVolumeMsg = HVRPluginCore.GetDialogueContent("Hvr_phone_volume_short");
        if (m_PhoneVolumeMsg == null) {
            HVRLogCore.LOGI(TAG, "mPhoneVolumeMsg is null");
            m_phoneHelpMessage.gameObject.SetActive(false);
            return;
        }
        m_PhoneBasckShortMsg = HVRPluginCore.GetDialogueContent("Hvr_phone_back_short");
        m_PhoneBackLongMsg = HVRPluginCore.GetDialogueContent("Hvr_phone_back_long");
        m_PhoneHomeShortMsg = HVRPluginCore.GetDialogueContent("Hvr_phone_home_short");
        m_PhoneHomeLongMsg = HVRPluginCore.GetDialogueContent("Hvr_phone_home_long");
        m_PhoneConfirmMsg = HVRPluginCore.GetDialogueContent("Hvr_phone_confirm");

        m_VolumeMsg = m_PhoneVolumeMsg;
        m_BasckShortMsg = m_PhoneBasckShortMsg;
        m_BackLongMsg = m_PhoneBackLongMsg;
        m_HomeShortMsg = m_PhoneHomeShortMsg;
        m_HomeLongMsg = m_PhoneHomeLongMsg;
        m_ConfirmMsg = m_PhoneConfirmMsg;

    }

    private void InitDefaultControllerMsg()
    {
        FindObj();
        m_trigger = m_helpMessage.transform.Find("Trigger");
        m_trigger_text = m_trigger.Find("Text").GetComponent<Text>();

        m_TriggerMsg = HVRPluginCore.GetDialogueContent("Hvr_trigger");
        m_VolumeMsg = HVRPluginCore.GetDialogueContent("Hvr_volume_short");
        m_BasckShortMsg = HVRPluginCore.GetDialogueContent("Hvr_back_short");
        m_BackLongMsg = HVRPluginCore.GetDialogueContent("Hvr_back_long");
        m_HomeShortMsg = HVRPluginCore.GetDialogueContent("Hvr_home_short");
        m_HomeLongMsg = HVRPluginCore.GetDialogueContent("Hvr_home_long");
        m_ConfirmMsg = HVRPluginCore.GetDialogueContent("Hvr_trigger");
    }

    void Update()
    {
        if (HVRControllerManager.m_IsLeftHandMode)
        {
            m_angleFront_z_min = -70;
            m_angleFront_z_max = 30;
        }
        else
        {
            m_angleFront_z_min = -30;
            m_angleFront_z_max = 70;
        }
        if (m_IsPhoneControllerMode)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (HVRPluginCore.IsSwitchToController())
                {
                    HVRLogCore.LOGI(TAG, "switch to show controller help msg");
                    m_IsPhoneControllerMode = false;
                    m_controllerHelpMessage.gameObject.SetActive(true);
                    m_phoneHelpMessage.gameObject.SetActive(false);
                    m_helpMessage = m_controllerHelpMessage;
                    InitDefaultControllerMsg();
                }
            }
        }
        else
        {
            m_confirm.gameObject.SetActive(isConfirmTrue);
            m_volume.gameObject.SetActive(isVolumeTrue);
        }
        DefaultOrCustomize();

        float currentAlpha = m_CanvasGroup.alpha;
        Vector3 eulerAngles = Vector3.zero;
        if (!m_IsRightController)
        {
            eulerAngles = HVRController.m_LeftContrllerRotation.eulerAngles;
        }
        else
        {
            eulerAngles = HVRController.m_RightContrllerRotation.eulerAngles;
        }
        if (eulerAngles.x < m_angleFront_x_max &&
            eulerAngles.x > m_angleFront_x_min &&
            CheckAngle(eulerAngles.z) < m_angleFront_z_max &&
            CheckAngle(eulerAngles.z) > m_angleFront_z_min)
        {
            ShowAlphaInc(currentAlpha);
        }
        else
        {
            ShowAlphaDec(currentAlpha);
        }

        if (m_CanvasGroup.alpha == 1)
        {
            string name = "CtrlHelp";
            int len = name.Length;
            HVRPluginCore.HVR_BDReport(name, len);
        }
    }

    private void ShowAlphaInc(float currentAlpha)
    {
        currentAlpha += Time.deltaTime * 0.5f;
        if (currentAlpha >= 1.0f)
        {
            currentAlpha = 1.0f;
        }
        m_CanvasGroup.alpha = currentAlpha;
    }

    private void ShowAlphaDec(float currentAlpha)
    {
        currentAlpha -= Time.deltaTime;
        if (currentAlpha <= 0.0f)
        {
            currentAlpha = 0.0f;
        }
        m_CanvasGroup.alpha = currentAlpha;
    }

    private float LayerGradation(float angle_inter, float angle_min, float angle_max, float angle_charge)
    {
        if (angle_inter <= (angle_min + angle_max) * 0.5f)
        {
            m_angle = angle_inter - angle_min;
        }
        if (angle_inter > (angle_min + angle_max) * 0.5f)
        {
            m_angle = angle_max - angle_inter;
        }
        return m_angle / angle_charge;
    }

    public float CheckAngle(float value)
    {
        float angle = value - 180;
        if (angle > 0)
        {
            return angle - 180;
        }
        return angle + 180;
    }

    private void DefaultOrCustomize()
    {
        if (!m_IsPhoneControllerMode)
        {
            if (!string.IsNullOrEmpty(m_trigger_msg))
            {
                m_trigger_text.text = m_trigger_msg;
            }
            else
            {
                m_trigger_text.text = m_TriggerMsg;
            }
        }

        if (!string.IsNullOrEmpty(m_volume_msg))
        {
            m_volume_text.text = m_volume_msg;
        }
        else
        {
            m_volume_text.text = m_VolumeMsg;
        }
        if (!string.IsNullOrEmpty(m_back_short_msg))
        {
            m_back_short_text.text = m_back_short_msg;
        }
        else
        {
            m_back_short_text.text = m_BasckShortMsg;
        }
        if (!string.IsNullOrEmpty(m_back_long_msg))
        {
            m_back_long_text.text = m_back_long_msg;
        }
        else
        {
            m_back_long_text.text = m_BackLongMsg;
        }
        if (!string.IsNullOrEmpty(m_home_short_msg))
        {
            m_home_short_text.text = m_home_short_msg;
        }
        else
        {
            m_home_short_text.text = m_HomeShortMsg;
        }
        if (!string.IsNullOrEmpty(m_home_long_msg))
        {
            m_home_long_text.text = m_home_long_msg;
        }
        else
        {
            m_home_long_text.text = m_HomeLongMsg;
        }
        if (!string.IsNullOrEmpty(m_confirm_msg))
        {
            m_confirm_text.text = m_confirm_msg;
        }
        else
        {
            m_confirm_text.text = m_ConfirmMsg;
        }
    }
}
