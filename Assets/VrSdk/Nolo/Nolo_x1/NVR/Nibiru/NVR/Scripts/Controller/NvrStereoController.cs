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

#define NVR_HACK

using UnityEngine;
using System.Collections;
using System.Linq;

/// Controls a pair of NvrEye objects that will render the stereo view
/// of the camera this script is attached to.
///
/// This script must be added to any camera that should render stereo when the app
/// is in VR Mode.  This includes picture-in-picture windows, whether their contents
/// are in stereo or not: the window itself must be twinned for stereo, regardless.
///
/// For each frame, StereoController decides whether to render via the camera it
/// is attached to (the _mono_ camera) or the stereo eyes that it controls (see
/// NvrEye). You control this  decision for all cameras at once by setting
/// the value of NvrViewer#VRModeEnabled.
///
/// For technical reasons, the mono camera remains enabled for the initial portion of
/// the frame.  It is disabled only when rendering begins in `OnPreCull()`, and is
/// reenabled again at the end of the frame.  This allows 3rd party scripts that use
/// `Camera.main`, for example, to refer the the mono camera even when VR Mode is
/// enabled.
///
/// At startup the script ensures it has a full stereo rig, which consists of two
/// child cameras with NvrEye scripts attached, and a NvrHead script
/// somewhere in the hierarchy of parents and children for head tracking.  The rig
/// is created if necessary, the NvrHead being attached to the controller
/// itself.  The child camera settings are then cloned or updated from the mono
/// camera.
///
/// It is permissible for a StereoController to contain another StereoController
/// as a child.  In this case, a NvrEye is controlled by its closest
/// StereoController parent.
///
/// The Inspector panel for this script includes a button _Update Stereo Cameras_.
/// This performs the same action as described above for startup, but in the Editor.
/// Use this to generate the rig if you intend to customize it.  This action is also
/// available via _Component -> NVR -> Update Stereo Cameras_ in the Editor’s
/// main menu, and in the context menu for the `Camera` component.
namespace Nvr.Internal
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("NVR/Controller/NvrStereoController")]
    public class NvrStereoController : MonoBehaviour
    {
  
        // Flags whether we rendered in stereo for this frame.
        private bool renderedStereo = false;

        // Cache for speed, except in editor (don't want to get out of sync with the scene).
        private NvrEye[] eyes;
        private NvrHead mHead;

        /// Returns an array of stereo cameras that are controlled by this instance of
        /// the script.
        /// @note This array is cached for speedier access.  Call
        /// InvalidateEyes if it is ever necessary to reset the cache.
        public NvrEye[] Eyes
        {
            get
            {
                if (eyes == null)
                {
                    eyes = GetComponentsInChildren<NvrEye>(true)
                           .Where(eye => eye.Controller == this)
                           .ToArray();
                }

                if (eyes == null)
                {
                    NvrEye[] NvrEyess = FindObjectsOfType<NvrEye>();
                    if (NvrEyess.Length > 0)
                    {
                        eyes = NvrEyess;
                    }
                }
                return eyes;
            }
        }

        /// Returns the nearest NvrHead that affects our eyes.
        /// @note Cached for speed.  Call InvalidateEyes to clear the cache.
        public NvrHead Head
        {
            get
            {
#if UNITY_EDITOR
                NvrHead mHead = null;  // Local variable rather than member, so as not to cache.
#endif
                if (mHead == null)
                {
                    mHead = FindObjectOfType<NvrHead>();
                }
                return mHead;
            }
        }


        public Camera cam { get; private set; }

        void Awake()
        {
            NvrViewer.Create();
            cam = GetComponent<Camera>();
            AddStereoRig();

            NvrOverrideSettings.OnProfileChangedEvent += OnProfileChanged;
        }

        void OnProfileChanged()
        {
            Debug.Log("OnProfileChanged");
            NvrEye[] eyes = NvrViewer.Instance.eyes;
            foreach (NvrEye eye in eyes)
            {
                if (eye != null)
                {
                    eye.UpdateCameraProjection();
                }
            }
        }

        /// Helper routine for creation of a stereo rig.  Used by the
        /// custom editor for this class, or to build the rig at runtime.
        public void AddStereoRig()
        {
            Debug.Log("AddStereoRig.CreateEye");
            CreateEye(NvrViewer.Eye.Left);
            CreateEye(NvrViewer.Eye.Right);

            if (Head == null)
            {
                gameObject.AddComponent<NvrHead>();
                // Don't track position for dynamically added Head components, or else
                // you may unexpectedly find your camera pinned to the origin.
            }
            Head.SetTrackPosition(NvrViewer.Instance.TrackerPosition);
        }

        // Helper routine for creation of a stereo eye.
        private void CreateEye(NvrViewer.Eye eye)
        {
            string nm = name + (eye == NvrViewer.Eye.Left ? " Left" : " Right");
            NvrEye[] eyes = GetComponentsInChildren<NvrEye>();
            NvrEye mNxrEye = null;
            if (eyes != null && eyes.Length > 0)
            {
                foreach(NvrEye mEye in eyes)
                {
                    if(mEye.eye == eye)
                    {
                        mNxrEye = mEye;
                        break;
                    }
                }
            }
            // 创建新的
            if (mNxrEye == null)
            {
                GameObject go = new GameObject(nm);
                go.transform.SetParent(transform, false);
                go.AddComponent<Camera>().enabled = false;
                mNxrEye = go.AddComponent<NvrEye>();
            }

            if(NvrOverrideSettings.OnEyeCameraInitEvent != null) NvrOverrideSettings.OnEyeCameraInitEvent(eye, mNxrEye.gameObject);

            mNxrEye.Controller = this;
            mNxrEye.eye = eye;
            mNxrEye.CopyCameraAndMakeSideBySide(this);
            mNxrEye.OnPostRenderListener += OnPostRenderListener;
            mNxrEye.OnPreRenderListener += OnPreRenderListener;
            NvrViewer.Instance.eyes[eye == NvrViewer.Eye.Left ? 0 : 1] = mNxrEye;
            Debug.Log("CreateEye:" + nm + (eyes == null));
        }

        void OnPreRenderListener(int cacheTextureId, NvrViewer.Eye eyeType)
        {
            if (NvrGlobal.isVR9Platform) return;
            if (NvrViewer.USE_DTR && NvrGlobal.supportDtr)
            {
                // 左右眼绘制开始
                RenderEventType eventType = eyeType == NvrViewer.Eye.Left ? RenderEventType.LeftEyeBeginFrame : RenderEventType.RightEyeBeginFrame;
                NvrPluginEvent.IssueWithData(eventType, cacheTextureId);
            }
        }

        void OnPostRenderListener(int cacheTextureId, NvrViewer.Eye eyeType)
        {
            if (NvrGlobal.isVR9Platform)
            {
                if (NvrViewer.USE_DTR)
                {
                    NvrViewer.Instance.EnterVRMode();
                }
                if (eyeType == NvrViewer.Eye.Right && Application.isMobilePlatform)
                {
                    NvrPluginEvent.Issue(RenderEventType.PrepareFrame);
                }
                return;
            }

            if (NvrViewer.USE_DTR && NvrGlobal.supportDtr)
            {
                // 左右眼绘制结束
                RenderEventType eventType = eyeType == NvrViewer.Eye.Left ? RenderEventType.LeftEyeEndFrame : RenderEventType.RightEyeEndFrame;
                // 左右眼绘制结束事件
                if (cacheTextureId == -1)
                {
                    cacheTextureId = (int)cam.targetTexture.GetNativeTexturePtr();
                }
                NvrPluginEvent.IssueWithData(eventType, cacheTextureId);
            }

            if (NvrViewer.USE_DTR)
            {
                NvrViewer.Instance.EnterVRMode();
            }
        }


        void OnEnable()
        {
            StartCoroutine("EndOfFrame");
        }

        void OnDisable()
        {
            StopCoroutine("EndOfFrame");
        }
         
        void OnPreCull()
        {
            if (NvrViewer.Instance.VRModeEnabled)
            {
                // Activate the eyes under our control.
                NvrEye[] eyes = Eyes;
                for (int i = 0, n = eyes.Length; i < n; i++)
                {
                    eyes[i].cam.enabled = true;
                }
                // Turn off the mono camera so it doesn't waste time rendering.  Remember to reenable.
                // @note The mono camera is left on from beginning of frame till now in order that other game
                // logic (e.g. referring to Camera.main) continues to work as expected.
#if NVR_HACK
                // Due to a Unity bug, a worldspace canvas in a camera that renders to a RenderTexture allocates infinite memory. Remove the hack ASAP as the fix gets released.
                BlackOutMonoCamera();
#else
                if(!NvrViewer.Instance.IsWinPlatform) cam.enabled = false;
#endif
                renderedStereo = true;
            }
        }

        IEnumerator EndOfFrame()
        {
            while (true)
            {
                // If *we* turned off the mono cam, turn it back on for next frame.
                if (renderedStereo)
                {
#if NVR_HACK
                    RestoreMonoCamera();
#else
                    cam.enabled = true;
#endif
                    renderedStereo = false;
                }
                yield return new WaitForEndOfFrame();
            }
        }
 

#if NVR_HACK
        private CameraClearFlags m_MonoCameraClearFlags;
        private Color m_MonoCameraBackgroundColor;
        private int m_MonoCameraCullingMask;

        private void BlackOutMonoCamera()
        {
            m_MonoCameraClearFlags = cam.clearFlags;
            m_MonoCameraBackgroundColor = cam.backgroundColor;
            m_MonoCameraCullingMask = cam.cullingMask;

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.cullingMask = 0;
        }

        private void RestoreMonoCamera()
        {
            cam.clearFlags = m_MonoCameraClearFlags;
            cam.backgroundColor = m_MonoCameraBackgroundColor;
            cam.cullingMask = m_MonoCameraCullingMask;
        }
#endif
    }
}