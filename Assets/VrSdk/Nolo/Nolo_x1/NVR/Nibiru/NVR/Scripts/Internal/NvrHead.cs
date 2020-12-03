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

using NibiruTask;
using Nvr.Internal;
using UnityEngine;

/// This script provides head tracking support for a camera.
///
/// Attach this script to any game object that should match the user's head motion.
/// By default, it continuously updates the local transform to NvrViewer.HeadView.
/// A target object may be specified to provide an alternate reference frame for the motion.
///
/// This script will typically be attached directly to a _Camera_ object, or to its
/// parent if you need to offset the camera from the origin.
/// Alternatively it can be inserted as a child of the _Camera_ but parent of the
/// NvrEye camera.  Do this if you already have steering logic driving the
/// mono Camera and wish to have the user's head motion be relative to that.  Note
/// that in the latter setup, head tracking is visible only when VR Mode is enabled.
///
/// In some cases you may need two instances of NvrHead, referring to two
/// different targets (one of which may be the parent), in order to split where
/// the rotation is applied from where the positional offset is applied.  Use the
/// #trackRotation and #trackPosition properties in this case.
/// 
namespace Nvr.Internal
{
    [AddComponentMenu("NVR/NvrHead")]
    public class NvrHead : MonoBehaviour
    {
        public Vector3 BasePosition { set; get; }

        /// Determines whether to apply the user's head rotation to this gameobject's
        /// orientation.  True means to update the gameobject's orientation with the
        /// user's head rotation, and false means don't modify the gameobject's orientation.
        private bool trackRotation = true;

        /// Determines whether to apply ther user's head offset to this gameobject's
        /// position.  True means to update the gameobject's position with the user's head offset,
        /// and false means don't modify the gameobject's position.
        private bool trackPosition = false;

        public void SetTrackPosition(bool b)
        {
            trackPosition = b;
        }

        public void SetTrackRotation(bool b)
        {
            trackRotation = b;
        }

        public bool IsTrackRotation()
        {
            return trackRotation;
        }

        public bool IsTrackPosition()
        {
            return trackPosition;
        }

        protected Transform mTransform;
        void Start()
        {
            mTransform = this.transform;
        }

        // Normally, update head pose now.
        void LateUpdate()
        {
            NvrViewer.Instance.UpdateHeadPose();
            UpdateHead();
        }

        // 初始的Yaw欧拉角，有时进入时正方向有偏转，此时需要校正一下
        private float initEulerYAngle = float.MaxValue;
        // Compute new head pose.
        private void UpdateHead()
        {
            if (trackRotation)
            {
                float[] eulerRange = NvrViewer.Instance.GetHeadEulerAnglesRange();
                Quaternion rot = NvrViewer.Instance.HeadPose.Orientation;
                if (rot.eulerAngles.y != 0 && initEulerYAngle == float.MaxValue)
                {
                    initEulerYAngle = rot.eulerAngles.y;
                    if (float.IsNaN(initEulerYAngle))
                    {
                        Debug.Log("DATA IS ABNORMAL--------------------------->>>>>>>>>");
                        initEulerYAngle = float.MaxValue;
                    } else
                    {
                        Debug.Log("initEulerYAngle=" + initEulerYAngle);
                    }
                }

                //if (initEulerYAngle != float.MaxValue)
                //{
                //    rot.eulerAngles = new Vector3(rot.eulerAngles.x, rot.eulerAngles.y - initEulerYAngle, rot.eulerAngles.z);
                //}
 
                Vector3 eulerAngles = rot.eulerAngles;
                if (eulerRange == null ||
                      (
                        //  水平有限制
                        (eulerRange != null && (eulerAngles[1] >= eulerRange[0] || eulerAngles[1] < eulerRange[1]) &&
                        //   垂直有限制
                        (eulerAngles[0] >= eulerRange[2] || eulerAngles[0] < eulerRange[3]))
                     )
                   )
                {
                    mTransform.localRotation = rot;
                }
            }

#if UNITY_ANDROID
             if (trackPosition)
            {
                Vector3 pos = NvrViewer.Instance.HeadPose.Position;
                mTransform.position = BasePosition + pos;
                if (PlayerCtrl.Instance != null)
                {
                    PlayerCtrl.Instance.HeadPosition = mTransform.position;
                }
            }
#endif

        }

        public void ResetInitEulerYAngle()
        {
            initEulerYAngle = 0;
        }

#if UNITY_EDITOR
        private void Update()
        {
            Vector3 start = transform.position;
            Vector3 vector = transform.TransformDirection(Vector3.forward);
            UnityEngine.Debug.DrawRay(start, vector * 20, Color.red);
        }
#endif
    }
}