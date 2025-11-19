namespace ModeComponent
{
    public abstract class ModeBaseComponent : XComponent
    {
        protected readonly Mode mode = null;

        public ModeBaseComponent(XComponent parent, Mode mode) : base(parent)
        {
            this.mode = mode;
        }
    }
}
