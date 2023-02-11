using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript

{
    abstract class Module
    {
        public DebugAPI Debug { get; set; }
        public GridInfo GridInfo { get; set; }
        public ILogger Logger { get; set; }
        public IMessageBroker MessageBroker { get; set; }
        public IStorage Storage { get; set; }
        public virtual Func<IMyTerminalBlock, bool> BlockFilter { get; } = b => true;

        public virtual void Awake(IEnumerable<IMyTerminalBlock> blocks){}
        public virtual void Start(){}
        public virtual void Tick(double dt, IEnumerable<IMyTerminalBlock> blocks){}
        public virtual void Tick(double dt){}
    }
}
