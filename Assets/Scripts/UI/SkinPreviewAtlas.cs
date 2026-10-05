using System.Collections.Generic;
using UnityEngine;

// Renders all the skin previews into a single texture
public class SkinPreviewAtlas : MonoBehaviour
{
    private const int       c_DepthBits = 16;
    private const int       c_AntiAliasing = 2;
    private const int       c_MaxCellPixelSize = 512;
    private const float     c_CellWorldSize = 4f;

    public Camera           m_Camera;
    public Transform        m_PreviewsParent;
    public int              m_Columns = 3;
    [Range(0.1f, 1f)]
    public float            m_FillPercent = 0.95f;
    public Vector3          m_PreviewEuler = new Vector3(0f, 0f, -20f);

    public RenderTexture    texture { get { return m_Texture; } }

    private RenderTexture   m_Texture;
    private int             m_Rows;

    void Awake()
    {
        m_Camera.enabled = false;
        m_PreviewsParent.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        ReleaseTexture();
    }

    public void Build(List<SkinData> _Skins)
    {
        m_Rows = Mathf.CeilToInt(_Skins.Count / (float)m_Columns);
        m_Camera.orthographicSize = m_Rows * c_CellWorldSize * 0.5f;

        // Bounds need active objects
        m_PreviewsParent.gameObject.SetActive(true);

        for (int i = 0; i < _Skins.Count; ++i)
            CreatePreview(_Skins[i], m_PreviewsParent, GetCellCenter(i), c_CellWorldSize * m_FillPercent);

        m_PreviewsParent.gameObject.SetActive(false);
    }

    public void Show(int _CellPixelSize)
    {
        int cellPixelSize = Mathf.Min(_CellPixelSize, c_MaxCellPixelSize);

        ReleaseTexture();

        m_Texture = new RenderTexture(m_Columns * cellPixelSize, m_Rows * cellPixelSize, c_DepthBits, RenderTextureFormat.ARGB32);
        m_Texture.name = "SkinPreviewAtlas";
        m_Texture.antiAliasing = c_AntiAliasing;

        m_Camera.targetTexture = m_Texture;
        m_Camera.allowMSAA = true;
        m_Camera.enabled = true;
        m_PreviewsParent.gameObject.SetActive(true);
    }

    public void Hide()
    {
        m_Camera.enabled = false;
        m_PreviewsParent.gameObject.SetActive(false);
        ReleaseTexture();
    }

    public Rect GetUVRect(int _Index)
    {
        int column = _Index % m_Columns;
        int row = _Index / m_Columns;
        float width = 1f / m_Columns;
        float height = 1f / m_Rows;

        // UVs start from the bottom
        return new Rect(column * width, 1f - (row + 1) * height, width, height);
    }

    // Also used for the selected skin, outside of the atlas
    public GameObject CreatePreview(SkinData _Skin, Transform _Parent, Vector3 _Center, float _Size)
    {
        GameObject preview = Instantiate(_Skin.Brush.m_PreviewPrefab, _Parent);
        preview.transform.localRotation = Quaternion.Euler(m_PreviewEuler);

        foreach (Transform child in preview.GetComponentsInChildren<Transform>(true))
            child.gameObject.layer = _Parent.gameObject.layer;

        Fit(preview.transform, _Center, _Size);
        preview.GetComponent<BrushMenu>().SetNewColor(_Skin.Color.m_Colors[0]);

        return preview;
    }

    // Scaled to the wanted height, then centered
    private void Fit(Transform _Preview, Vector3 _Center, float _Size)
    {
        _Preview.localScale *= _Size / GetBounds(_Preview).size.y;
        _Preview.position += _Center - GetBounds(_Preview).center;
    }

    private Bounds GetBounds(Transform _Preview)
    {
        Renderer[] renderers = _Preview.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; ++i)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }

    private Vector3 GetCellCenter(int _Index)
    {
        int column = _Index % m_Columns;
        int row = _Index / m_Columns;
        float x = (column - (m_Columns - 1) * 0.5f) * c_CellWorldSize;
        float y = ((m_Rows - 1) * 0.5f - row) * c_CellWorldSize;

        return m_PreviewsParent.TransformPoint(new Vector3(x, y, 0f));
    }

    private void ReleaseTexture()
    {
        if (m_Texture == null)
            return;

        m_Camera.targetTexture = null;
        m_Texture.Release();
        Destroy(m_Texture);
        m_Texture = null;
    }
}
