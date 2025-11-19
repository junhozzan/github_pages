using System.Collections.Generic;
using System.Linq;

public class DataLoaderSaga : DataLoaderBase
{
    private Dictionary<int, DataSagaBubble> bubbles = null;

    public override void Load()
    {
        bubbles = LoadResources(x => new DataSagaBubble(x), "data_saga_bubble").ToDictionary(x => x.id, x => x);
    }

    public DataSagaBubble GetBubble(int id)
    {
        return bubbles.TryGetValue(id, out var v) ? v : null;
    }
}
