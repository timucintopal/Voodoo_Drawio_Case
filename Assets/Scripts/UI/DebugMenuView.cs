using System.Collections.Generic;
using UnityEngine.UI;
using Zenject;

public class DebugMenuView : View<DebugMenuView>
{
    public List<DebugFeatureRow> m_Rows;
    public Button               m_DisableAllButton;

    private IFeatureService     m_FeatureService;

    [Inject]
    public void Construct(IFeatureService featureService)
    {
        m_FeatureService = featureService;
    }

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < m_Rows.Count; ++i)
            m_Rows[i].Init(OnRowClicked);
    }

    public void Open()
    {
        if (GameService.currentPhase != GamePhase.MAIN_MENU || m_Visible)
            return;

        Refresh();

        // 3D, not hidden by the view
        MainMenuView.Instance.m_BrushesPrefab.SetActive(false);
        MainMenuView.Instance.Transition(false);
        Transition(true);
    }

    public void Close()
    {
        if (m_Visible == false)
            return;

        Transition(false);

        MainMenuView.Instance.RefreshFeatures();
        GameService.SetColor(GameService.ComputeCurrentPlayerColor(true, 0));
        MainMenuView.Instance.Transition(true);
    }

    public void OnDisableAllButton()
    {
        for (int i = 0; i < m_Rows.Count; ++i)
            m_FeatureService.SetEnabled(m_Rows[i].m_Feature, false);

        Refresh();
    }

    private void OnRowClicked(DebugFeatureRow _Row)
    {
        m_FeatureService.SetEnabled(_Row.m_Feature, m_FeatureService.IsEnabled(_Row.m_Feature) == false);
        Refresh();

        MobileHapticManager.Instance.Vibrate(MobileHapticManager.E_FeedBackType.SelectionChange);
    }

    private void Refresh()
    {
        int enabledCount = 0;

        for (int i = 0; i < m_Rows.Count; ++i)
        {
            bool enabled = m_FeatureService.IsEnabled(m_Rows[i].m_Feature);
            m_Rows[i].SetState(enabled);

            if (enabled)
                ++enabledCount;
        }

        bool isOriginal = enabledCount == 0;
        m_DisableAllButton.interactable = isOriginal == false;
    }
}
