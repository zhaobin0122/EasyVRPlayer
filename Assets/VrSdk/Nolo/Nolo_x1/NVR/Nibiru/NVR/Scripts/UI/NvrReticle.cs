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
using UnityEngine.UI;

//   在凝视的物体前方绘制一个圆圈，选中物体，自动放大
/// Draws a circular reticle in front of any object that the user gazes at.
/// The circle dilates if the object is clickable.
/// 
namespace Nvr.Internal
{
    [AddComponentMenu("NVR/UI/NvrReticle")]
    [RequireComponent(typeof(Renderer))]
    public class NvrReticle : MonoBehaviour, INvrGazePointer
    {
        /// Number of segments making the reticle circle.20
        private int reticleSegments = 20;

        /// Growth speed multiplier for the reticle/
        public float reticleGrowthSpeed = 8.0f;

        ///  Show reticle or not ,if show the material's alpha is 1,else is 0.
        private bool showReticle = true;

        // Private members
        private Material materialComp;
        // private GameObject targetObj;

        // Current inner angle of the reticle (in degrees).
        private float reticleInnerAngle = 0.0f;
        // Current outer angle of the reticle (in degrees).
        private float reticleOuterAngle = 0.5f;
        // Current distance of the reticle (in meters).
        private float reticleDistanceInMeters = 10.0f;

        // Minimum inner angle of the reticle (in degrees).
        public float kReticleMinInnerAngle = 0.0f;
        // Minimum outer angle of the reticle (in degrees).
        public float kReticleMinOuterAngle = 0.5f;
        // Angle at which to expand the reticle when intersecting with an object
        // (in degrees).
        private const float kReticleGrowthAngle = 1.0f;

        // Minimum distance of the reticle (in meters).
        private const float kReticleDistanceMin = 0.45f;
        // Maximum distance of the reticle (in meters).
        private const float kReticleDistanceMax = 10f;

        // Current inner and outer diameters of the reticle,
        // before distance multiplication.
        private float reticleInnerDiameter = 0.0f;
        private float reticleOuterDiameter = 0.0f;

        /// Sorting order to use for the reticle's renderer.
        /// Range values come from https://docs.unity3d.com/ScriptReference/Renderer-sortingOrder.html.
        /// Default value 32767 ensures gaze reticle is always rendered on top.
        [Range(-32767, 32767)]
        public int reticleSortingOrder = 32767;

        GameObject reticlePointer;
        Transform cacheTransform;
        private void Awake()
        {
            cacheTransform = transform;
            reticlePointer = new GameObject("Pointer");
            reticlePointer.transform.parent = cacheTransform;
        }

        void Start()
        {
            CreateReticleVertices();

            Renderer rendererComponent = GetComponent<Renderer>();
            rendererComponent.sortingOrder = reticleSortingOrder;
            materialComp = rendererComponent.material;
        }

        public GameObject GetReticlePointer()
        {
            return reticlePointer;
        }

        void OnEnable()
        {
            Debug.Log("NvrReticle OnEnable");
        }

        void OnDisable()
        {
            Debug.Log("NvrReticle OnDisable");
            if (GazeInputModule.gazePointer == this)
            {
                GazeInputModule.gazePointer = null;
                showReticle = false;
            }
            if (headControl != null)
            {
                Destroy(headControl);
            }

        }

        private float alphaValue = -1.0f;

        // Only call device.UpdateState() once per frame.
        private int updatedToFrame = 0;

        private void Update()
        {
            if(GazeInputModule.gazePointer == null && !showReticle)
            {
                UpdateStatus();
            }
        }

        public void UpdateStatus()
        {
            if (updatedToFrame == Time.frameCount) return;

            updatedToFrame = Time.frameCount;
            if (showReticle)
            {
                UpdateDiameters();
            }

            float valueTmp = showReticle ? 1.0f : 0.0f;
            if (valueTmp != alphaValue)
            {
                alphaValue = valueTmp;
                materialComp.color = new Color(materialComp.color.r, materialComp.color.g, materialComp.color.b, alphaValue);
            }
        }

        /// This is called when the 'BaseInputModule' system should be enabled.
        public void OnGazeEnabled()
        {

        }

        /// This is called when the 'BaseInputModule' system should be disabled.
        public void OnGazeDisabled()
        {

        }

