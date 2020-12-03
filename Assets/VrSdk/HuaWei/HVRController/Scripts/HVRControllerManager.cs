using UnityEngine;
using HVRCORE;

public class HVRControllerManager : MonoBehaviour
{

    public static bool m_IsLeftHandMode = false;
    private static readonly string TAG = "Unity_HVRControllerManager";
    void Awake()
    {
        HVRLogCore.GetAndroidLogClass();
        foreach (Transform child in transform)
        {
            if (child.name == "HVRRightController" || child.name == "HVRLeftController")
            {
                child.gameObject.AddComponent<HVRController>();
            }
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                m_IsLeftHandMode = HvrApi.GetControllerHandle().IsLeftHandMode();
                HVRLogCore.LOGI(TAG, "OnApplicationPause m_IsLeftHandMode: " + m_IsLeftHandMode);
            }
        }
    }

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            m_IsLeftHandMode = HvrApi.GetControllerHandle().IsLeftHandMode();
            HVRLogCore.LOGI(TAG, "Start m_IsLeftHandMode: " + m_IsLeftHandMode);
        }
    }

}
