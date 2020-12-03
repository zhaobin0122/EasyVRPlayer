using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent(typeof(NoloVR_TrackedDevice))]
public class NoloVR_Teleport : MonoBehaviour
{
    public bool enableTeleport = true;
    public Color pointerHitColor = new Color(0f, 0.5f, 0f, 1f);
    public Color pointerMissColor = new Color(0.8f, 0f, 0f, 1f);
    public Material pointerMaterial;
    public LayerMask layersToIgnore = Physics.IgnoreRaycastLayer;
    public int pointerDensity = 10;
    public float pointerLength = 10f;
    public float beamCurveOffset = 1f;
    [Range(1, 100)]
    public float beamHeightLimitAngle = 100f;
    public float pointerCursorRadius = 0.5f;
    public GameObject customPointerTracer;
    public GameObject customPointerCursor;

    public bool rescalePointerTracer = false;
    private GameObject pointerCursor;
    private GameObject curvedBeamContainer;
    private CurveGenerator curvedBeam;
    private NoloDeviceType deviceType;

    private Vector3 destinationPosition;
    private Transform pointerContactTarget = null;
    private Vector3 contactNormal;
    private float pointerContactDistance = 0f;
    private RaycastHit pointerContactRaycastHit = new RaycastHit();
    private Vector3 fixedForwardBeamForward;
    private const float BEAM_ADJUST_OFFSET = 0.00001f;
    private Color currentPointerColor;

    private Vector3 downPosition;
    void OnEnable()
    {
        InitPointer();
        deviceType = GetComponent<NoloVR_TrackedDevice>().deviceType;
    }
    void OnDisable()
    {
        if (pointerCursor != null)
        {
            Destroy(pointerCursor);
        }
        if (curvedBeam != null)
        {
            Destroy(curvedBeam);
        }
        if (curvedBeamContainer != null)
        {
            Destroy(curvedBeamContainer);
        }
    }

    void Update()
    {
        //按下touchpad
        if (NoloVR_Controller.GetDevice(deviceType).GetNoloButtonPressed(NoloButtonID.TouchPad))
        //if (true)
        {
            PointerActivate(true);
            var jointPosition = ProjectForwardBeam();
            downPosition = ProjectDownBeam(jointPosition);
            DisplayCurvedBeam(jointPosition, downPosition);
            SetPointerCursor(downPosition);
        }
        if (NoloVR_Controller.GetDevice(deviceType).GetNoloButtonUp(NoloButtonID.TouchPad))
        {
            if (pointerContactTarget != null)
            {
                GameObject.FindObjectsOfType<NoloVR_Manager>()[0].transform.position = downPosition -
                    new Vector3(NoloVR_Controller.GetDevice(NoloDeviceType.Hmd).GetPose().pos.x, 0, NoloVR_Controller.GetDevice(NoloDeviceType.Hmd).GetPose().pos.z);
            }
            PointerActivate(false);
        }
    }

    private void InitPointer()
    {
        pointerCursor = (customPointerCursor ? Instantiate(customPointerCursor) : CreateCursor());
        pointerCursor.name = "PointerCursor";
        pointerCursor.layer = LayerMask.NameToLayer("Ignore Raycast");
        pointerCursor.SetActive(false);

        curvedBeamContainer = new GameObject("CurvedBeamContainer");
        curvedBeam = curvedBeamContainer.gameObject.AddComponent<CurveGenerator>();
        curvedBeamContainer.SetActive(false);
        curvedBeam.transform.parent = null;
        curvedBeam.Create(pointerDensity, pointerCursorRadius, customPointerTracer, rescalePointerTracer);

        pointerCursor.SetActive(false);
    }

    private void PointerActivate(bool state)
    {
        pointerCursor.SetActive(state);
        curvedBeamContainer.SetActive(state);
    }

    private void UpdatePointerMaterial(Color color)
    {
        var pointerRenderer = pointerCursor.GetComponent<Renderer>();
        pointerRenderer.material.color = color;
        foreach (GameObject item in curvedBeam.items)
        {
            var itemRenderer = item.GetComponent<Renderer>();
            itemRenderer.material.color = color;
        }
    }

