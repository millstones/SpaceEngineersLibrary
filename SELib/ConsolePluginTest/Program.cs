using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        const string GRID_NAME = "GUZUN OS CONSOLE TEST";
        readonly SEOS _os;
        public Program()
        {
            _os = new SEOS(this, GlobalConst.GRID_GROUP_STATION, GRID_NAME)
                .AddPlugin(new TaskPlugin())
                .AddPlugin(new ConsolePlugin())
                //.AddConsoleSite(new TestConsole())
                .Build(new DefaultLogger(this))
                ;
        }

        public void Save()
        {
            _os.Save();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            _os.Message(argument, updateSource);
            _os.Tick();
        }
    }
}
