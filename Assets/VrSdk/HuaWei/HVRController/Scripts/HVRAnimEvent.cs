using UnityEngine;

public class HVRAnimEvent : MonoBehaviour
{

    private static readonly string TAG = "Unity_HVRAnimEvent";
    private MeshRenderer m_MeshRenderer;
    private Material m_ResourcesIncMaterial, m_ResourcesDecMaterial;
    private Material m_color;
    private void Awake()
    {
        m_MeshRenderer = GetComponent<MeshRenderer>();
        m_color = m_MeshRenderer.material;
        m_ResourcesIncMaterial = Resources.Load<Material>("Materials/Inc");
        m_ResourcesDecMaterial = Resources.Load<Material>("Materials/Dec");
    }

    private void ChangColor()
    {

        m_MeshRenderer.material = m_ResourcesIncMaterial;
    }
    private void ChangIncColor()
    {
        m_MeshRenderer.material = m_ResourcesIncMaterial;
    }
    private void ChangDecColor()
    {
        m_MeshRenderer.material = m_ResourcesDecMaterial;
    }

    private void BackColor()
    {
        m_MeshRenderer.material = m_color;
    }
}
