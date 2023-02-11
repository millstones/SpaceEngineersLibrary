using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Crane
{
    class CraneModule : Module
    {
        Crane _crane;

        public override void Awake(IEnumerable<IMyTerminalBlock> blocks)
        {
            var myTerminalBlocks = blocks as IMyTerminalBlock[] ?? blocks.ToArray();
            var gridController = myTerminalBlocks.OfType<IMyShipController>().First(x => x.CustomName.Contains("[Crane cabin]"));
            var gridDirection = myTerminalBlocks.First(x => x.CustomName.Contains("[Crane direction]"));

            var moverBlocks = myTerminalBlocks.OfType<IMyPistonBase>()
                .Where(x => x.CustomName.Contains("[Crane mover]"));

            var cableBlocks = myTerminalBlocks.OfType<IMyPistonBase>()
                .Where(x => x.CustomName.Contains("[Crane cable]"));

            var armBlocks = myTerminalBlocks.OfType<IMyPistonBase>().Where(x => x.CustomName.Contains("[Crane arm]"));

            var rotorBlocks = myTerminalBlocks.OfType<IMyMotorStator>()
                .First(x => x.CustomName.Contains("[Crane rotor]"));

            var hookRotorBlocks = myTerminalBlocks.OfType<IMyMotorStator>()
                .First(x => x.CustomName.Contains("[Crane hook rotor]"));

            var hookHugeBlocks1 = myTerminalBlocks.OfType<IMyMotorStator>()
                .First(x => x.CustomName.Contains("[Crane hook huge 1]"));
            var hookHugeBlocks2 = myTerminalBlocks.OfType<IMyMotorStator>()
                .First(x => x.CustomName.Contains("[Crane hook huge 2]"));

            var moveLim = new LimitsF {Max = 0.5f, Min = 0.1f};
            var rotateLim = new LimitsF {Max = 0.5f, Min = 0.1f};

            var cable = new SEPiston(cableBlocks, moveLim);
            var arm = new SEPiston(armBlocks, moveLim);
            var rotor = new SERotor(rotorBlocks, rotateLim);
            var mover = new SEPiston(moverBlocks, rotateLim);
            var hookRotor = new SERotor(hookRotorBlocks, rotateLim);
            var hookHuge1 = new SERotor(hookHugeBlocks1, rotateLim);
            var hookHuge2 = new SERotor(hookHugeBlocks2, rotateLim);


            var frame = new CraneFrame(cable, arm, rotor, mover);
            var hook = new CraneHook(hookRotor, hookHuge1, hookHuge2);

            _crane = new Crane(gridDirection, gridController, frame, hook);

            MessageBroker.Post("enable", EnableControl);
            MessageBroker.Post("disable", DisableControl);
        }

        TaskBase _process;

        void EnableControl()
        {
            _process?.Cancel();
            _process = Task.EveryTick(() =>
            {
                _crane.Control();
            });
            _process.Run();
        }
        void DisableControl()
        {
            _process?.Cancel();
            _process = null;
        }
    }

    class Crane
    {
        readonly IMyTerminalBlock _direction;
        readonly IMyShipController _controller;
        readonly CraneFrame _frame;
        readonly CraneHook _hook;

        public Crane(IMyTerminalBlock direction, IMyShipController controller, CraneFrame frame, CraneHook hook)
        {
            _direction = direction;
            _controller = controller;
            _frame = frame;
            _hook = hook;
        }

        public void Control()
        {
            var moveDir = _controller.MoveIndicator;
            var yawPith = _controller.RotationIndicator;
            var roll = _controller.RollIndicator;

            var wDir = _controller.Body2WorldDirection(moveDir);
            var transDir = _direction.World2BodyDirection(wDir);
            var a = Vector3D.Dot(_direction.WorldMatrix.Forward, _controller.WorldMatrix.Forward);
            var x = transDir.X * Math.Cos(a);
            transDir = new Vector3D(x, transDir.Y, transDir.Z);
            
            _frame.Move(transDir, roll+(float) (a * transDir.X));
            _hook.Rotate(yawPith);
        }
    }

    class CraneHook
    {
        readonly SEMotor _rotor;
        readonly SEMotor _huge1;
        readonly SEMotor _huge2;

        public CraneHook(SEMotor rotor, SEMotor huge1, SEMotor huge2)
        {
            _rotor = rotor;
            _huge1 = huge1;
            _huge2 = huge2;
        }

        public void Rotate(Vector2 speed)
        {
            _rotor.Move(-speed.Y);
            _huge1.Move(-speed.X);
            _huge2.Move(-speed.X);
        }
    }

    class CraneFrame
    {
        readonly SEMotor _cable;
        readonly SEMotor _arm;
        readonly SEMotor _rotor;
        readonly SEMotor _mover;

        public CraneFrame(SEMotor cable, SEMotor arm, SEMotor rotor, SEMotor mover)
        {
            _cable = cable;
            _arm = arm;
            _rotor = rotor;
            _mover = mover;
        }

        public void Move(Vector3 speed, float roll)
        {
            _arm.Move(-speed.X+speed.Z);
            _mover.Move(-speed.Z);
            _cable.Move(-speed.Y);
            _rotor.Move(roll);
        }
    }
}