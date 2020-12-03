using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace NibiruTask
{
    public abstract class NvrBaseArmModel : MonoBehaviour
    {
        public abstract Vector3 ControllerPositionFromHead { get; }

        public abstract Quaternion ControllerRotationFromHead { get; }

        public abstract float PreferredAlpha { get; }

        public abstract float TooltipAlphaValue { get; }
    }
}