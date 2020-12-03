using UnityEngine;
using System.Collections;
using NibiruTask;
using Nvr.Internal;
namespace NibiruTask
{
    public class NvrArmModel : NvrBaseArmModel
    {
        public Transform head;

        public Vector3 elbowRestPosition = DEFAULT_ELBOW_REST_POSITION;

        public Vector3 wristRestPosition = DEFAULT_WRIST_REST_POSITION;

        public Vector3 controllerRestPosition = DEFAULT_CONTROLLER_REST_POSITION;

        public Vector3 armExtensionOffset = DEFAULT_ARM_EXTENSION_OFFSET;

        [Range(0.0f, 1.0f)]
        public float elbowBendRatio = DEFAULT_ELBOW_BEND_RATIO;

        [Range(0.0f, 0.4f)]
        public float fadeControllerOffset = 0.0f;

        [Range(0.0f, 0.4f)]
        public float fadeDistanceFromHeadForward = 0.25f;

        [Range(0.0f, 0.4f)]
        public float fadeDistanceFromHeadSide = 0.15f;

        [Range(0.4f, 0.6f)]
        public float tooltipMinDistanceFromFace = 0.45f;

        [Range(0, 180)]
        public int tooltipMaxAngleFromCamera = 80;

        public bool isLockedToNeck = false;

        public override Vector3 ControllerPositionFromHead
        {
            get
            {
                return controllerPosition;
            }
        }

        public override Quaternion ControllerRotationFromHead
        {
            get
            {
                return controllerRotation;
            }
        }

        public override float PreferredAlpha
        {
            get
            {
                return preferredAlpha;
            }
        }
        public override float TooltipAlphaValue
        {
            get
            {
                return tooltipAlphaValue;
            }
        }
        public Vector3 NeckPosition
        {
            get
            {
                return neckPosition;
            }
        }

        public Vector3 ShoulderPosition
        {
            get
            {
                Vector3 shoulderPosition = neckPosition + torsoRotation * Vector3.Scale(SHOULDER_POSITION, handedMultiplier);
                return shoulderPosition;
            }
        }

        public Quaternion ShoulderRotation
        {
            get
            {
                return torsoRotation;
            }
        }

        public Vector3 ElbowPosition
        {
            get
            {
                return elbowPosition;
            }
        }

        public Quaternion ElbowRotation
        {
            get
            {
                return elbowRotation;
            }
        }

        public Vector3 WristPosition
        {
            get
            {
                return wristPosition;
            }
        }

        public Quaternion WristRotation
        {
            get
            {
                return wristRotation;
            }
        }

        protected Vector3 neckPosition;
        protected Vector3 elbowPosition;
        protected Quaternion elbowRotation;
        protected Vector3 wristPosition;
        protected Quaternion wristRotation;
        protected Vector3 controllerPosition;
        protected Quaternion controllerRotation;
        protected float preferredAlpha;
        protected float tooltipAlphaValue;


        protected Vector3 handedMultiplier;


        protected Vector3 torsoDirection;


        protected Quaternion torsoRotation;

        public static readonly Vector3 DEFAULT_ELBOW_REST_POSITION = new Vector3(0.195f, -0.5f, 0.005f);
        public static readonly Vector3 DEFAULT_WRIST_REST_POSITION = new Vector3(0.0f, 0.0f, 0.25f);
        public static readonly Vector3 DEFAULT_CONTROLLER_REST_POSITION = new Vector3(0.0f, 0.0f, 0.05f);
        public static readonly Vector3 DEFAULT_ARM_EXTENSION_OFFSET = new Vector3(-0.13f, 0.14f, 0.08f);
        public const float DEFAULT_ELBOW_BEND_RATIO = 0.6f;

        protected const float EXTENSION_WEIGHT = 0.4f;

        protected static readonly Vector3 SHOULDER_POSITION = new Vector3(0.17f, -0.2f, -0.03f);

        protected static readonly Vector3 NECK_OFFSET = new Vector3(0.0f, 0.075f, 0.08f);

        protected const float DELTA_ALPHA = 4.0f;

        protected const float MIN_EXTENSION_ANGLE = 7.0f;
        protected const float MAX_EXTENSION_ANGLE = 60.0f;

        private NvrTrackedDevice nvrTrackedDevice;

        protected virtual void OnEnable()
        {
            if(head == null && NvrViewer.Instance.GetHead() != null)
            {
                head = NvrViewer.Instance.GetHead().transform;
            }
            nvrTrackedDevice = GetComponent<NvrTrackedDevice>();
            UpdateTorsoDirection(true);
            OnControllerInputUpdated();
        }

