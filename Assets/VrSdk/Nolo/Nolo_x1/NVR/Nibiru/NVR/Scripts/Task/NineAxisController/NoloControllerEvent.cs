using NibiruTask;
using UnityEngine;
using XR;

namespace NibiruAxis
{
    public class NoloControllerEvent : MonoBehaviour
    {
        public TextMesh textMesh;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (ControllerAndroid.isDeviceConn((int)CDevice.NOLO_TYPE.LEFT))
            {
                int[] leftKeyState = ControllerAndroid.getKeyState((int)CDevice.NOLO_TYPE.LEFT, 1);
                processControllerKeyEvent(CDevice.NOLO_TYPE.LEFT, leftKeyState);
            }

            if (ControllerAndroid.isDeviceConn((int)CDevice.NOLO_TYPE.RIGHT))
            {
                int[] rightKeyState = ControllerAndroid.getKeyState((int)CDevice.NOLO_TYPE.RIGHT, 1);
                processControllerKeyEvent(CDevice.NOLO_TYPE.RIGHT, rightKeyState);
            }

        }

        private int[] lastState = new int[256];
        private int[] curState = new int[256];
        private void processControllerKeyEvent(CDevice.NOLO_TYPE type, int[] state)
        {
            lastState = curState;
            curState = state;
            int btnNibiru = curState[CKeyEvent.KEYCODE_BUTTON_NIBIRU];
            int btnStart = curState[CKeyEvent.KEYCODE_BUTTON_START];
            // Side A/B
            int btnSelect = curState[CKeyEvent.KEYCODE_BUTTON_SELECT];
            // Menu
            int btnApp = curState[CKeyEvent.KEYCODE_BUTTON_APP];
            // TouchPad
            int btnCenter = curState[CKeyEvent.KEYCODE_DPAD_CENTER];
            // Trigger
            int btnR1 = curState[CKeyEvent.KEYCODE_BUTTON_R1];

            // Nolo TouchPad = Center
            // Nolo Menu = App
            // Nolo Trigger = R1
            // Nolo Side = Select
            if (btnCenter == 0)
            {
                if (textMesh) textMesh.text = "Nolo TouchPad Down";
            }
            else if (lastState[CKeyEvent.KEYCODE_DPAD_CENTER] == 0)
            {
                if (textMesh) textMesh.text = "Nolo TouchPad Up";
            }

            if (btnApp == 0)
            {
                if (textMesh) textMesh.text = "Nolo Menu Down";
            }
            else if (lastState[CKeyEvent.KEYCODE_BUTTON_APP] == 0)
            {
                if (textMesh) textMesh.text = "Nolo Menu Up";
            }

            if (btnR1 == 0)
            {
                if (textMesh) textMesh.text = "Nolo Trigger Down";
            }
            else if (lastState[CKeyEvent.KEYCODE_BUTTON_R1] == 0)
            {
                if (textMesh) textMesh.text = "Nolo Trigger Up";
            }

            if (btnSelect == 0)
            {
                if (textMesh) textMesh.text = "Nolo Side Down";
            }
            else if (lastState[CKeyEvent.KEYCODE_BUTTON_SELECT] == 0)
            {
                if (textMesh) textMesh.text = "Nolo Side Up";
            }

            if (btnSelect == 0 || btnStart == 0 || btnNibiru == 0 || btnApp == 0
                || btnCenter == 0 || btnR1 == 0)
            {
                Debug.LogError("->Start=" + btnStart +
               "->Nibiru=" + btnNibiru +
               "->Select=" + btnSelect +
               "->App=" + btnApp +
               "->Center=" + btnCenter +
                "->R1=" + btnR1
               );
            }


        }

    }
}