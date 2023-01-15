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
        
        const string GRID_NAME = "GUZUN driller crane";
        readonly SEOS _os;

        public Program()
        {
            
            _os = new SEOS(this, GlobalConst.GRID_GROUP_GROUND_CARRIER_DRONE, GRID_NAME)
                    .AddPlugin(new TaskPlugin())
                    .AddModule<CNCModule>(UpdateFrequency.Update100)
                    //.AddModule<DroneRadarMasterRadio>()
                    //.AddSurface(surface, nameof(DroneRadarMasterRadio), "console F", "console B", "console S")
                    .Build(new DefaultLogger(this))
                ;
            
            /*
            Me.CustomData = "";

            var mainRotor = GridTerminalSystem.GetBlockWithName("Экскаватор:Шарнир основной");
            Me.CustomData = mainRotor.GetType().ToString() + '\n';
            var tActs = new List<ITerminalAction>();
            var tProps = new List<ITerminalProperty>();

            mainRotor.GetActions(tActs);
            mainRotor.GetProperties (tProps);

            foreach (var item in tActs)
            {
                Me.CustomData += $"{item.Name} = '{item.Id}'\n";
            }
            foreach (var item in tProps)
            {
                Me.CustomData += $"{item.Id} = '{item.TypeName}'\n"; 
            }
            */
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