        protected virtual void OnDisable()
        {

        }

        public virtual void OnControllerInputUpdated()
        {
            UpdateHandedness();
            UpdateTorsoDirection(false);
            UpdateNeckPosition();
            ApplyArmModel();
            UpdateTransparency();
        }

        protected virtual void UpdateHandedness()
        {

            //判断手柄是否为空
            if (PlayerCtrl.Instance == null || !PlayerCtrl.Instance.IsQuatConn())
            {
                return;
            }

            handedMultiplier.Set(0, 1, 1);
            //设置左右手
            if (nvrTrackedDevice.deviceType == NvrTrackedDevice.NibiruDeviceType.RightController)
            {
                handedMultiplier.x = 1.0f;
            }
            else
            {
                handedMultiplier.x = -1.0f;
            }
        }

        protected virtual void UpdateTorsoDirection(bool forceImmediate)
        {
            //head朝向
            Vector3 gazeDirection = head.localRotation * Vector3.forward;
            gazeDirection.y = 0.0f;
            gazeDirection.Normalize();
            bool IsQuatConn = PlayerCtrl.Instance != null && PlayerCtrl.Instance.IsQuatConn();
            if (forceImmediate ||  IsQuatConn)
            {
                torsoDirection = gazeDirection;
            }
            else
            {
                //设置角速度
                float angularVelocity = IsQuatConn ? 0.2f : 0;
                float gazeFilterStrength = Mathf.Clamp((angularVelocity - 0.2f) / 45.0f, 0.0f, 0.1f);
                torsoDirection = Vector3.Slerp(torsoDirection, gazeDirection, gazeFilterStrength);
            }
            torsoRotation = Quaternion.FromToRotation(Vector3.forward, torsoDirection);
        }

        protected virtual void UpdateNeckPosition()
        {
            if (isLockedToNeck)
            {
                neckPosition = head.localPosition;
                neckPosition = ApplyInverseNeckModel(neckPosition);
            }
            else
            {
                neckPosition = Vector3.zero;
            }
        }

        protected virtual void ApplyArmModel()
        {
            SetUntransformedJointPositions();
            Quaternion controllerOrientation;
            Quaternion xyRotation;
            float xAngle;
            GetControllerRotation(out controllerOrientation, out xyRotation, out xAngle);
            float extensionRatio = CalculateExtensionRatio(xAngle);
            ApplyExtensionOffset(extensionRatio);
            Quaternion lerpRotation = CalculateLerpRotation(xyRotation, extensionRatio);

            CalculateFinalJointRotations(controllerOrientation, xyRotation, lerpRotation);
            ApplyRotationToJoints();
        }

        public virtual void SetUntransformedJointPositions()
        {
            elbowPosition = Vector3.Scale(elbowRestPosition, handedMultiplier);
            wristPosition = Vector3.Scale(wristRestPosition, handedMultiplier);
            controllerPosition = Vector3.Scale(controllerRestPosition, handedMultiplier);
        }
        protected virtual float CalculateExtensionRatio(float xAngle)
        {
            float normalizedAngle = (xAngle - MIN_EXTENSION_ANGLE) / (MAX_EXTENSION_ANGLE - MIN_EXTENSION_ANGLE);
            float extensionRatio = Mathf.Clamp(normalizedAngle, 0.0f, 1.0f);
            return extensionRatio;
        }
        protected virtual void ApplyExtensionOffset(float extensionRatio)
        {
            Vector3 extensionOffset = Vector3.Scale(armExtensionOffset, handedMultiplier);
            elbowPosition += extensionOffset * extensionRatio;
        }
        protected virtual Quaternion CalculateLerpRotation(Quaternion xyRotation, float extensionRatio)
        {
            float totalAngle = Quaternion.Angle(xyRotation, Quaternion.identity);
            float lerpSuppresion = 1.0f - Mathf.Pow(totalAngle / 180.0f, 6.0f);
            float inverseElbowBendRatio = 1.0f - elbowBendRatio;
            float lerpValue = inverseElbowBendRatio + elbowBendRatio * extensionRatio * EXTENSION_WEIGHT;
            lerpValue *= lerpSuppresion;
            return Quaternion.Lerp(Quaternion.identity, xyRotation, lerpValue);
        }
        protected virtual void CalculateFinalJointRotations(Quaternion controllerOrientation, Quaternion xyRotation, Quaternion lerpRotation)
        {
            elbowRotation = torsoRotation * Quaternion.Inverse(lerpRotation) * xyRotation;
            wristRotation = elbowRotation * lerpRotation;
            controllerRotation = torsoRotation * controllerOrientation;
        }
        protected virtual void ApplyRotationToJoints()
        {
            elbowPosition = neckPosition + torsoRotation * elbowPosition;
            wristPosition = elbowPosition + elbowRotation * wristPosition;
            controllerPosition = wristPosition + wristRotation * controllerPosition;
        }

