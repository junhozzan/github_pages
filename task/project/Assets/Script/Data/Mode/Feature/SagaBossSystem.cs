namespace ModeFeature
{
    public class SagaBossSystem : ModeFeatureBase
    {
        public readonly string bossPrefab;
        public readonly int bossHp;
        public readonly int limitShot;

        public SagaBossSystem(TXMLElement e) : base(e)
        {
            this.bossPrefab = e.GetChildText("BossPrefab");
            this.bossHp = e.GetChildInt("BossHP");
            this.limitShot = e.GetChildInt("LimitShot");
        }
    }
}
