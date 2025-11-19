using ModeComponent;
using System;

public class Mode
{
    public readonly DataMode dataMode = null;
    public readonly ModeCoreBaseComponent core = null;

    public Mode(DataMode dataMode)
    {
        this.dataMode = dataMode;
        this.core = Activator.CreateInstance(Type.GetType(dataMode.core), this) as ModeCoreBaseComponent;
    }

    public void Initialize()
    {
        core.Initialize();
    }

    public void OnEnable()
    {
        core.OnEnable();
    }

    public void OnDisable()
    {
        core.OnDisable();
    }

    public void UpdateDt(float dt)
    {
        core.XUpdateDt(dt);
    }
}
