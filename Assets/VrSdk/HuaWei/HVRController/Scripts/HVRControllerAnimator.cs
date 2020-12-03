using HVRCORE;
using UnityEngine;

public class HVRControllerAnimator : MonoBehaviour
{

    private Animator m_Animation, mAnim_back, mAnim_home, mAnim_trigger, mAnim_confirm, mAnim_volume;

    private IController m_Controller = null;
    public ControllerIndex controllerIndex = 0;

    void Start()
    {
        m_Animation = GetComponent<Animator>();
        mAnim_back = transform.Find("b_l").GetComponent<Animator>();
        mAnim_home = transform.Find("b_r").GetComponent<Animator>();
        mAnim_trigger = transform.Find("c_t").GetComponent<Animator>();
        mAnim_confirm = transform.Find("c_f").GetComponent<Animator>();
        mAnim_volume = transform.Find("v_m").GetComponent<Animator>();
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
        AnimationProcess();

    }
    private void AnimationProcess()
    {
        if (m_Controller == null || !m_Controller.IsAvailable())
        {
            return;
        }
        if (m_Controller.IsButtonPressed(ButtonType.ButtonTrigger))
        {
            mAnim_trigger.SetBool("isPressed", true);
        }
        else if (m_Controller.IsButtonUp(ButtonType.ButtonTrigger))
        {
            mAnim_trigger.SetBool("isPressed", false);
        }
        if (m_Controller.IsButtonPressed(ButtonType.ButtonBack))
        {
            mAnim_back.SetBool("isPressed", true);
        }
        else if (m_Controller.IsButtonUp(ButtonType.ButtonBack))
        {
            mAnim_back.SetBool("isPressed", false);
        }
        if (m_Controller.IsButtonPressed(ButtonType.ButtonHome))
        {
            mAnim_home.SetBool("isPressed", true);
        }
        else if (m_Controller.IsButtonUp(ButtonType.ButtonHome))
        {
            mAnim_home.SetBool("isPressed", false);
        }
        if (m_Controller.IsButtonPressed(ButtonType.ButtonConfirm))
        {
            mAnim_confirm.SetBool("isPressed", true);
        }
        else if (m_Controller.IsButtonUp(ButtonType.ButtonConfirm))
        {
            mAnim_confirm.SetBool("isPressed", false);
        }
        if (m_Controller.IsButtonPressed(ButtonType.ButtonVolumeDec))
        {
            mAnim_volume.SetBool("isVMDPressed", true);
        }
        else if (m_Controller.IsButtonUp(ButtonType.ButtonVolumeDec))
        {
            mAnim_volume.SetBool("isVMDPressed", false);
        }
        if (m_Controller.IsButtonPressed(ButtonType.ButtonVolumeInc))
        {
            mAnim_volume.SetBool("isVMIPressed", true);
        }
        else if (m_Controller.IsButtonUp(ButtonType.ButtonVolumeInc))
        {
            mAnim_volume.SetBool("isVMIPressed", false);
        }
    }
}
