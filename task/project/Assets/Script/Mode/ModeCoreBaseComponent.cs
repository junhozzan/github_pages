using System;

namespace ModeComponent
{
    public abstract class ModeCoreBaseComponent : ModeBaseComponent
    {
        public ModeCoreBaseComponent(Mode mode) : base(null, mode)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }
    }
}