        protected virtual Vector3 ApplyInverseNeckModel(Vector3 headPosition)
        {
            Quaternion headRotation = head.localRotation;
            Vector3 rotatedNeckOffset =
              headRotation * NECK_OFFSET - NECK_OFFSET.y * Vector3.up;
            headPosition -= rotatedNeckOffset;

            return headPosition;
        }

        protected virtual void UpdateTransparency()
        {
            Vector3 controllerForward = controllerRotation * Vector3.forward;
            Vector3 offsetControllerPosition = controllerPosition + (controllerForward * fadeControllerOffset);
            Vector3 controllerRelativeToHead = offsetControllerPosition - neckPosition;

            Vector3 headForward = head.localRotation * Vector3.forward;
            float distanceToHeadForward = Vector3.Scale(controllerRelativeToHead, headForward).magnitude;
            Vector3 headRight = Vector3.Cross(headForward, Vector3.up);
            float distanceToHeadSide = Vector3.Scale(controllerRelativeToHead, headRight).magnitude;
            float distanceToHeadUp = Mathf.Abs(controllerRelativeToHead.y);

            bool shouldFadeController = distanceToHeadForward < fadeDistanceFromHeadForward
              && distanceToHeadUp < fadeDistanceFromHeadForward
              && distanceToHeadSide < fadeDistanceFromHeadSide;

            float animationDelta = DELTA_ALPHA * Time.unscaledDeltaTime;
            if (shouldFadeController)
            {
                preferredAlpha = Mathf.Max(0.0f, preferredAlpha - animationDelta);
            }
            else
            {
                preferredAlpha = Mathf.Min(1.0f, preferredAlpha + animationDelta);
            }

            float dot = Vector3.Dot(controllerRotation * Vector3.up, -controllerRelativeToHead.normalized);
            float minDot = (tooltipMaxAngleFromCamera - 90.0f) / -90.0f;
            float distToFace = Vector3.Distance(controllerRelativeToHead, Vector3.zero);
            if (shouldFadeController
              || distToFace > tooltipMinDistanceFromFace
              || dot < minDot)
            {
                tooltipAlphaValue = Mathf.Max(0.0f, tooltipAlphaValue - animationDelta);
            }
            else
            {
                tooltipAlphaValue = Mathf.Min(1.0f, tooltipAlphaValue + animationDelta);
            }
        }

        protected void GetControllerRotation(out Quaternion rotation, out Quaternion xyRotation, out float xAngle)
        {
            bool IsQuatConn = PlayerCtrl.Instance != null && PlayerCtrl.Instance.IsQuatConn();
            //设置角度
            rotation = IsQuatConn ? PlayerCtrl.Instance.mTransform.rotation : Quaternion.identity;
            rotation = Quaternion.Inverse(torsoRotation) * rotation;

            Vector3 controllerForward = rotation * Vector3.forward;
            xAngle = 90.0f - Vector3.Angle(controllerForward, Vector3.up);

            xyRotation = Quaternion.FromToRotation(Vector3.forward, controllerForward);
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            if (!enabled)
            {
                return;
            }

            if (transform.parent == null)
            {
                return;
            }

            Vector3 worldShoulder = transform.parent.TransformPoint(ShoulderPosition);
            Vector3 worldElbow = transform.parent.TransformPoint(elbowPosition);
            Vector3 worldwrist = transform.parent.TransformPoint(wristPosition);
            Vector3 worldcontroller = transform.parent.TransformPoint(controllerPosition);


            Gizmos.color = Color.red;
            Gizmos.DrawSphere(worldShoulder, 0.02f);
            Gizmos.DrawLine(worldShoulder, worldElbow);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(worldElbow, 0.02f);
            Gizmos.DrawLine(worldElbow, worldwrist);

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(worldwrist, 0.02f);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(worldcontroller, 0.02f);
        }
#endif 
    }
}
