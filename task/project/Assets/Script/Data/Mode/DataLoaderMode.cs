using System.Collections.Generic;
using System.Linq;

public class DataLoaderMode : DataLoaderBase
{
    private Dictionary<int, DataMode> modes = null;

    public override void Load()
    {
        modes = LoadResources(x => new DataMode(x), "data_mode").ToDictionary(x => x.id, x => x);
    }

    public DataMode GetMode(int id)
    {
        return modes.TryGetValue(id, out var v) ? v : null;
    }
}
