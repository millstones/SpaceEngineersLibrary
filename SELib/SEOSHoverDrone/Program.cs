using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
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
using VRageRender;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        const string GRID_NAME = "GUZUN v.1";
        readonly SEOS _os;

        public Program()
        {
            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocks(blocks);
            var cockpitPb = Me.GetSurface(0);
            var cockpitSurf = cockpitPb;
            var cockpits  = blocks.OfType<IMyCockpit>().ToArray();//.First().GetSurface(0);
            if (cockpits.Any())
                cockpitSurf = cockpits.First().GetSurface(0);

            _os = new SEOS(this, GlobalConst.GRID_GROUP_FLY_CARRIER_DRONE, GRID_NAME)
                    .UseSerializer<DroneActualStatus>()
                    .UseSerializer<StationActualStatus>()
                    .UseSerializer<GridInfo>()
                    .UseSerializer<Product>()
                    .AddPlugin(new TaskPlugin())
                    .AddModule<CarrierController>(UpdateFrequency.Update1)
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
