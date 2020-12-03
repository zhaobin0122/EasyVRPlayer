using UnityEngine;
using HVRCORE;
using UnityEngine.UI;

public class HVRLinePointer : MonoBehaviour
{
    private static readonly string TAG = "Unity_HVRLinePointer";
    [SerializeField]
    private GameObject m_Line;
    [SerializeField]
    private GameObject m_Anchor;
    private LineRenderer m_LineRenderer;

    private float m_MaxLineDistance = 100f;
    private float m_ObjUpDir = 0.018f;
    private float m_ObjForwardDir = 0.062f;
    private float m_SpotDistance = 0.92f;

    public static HVRLinePointer Instance;
    private bool m_IsPointerIntersecting;
    private Vector3 m_PointerIntersection;
    private float m_Distance;

    private static bool m_IsAlternately = true;

    public ControllerIndex controllerIndex = 0;

    private Material m_ResourcesCursorMaterial, m_ResourcesSpotMaterial;
    private MeshRenderer m_MeshRenderer;
    
    private bool m_InitOnce = true;
    public void OnLineEnter(Vector3 intersectionPosition, bool isInteractive)
    {
        this.m_PointerIntersection = intersectionPosition;
        this.m_IsPointerIntersecting = isInteractive;

    }
    public void OnLineExit(Vector3 intersectionPosition, bool isInteractive)
    {
        this.m_PointerIntersection = intersectionPosition;
        this.m_IsPointerIntersecting = isInteractive;

    }
    public void OnLineHover(Vector3 intersectionPosition, bool isInteractive)
    {
        this.m_PointerIntersection = intersectionPosition;
        this.m_IsPointerIntersecting = isInteractive;
    }

    public void ShowCircle(bool isTrue)
    {
        m_IsAlternately = isTrue;
    }

    private void Awake()
    {
        m_Distance = m_MaxLineDistance;
        Instance = this;
        if (this.m_Line != null)
        {
            this.m_LineRenderer = this.m_Line.GetComponent<LineRenderer>();
        }
        m_MeshRenderer = this.m_Anchor.GetComponent<MeshRenderer>();
        m_ResourcesCursorMaterial = Resources.Load<Material>("Materials/controller_cursor");
        m_ResourcesSpotMaterial = Resources.Load<Material>("Materials/controller_spot");
    }

    public void OnInit()
    {
        if (this.m_Anchor != null)
        {
            this.m_Anchor.transform.position = this.transform.position +
                (this.transform.forward * this.m_MaxLineDistance) + this.transform.up * m_ObjUpDir;
        }
        this.m_LineRenderer.SetPosition(0, transform.parent.position + transform.up * m_ObjUpDir);
        this.m_LineRenderer.SetPosition(1, this.transform.position + 
            (this.transform.forward * this.m_MaxLineDistance) + this.transform.up * m_ObjUpDir);
    }
    private void Update()
    {
        if (m_InitOnce) {
            m_InitOnce = false;
            OnInit();
        }
    }
    /// <summary>
    /// Lates the update.
    /// </summary>
    private void LateUpdate()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            this.m_Anchor.transform.localScale = 0.15f * Vector3.one;
            this.m_LineRenderer.SetPosition(0, transform.parent.position + this.transform.up * m_ObjUpDir * HVRController.m_Radio + this.transform.forward * m_ObjForwardDir * HVRController.m_Radio);
            this.m_LineRenderer.SetPosition(1, transform.position + (transform.forward * this.m_MaxLineDistance) + this.transform.up * m_ObjUpDir);
        }
        if (Application.platform == RuntimePlatform.Android)
        {
            if (m_IsAlternately)
            {
                m_MeshRenderer.material = m_ResourcesCursorMaterial;

                this.m_Anchor.transform.localScale = 0.24f * Vector3.one;
            }
            else
            {
                m_MeshRenderer.material = m_ResourcesSpotMaterial;
                this.m_Anchor.transform.localScale = 0.1f * Vector3.one;
            }
            this.m_LineRenderer.SetPosition(0, transform.position +
                this.transform.up * m_ObjUpDir * HVRController.m_Radio + 
                this.transform.forward * m_ObjForwardDir * HVRController.m_Radio);
            Vector3 lineEndPoint = this.transform.position + 
                (this.transform.forward * this.m_MaxLineDistance) + this.transform.up * m_ObjUpDir;

            if (this.m_IsPointerIntersecting && 
                Vector3.Distance(transform.position, this.m_PointerIntersection) < this.m_MaxLineDistance)
            {
                this.m_Anchor.transform.position = this.m_PointerIntersection + this.transform.up * m_ObjUpDir;
                lineEndPoint = this.m_PointerIntersection * m_SpotDistance +
                        transform.position * (1 - m_SpotDistance) + this.transform.up * m_ObjUpDir;
            }
            else
            {
                this.m_Anchor.transform.position = transform.position + 
                    (transform.forward * this.m_MaxLineDistance) + this.transform.up * m_ObjUpDir;
                lineEndPoint = transform.position + 
                    (transform.forward * this.m_MaxLineDistance) + this.transform.up * m_ObjUpDir;
            }

            this.m_LineRenderer.SetPosition(1, lineEndPoint);
        }
    }
}
