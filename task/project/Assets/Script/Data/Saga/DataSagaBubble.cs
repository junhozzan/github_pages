using System.Collections.Generic;
using System.Linq;

public class DataSagaBubble : DataBase
{
    public readonly string prefab;
    public readonly string atlas;
    public readonly string sprite;
    public readonly HashSet<int> matchIDs;
    public readonly int minMatchCount;
    public readonly bool isMine;
    public readonly string explosionSO;
    public readonly string popEffect;
    public readonly int damage;

    public DataSagaBubble(SmartXmlNode e) : base(e)
    {
        this.prefab = e.GetChildText("Prefab");
        this.atlas = e.GetChildText("Atlas");
        this.sprite = e.GetChildText("Sprite");
        this.matchIDs = e.GetChildren("MatchID").Select(x => int.Parse(x.text)).ToHashSet();
        this.isMine = e.GetChildBoolean("IsMine", false);
        this.minMatchCount = e.GetChildInt("MinMatchCount");
        this.explosionSO = e.GetChildText("ExplosionSO");

        this.popEffect = e.GetChildText("PopEffect");
        this.damage = e.GetChildInt("Damage");
    }
}
