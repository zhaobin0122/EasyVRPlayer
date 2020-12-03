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
using UnityEngine.UI;
using System.Threading;
using Nvr.Internal;
/// Controls one camera of a stereo pair.  Each frame, it mirrors the settings of
/// the parent mono Camera, and then sets up side-by-side stereo with
/// the view and projection matrices from the NvrViewer.EyeView and NvrViewer.Projection.
/// The render output is directed to the NvrViewer.StereoScreen render texture, either
/// to the left half or right half depending on the chosen eye.
///
/// To enable a stereo camera pair, enable the parent mono camera and set
/// NvrViewer.vrModeEnabled = true.
///
/// @note If you programmatically change the set of NvrEyes belonging to a
/// StereoController, be sure to call StereoController::InvalidateEyes on it
/// in order to reset its cache.
/// 
namespace Nvr.Internal
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("NVR/Internal/NvrEye")]
    public class NvrEye : MonoBehaviour
    {
        public delegate void OnPostRenderCallback(int cacheTextureId, NvrViewer.Eye eyeType);
        public OnPostRenderCallback OnPostRenderListener;

        public delegate void OnPreRenderCallback(int cacheTextureId, NvrViewer.Eye eyeType);
        public OnPreRenderCallback OnPreRenderListener;


        /// Whether this is the left eye or the right eye.
        /// Determines which stereo eye to render, that is, which `EyeOffset` and
        /// `Projection` matrix to use and which half of the screen to render to.
        public NvrViewer.Eye eye;

        /// The StereoController in charge of this eye (and whose mono camera
        /// we will copy settings from).
        public NvrStereoController Controller
        {
            // This property is set up to work both in editor and in player.
            get
            {
                if (transform.parent == null)
                { // Should not happen.
                    return null;
                }
                if ((Application.isEditor && !Application.isPlaying) || controller == null)
                {
                    // Go find our controller.
                    controller = transform.parent.GetComponentInParent<NvrStereoController>();
                    if (controller == null)
                    {
                        controller = FindObjectOfType<NvrStereoController>();
                    }
                }
                return controller;
            }
            set
            {
                controller = value;
            }
        }

        public NvrStereoController controller;
        private StereoRenderEffect stereoEffect;
        private Camera monoCamera;
  
        // Convenient accessor to the camera component used throughout this script.
        public Camera cam { get; private set; }

        public Transform cacheTransform;
        void Awake()
        {
            cam = GetComponent<Camera>();
        }

        void Start()
        {
            var ctlr = Controller;
            if (ctlr == null)
            {
                Debug.LogError("NvrEye must be child of a StereoController.");
                enabled = false;
                return;
            }
            // Save reference to the found controller and it's camera.
            controller = ctlr;
            monoCamera = controller.GetComponent<Camera>();
            cacheTransform = transform;
        }

        public void UpdateCameraProjection()
        {
            Matrix4x4 proj = NvrViewer.Instance.Projection(eye);

            Debug.Log("NvrEye->UpdateCameraProjection,"+eye.ToString() + "/" + proj.ToString());

            bool useDFT = NvrViewer.USE_DTR && !NvrGlobal.supportDtr;
            //DTR不需要修正
            if (Application.isEditor || useDFT)
            {
                if (monoCamera == null && controller != null) monoCamera = controller.GetComponent<Camera>();
                // Fix aspect ratio and near/far clipping planes.
                float nearClipPlane = monoCamera.nearClipPlane;
                float farClipPlane = monoCamera.farClipPlane;
                float near = (NvrGlobal.fovNear >= 0 && NvrGlobal.fovNear < nearClipPlane) ? NvrGlobal.fovNear : nearClipPlane;
                float far = (NvrGlobal.fovFar >= 0 && NvrGlobal.fovFar > farClipPlane) ? NvrGlobal.fovFar : farClipPlane;
                // DFT & 编辑器模式修正投影矩阵      
                NvrCameraUtils.FixProjection(cam.rect, near, far, ref proj);
                Debug.Log("FixProjection." + eye.ToString() + ", " + cam.rect.ToString());
            } 

            // Set the eye camera's projection for rendering.
            cam.projectionMatrix = proj;
            NvrViewer.Instance.UpdateEyeCameraProjection(eye);

            float ipd = NvrViewer.Instance.GetDevice().Profile.viewer.lenses.separation;
            Vector3 localPosition = (eye == NvrViewer.Eye.Left ? -ipd / 2 : ipd / 2) * Vector3.right;
            if (localPosition.x  != transform.localPosition.x) {
                transform.localPosition = localPosition;
            }
        }

        private int cacheTextureId = -1;

        public int GetTargetTextureId()
        {
            return cacheTextureId;
        }

        public void UpdateTargetTexture()
        {
            // 从so获取纹理idx
            int eyeType = eye == NvrViewer.Eye.Left ? 0 : 1;
            cacheTextureId = NvrViewer.Instance.GetEyeTextureId(eyeType);
            cam.targetTexture = NvrViewer.Instance.GetStereoScreen(eyeType);
            cam.targetTexture.DiscardContents();
        }

        void OnPreRender()
        {
            if (cacheTextureId == -1 && cam.targetTexture != null)
            {
                cacheTextureId = (int)cam.targetTexture.GetNativeTexturePtr();
            }
            if(OnPreRenderListener != null) OnPreRenderListener(cacheTextureId, eye);
        }

        int frameId = 0;
        void OnPostRender()
        {
            if (cacheTextureId == -1 && cam.targetTexture != null)
            {
                cacheTextureId = (int)cam.targetTexture.GetNativeTexturePtr();
            }
            if(OnPostRenderListener != null) OnPostRenderListener(cacheTextureId, eye);

            if (eye == NvrViewer.Eye.Left)
            {
                // 录屏
                RenderTexture stereoScreen = cam.targetTexture;
                /*if (stereoScreen != null && NvrViewer.Instance.GetNibiruService() != null)
                {
                    int textureId = (int)stereoScreen.GetNativeTexturePtr();
                    bool isCapturing = NxrViewer.Instance.GetNibiruService().CaptureDrawFrame(textureId, frameId);
                    if (isCapturing)
                    {
                        GL.InvalidateState();
                    }
                    frameId++;
                }*/
            }
        }

        private void SetupStereo()
        {
            int eyeType = eye == NvrViewer.Eye.Left ? 0 : 1;
            if (cam.targetTexture == null
                 && NvrViewer.Instance.GetStereoScreen(eyeType) != null){
                cam.targetTexture = monoCamera.targetTexture ?? NvrViewer.Instance.GetStereoScreen(eyeType);
            }
        }

        void OnPreCull()
        {
            // cam.transform.position =new Vector3(0,Random.value/6,0); 
            if (NvrGlobal.isVR9Platform)
            {
                cam.targetTexture = null;
                return;
            }

            if (!NvrViewer.Instance.VRModeEnabled)
            {
                // Keep stereo enabled flag in sync with parent mono camera.
                cam.enabled = false;
                return;
            }

            SetupStereo();

            int eyeType = eye == NvrViewer.Eye.Left ? 0 : 1;
            if (NvrViewer.Instance.EffectRender && NvrViewer.Instance.GetStereoScreen(eyeType) != null)
            {
                // Some image effects clobber the whole screen.  Add a final image effect to the chain
                // which restores side-by-side stereo.
                stereoEffect = GetComponent<StereoRenderEffect>();
                if (stereoEffect == null)
                {
                    stereoEffect = gameObject.AddComponent<StereoRenderEffect>();
#if UNITY_5_6_OR_NEWER
                        stereoEffect.UpdateEye(eye);
#endif  // UNITY_5_6_OR_NEWER
                }
                stereoEffect.enabled = true;
            }
            else if (stereoEffect != null)
            {
                // Don't need the side-by-side image effect.
                stereoEffect.enabled = false;
            }


        }

        public void CopyCameraAndMakeSideBySide(NvrStereoController controller)
        {
#if UNITY_EDITOR
            // Member variable 'cam' not always initialized when this method called in Editor.
            // So, we'll just make a local of the same name.
            var cam = GetComponent<Camera>();
#endif

            float ipd = NvrViewer.Instance.Profile.viewer.lenses.separation;
            Vector3 localPosition = (eye == NvrViewer.Eye.Left ? -ipd / 2 : ipd / 2) * Vector3.right;

            if (monoCamera == null)
            {
                monoCamera = controller.GetComponent<Camera>();
            }

            // Sync the camera properties.
            cam.CopyFrom(monoCamera);
            monoCamera.useOcclusionCulling = false;

            // Not sure why we have to do this, but if we don't then switching between drawing to
            // the main screen or to the stereo rendertexture acts very strangely.
            cam.depth = eye == NvrViewer.Eye.Left ? monoCamera.depth + 1 : monoCamera.depth + 2;

            // Reset transform, which was clobbered by the CopyFrom() call.
            // Since we are a child of the mono camera, we inherit its transform already.
            transform.localPosition = localPosition;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
         
            Rect left = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
            Rect right = new Rect(0.5f, 0.0f, 0.5f, 1.0f);
            if (eye == NvrViewer.Eye.Left)
                cam.rect = left;
            else
                cam.rect = right;
           
            // VR9 采用左右眼各分一半效果
            if (!NvrGlobal.isVR9Platform && NvrViewer.USE_DTR && NvrGlobal.supportDtr && Application.platform == RuntimePlatform.Android)
            {
                // DTR&DFT的Android模式左右眼视窗大小均为0~1
                cam.rect =  new Rect(0, 0, 1, 1);
            }


            if (cam.farClipPlane < NvrGlobal.fovFar)
            {
                cam.farClipPlane = NvrGlobal.fovFar;
            }
            if(NvrGlobal.isVR9Platform)
            {
                cam.clearFlags = CameraClearFlags.Nothing;
                monoCamera.clearFlags = CameraClearFlags.Nothing;
            }

            cam.aspect = 1.0f;
        }
    }
}