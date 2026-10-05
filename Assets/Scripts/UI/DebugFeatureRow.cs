using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugFeatureRow : MonoBehaviour
{
    private const string c_OnLabel = "ON";
    private const string c_OffLabel = "OFF";

    public GameFeature  m_Feature;
    public Button       m_Button;
    public Image        m_StateBackground;
    public TextMeshProUGUI         m_StateText;
    public Color        m_OnColor = Color.green;
    public Color        m_OffColor = Color.gray;

    private Action<DebugFeatureRow> m_OnClick;

    void Awake()
    {
        m_Button.onClick.AddListener(OnClick);
    }

    public void Init(Action<DebugFeatureRow> _OnClick)
    {
        m_OnClick = _OnClick;
    }

    public void SetState(bool _Enabled)
    {
        m_StateText.text = _Enabled ? c_OnLabel : c_OffLabel;
        m_StateBackground.color = _Enabled ? m_OnColor : m_OffColor;
    }

    private void OnClick()
    {
        if (m_OnClick != null)
            m_OnClick.Invoke(this);
    }
}
