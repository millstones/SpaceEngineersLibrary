using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using EmptyKeys.UserInterface.Generated.DataTemplatesContracts_Bindings;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;
using VRageRender;

namespace IngameScript
{
    class Engine
    {
        const double K_MOVE = 0.5;                     // плавность хода
        const double K_ROTATE = 0.5;                   // плавность поворотов
        const double MAX_RPM = MathHelper.RadiansPerSecondToRPM * 1;

        
        IMyCubeBlock _relative;
        readonly DebugAPI _debug;
        IMyShipController _shipController;
        ThrustController _thrustController;
        GyroController _gyroController;
        public Engine(IEnumerable<IMyTerminalBlock> blocks, IMyCubeBlock relative, DebugAPI debug)
        {
            var terminalBlocks = blocks as IMyTerminalBlock[] ?? blocks.ToArray();

            _shipController = terminalBlocks.OfType<IMyRemoteControl>().First();
            _thrustController = new ThrustController(terminalBlocks, _shipController);
            _gyroController = new GyroController(terminalBlocks, _shipController);

            _relative = relative;
            _debug = debug;

        }

        public IEnumerator<PathBuilder.PathPoint> Path { get; set; }
        public bool IsMoveDone => Path == null;
        public void SetAutopilot()
        {
            _enable = true;
            _shipController.DampenersOverride = false;
        }
        public void ResetAutopilot()
        {
            _enable = false;
            _gyroController.Reset();
            _thrustController.Reset();
            _shipController.DampenersOverride = true;
        }

        bool _enable;

        int _debugRotM, _debugMoveV;
        public void Tick()
        {
            var target = _shipController.CreateHorizontalMatrix(_relative);
            var mode = false;

            if (Path != null)
            {
                if (Path.MoveNext())
                {
                    target = Path.Current.target;
                    mode = Path.Current.mode == FlyMode.Docking;
                }
                else
                    Path = null;
            }

            var gravity = _shipController.GetTotalGravity();
            var mass = _shipController.CalculateShipMass().TotalMass;
            var speed = _shipController.GetShipVelocities2Body();

            Move(_relative.WorldMatrix, target, gravity, speed, mass);
            Rotate(_shipController.WorldMatrix, target, gravity, mode);
            
        }

        Vector3D _eAng, _movePower, _distance;
        void Rotate(MatrixD my, MatrixD target, Vector3D gravity, bool matrixMode = false)
        {
            var gravityNormal = Vector3D.Normalize(gravity);
            var dirNormal = Vector3D.Normalize(target.Translation - my.Translation);

            if (gravity != Vector3D.Zero)
            {
                dirNormal *= -1;
                var targetFwd = target.Forward;
                var up = -gravityNormal;
                var fwd = Vector3D.Normalize(Vector3D.ProjectOnPlane(ref dirNormal, ref up));
                var fwdTarget = Vector3D.Normalize(Vector3D.ProjectOnPlane(ref targetFwd, ref up));
                
                target = MatrixD.CreateWorld(target.Translation, matrixMode ? fwdTarget : fwd, up);
            }

            _debug.Remove(_debugRotM);
            _debugRotM = _debug.DrawMatrix(target);

            var myDown = my.Down;
            var myFwd = my.Forward;
            var targetRight = target.Right;
            var targetForward = target.Forward;
            var myFwd2HorProj = Vector3D.Normalize(Vector3D.ProjectOnPlane(ref myFwd, ref gravityNormal));
            var myDown2RightProj = Vector3D.Normalize(Vector3D.ProjectOnPlane(ref myDown, ref targetRight));
            var myDown2ForwardProj = Vector3D.Normalize(Vector3D.ProjectOnPlane(ref myDown, ref targetForward));
            
            var yaw = Vector3D.Dot(myFwd2HorProj, (target.Right));
            var pith = Vector3D.Dot(myDown2RightProj, (target.Forward));
            var roll = Vector3D.Dot(myDown2ForwardProj, (target.Right));

            var retVal = new Vector3D(yaw, pith, roll);
            
            if (_enable)
                _gyroController.Override(K_ROTATE * retVal * MAX_RPM);

            _eAng = retVal;
            /*
            return new Vector3D
            {
                X = MathHelperD.ToDegrees(Math.Acos(yaw)) * yawMantisa,
                Y = MathHelperD.ToDegrees(Math.Acos(pith) * pithMantisa),
                Z = MathHelperD.ToDegrees(Math.Acos(roll) * rollMantisa),
            };
            */
        }
        void Move(MatrixD my, MatrixD target, Vector3D gravity, Vector3D v, float m)
        {
            const double distanceDeadZone = 0.25;

            gravity = my.World2BodyDirection(gravity);
            
            _distance = _relative.Distance2Body(target.Translation);
            
            _distance = new Vector3D
            {
                X = Math.Abs(_distance.X) < distanceDeadZone? 0: _distance.X,
                Y = Math.Abs(_distance.Y) < distanceDeadZone? 0: _distance.Y,
                Z = Math.Abs(_distance.Z) < distanceDeadZone? 0: -_distance.Z,
            };
            
            //_distance = Vector3D.Backward * 100;

            var brThrust = _thrustController.GetMaxForce
            (
                new Vector3D
                {
                    X = v.X > 0 ? 1 : -1,
                    Y = v.Y > 0 ? 1 : -1,
                    Z = v.Z > 0 ? 1 : -1
                }
            );
            
            var maxBrA = NotNan(gravity + (brThrust / m));                           // +
            var brV = -NotNan(_distance / Vector3D.Abs(v / maxBrA));
            var targetV = NotNan(Vector3D.Normalize(_distance));                // для того чтобы сдернуть с места

            var movePower = (gravity + targetV - brV - v/K_MOVE) * m;
            _movePower = movePower / m;//movePower / m;

            
            _debug.Remove(_debugMoveV);
            _debugMoveV = _debug.DrawLine(target.Translation, target.Translation + target.World2BodyDirection(movePower / m), Color.Blue, 0.05f);
            
            if (_enable)
            {
                _thrustController.SetThrust(movePower);
            }
        }
        double NotNan(double v) => double.IsNaN(v) || double.IsInfinity(v) ? 0 : v;
        Vector3D NotNan(Vector3D v) => new Vector3D(NotNan(v.X), NotNan(v.Y), NotNan(v.Z));
    }
}