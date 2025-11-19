namespace ModeComponent
{
    public class PlayBossModeUIComponent : ModeBaseComponent
    {
        private UIPlayBoss uiBoss = null;

        public PlayBossModeUIComponent(XComponent parent, Mode mode) : base(parent, mode)
        {

        }

        public override void OnEnable()
        {
            base.OnEnable();
            uiBoss = UIManager.Instance.GetUI<UIPlayBoss>("pf_ui_playboss").On();
        }

        public override void OnDisable()
        {
            uiBoss?.Off();
            uiBoss = null;
            base.OnDisable();
        }

        public void SetHpGauag(float rate)
        {
            uiBoss?.SetHpGauge(rate);
        }

        public void SetLimitCount(int count)
        {
            uiBoss?.SetLimitText(count.ToString());
        }

        public void ShowResult(string resultText)
        {
            var uiResult = UIManager.Instance.GetUI<UIResult>("pf_ui_result");
            uiResult.BindOnGoToLobby(GoToLobby);
            uiResult.BindOnEsc(GoToLobby);
            uiResult.SetResultText(resultText);
            uiResult.On();
        }

        private void GoToLobby()
        {
            ModeManager.Instance.Enter(SagaGameData.Instance.lobbyModeID);
        }
    }
}
