namespace ModeComponent
{
    public class LobbyModeUIComponent : ModeBaseComponent
    {
        private UILobby uiLobby = null;

        public LobbyModeUIComponent(XComponent parent, Mode mode) : base(parent, mode)
        {
        }

        public override void OnEnable()
        {
            base.OnEnable();
            uiLobby = UIManager.Instance.GetUI<UILobby>("pf_ui_lobby").On();
            uiLobby.BindOnPlay(PlayBubbleMode);
        }

        public override void OnDisable()
        {
            uiLobby?.Off();
            uiLobby = null;
            base.OnDisable();
        }

        private void PlayBubbleMode()
        {
            ModeManager.Instance.Enter(SagaGameData.Instance.bubbleModeID);
        }
    }
}
