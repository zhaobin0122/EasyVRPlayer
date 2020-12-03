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

using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
/// @cond
namespace Nvr.Internal
{
    public abstract class NvrDevice :
#if UNITY_ANDROID
  BaseAndroidDevice
#else
  BaseVRDevice
#endif
    {
         
        // A relatively unique id to use when calling our C++ native render plugin.
        private const int kRenderEvent = 0x47554342;

        // Event IDs sent up from native layer.  Bit flags.
        // Keep in sync with the corresponding declaration in unity.h.
        private const int kTilted = 1 << 1;
        private const int kProfileChanged = 1 << 2;
        private const int kVRBackButtonPressed = 1 << 3;

        private float[] position = new float[3];
        private float[] headData = new float[16];
        private float[] viewData = new float[16 * 6 + 12];
        private float[] profileData = new float[15];

        private Matrix4x4 headView = new Matrix4x4();
        private Matrix4x4 leftEyeView = new Matrix4x4();
        private Matrix4x4 rightEyeView = new Matrix4x4();

        private int _timewarp_view_number = 0; 

        public override NibiruService GetNibiruService()
        {
            return NvrGlobal.nibiruService;
        }

        public override void Init()
        { 
            // Start will send a log event, so SetUnityVersion first.
            byte[] version = System.Text.Encoding.UTF8.GetBytes(Application.unityVersion);
            if (NvrViewer.USE_DTR)
            {  
                if (!NvrGlobal.nvrStarted)
                {
                    if (nibiruVRServiceId == 0)
                    {
                        // 初始化1次service
                        nibiruVRServiceId = CreateNibiruVRService(); 
                    }
                    _NVR_InitAPIs(NvrGlobal.useNvrSo);
                    _NVR_SetUnityVersion(version, version.Length);
                    _NVR_Start(nibiruVRServiceId);
                    SetDisplayQuality((int) NvrViewer.Instance.TextureQuality);
                    //
                    if (NvrGlobal.soVersion >= 361)
                    {
                        ColorSpace colorSpace = QualitySettings.activeColorSpace;
                        if (colorSpace == ColorSpace.Gamma)
                        {
                            Debug.Log("Color Space - Gamma");
                            SetColorspaceType(0);
                        }
                        else if (colorSpace == ColorSpace.Linear)
                        {
                            Debug.Log("Color Space - Linear");
                            SetColorspaceType(1);
                        }
                    } else
                    {
                        Debug.LogError("System Api Not Support ColorSpace!!!");
                    }

                    if (NvrGlobal.soVersion >= 365)
                    {
                        Debug.Log("Controller Support Mode - " + NvrViewer.Instance.controllerSupportMode.ToString());
                        SetControllerSupportMode(NvrViewer.Instance.controllerSupportMode);
                    }
                    NvrGlobal.nvrStarted = true;
                    // 初始化服务
                    NibiruService nibiruService = new NibiruService();
                    nibiruService.Init();
                    NvrGlobal.nibiruService = nibiruService;
                } 
            } 
        Debug.Log("NvrDevice->Init");
        }

        public override int GetTimewarpViewNumber()
        {
            return _timewarp_view_number;
        }

        public override void UpdateState()
        {
            if (NvrViewer.USE_DTR)
            {
                _NVR_GetHeadPoseAndPosition(position, headData , ref _timewarp_view_number);
            }
            
            // 头部锁定
            if (NvrViewer.Instance.LockHeadTracker || (headData[0] == 0 && headData[15] == 0))
            {
                headData = new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            } 

            ExtractMatrix(ref headView, headData);
            headPose.SetRightHanded(headView);
            headPose.SetPosition(new Vector3(position[0], position[1], position[2]));
        }

        public override void UpdateScreenData()
        {
            bool useDFT = NvrViewer.USE_DTR && !NvrGlobal.supportDtr;

            // so获取
            UpdateProfile();
            UpdateView();
            // so获取

            if(useDFT)
            {
                float far = NvrGlobal.fovFar > -1 ? NvrGlobal.fovFar : (Camera.main != null ? Camera.main.farClipPlane : 20000.0f);
                ComputeEyesFromProfile(1, far);
            }
           
            profileChanged = true;
        }

        public override void Recenter()
        {
            if (NvrViewer.USE_DTR)
            {
                _NVR_ResetHeadPose();
            }
        }

        public override void PostRender(RenderTexture stereoScreen)
        {
           // do nothing
        }

        public override void EnterVRMode() {
            Debug.Log("NvrDevice->EnterVRMode");
            _NVR_ApplicationResume();
            // 更新参数信息
            UpdateScreenData();
        }

        public override void OnApplicationPause(bool pause)
        {
            Debug.Log("NvrDevice->OnApplicationPause");
            base.OnApplicationPause(pause);
            // 程序暂停
            if (pause)
            {
                Debug.Log("NvrDevice->OnPause");
                if (NvrViewer.USE_DTR)
                {
                    _NVR_ApplicationPause();
                }
            }
            else
            {
                Debug.Log("NvrDevice->OnResume");
                if (NvrViewer.USE_DTR)
                {
                    _NVR_ApplicationResume();
                }
            }
        }

        public override void Destroy()
        {
            Debug.Log("NvrDevice->Destroy");
            base.Destroy();
        }

        private bool applicationQuited = false;
        public override void OnApplicationQuit()
        {
            if (NvrViewer.USE_DTR && !applicationQuited)
            {  // 关闭陀螺仪
                Input.gyro.enabled = false; 
               _NVR_ApplicationDestory();
            } 
            applicationQuited = true;
            base.OnApplicationQuit();
            Debug.Log("NvrDevice->OnApplicationQuit.");
        }

        private void UpdateView()
        {
            if (NvrViewer.USE_DTR)
            {
                _NVR_GetViewParameters(viewData);
            }

            int j = 0; 

            j = ExtractMatrix(ref leftEyeView, viewData, j);
            j = ExtractMatrix(ref rightEyeView, viewData, j);
            if (NvrViewer.USE_DTR)
            {
                // 转置处理
                leftEyeView = leftEyeView.transpose;
                rightEyeView = rightEyeView.transpose;
            }
             
            j = ExtractMatrix(ref leftEyeDistortedProjection, viewData, j);
            j = ExtractMatrix(ref rightEyeDistortedProjection, viewData, j);
            j = ExtractMatrix(ref leftEyeUndistortedProjection, viewData, j);
            j = ExtractMatrix(ref rightEyeUndistortedProjection, viewData, j);
            if (NvrViewer.USE_DTR)
            {
                // 转置处理
                leftEyeDistortedProjection = leftEyeDistortedProjection.transpose;
                rightEyeDistortedProjection = rightEyeDistortedProjection.transpose;
                leftEyeUndistortedProjection = leftEyeUndistortedProjection.transpose;
                rightEyeUndistortedProjection = rightEyeUndistortedProjection.transpose;
            }

            leftEyeUndistortedViewport.Set(viewData[j], viewData[j + 1], viewData[j + 2], viewData[j + 3]);
            leftEyeDistortedViewport = leftEyeUndistortedViewport;
            j += 4;

            rightEyeUndistortedViewport.Set(viewData[j], viewData[j + 1], viewData[j + 2], viewData[j + 3]);
            rightEyeDistortedViewport = rightEyeUndistortedViewport;
            j += 4;
            //  屏幕大小，纹理生成大小 1920*1080
            int screenWidth = (int)viewData[j];
            int screenHeight = (int)viewData[j + 1];
            j += 2;

            recommendedTextureSize = new Vector2(viewData[j], viewData[j + 1]);
            j += 2;

            if (NvrViewer.USE_DTR && !NvrGlobal.supportDtr) {
                // DFT 
                recommendedTextureSize = new Vector2(screenWidth, screenHeight);
                // Debug.Log("DFT texture size : " +screenWidth + "," + screenHeight);
            }
        }

        private void UpdateProfile()
        {
            if (NvrViewer.USE_DTR)
            {
                _NVR_GetNVRConfig(profileData);
            }

            if (profileData[13] > 0)
            {
                NvrGlobal.fovNear = profileData[13];
            }

            if (profileData[14] > 0)
            {
                NvrGlobal.fovFar = profileData[14] > NvrGlobal.fovFar ? profileData[14] : NvrGlobal.fovFar;
            }


            if (NvrViewer.USE_DTR && !NvrGlobal.supportDtr && NvrGlobal.dftProfileParams[0] != 0)
            {
                // DFT模式加载cardboard参数
                // fov
                profileData[0] = NvrGlobal.dftProfileParams[3];//45;
                profileData[1] = NvrGlobal.dftProfileParams[4]; //45;
                profileData[2] = NvrGlobal.dftProfileParams[5]; //51.5f;
                profileData[3] = NvrGlobal.dftProfileParams[6]; //51.5f;
                // screen size 
                profileData[4] = NvrGlobal.dftProfileParams[12]; //0.110f;
                profileData[5] = NvrGlobal.dftProfileParams[13]; //0.062f;
                // ipd
                profileData[7] = NvrGlobal.dftProfileParams[0]; //0.063f;
                // screen to lens
                profileData[9] = NvrGlobal.dftProfileParams[2]; //0.035f;
                // k1 k2
                profileData[11] = NvrGlobal.dftProfileParams[7]; //0.252f;
                profileData[12] = NvrGlobal.dftProfileParams[8]; //0.019f;
            }

            NvrProfile.Viewer device = new NvrProfile.Viewer();
            NvrProfile.Screen screen = new NvrProfile.Screen();
            // left top right bottom
            device.maxFOV.outer = profileData[0];
            device.maxFOV.upper = profileData[2];
            device.maxFOV.inner = profileData[1];
            device.maxFOV.lower = profileData[3];
            screen.width = profileData[4];
            screen.height = profileData[5];
            screen.border = profileData[6];
            device.lenses.separation = profileData[7];
            device.lenses.offset = profileData[8];
            device.lenses.screenDistance = profileData[9];
            device.lenses.alignment = (int)profileData[10];
            device.distortion.Coef = new[] { profileData[11], profileData[12] };
            Profile.screen = screen;
            Profile.viewer = device;

            float[] rect = new float[4];
            Profile.GetLeftEyeNoLensTanAngles(rect);
            float maxRadius = NvrProfile.GetMaxRadius(rect);
            Profile.viewer.inverse = NvrProfile.ApproximateInverse(
            Profile.viewer.distortion, maxRadius);
        }

        private static int ExtractMatrix(ref Matrix4x4 mat, float[] data, int i = 0)
        {
            // 列优先
            // Matrices returned from our native layer are in row-major order.
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++, i++)
                {
                    mat[r, c] = data[i];
                }
            }
            return i;
        }
         
        public override IntPtr NGetRenderEventFunc() {
            return _NVR_GetRenderEventFunc();
        }

        public override void NSetSystemVRMode(int flag) {
            _NVR_SetSystemVRMode(flag);
        }

        public override void NLockTracker()
        {
            _NVR_LockHeadPose();
        }

        public override void NUnLockTracker()
        {
            _NVR_UnLockHeadPose();
        }

        public override void SetTextureSizeNative(int w, int h)
        {
            // 1920*1080 = 1920*10000 + 1080 = 19201080
            _NVR_SetParamI(1002, w * 10000 + h);
        }

        public override void SetCpuLevel(NvrOverrideSettings.PerfLevel level)
        {
            _NVR_SetParamI(1003, (int) level);
        }

        public override void SetGpuLevel(NvrOverrideSettings.PerfLevel level)
        {
            _NVR_SetParamI(1004, (int)level);
        }

        public override void NIssuePluginEvent(int eventID)
        {
            // Queue a specific callback to be called on the render thread
            GL.IssuePluginEvent(NvrViewer.Instance.GetDevice().NGetRenderEventFunc(), eventID);
        }
		
		public override void SetColorspaceType(int colorSpace)
        {
            _NVR_SetParamI((int)PARAMS_KEY.COLOR_SPACE, colorSpace);
        }

        public override void SetControllerSupportMode(ControllerSupportMode csm)
        {
            _NVR_SetParamI((int)PARAMS_KEY.CONTROLLER_SUPPORT, (int) csm);
        }

        public enum PARAMS_KEY
        {
            CONTROLLER_SUPPORT=1006,
            COLOR_SPACE =1007
        }

        //  调用跳转so
        private const string nvrDllName = "nvr_unity";

        [DllImport(nvrDllName)]
        private static extern int _NVR_InitAPIs(bool supportDTR);

        [DllImport(nvrDllName)]
        private static extern bool _NVR_Start(long pointer);

        [DllImport(nvrDllName)]
        private static extern void _NVR_SetUnityVersion(byte[] version_str, int version_length);

        [DllImport(nvrDllName)]
        private static extern int _NVR_GetEventFlags();

        [DllImport(nvrDllName)]
        private static extern void _NVR_GetNVRConfig(float[] profile);

        [DllImport(nvrDllName)]
        private static extern void _NVR_GetHeadPose(float[] pose, ref int viewNumber);

        [DllImport(nvrDllName)]
        private static extern void _NVR_GetHeadPoseAndPosition(float[] position, float[] pose, ref int viewNumber);

        [DllImport(nvrDllName)]
        private static extern void _NVR_ResetHeadPose();

        [DllImport(nvrDllName)]
        private static extern void _NVR_GetViewParameters(float[] viewParams);

        [DllImport(nvrDllName)]
        private static extern void _NVR_ApplicationPause();

        [DllImport(nvrDllName)]
        private static extern void _NVR_ApplicationResume();

        [DllImport(nvrDllName)]
        private static extern void _NVR_ApplicationDestory();

        [DllImport(nvrDllName)]
        private static extern IntPtr _NVR_GetRenderEventFunc();

        [DllImport(nvrDllName)]
        private static extern void _NVR_LockHeadPose();

        [DllImport(nvrDllName)]
        private static extern void _NVR_UnLockHeadPose();

        [DllImport(nvrDllName)]
        private static extern void _NVR_SetSystemVRMode(int flag);

        [DllImport(nvrDllName)]
        private static extern void _NVR_SetParamI(int key, int value);

        [DllImport(nvrDllName)]
        private static extern int _NVR_GetParamI(int key);
    }
}
/// @endcond
