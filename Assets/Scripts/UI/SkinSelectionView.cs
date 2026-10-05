using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SkinSelectionView : View<SkinSelectionView>
{
    public SkinPreviewAtlas     m_Atlas;
    public SkinCell             m_CellPrefab;
    public GridLayoutGroup      m_Grid;
    public List<SkinData>       m_DisplayedSkins; // Display order

    [Header("Selected skin")]
    public GameObject           m_Stage; // 3D, not faded by the group
    public Transform            m_HeroParent;
    public float                m_HeroSize = 420f;
    public float                m_HeroPunchScale = 0.2f;
    public float                m_HeroPunchDuration = 0.3f;

    private IStatsService       m_StatsService;

    private List<SkinCell>      m_Cells = new List<SkinCell>();
    private GameObject          m_Hero;
    private int                 m_SelectedIndex = -1;
    private bool                m_IsBuilt;
    private bool                m_IsAtlasVisible;

    [Inject]
    public void Construct(IStatsService statsService)
    {
        m_StatsService = statsService;
    }

    protected override void Awake()
    {
        base.Awake();

        m_Stage.SetActive(false);
    }

    public void Open()
    {
        if (m_Visible)
            return;

        if (m_IsBuilt == false)
            Build();

        FitCellSize();
        ShowAtlas();

        m_Stage.SetActive(true);
        Select(GetFavoriteIndex());

        MainMenuView.Instance.Transition(false);
        Transition(true);
    }

    public void Close()
    {
        if (m_Visible == false)
            return;

        m_HeroParent.DOKill(true);
        m_Stage.SetActive(false);

        Transition(false);
        MainMenuView.Instance.Transition(true);
    }

    protected override void Update()
    {
        base.Update();

        if (m_IsAtlasVisible && m_Visible == false && m_Group.alpha <= 0f)
            HideAtlas();
    }

    #region List

    private void Build()
    {
        m_Atlas.Build(m_DisplayedSkins);

        m_Grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        m_Grid.constraintCount = m_Atlas.m_Columns;

        for (int i = 0; i < m_DisplayedSkins.Count; ++i)
        {
            SkinCell cell = Instantiate(m_CellPrefab, m_Grid.transform);
            cell.Init(OnCellClicked);
            m_Cells.Add(cell);
        }

        m_IsBuilt = true;
    }

    private void ShowAtlas()
    {
        float scaleFactor = m_Grid.GetComponentInParent<Canvas>().scaleFactor;
        m_Atlas.Show(Mathf.CeilToInt(m_Grid.cellSize.x * scaleFactor));
        m_IsAtlasVisible = true;

        for (int i = 0; i < m_Cells.Count; ++i)
            m_Cells[i].SetPreview(m_Atlas.texture, m_Atlas.GetUVRect(i));
    }

    private void HideAtlas()
    {
        m_Atlas.Hide();
        m_IsAtlasVisible = false;
    }

    private void FitCellSize()
    {
        int columns = m_Atlas.m_Columns;
        RectTransform gridRect = (RectTransform)m_Grid.transform;
        float width = gridRect.rect.width - m_Grid.padding.horizontal - m_Grid.spacing.x * (columns - 1);

        m_Grid.cellSize = Vector2.one * (width / columns);
    }

    #endregion

    #region Selection

    private void OnCellClicked(SkinCell _Cell)
    {
        int index = m_Cells.IndexOf(_Cell);
        if (index == m_SelectedIndex)
            return;

        SaveFavorite(m_DisplayedSkins[index]);
        Select(index);
        PlaySelectFeedback(_Cell);
    }

    private void PlaySelectFeedback(SkinCell _Cell)
    {
        _Cell.Punch();
        m_HeroParent.DOPunchScale(Vector3.one * m_HeroPunchScale, m_HeroPunchDuration, 0, 0);

        MobileHapticManager.Instance.Vibrate(MobileHapticManager.E_FeedBackType.SelectionChange);
    }

    private void Select(int _Index)
    {
        if (_Index < 0 || _Index >= m_DisplayedSkins.Count)
            return;

        m_SelectedIndex = _Index;

        for (int i = 0; i < m_Cells.Count; ++i)
            m_Cells[i].SetSelected(i == _Index);

        RefreshHero(m_DisplayedSkins[_Index]);
    }

    private void RefreshHero(SkinData _Skin)
    {
        // Bounds must not be scaled by the punch
        m_HeroParent.DOKill(true);

        if (m_Hero != null)
            Destroy(m_Hero);

        float worldSize = m_HeroSize * m_HeroParent.lossyScale.y;
        m_Hero = m_Atlas.CreatePreview(_Skin, m_HeroParent, m_HeroParent.position, worldSize);
    }

    private void SaveFavorite(SkinData _Skin)
    {
        int skinID = GameService.m_Skins.IndexOf(_Skin);

        m_StatsService.FavoriteSkin = skinID;
        GameService.SetColor(GameService.ComputeCurrentPlayerColor(true, 0));
    }

    private int GetFavoriteIndex()
    {
        int favoriteSkin = Mathf.Min(m_StatsService.FavoriteSkin, GameService.m_Skins.Count - 1);
        return Mathf.Max(0, m_DisplayedSkins.IndexOf(GameService.m_Skins[favoriteSkin]));
    }

    #endregion
}
