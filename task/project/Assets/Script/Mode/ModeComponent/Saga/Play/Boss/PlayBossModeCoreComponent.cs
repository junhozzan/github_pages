namespace ModeComponent
{
    public class PlayBossModeCoreComponent : ModeCoreBaseComponent
    {
        public PlayBossModeCoreComponent(Mode mode) : base(mode)
        {
            AddComponent<ModeCameraComponent>(mode);
            AddComponent<ModeInputComponent>(mode);
            AddComponent<PlayModeWorldComponent>(mode);
            AddComponent<PlayModeSpawnerComponent>(mode);
            AddComponent<PlayModeControlComponent>(mode);
            AddComponent<PlayBossModeUIComponent>(mode);
            AddComponent<PlayBossModeMainComponent>(mode);
        }
    }
}
