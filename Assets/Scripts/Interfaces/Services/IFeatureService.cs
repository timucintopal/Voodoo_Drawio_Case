using System;

public enum GameFeature
{
    BoosterMode,
    SkinSelection
}

public interface IFeatureService
{
    event Action<GameFeature> onFeatureChanged;
    bool m_IsDebugMenuEnabled { get; }
    bool IsEnabled(GameFeature _Feature);
    void SetEnabled(GameFeature _Feature, bool _Enabled);
}
