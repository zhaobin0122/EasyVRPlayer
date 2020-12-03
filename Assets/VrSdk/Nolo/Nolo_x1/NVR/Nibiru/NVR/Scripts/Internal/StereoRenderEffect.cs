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
namespace Nvr.Internal
{
    /// @cond
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("NVR/Internal/StereoRenderEffect")]
    public class StereoRenderEffect : MonoBehaviour
    {
        private Material material;

        private Camera cam;


#if UNITY_5_6_OR_NEWER
        private Rect fullRect;
        public NvrViewer.Eye eye;
#else
  private static readonly Rect fullRect = new Rect(0, 0, 1, 1);
#endif  // UNITY_5_6_OR_NEWER

        void Awake()
        {
            cam = GetComponent<Camera>();
        }

        void Start()
        {
            material = new Material(Shader.Find("NVR/UnlitTexture"));
#if UNITY_5_6_OR_NEWER
            fullRect = (eye == NvrViewer.Eye.Left ? new Rect(0, 0, 0.5f, 1) : new Rect(0.5f, 0, 0.5f, 1));
#endif
        }

        public void UpdateEye(NvrViewer.Eye eyeTmp)
        {
#if UNITY_5_6_OR_NEWER
            this.eye = eyeTmp;
            fullRect = (eye == NvrViewer.Eye.Left ? new Rect(0, 0, 0.5f, 1) : new Rect(0.5f, 0, 0.5f, 1));
#endif
        }

        void OnRenderImage(RenderTexture source, RenderTexture dest)
        {
            GL.PushMatrix();
            int width = dest ? dest.width : Screen.width;
            int height = dest ? dest.height : Screen.height;
            GL.LoadPixelMatrix(0, width, height, 0);
            // Camera rects are in screen coordinates (bottom left is origin), but DrawTexture takes a
            // rect in GUI coordinates (top left is origin).
            Rect blitRect = cam.pixelRect;
            blitRect.y = height - blitRect.height - blitRect.y;
            RenderTexture oldActive = RenderTexture.active;
            RenderTexture.active = dest;
            Graphics.DrawTexture(blitRect, source, fullRect, 0, 0, 0, 0, Color.white, material);
            RenderTexture.active = oldActive;
            GL.PopMatrix();
        }
    }
    /// @endcond

}