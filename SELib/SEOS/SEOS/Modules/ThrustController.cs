using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    class ThrustController
    {
        class AxisThrust
        {
            List<IMyThrust> _posThrust;
            List<IMyThrust> _negThrust;
            public AxisThrust(IEnumerable<IMyThrust> blocks, Vector3D dir)
            {
                var myThrusts = blocks as IMyThrust[] ?? blocks.ToArray();
                _posThrust = myThrusts.Where(block => Vector3D.Dot(block.WorldMatrix.Backward, dir) > 0.98).ToList();
                _negThrust = myThrusts.Where(block => Vector3D.Dot(block.WorldMatrix.Forward, dir) > 0.98).ToList();
            }

            public double GetMaxPosForce() => GetMaxForce(_posThrust);
            public double GetMaxNegForce() => GetMaxForce(_negThrust);
            double GetMaxForce(IEnumerable<IMyThrust> t)
            {
                var power = 0.0;
                foreach (var item in t)
                {
                    power += item.MaxEffectiveThrust;
                }

                return power;
            }

            public void SetThrust(double kN)
            {
                if (kN == 0)
                {
                    Reset();
                    return;
                }

                var thrustersUpForce = kN > 0 ? _posThrust : _negThrust;
                var thrustersDnForce = kN > 0 ? _negThrust : _posThrust;

                kN = Math.Abs(kN);

                foreach (var thrust in thrustersUpForce)
                {
                    var maxThrust = thrust.MaxEffectiveThrust;
                    var p = MathHelper.Clamp((float) kN / maxThrust, 0, 1);
                    thrust.ThrustOverridePercentage = p;

                    if (p < 1) break;
                    kN -= maxThrust;
                }
                
                Reset(thrustersDnForce);
            }

            void Reset(IEnumerable<IMyThrust> thrusts)
            {
                foreach (var thrust in thrusts)
                    thrust.ThrustOverridePercentage = 0;
            }

            public void Reset()
            {
                Reset(_posThrust);
                Reset(_negThrust);
            }
        }

        AxisThrust _right, _up, _forward;

        public ThrustController(IEnumerable<IMyTerminalBlock> blocks, IMyCubeBlock reference)
        {
            var thrusters = blocks
                .OfType<IMyThrust>()
                .Where(x=> x.CubeGrid == reference.CubeGrid)
                .ToArray();

            _right = new AxisThrust(thrusters, reference.WorldMatrix.Right);
            _up = new AxisThrust(thrusters, reference.WorldMatrix.Up);
            _forward = new AxisThrust(thrusters, reference.WorldMatrix.Backward);
        }

        public void SetThrust(Vector3D kN)
        {
            _right.SetThrust(kN.X);
            _up.SetThrust(kN.Y);
            _forward.SetThrust(kN.Z);
        }
        public void Reset()
        {
            _right.Reset();
            _up.Reset();
            _forward.Reset();
        }

        public Vector3D GetMaxForce(Vector3D dirNormal)
        {
            return Vector3D.Abs(new Vector3D
            {
                X = (dirNormal.X > 0 ? _right.GetMaxPosForce() : _right.GetMaxNegForce()) * dirNormal.X,
                Y = (dirNormal.Y > 0 ? _up.GetMaxPosForce() : _up.GetMaxNegForce()) * dirNormal.Y,
                Z = (dirNormal.Z > 0 ? _forward.GetMaxPosForce() : _forward.GetMaxNegForce()) * dirNormal.Z
            });
        }
    }
}