        /// Called when the user is looking on a valid GameObject. This can be a 3D
        /// or UI element.
        ///
        /// The camera is the event camera, the target is the object
        /// the user is looking at, and the intersectionPosition is the intersection
        /// point of the ray sent from the camera on the object.
        public void OnGazeStart(Camera camera, GameObject targetObject, Vector3 intersectionPosition,
                                bool isInteractive)
        {

            SetGazeTarget(intersectionPosition, isInteractive);

            if (headControl != null && isInteractive)
            {
                NvrHeadControl mNvrHeadControl = headControl.GetComponent<NvrHeadControl>();
                mNvrHeadControl.Show();
                mNvrHeadControl.HandleDown();
                NvrHeadControl.eventGameObject = targetObject;
            }
        }

        /// Called every frame the user is still looking at a valid GameObject. This
        /// can be a 3D or UI element.
        ///
        /// The camera is the event camera, the target is the object the user is
        /// looking at, and the intersectionPosition is the intersection point of the
        /// ray sent from the camera on the object.
        public void OnGazeStay(Camera camera, GameObject targetObject, Vector3 intersectionPosition,
                               bool isInteractive)
        {
            SetGazeTarget(intersectionPosition, isInteractive);
        }

        /// Called when the user's look no longer intersects an object previously
        /// intersected with a ray projected from the camera.
        /// This is also called just before **OnGazeDisabled** and may have have any of
        /// the values set as **null**.
        ///
        /// The camera is the event camera and the target is the object the user
        /// previously looked at.
        public void OnGazeExit(Camera camera, GameObject targetObject)
        {
            reticleDistanceInMeters = kReticleDistanceMax;
            reticleInnerAngle = kReticleMinInnerAngle;
            reticleOuterAngle = kReticleMinOuterAngle;

            if (headControl != null)
            {
                headControl.GetComponent<NvrHeadControl>().HandleUp();
                this.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        /// Called when a trigger event is initiated. This is practically when
        /// the user begins pressing the trigger.
        public void OnGazeTriggerStart(Camera camera)
        {
            // Put your reticle trigger start logic here :)
        }

        /// Called when a trigger event is finished. This is practically when
        /// the user releases the trigger.
        public void OnGazeTriggerEnd(Camera camera)
        {
            // Put your reticle trigger end logic here :)
        }

        public void GetPointerRadius(out float innerRadius, out float outerRadius)
        {
            float min_inner_angle_radians = Mathf.Deg2Rad * kReticleMinInnerAngle;
            float max_inner_angle_radians = Mathf.Deg2Rad * (kReticleMinInnerAngle + kReticleGrowthAngle);
            innerRadius = 2.0f * Mathf.Tan(min_inner_angle_radians);
            outerRadius = 2.0f * Mathf.Tan(max_inner_angle_radians);
        }

        private void CreateReticleVertices()
        {
            Mesh mesh = new Mesh();
            gameObject.AddComponent<MeshFilter>();
            GetComponent<MeshFilter>().mesh = mesh;

            int segments_count = reticleSegments;
            int vertex_count = (segments_count + 1) * 2;

            #region Vertices

            Vector3[] vertices = new Vector3[vertex_count];

            const float kTwoPi = Mathf.PI * 2.0f;
            int vi = 0;
            for (int si = 0; si <= segments_count; ++si)
            {
                // Add two vertices for every circle segment: one at the beginning of the
                // prism, and one at the end of the prism.
                float angle = (float)si / (float)(segments_count) * kTwoPi;

                float x = Mathf.Sin(angle);
                float y = Mathf.Cos(angle);

                vertices[vi++] = new Vector3(x, y, 0.0f); // Outer vertex.
                vertices[vi++] = new Vector3(x, y, 1.0f); // Inner vertex.
            }
            #endregion

            #region Triangles
            int indices_count = (segments_count + 1) * 3 * 2;
            int[] indices = new int[indices_count];

            int vert = 0;
            int idx = 0;
            for (int si = 0; si < segments_count; ++si)
            {
                indices[idx++] = vert + 1;
                indices[idx++] = vert;
                indices[idx++] = vert + 2;

                indices[idx++] = vert + 1;
                indices[idx++] = vert + 2;
                indices[idx++] = vert + 3;

                vert += 2;
            }
            #endregion

            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.RecalculateBounds();
            //mesh.Optimize();
        }

        private void UpdateDiameters()
        {
            reticleDistanceInMeters =
              Mathf.Clamp(reticleDistanceInMeters, kReticleDistanceMin, kReticleDistanceMax);

            if (reticleInnerAngle < kReticleMinInnerAngle)
            {
                reticleInnerAngle = kReticleMinInnerAngle;
            }

            if (reticleOuterAngle < kReticleMinOuterAngle)
            {
                reticleOuterAngle = kReticleMinOuterAngle;
            }

            float inner_half_angle_radians = Mathf.Deg2Rad * reticleInnerAngle * 0.5f;
            float outer_half_angle_radians = Mathf.Deg2Rad * reticleOuterAngle * 0.5f;

            float inner_diameter = 2.0f * Mathf.Tan(inner_half_angle_radians);
            float outer_diameter = 2.0f * Mathf.Tan(outer_half_angle_radians);

            reticleInnerDiameter =
                Mathf.Lerp(reticleInnerDiameter, inner_diameter, Time.deltaTime * reticleGrowthSpeed);
            reticleOuterDiameter =
                Mathf.Lerp(reticleOuterDiameter, outer_diameter, Time.deltaTime * reticleGrowthSpeed);

            materialComp.SetFloat("_InnerDiameter", reticleInnerDiameter * reticleDistanceInMeters);
            materialComp.SetFloat("_OuterDiameter", reticleOuterDiameter * reticleDistanceInMeters);
            materialComp.SetFloat("_DistanceInMeters", reticleDistanceInMeters);
        }

        internal void Show()
        {
            if (materialComp != null)
            {
                materialComp.color = new Color(materialComp.color.r, materialComp.color.g, materialComp.color.b, 1.0f);
            }
            showReticle = true;
        }

        public bool IsShowing()
        {
            return showReticle;
        }

        private static GameObject headControl;
        public void HeadShow()
        {
            if (headControl == null)
            {
                headControl = (GameObject)Instantiate(Resources.Load<GameObject>("Reticle/NvrHeadControl"),  gameObject.transform);
            }
            else
            {
                headControl.GetComponentInChildren<Image>().color = new Color(255, 255, 255, 1);
            }
            Debug.Log("HeadShow");
        }

        public void HeadDismiss()
        {
            if (headControl != null)
            {
                headControl.GetComponentInChildren<Image>().color = new Color(255, 255, 255, 0);
            }
        }

        public void Dismiss()
        {
            // 隐藏
            if (materialComp != null)
            {
                materialComp.color = new Color(materialComp.color.r, materialComp.color.g, materialComp.color.b, 0);
            }
            showReticle = false;
        }

        private void SetGazeTarget(Vector3 target, bool interactive)
        {
            Vector3 targetLocalPosition = cacheTransform.InverseTransformPoint(target);
            reticlePointer.transform.localPosition = new Vector3(0, 0, targetLocalPosition.z - 0.01f);

            reticleDistanceInMeters =
                Mathf.Clamp(targetLocalPosition.z, kReticleDistanceMin, kReticleDistanceMax);
            if (interactive)
            {
                if (NvrViewer.Instance != null)
                {
                    //判断头喵类型
                    if (NvrViewer.Instance.HeadControl != HeadControl.Hover)
                    {
                        reticleInnerAngle = kReticleMinInnerAngle + kReticleGrowthAngle;
                        reticleOuterAngle = kReticleMinOuterAngle + kReticleGrowthAngle;
                    }
                    else
                    {
                        this.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                        headControl.transform.localPosition = new Vector3(0, 0, reticleDistanceInMeters - 0.01f);
                    }
                }
            }
            else
            {
                reticleInnerAngle = kReticleMinInnerAngle;
                reticleOuterAngle = kReticleMinOuterAngle;
            }

        }

        public void UpdateColor(Color color)
        {
            if(materialComp  != null)
            {
                float alpha = materialComp.color.a;
                alphaValue = alpha;
                materialComp.color = new Color(color.r, color.g, color.b, alphaValue);
            }
        }

        public void UpdateSize(float size)
        {
            kReticleMinOuterAngle = size;
        }

        public float GetSize()
        {
            return kReticleMinOuterAngle;
        }
    }
}