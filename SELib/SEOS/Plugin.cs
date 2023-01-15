using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    abstract class Plugin
    {
        protected ILogger Logger;

        public virtual void Init(SEOS os)
        {
            Logger = os.Logger;
        }
        public virtual void Tick(double dt)
        { }
        public virtual void Message(string argument, UpdateType updateSource)
        { }
        public virtual void Save()
        { }
    }
}