    private GameObject CreateCursor()
    {
        var cursorYOffset = 0.02f;
        var cursor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        var cursorRenderer = cursor.GetComponent<MeshRenderer>();

        cursor.transform.localScale = new Vector3(pointerCursorRadius, cursorYOffset, pointerCursorRadius);
        cursorRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        cursorRenderer.receiveShadows = false;
        if (pointerMaterial != null)
        {
            cursorRenderer.material = pointerMaterial;
        }
        Destroy(cursor.GetComponent<CapsuleCollider>());
        return cursor;
    }

    private Vector3 ProjectForwardBeam()
    {
        var controllerRotation = Vector3.Dot(Vector3.up, transform.forward.normalized);
        var calculatedLength = pointerLength;
        var useForward = transform.forward;
        if ((controllerRotation * 100f) > beamHeightLimitAngle)
        {
            useForward = new Vector3(transform.forward.x, fixedForwardBeamForward.y, transform.forward.z);
            var controllerRotationOffset = 1f - (controllerRotation - (beamHeightLimitAngle / 100f));
            calculatedLength = (pointerLength * controllerRotationOffset) * controllerRotationOffset;
        }
        else
        {
            fixedForwardBeamForward = transform.forward;
        }

        var actualLength = calculatedLength;
        Ray pointerRaycast = new Ray(transform.position, useForward);

        RaycastHit collidedWith;
        var hasRayHit = Physics.Raycast(pointerRaycast, out collidedWith, calculatedLength, ~layersToIgnore);

        //reset if beam not hitting or hitting new target
        if (!hasRayHit || (pointerContactRaycastHit.collider && pointerContactRaycastHit.collider != collidedWith.collider))
        {
            pointerContactDistance = 0f;
        }

        //check if beam has hit a new target
        if (hasRayHit)
        {
            pointerContactDistance = collidedWith.distance;
        }

        //adjust beam length if something is blocking it
        if (hasRayHit && pointerContactDistance < calculatedLength)
        {
            actualLength = pointerContactDistance;
        }

        //Use BEAM_ADJUST_OFFSET to move point back and up a bit to prevent beam clipping at collision point
        return (pointerRaycast.GetPoint(actualLength - BEAM_ADJUST_OFFSET) + (Vector3.up * BEAM_ADJUST_OFFSET));
    }

    private Vector3 ProjectDownBeam(Vector3 jointPosition)
    {
        Ray projectedBeamDownRaycast = new Ray(jointPosition, Vector3.down);
        RaycastHit collidedWith;

        var downRayHit = Physics.Raycast(projectedBeamDownRaycast, out collidedWith, float.PositiveInfinity, ~layersToIgnore);

        if (!downRayHit || (pointerContactRaycastHit.collider && pointerContactRaycastHit.collider != collidedWith.collider))
        {
            if (pointerContactRaycastHit.collider != null)
            {
                PointerOut();
            }
            pointerContactTarget = null;
            pointerContactRaycastHit = new RaycastHit();
            contactNormal = Vector3.zero;
            destinationPosition = projectedBeamDownRaycast.GetPoint(0f);
        }

        if (downRayHit)
        {
            pointerContactTarget = collidedWith.transform;
            pointerContactRaycastHit = collidedWith;
            contactNormal = collidedWith.normal;
            destinationPosition = projectedBeamDownRaycast.GetPoint(collidedWith.distance);
            PointerIn();
        }
        return destinationPosition;
    }

    private void DisplayCurvedBeam(Vector3 jointPosition, Vector3 downPosition)
    {
        Vector3[] beamPoints = new Vector3[]
        {
                transform.position,
                jointPosition + new Vector3(0f, beamCurveOffset, 0f),
                downPosition,
                downPosition,
        };
        var tracerMaterial = (customPointerTracer ? null : pointerMaterial);
        curvedBeam.SetPoints(beamPoints, tracerMaterial, currentPointerColor);
    }

    private void SetPointerCursor(Vector3 downPosition)
    {
        if (pointerContactTarget != null)
        {
            pointerCursor.transform.position = downPosition;
            pointerCursor.transform.rotation = Quaternion.FromToRotation(Vector3.up, contactNormal);
            UpdatePointerMaterial(pointerHitColor);
        }
        else
        {
            PointerActivate(false);
            UpdatePointerMaterial(pointerMissColor);
        }
    }

    private void PointerOut() { }

    private void PointerIn() { }
}

