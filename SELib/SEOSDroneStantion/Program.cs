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
        const string GRID_NAME = "GUZUN drone stantion 1";
        readonly SEOS _os;

        public Program()
        {
            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocks(blocks);
            var surface = blocks.OfType<IMyTextSurface>().First();

            _os = new SEOS(this, GlobalConst.GRID_GROUP_STATION, GRID_NAME)
                    .UseSerializer<DroneActualStatus>()
                    .UseSerializer<StationActualStatus>()
                    .UseSerializer<GridInfo>()
                    .UseSerializer<Product>()
                    .AddPlugin(new TaskPlugin())
                    .AddModule<ProductController>(UpdateFrequency.Update100)
                    .AddModule<DroneStantonMasterRadio>(UpdateFrequency.Update100)
                    //.AddModule<DroneRadarMasterRadio>()
                    //.AddSurface(surface, nameof(DroneRadarMasterRadio), "console F", "console B", "console S")
                    .RenameMyBlocks()
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
