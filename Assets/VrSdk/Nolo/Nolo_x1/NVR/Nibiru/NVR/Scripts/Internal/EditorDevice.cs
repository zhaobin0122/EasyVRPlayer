// Copyright 2016 Nibiru. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using UnityEngine;
using System.Collections.Generic;

/// @cond
namespace Nvr.Internal
{
    // Sends simulated values for use when testing within the Unity Editor.
    public class EditorDevice : BaseVRDevice
    {

        // Simulated neck model.  Vector from the neck pivot point to the point between the eyes.
        private static readonly Vector3 neckOffset = new Vector3(0, 0.075f, 0.08f);

        // Use mouse to emulate head in the editor.
        private float mouseX = 0;
        private float mouseY = 0;
        private float mouseZ = 0;

        public override void Init()
        {
            Input.gyro.enabled = true;
#if UNITY_STANDALONE_WIN
            NvrViewer.Instance.DistortionEnabled = false;
#endif
        }

        public override bool SupportsNativeDistortionCorrection(List<string> diagnostics)
        {
            return false;  // No need for diagnostic message.
        }

        // Since we can check all these settings by asking Nvr.Instance, no need
        // to keep a separate copy here.
        public override void SetVRModeEnabled(bool enabled) { }
 
        public override void UpdateState()
        {
            Quaternion rot;

            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                mouseX += Input.GetAxis("Mouse X") * 5;
                if (mouseX <= -180)
                {
                    mouseX += 360;
                }
                else if (mouseX > 180)
                {
                    mouseX -= 360;
                }
                mouseY -= Input.GetAxis("Mouse Y") * 2.4f;
                mouseY = Mathf.Clamp(mouseY, -85, 85);
            }
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                mouseZ += Input.GetAxis("Mouse X") * 5;
                mouseZ = Mathf.Clamp(mouseZ, -85, 85);
            }

            if (mouseX != 0 || mouseY != 0 || mouseZ != 0)
            {
                rot = Quaternion.Euler(mouseY, mouseX, mouseZ);

                var neck = (rot * neckOffset - neckOffset.y * Vector3.up);
                headPose.Set(neck, rot);
            }

#if UNITY_STANDALONE_WIN
            // ÊÖ±ú¼üÖµ×´Ì¬
            if (NvrInstantNativeApi.Inited)
            {
                NvrInstantNativeApi.Nibiru_Pose pose = NvrInstantNativeApi.GetPoseByDeviceType(NvrInstantNativeApi.NibiruDeviceType.Hmd);
                if(pose.rotation.w == 0)
                {
                    pose.rotation.w = 1;
                }
                this.headPose.Set(pose.position, new Quaternion(pose.rotation.x, pose.rotation.y, -pose.rotation.z, -pose.rotation.w));
            }
#endif
        }

        public override void UpdateScreenData()
        {
            Profile = NvrProfile.GetKnownProfile(NvrViewer.Instance.ScreenSize, NvrViewer.Instance.ViewerType);
            if (userIpd > 0)
            {
                Profile.viewer.lenses.separation = userIpd;
            }

            ComputeEyesFromProfile(1, 2000);

            profileChanged = true;
            Debug.Log("UpdateScreenData=" + Profile.viewer.lenses.separation);
        }

        public override void Recenter()
        {
            mouseX = mouseZ = 0;  // Do not reset pitch, which is how it works on the phone.
        }

        public override bool GazeApi(GazeTag tag, string param)
        {
            return true;
        }

        public override void SetCameraNearFar(float near, float far)
        {
            Debug.Log("EditorDevice.SetCameraNearFar : " + near + "," + far);
        }

        private float userIpd = -1;
        public override void SetIpd(float ipd)
        {
            userIpd = ipd;
        }

        public override void PostRender(RenderTexture stereoScreen)
        {
            // Do nothing.
        }
    }
}

#endif
