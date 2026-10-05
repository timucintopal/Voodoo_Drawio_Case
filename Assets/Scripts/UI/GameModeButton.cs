using UnityEngine;
using TMPro;
using Zenject;

public class GameModeButton : MonoBehaviour
{
	public GameModeData 	m_Mode;
	public GameFeature 		m_Feature = GameFeature.BoosterMode;
	public TMP_Text 		m_LevelText;

	private IGameModeService 	m_GameModeService;
	private IFeatureService 	m_FeatureService;

	[Inject]
	public void Construct(IGameModeService gameModeService, IFeatureService featureService)
	{
		m_GameModeService = gameModeService;
		m_FeatureService = featureService;
	}

	void Awake()
	{
		Refresh();
	}

	public void Refresh()
	{
		gameObject.SetActive(m_Mode != null && m_FeatureService.IsEnabled(m_Feature));
	}

	void Start()
	{
		if (m_LevelText != null)
			m_LevelText.text = "LEVEL " + m_GameModeService.GetLevel(m_Mode);
	}

	public void OnClick()
	{
		MainMenuView.Instance.OnPlayModeButton(m_Mode);
	}
}
