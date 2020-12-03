using System.Collections;
using UnityEngine;
using HVRCORE;

public class HVRTouchPad : MonoBehaviour
{

    private GameObject m_TouchPadPoint;
    private Vector3 m_PointLocalScale;
    private float m_TouchpadDiameter = 4.5f;
    private IController m_Controller = null;
    public ControllerIndex controllerIndex = 0;

    void Awake()
    {

        m_TouchPadPoint = transform.Find("TouchPoint").gameObject;
        m_PointLocalScale = m_TouchPadPoint.transform.localScale;
        Color color = new Color(0.05098f, 0.62353f, 0.98431f, 0.5f); //0D9FFB
        m_TouchPadPoint.GetComponent<MeshRenderer>().material.color = color;
    }


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
        UpdateTouchPoint();
    }
    private void UpdateTouchPoint()
    {
        if (m_Controller == null || !m_Controller.IsAvailable())
        {
            return;
        }
        if (m_Controller.IsTouchpadTouching())
        {
            m_TouchPadPoint.SetActive(true);

            Vector2 touchPos = new Vector2();
            m_Controller.GetTouchpadTouchPos(ref touchPos);

            float touch_x = 0.5f - Mathf.Clamp01(touchPos.x);
            float touch_y = 0.5f - Mathf.Clamp01(touchPos.y);
            float diameterx = 0.0248f;
            float diametery = 0.0252f;
            Vector3 offset = new Vector3(touch_x * diameterx, touch_y * diametery, 0.0f);

            offset.y += 0.0006f;
            m_TouchPadPoint.transform.localPosition = offset;

            if (m_Controller.IsButtonPressed(ButtonType.ButtonConfirm))
            {
                m_TouchPadPoint.transform.localPosition = new Vector3(0, 0, 0.0003f);

                UpdateTouchPointScale();
            }
            else
            {
                m_TouchPadPoint.transform.localScale = m_PointLocalScale;
            }
        }
        else
        {
            m_TouchPadPoint.SetActive(false);
        }
    }
    private void UpdateTouchPointScale()
    {
        m_TouchPadPoint.transform.localScale += new Vector3(0.5f, 0.5f, 0.5f);
        if (m_TouchPadPoint.transform.localScale.x > m_TouchpadDiameter)
        {
            m_TouchPadPoint.transform.localScale = new Vector3(m_TouchpadDiameter, m_TouchpadDiameter, m_TouchpadDiameter);
        }
    }
}

