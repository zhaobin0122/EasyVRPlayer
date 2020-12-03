using HVRCORE;
using UnityEngine;

public class HVRBattery : MonoBehaviour
{
    private int m_LastBatteryLevel = -1;
    private IController m_Controller = null;
    public ControllerIndex controllerIndex = 0;

    void Update()
    {
        if (m_Controller == null)
        {
            if (controllerIndex == ControllerIndex.LEFT_CONTROLLER)
            {
                m_Controller = HVRController.m_LeftController;
            }
            else
            {
                m_Controller = HVRController.m_RightController;
            }
        }
        if (m_Controller == null || !m_Controller.IsAvailable()) {
            return;
        }
        int batteryValue = Mathf.Max(m_Controller.GetBatteryLevel() - 1, 0);
        int batteryLevel = batteryValue / 25;
       
        if(m_LastBatteryLevel != batteryLevel)
        {
            m_LastBatteryLevel = batteryLevel;
            BatteryLevel(batteryLevel);
        }
    }

    private void BatteryLevel(int batteryLevel)
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            if (i == batteryLevel)
            {
                transform.Find("level" + i).gameObject.SetActive(true);
            }
            else
            {
                transform.Find("level" + i).gameObject.SetActive(false);
            }
        }
    }
}
