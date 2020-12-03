using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HVRLeftGraphicRaycaster : HVRRaycasterBase
{
    public enum BlockingObjects
    {
        None = 0,
        TwoD = 1,
        ThreeD = 2,
        All = 3
    }

    private const int NO_EVENT_MASK_SET = -1;

    private static readonly List<Graphic> sortedGraphics = new List<Graphic>();
    private readonly List<Graphic> raycastResults = new List<Graphic>();
    public LayerMask blockingMask = NO_EVENT_MASK_SET;
    private BlockingObjects blockingObjs = BlockingObjects.None;

    private bool ignoreReversedGraphics = true;

    private Canvas targetCanvas;
    private Camera myCamera;
    protected HVRLeftGraphicRaycaster()
    {
    }

    public override Camera eventCamera
    {
        get
        {
            if (myCamera == null)
            {
                myCamera = HVRController.m_LeftEventCamera.GetComponent<Camera>();
            }
            return myCamera;
        }
    }

    private Canvas canvas
    {
        get
        {
            if (targetCanvas != null)
                return targetCanvas;

            targetCanvas = GetComponent<Canvas>();
            return targetCanvas;
        }
    }

    public bool IgnoreReversedGraphics
    {
        get { return ignoreReversedGraphics; }

        set { ignoreReversedGraphics = value; }
    }

    public BlockingObjects BlockingObjs
    {
        get { return blockingObjs; }

        set { blockingObjs = value; }
    }

    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        if (canvas == null)
        {
            return;
        }

        if (eventCamera == null)
        {
            return;
        }

        if (canvas.renderMode != RenderMode.WorldSpace)
        {
            return;
        }

        var ray = GetRay();
        var hitDistance = float.MaxValue;

        if (BlockingObjs != BlockingObjects.None)
        {
            var dist = eventCamera.farClipPlane - eventCamera.nearClipPlane;

            if (BlockingObjs == BlockingObjects.ThreeD || BlockingObjs == BlockingObjects.All)
            {
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, dist, blockingMask))
                {
                    hitDistance = hit.distance;
                }
            }

            if (BlockingObjs == BlockingObjects.TwoD || BlockingObjs == BlockingObjects.All)
            {
                var hit = Physics2D.Raycast(ray.origin, ray.direction, dist, blockingMask);

                if (hit.collider != null)
                {
                    hitDistance = hit.fraction * dist;
                }
            }
        }

        raycastResults.Clear();
        Ray finalRay;
        Raycast(canvas, ray, eventCamera, maxPointerDistance, raycastResults, out finalRay);

        for (var index = 0; index < raycastResults.Count; index++)
        {
            var go = raycastResults[index].gameObject;
            var appendGraphic = true;

            if (IgnoreReversedGraphics)
            {
                var cameraFoward = eventCamera.transform.rotation * Vector3.forward;
                var dir = go.transform.rotation * Vector3.forward;
                appendGraphic = Vector3.Dot(cameraFoward, dir) > 0;
            }

            if (appendGraphic)
            {
                float distance = 0;

                var trans = go.transform;
                var transForward = trans.forward;
                distance = Vector3.Dot(transForward, trans.position - finalRay.origin) /
                           Vector3.Dot(transForward, finalRay.direction);

                if (distance < 0)
                {
                    continue;
                }
                if (distance >= hitDistance)
                {
                    continue;
                }
                var castResult = new RaycastResult
                {
                    gameObject = go,
                    module = this,
                    distance = distance,
                    worldPosition = finalRay.origin + finalRay.direction * distance,
                    screenPosition = eventData.position,
                    index = resultAppendList.Count,
                    depth = raycastResults[index].depth,
                    sortingLayer = canvas.sortingLayerID,
                    sortingOrder = canvas.sortingOrder
                };
                resultAppendList.Add(castResult);
            }
        }
    }

    private static void Raycast(Canvas canvas, Ray ray, Camera cam, float maxPointerDistance,
        List<Graphic> results, out Ray finalRay)
    {
        var screenPoint = cam.WorldToScreenPoint(ray.GetPoint(maxPointerDistance));
        finalRay = cam.ScreenPointToRay(screenPoint);

        var foundGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
        for (var i = 0; i < foundGraphics.Count; ++i)
        {
            var graphic = foundGraphics[i];

            if (graphic.depth == -1 || !graphic.raycastTarget)
            {
                continue;
            }

            if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, screenPoint, cam))
            {
                continue;
            }

            if (graphic.Raycast(screenPoint, cam))
            {
                sortedGraphics.Add(graphic);
            }
        }

        sortedGraphics.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));

        for (var i = 0; i < sortedGraphics.Count; ++i)
        {
            results.Add(sortedGraphics[i]);
        }

        sortedGraphics.Clear();
    }
}