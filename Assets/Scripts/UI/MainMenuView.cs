using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MainMenuView : View<MainMenuView>
{
    private const string m_BestScorePrefix = "BEST SCORE ";

    public Text m_BestScoreText;
    public Image m_BestScoreBar;
    public GameObject m_BestScoreObject;
    public InputField m_InputField;
    public List<Image> m_ColoredImages;
    public List<Text> m_ColoredTexts;

    public GameObject m_BrushGroundLight;
    public GameObject m_BrushesPrefab;
    public GameObject m_BrushSelect;
    public GameObject m_SkinSelectionButton;
    public GameObject m_DebugButton;
    public int m_IdSkin = 0;
    public GameObject m_PointsPerRank;
    public RankingView m_RankingView;

    [Header("Ranks")]
    public string[] m_Ratings;

    private IStatsService m_StatsService;
    private IGameModeService m_GameModeService;
    private IFeatureService m_FeatureService;

    [Inject]
    public void Construct(IStatsService statsService, IGameModeService gameModeService, IFeatureService featureService)
    {
        m_StatsService = statsService;
        m_GameModeService = gameModeService;
        m_FeatureService = featureService;
    }

    protected override void Awake()
    {
        base.Awake();

        RefreshFeatures();
    }

    public void RefreshFeatures()
    {
        bool skinSelectionEnabled = m_FeatureService.IsEnabled(GameFeature.SkinSelection);

        m_BrushSelect.SetActive(skinSelectionEnabled == false);
        m_SkinSelectionButton.SetActive(skinSelectionEnabled);

        if (skinSelectionEnabled)
            m_BrushesPrefab.SetActive(false);

        m_IdSkin = Mathf.Min(m_StatsService.FavoriteSkin, GameService.m_Skins.Count - 1);

        foreach (GameModeButton modeButton in GetComponentsInChildren<GameModeButton>(true))
            modeButton.Refresh();

        if (m_DebugButton != null)
            m_DebugButton.SetActive(m_FeatureService.m_IsDebugMenuEnabled);
    }

    public void OnPlayButton()
    {
        if (GameService.currentPhase == GamePhase.MAIN_MENU)
            GameService.ChangePhase(GamePhase.LOADING);
    }

    public void OnSkinSelectionButton()
    {
        if (GameService.currentPhase == GamePhase.MAIN_MENU)
            SkinSelectionView.Instance.Open();
    }

    public void OnDebugButton()
    {
        if (GameService.currentPhase == GamePhase.MAIN_MENU)
            DebugMenuView.Instance.Open();
    }

    public void OnPlayModeButton(GameModeData _Mode)
    {
        if (_Mode == null)
            return;

        if (GameService.currentPhase != GamePhase.MAIN_MENU)
            return;

        m_GameModeService.SetMode(_Mode);
        GameService.ChangePhase(GamePhase.LOADING);
    }

    protected override void OnGamePhaseChanged(GamePhase _GamePhase)
    {
        base.OnGamePhaseChanged(_GamePhase);

        switch (_GamePhase)
        {
            case GamePhase.MAIN_MENU:
                m_BrushGroundLight.SetActive(true);
                Transition(true);
                break;

            case GamePhase.LOADING:
                m_BrushGroundLight.SetActive(false);

                    m_BrushesPrefab.SetActive(false);

                if (m_Visible)
                    Transition(false);
                break;
        }
    }

    public void SetTitleColor(Color _Color)
    {
        if (m_FeatureService.IsEnabled(GameFeature.SkinSelection) == false)
        {
            m_BrushesPrefab.SetActive(true);
            int favoriteSkin = Mathf.Min(m_StatsService.FavoriteSkin, GameService.m_Skins.Count - 1);
            m_BrushesPrefab.GetComponent<BrushMainMenu>().Set(GameService.m_Skins[favoriteSkin]);
        }

        string playerName = m_StatsService.GetNickname();

        if (playerName != null)
            m_InputField.text = playerName;

        for (int i = 0; i < m_ColoredImages.Count; ++i)
            m_ColoredImages[i].color = _Color;

        for (int i = 0; i < m_ColoredTexts.Count; i++)
            m_ColoredTexts[i].color = _Color;
            
        m_RankingView.gameObject.SetActive(true);
        m_RankingView.RefreshNormal();
    }

    public void OnSetPlayerName(string _Name)
    {
        m_StatsService.SetNickname(_Name);
    }

    public string GetRanking(int _Rank)
    {
        return m_Ratings[_Rank];
    }

    public int GetRankingCount()
    {
        return m_Ratings.Length;
    }

    public void LeftButtonBrush()
    {
        ChangeBrush(m_IdSkin - 1);
    }

    public void RightButtonBrush()
    {
        ChangeBrush(m_IdSkin + 1);
    }

    public void ChangeBrush(int _NewBrush)
    {
        _NewBrush = Mathf.Clamp(_NewBrush, 0, GameService.m_Skins.Count);
        m_IdSkin = _NewBrush;
        if (m_IdSkin >= GameService.m_Skins.Count)
            m_IdSkin = 0;
        GameService.m_PlayerSkinID = m_IdSkin;
        m_StatsService.FavoriteSkin = m_IdSkin;

        GameService.SetColor(GameService.ComputeCurrentPlayerColor(true, 0));
    }
}
