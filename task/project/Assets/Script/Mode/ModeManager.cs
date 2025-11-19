using System.Collections.Generic;

public class ModeManager : Singleton<ModeManager>
{
    private readonly Dictionary<int, Mode> modes = new();
    public Mode mode { get; private set; } = null;

    public void Clear()
    {
        mode?.OnDisable();
        mode = null;
    }

    public void Enter(int modeID)
    {
        var prevMode = mode;
        if (!TryGetMode(modeID, out var nextMode))
        {
            return;
        }

        prevMode?.OnDisable();

        mode = nextMode;
        mode.OnEnable();
    }

    public bool TryGetMode(int modeID, out Mode mode)
    {
        if (!modes.TryGetValue(modeID, out mode))
        {
            var dataMode = DataManager.Instance.mode.GetMode(modeID);
            if (dataMode == null)
            {
                return false;
            }

            mode = new Mode(dataMode);
            mode.Initialize();
            modes.Add(modeID, mode);
        }

        return true;
    }

    public void UpdateDt(float dt)
    {
        mode?.UpdateDt(dt);
    }
}
