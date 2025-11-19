namespace ModeFeature
{
    public class SagaWorld : ModeFeatureBase
    {
        public readonly string cellSo;
        public readonly string mapPrefab;

        public SagaWorld(TXMLElement e) : base(e)
        {
            this.cellSo = e.GetChildText("CellSO");
            this.mapPrefab = e.GetChildText("MapPrefab");
        }
    }
}
