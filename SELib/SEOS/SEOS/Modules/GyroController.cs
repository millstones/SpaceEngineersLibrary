using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    class GyroController
    {
        class GyroInfo
        {
            IMyGyro _gyro;
            Action<float> _setYaw, _setPitch, _setRoll;
            Func<float> _getYaw, _getPitch, _getRoll;

            public GyroInfo(IMyGyro gyro, IMyCubeBlock relative)
            {
                _gyro = gyro;
                
                Matrix vM, gM;
                var aRes = new Action<float>[3];
                var fRes = new Func<float>[3];
                relative.Orientation.GetMatrix(out vM);
                var vecRef = new List<Vector3> {vM.Up, vM.Left, vM.Backward};

                gyro.Orientation.GetMatrix(out gM);
                for (var k = 0; k < 3; k++)
                {
                    if (vecRef[k] == gM.Up)
                    {
                        aRes[k] = f => gyro.Yaw = f; 
                        fRes[k] = () => gyro.Yaw;
                    }
                    else if (vecRef[k] == gM.Down)
                    {
                        aRes[k] = f => gyro.Yaw = -f; 
                        fRes[k] = () => -gyro.Yaw;
                    }
                    else if (vecRef[k] == gM.Left)
                    {
                        aRes[k] = f => gyro.Pitch = -f; 
                        fRes[k] = () => -gyro.Pitch;
                    }
                    else if (vecRef[k] == gM.Right)
                    {
                        aRes[k] = f => gyro.Pitch = f; 
                        fRes[k] = () => gyro.Pitch;
                    }
                    else if (vecRef[k] == gM.Backward)
                    {
                        aRes[k] = f => gyro.Roll = f; 
                        fRes[k] = () => gyro.Roll;
                    }
                    else if (vecRef[k] == gM.Forward)
                    {
                        aRes[k] = f => gyro.Roll = -f; 
                        fRes[k] = () => -gyro.Roll;
                    }
                }
                _setYaw = aRes[0];
                _setPitch = aRes[1];
                _setRoll = aRes[2];
                _getYaw = fRes[0];
                _getPitch = fRes[1];
                _getRoll = fRes[2];
            }
            public void Override(Vector3D rpm)
            {
                _gyro.GyroOverride = true;
                _setYaw((float)rpm.X);
                _setPitch((float)rpm.Y);
                _setRoll((float)-rpm.Z);
            }
            public void Reset()
            {
                Override(Vector3D.Zero);
                _gyro.GyroOverride = false;
            }
        }

        List<GyroInfo> _gyros = new List<GyroInfo>();

        public GyroController(IEnumerable<IMyTerminalBlock> blocks, IMyCubeBlock reference)
        {
            var gyros = blocks
                .OfType<IMyGyro>()
                .Where(x=> x.CubeGrid == reference.CubeGrid)
                .ToArray();

            foreach (var gyro in gyros)
            {
                _gyros.Add(new GyroInfo(gyro, reference));
            }
        }

        public void Override(Vector3D rpm)
        {
            foreach (var gyro in _gyros)
                gyro.Override(rpm);
        }

        public void Reset()
        {
            foreach (var gyro in _gyros)
                gyro.Reset();
        }
    }
}