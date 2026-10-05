using System;
using UnityEngine;
using Zenject;

public class FeatureService : IFeatureService
{
    public event Action<GameFeature> onFeatureChanged;

    public bool m_IsDebugMenuEnabled => m_GameConfig.m_DebugMenuEnabled;

    private GameConfig m_GameConfig;

    [Inject]
    public void Construct(GameConfig gameConfig)
    {
        m_GameConfig = gameConfig;
    }

    public bool IsEnabled(GameFeature _Feature)
    {
        bool defaultValue = m_GameConfig.m_EnabledFeatures.Contains(_Feature);

        // No override without the debug menu
        if (m_IsDebugMenuEnabled == false)
            return defaultValue;

        return PlayerPrefs.GetInt(GetSaveKey(_Feature), defaultValue ? 1 : 0) == 1; // Converting int to bool
    }

    public void SetEnabled(GameFeature _Feature, bool _Enabled)
    {
        if (IsEnabled(_Feature) == _Enabled)
            return;

        PlayerPrefs.SetInt(GetSaveKey(_Feature), _Enabled ? 1 : 0); // Converting bool to int

        if (onFeatureChanged != null)
            onFeatureChanged.Invoke(_Feature);
    }

    private string GetSaveKey(GameFeature _Feature)
    {
        return Constants.c_FeatureSave + _Feature.ToString();
    }
}
