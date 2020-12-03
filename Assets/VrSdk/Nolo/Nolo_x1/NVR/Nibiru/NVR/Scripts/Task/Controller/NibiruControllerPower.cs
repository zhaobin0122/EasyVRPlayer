using UnityEngine;
namespace NibiruTask
{
    public class NibiruControllerPower : MonoBehaviour
    {

        [SerializeField]
        private Material power1;
        [SerializeField]
        private Material power1Red;
        [SerializeField]
        private Material power2;
        [SerializeField]
        private Material power3;
        [SerializeField]
        private Material power4;
        [SerializeField]
        private Material power5;

        private MeshRenderer powerRenderMat;
        private int powerValue;
        public CDevice.NOLO_TYPE noloType = CDevice.NOLO_TYPE.NONE;
        private Transform m_transform;
        void Start()
        {
            m_transform = transform;
            powerRenderMat = GetComponent<MeshRenderer>();
            powerRenderMat.enabled = false;
            powerValue = 0;
            if(power1 == null)
            {
                power1 = Resources.Load<Material>("Controller/power/power1");
            }

            if (power1Red == null)
            {
                power1Red = Resources.Load<Material>("Controller/power/power1Red");
            }

            if (power2 == null)
            {
                power2 = Resources.Load<Material>("Controller/power/power2");
            }

            if (power3 == null)
            {
                power3= Resources.Load<Material>("Controller/power/power3");
            }

            if (power4 == null)
            {
                power4 = Resources.Load<Material>("Controller/power/power4");
            }

            if (power5 == null)
            {
                power5 = Resources.Load<Material>("Controller/power/power5");
            }
            powerRenderMat.material = power1Red;
        }
        // Update is called once per frame
        void Update()
        {
            RefreshPower();
        }

        private void RefreshPower()
        {
            float eulerX = m_transform.parent.eulerAngles.x;
            bool showBattery = (eulerX < 180 && eulerX >= 20) || (eulerX>180 && eulerX <= 340);
            if (!showBattery && powerRenderMat.enabled)
            {
                powerRenderMat.enabled = false;
                return;
            }

            powerRenderMat.enabled = showBattery;

            int getControllerPower = 0;
            if (InteractionManager.IsControllerConnected())
            {
                getControllerPower = InteractionManager.GetControllerPower(noloType==CDevice.NOLO_TYPE.LEFT ? InteractionManager.NACTION_HAND_TYPE.HAND_LEFT :
                    InteractionManager.NACTION_HAND_TYPE.HAND_RIGHT);
            }
            else
            {
                if (ControllerAndroid.isDeviceConn((int)noloType))
                {
                    getControllerPower = ControllerAndroid.getNoloControllerPower(noloType);
                }
                else if (ControllerAndroid.isQuatConn())
                {
                    getControllerPower = ControllerAndroid.getControllerPower();
                }
            }

            if (powerRenderMat.enabled && powerValue != getControllerPower)
            {
                if(getControllerPower <= 10)
                {
                    powerRenderMat.material = power1Red;
                } else if(getControllerPower < 20)
                {
                    powerRenderMat.material = power1;
                }
                else if (getControllerPower < 40)
                {
                    powerRenderMat.material = power2;
                }
                else if (getControllerPower < 60)
                {
                    powerRenderMat.material = power3;
                }
                else if (getControllerPower < 80)
                {
                    powerRenderMat.material = power4;
                }
                else
                {
                    powerRenderMat.material = power5;
                } 
               
                powerValue = getControllerPower;
            }
        }
 
    }
}