using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI.Ingame;
using VRage.Library.Utils;
using VRageMath;

namespace IngameScript
{
    class CNCModule : Module
    {
        CNCSystem _cnc;
        public override void Awake(IEnumerable<IMyTerminalBlock> blocks)
        {
            var myTerminalBlocks = blocks as IMyTerminalBlock[] ?? blocks.ToArray();
            var gridController = myTerminalBlocks.OfType<IMyShipController>().First(x => x.IsMainCockpit);

            var cargo =  myTerminalBlocks.OfType<IMyCargoContainer>().Where(x => x.CustomName.Contains("[CNC cargo]"));
            var helpSensor =  myTerminalBlocks.OfType<IMySensorBlock>().FirstOrDefault(x => x.CustomName.Contains("[CNC help sensor]"));
            var mainTool = myTerminalBlocks.OfType<IMyShipToolBase>().FirstOrDefault(x => x.CustomName.Contains("[CNC main tool]"));
            var mainRotor = myTerminalBlocks.OfType<IMyMotorStator>().FirstOrDefault(x => x.CustomName.Contains("[CNC main rotor]"));
            var mainHinge = myTerminalBlocks.OfType<IMyMotorStator>().FirstOrDefault(x=> x.CustomName.Contains("[CNC main hinge]"));
            var rotorX = myTerminalBlocks.OfType<IMyMotorStator>().FirstOrDefault(x => x.CustomName.Contains("[CNC X rotor]"));
            //var rotorY = myTerminalBlocks.OfType<IMyMotorStator>().FirstOrDefault(x=> x.CustomName.Contains("[CNC Y rotor]"));
            //var rotorZ = myTerminalBlocks.OfType<IMyMotorStator>().FirstOrDefault(x=> x.CustomName.Contains("[CNC Z rotor]"));
            //var moverX = myTerminalBlocks.OfType<IMyPistonBase>().Where(x=> x.CustomName.Contains("[CNC X mover]"));
            var moverY = myTerminalBlocks.OfType<IMyPistonBase>().Where(x=> x.CustomName.Contains("[CNC Y mover]"));
            var moverZ = myTerminalBlocks.OfType<IMyPistonBase>().Where(x=> x.CustomName.Contains("[CNC Z mover]"));


            _cnc = new CNCSystem(gridController, mainRotor, mainTool,helpSensor, cargo, mainHinge, rotorX, moverY, moverZ, Logger);
            
            MessageBroker.Post("init", _cnc.Init);
            MessageBroker.Post("stop", _cnc.Stop);
            MessageBroker.Post<string>("test", _cnc.Test);
            MessageBroker.Post("start", _cnc.Start);
        }
    }

    struct LimitsF
    {
        public float Max, Min;
        
        public LimitsF(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float Clamp(float value) => MathHelper.Clamp(value, Min, Max);
        public bool InRange(float value) => value >= Min && value <= Max;
    }

    class CNCHelpSensor
    {
        readonly IMySensorBlock _sensor;

        public CNCHelpSensor(IMySensorBlock sensor)
        {
            _sensor = sensor;
        }

        public BoundingBox GetPithRange(MatrixD relative)
        {
            var vMin = new Vector3D
            {
                X = -_sensor.RightExtend,
                Y = -_sensor.BackExtend,
                Z = -_sensor.BottomExtend
            };
            var vMax = new Vector3D
            {
                X = _sensor.LeftExtend,
                Y = _sensor.FrontExtend,
                Z = _sensor.TopExtend
            };

            var p = relative.World2BodyPosition(_sensor.GetPosition());
            vMin += p;
            vMax += p;

            return new BoundingBox(vMin, vMax);
        }
        
    }
    class CNCTool
    {
        readonly IMyFunctionalBlock _tool;

        public CNCTool(IMyFunctionalBlock tool)
        {
            if (tool == null) throw new Exception("Tool is NULL!");
            
            _tool = tool;
        }

        public Vector3D GetDirection(MatrixD local) => local.World2BodyDirection(_tool.WorldMatrix.Forward);
        public Vector3D GetPosition(MatrixD local) => local.World2BodyPosition(_tool.GetPosition());

        public float GetAngle(MatrixD local)
        {
            var toolFwd = GetDirection(local);
            var sysUp = Vector3D.Normalize(GetPosition(local));
            
            var aX = (float)Math.Acos(Vector3D.Dot(toolFwd,sysUp));

            aX = MathHelper.ToDegrees(aX);

            
            if (toolFwd.Z < sysUp.Z)
                aX = -aX;

            return aX;
        }

        public void Enable()
        {
            _tool.Enabled = true;
        }
        public void Stop()
        {
            _tool.Enabled = false;
        }
    }

    class CNCCargo
    {
        readonly IEnumerable<IMyCargoContainer> _cargo;

        public bool IsFull =>
            _cargo.All(x => x.GetInventory().IsFull);
        public CNCCargo(IEnumerable<IMyCargoContainer> cargo)
        {
            _cargo = cargo;
        }
    }
}