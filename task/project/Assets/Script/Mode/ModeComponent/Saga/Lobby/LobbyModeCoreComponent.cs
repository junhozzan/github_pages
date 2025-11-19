namespace ModeComponent
{
    public class LobbyModeCoreComponent : ModeCoreBaseComponent
    {
        public LobbyModeCoreComponent(Mode mode) : base(mode)
        {
            AddComponent<LobbyModeUIComponent>(mode);
        }
    }
}
