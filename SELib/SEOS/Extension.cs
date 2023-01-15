using Sandbox.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRage;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    static class Extension
    {
        public static string ToStringPostfix(this double val)
        {
            // https://translated.turbopages.org/proxy_u/en-ru.ru.27f7c658-63bed4d9-c1fb76dd-74722d776562/https/stackoverflow.com/questions/26209217/how-to-code-metric-prefix-kilo-mega-giga-etc-and-do-calculation-on-them
            string[] prefixeSI = {"y", "z", "a", "f","p", "n", "µ", "m", "", "k", "M", "G", "T", "P", "E", "Z", "Y"};

            var log10 = (int)Math.Log10(Math.Abs(val));
            if(log10 < -27)
                return "0.000";
            if(log10 % -3 < 0 )
                log10 -= 3;
            var log1000 = Math.Max(-8, Math.Min(log10 / 3, 8));

            return (val / Math.Pow(10, log1000 * 3)).ToString("###.#" + prefixeSI[log1000+8]);

        }

        public static string ToStringPostfix(this long val)=> ((double) val).ToStringPostfix();

        #region MathHelper

        public static void LimitRadiansPI(ref float angle)
        {
            if (angle >= 3.14159297943115)
            {
                angle = (float) (angle % 3.14159297943115 - 3.14159297943115);
            }
            else
            {
                if (angle <= -3.14159297943115)
                    angle = (float) (angle % 3.14159297943115 + 3.14159297943115); 
            }
        }
        public static void LimitDegreesPI(ref float angle)
        {
            angle = MathHelper.ToRadians(angle);
            LimitRadiansPI(ref angle);
            angle = MathHelper.ToDegrees(angle);
        }

        #endregion

        #region IMyTerminalBlock

        public static void Enable(this IMyFunctionalBlock block, bool value) => block.Enabled = value;

        public static void Enable(this IEnumerable<IMyFunctionalBlock> blocks, bool value)
        {
            foreach (var block in blocks)
            {
                block.Enabled = value;
            }
        }
        #endregion
        #region IMyCubeBlock
        public static MatrixD CreateHorizontalMatrix(this IMyCubeBlock relative, IMyShipController controller)
        {
            return controller.CreateHorizontalMatrix(relative);
        }

        public static Vector3D Distance2Body(this IMyCubeBlock block, Vector3D target) => 
            //block.World2BodyPosition(target - block.WorldMatrix.Translation);
            -block.World2BodyPosition(target);// - block.World2BodyPosition(block.WorldMatrix.Translation);
        
        /// <summary>
        /// Convert worldDirection into a local direction
        /// </summary>
        public static Vector3D World2BodyDirection(this MatrixD block, Vector3D worldDirection)
        {
            return Vector3D.TransformNormal(worldDirection, MatrixD.Transpose(block));
        }
        public static Vector3D World2BodyDirection(this IMyCubeBlock block, Vector3D worldDirection)
        {
            return Vector3D.TransformNormal(worldDirection, MatrixD.Transpose(block.WorldMatrix));
        }
        /// <summary>
        /// Convert the local vector to a world direction vector
        /// </summary>
        public static Vector3D Body2WorldDirection(this MatrixD block, Vector3D bodyDirection)
        {
            return Vector3D.TransformNormal(bodyDirection, block);
        }
        public static Vector3D Body2WorldDirection(this IMyCubeBlock block, Vector3D bodyDirection)
        {
            return Vector3D.TransformNormal(bodyDirection, block.WorldMatrix);
        }
        /// <summary>
        /// Convert worldDirection into a local direction
        /// </summary>
        public static Vector3D World2BodyPosition(this MatrixD block, Vector3D worldPosition)
        {
            var worldDirection = worldPosition - block.Translation;
            return Vector3D.TransformNormal(worldDirection, MatrixD.Transpose(block));
        }
        public static Vector3D World2BodyPosition(this IMyCubeBlock block, Vector3D worldPosition)
        {
            return block.WorldMatrix.World2BodyPosition(worldPosition);
        }
        /// <summary>
        /// Convert the local position to a world position
        /// </summary>
        public static Vector3D Body2WorldPosition(this MatrixD block, Vector3D bodyPosition)
        {
            return Vector3D.Transform(bodyPosition, block);
        }
        public static Vector3D Body2WorldPosition(this IMyCubeBlock block, Vector3D bodyPosition)
        {
            return Vector3D.Transform(bodyPosition, block.WorldMatrix);
        }
        #endregion
        #region IMyShipController
        public static double ElevationToSurface(this IMyShipController controller)
        {
            double h;
            controller.TryGetPlanetElevation(MyPlanetElevation.Surface, out h);
            return h;
        }

        public static Vector3D GetErrorAngels(this IMyShipController controller, IMyCubeBlock relative, Vector3D target)
        {
            var myPosition = relative.WorldMatrix.Translation;
            var upDir = Vector3D.Normalize(-controller.GetTotalGravity());
            var dir = Vector3D.Normalize(target - myPosition);
            var fwdDir = Vector3D.Normalize(Vector3D.ProjectOnPlane(ref dir, ref upDir));
            var targetMatrix = MatrixD.CreateWorld(target, fwdDir, upDir);
            
            return GetErrorAngels(relative, targetMatrix);
        }
        public static Vector3D GetErrorAngels(this IMyShipController controller, Vector3D target)
        {
            var myPosition = controller.WorldMatrix.Translation;
            var upDir = Vector3D.Normalize(-controller.GetTotalGravity());
            var dir = Vector3D.Normalize(target - myPosition);
            var fwdDir = Vector3D.Normalize(Vector3D.ProjectOnPlane(ref dir, ref upDir));
            var targetMatrix = MatrixD.CreateWorld(target, fwdDir, upDir);
            
            return GetErrorAngels(controller, targetMatrix);
        }
        
        public static Vector3D GetErrorAngels(this IMyShipController controller)
        {
            return GetErrorAngels(controller, CreateHorizontalMatrix(controller));
        }

        public static MatrixD CreateHorizontalMatrix(this IMyShipController controller)
        {
            return controller.CreateHorizontalMatrix(controller as IMyCubeBlock);
        }
        public static MatrixD CreateHorizontalMatrix(this IMyShipController controller, IMyCubeBlock relative)
        {
            return controller.CreateHorizontalMatrix(relative, relative.WorldMatrix.Translation);
        }

        public static MatrixD CreateHorizontalMatrix(this IMyShipController controller, Vector3D position)
        {
            return controller.CreateHorizontalMatrix(controller, position);
        }
        public static MatrixD CreateHorizontalMatrix(this IMyShipController controller, IMyCubeBlock relative, Vector3D position)
        {
            var myPosition = position;
            var upDir = Vector3D.Normalize(-controller.GetTotalGravity());
            //var upDir = Vector3D.Normalize(controller.GetTotalGravity());
            var fwd = relative.WorldMatrix.Forward;
            var fwdDir = Vector3D.Normalize(Vector3D.ProjectOnPlane(ref fwd, ref upDir));
            return MatrixD.CreateWorld(myPosition, fwdDir, upDir);
        }

        public static Vector3D GetErrorAngels(this IMyCubeBlock relative, MatrixD target)
        {
            var a = target;
            var b = relative.WorldMatrix;
            
            var aF = a.Forward;
            var aU = a.Up;
            var aL = a.Left;
            var bF = b.Forward;
            var bU = b.Up;
            var bL = b.Left;
            
            
            var dirIndicator = Vector3D.ProjectOnPlane(ref aL, ref bU).Dot(bF);
            var yaw = Vector3D.ProjectOnPlane(ref aF, ref bU).Cross(bF).Length();
            yaw = Math.Asin((float) yaw);
            if (dirIndicator > 0)
                yaw *= -1;

            dirIndicator = Vector3D.ProjectOnPlane(ref aU, ref bL).Dot(bF);
            var pith = Vector3D.ProjectOnPlane(ref aU, ref bL).Cross(bU).Length();
            pith = Math.Asin((float) pith);
            if (dirIndicator < 0)
                pith *= -1;

            dirIndicator = Vector3D.ProjectOnPlane(ref aL, ref bF).Dot(bU);
            var roll = Vector3D.ProjectOnPlane(ref aL, ref bF).Cross(bL).Length();
            roll = Math.Asin((float) roll);
            if (dirIndicator < 0)
                roll *= -1;

            yaw = MathHelper.ToDegrees(yaw);
            pith = MathHelper.ToDegrees(pith);
            roll = MathHelper.ToDegrees(roll);
            return new Vector3D(yaw, pith, roll);
        }

        public static Vector3D GetShipVelocities2Body(this IMyShipController controller)=>
            controller.World2BodyDirection(controller.GetShipVelocities().LinearVelocity);
        public static Vector3D GetTotalGravity2Body(this IMyShipController controller)=>
            controller.World2BodyDirection(controller.GetTotalGravity());
        #endregion
        #region IMyEntity
        public static double InventoryPercent(this IMyEntity entity)
        {
            var invent = entity.GetInventory();
            return invent.CurrentVolume.RawValue / invent.MaxVolume.RawValue;
        }
        public static double InventoryPercent(this IEnumerable<IMyEntity> entity)
        {
            var myEntities = entity.ToList();
            var c = 0.0;
            var m = 0.0;
            foreach (var myEntity in myEntities)
            {
                var invent = myEntity.GetInventory();
                c += invent.CurrentVolume.RawValue;
                m += invent.MaxVolume.RawValue;
            }
            return c / m;
        }

        #endregion
        #region IMyPistonBase
        public static double PositionPercent(this IMyPistonBase piston) =>
            piston.CurrentPosition / Math.Abs(piston.LowestPosition - piston.HighestPosition);

        public static void SetVelocity(this IEnumerable<IMyPistonBase> pistons, float v)
        {
            var myPistonBases = pistons as IMyPistonBase[] ?? pistons.ToArray();
            var vSp = Math.Abs(v) < 0.001 ? 0 : v / myPistonBases.Length;
            foreach (var piston in myPistonBases)
            {
                piston.Velocity = vSp;
            }
        }
        #region PISTON FUNCS
        public static float GetCurrentPistonPosition(this IEnumerable<IMyPistonBase> pistons)
        {
            var retVal = 0f;
            foreach (var piston in pistons)
            {
                retVal += piston.CurrentPosition;
            }

            return retVal;
        }
        public static float GetMaxPistonPosition(this IEnumerable<IMyPistonBase> pistons)
        {
            var retVal = 0f;
            foreach (var piston in pistons)
            {
                retVal += piston.HighestPosition;
            }

            return retVal;
        }
        public static float GetMinPistonPosition(this IEnumerable<IMyPistonBase> pistons)
        {
            var retVal = 0f;
            foreach (var piston in pistons)
            {
                retVal += piston.LowestPosition;
            }

            return retVal;
        }
        #endregion
        #region IMyMotorStator FUNCS

        public static float AngleLimited(this IMyMotorStator stator)
        {
            var retVal = stator.Angle;
            LimitRadiansPI(ref retVal);
            return retVal;
        }
        #endregion
        public static IEnumerator MoveTo(this IEnumerable<IMyPistonBase> pistons, float l, float v)
        {
            var pistonBases = pistons as IMyPistonBase[] ?? pistons.ToArray();
            l = MathHelper.Clamp(l, pistonBases.GetMinPistonPosition(), pistonBases.GetMaxPistonPosition());
            var currentPos = pistonBases.GetCurrentPistonPosition();
            v *= l < currentPos? -1 : 1;

            while (Math.Abs(currentPos - l) > 0.1f)
            {
                foreach (var piston in pistonBases)
                {
                    piston.Enabled = true;
                    piston.Velocity = v;
                }

                yield return null;
                currentPos = pistonBases.GetCurrentPistonPosition();
            }
            
            foreach (var piston in pistonBases)
            {
                piston.Enabled = false;
                piston.Velocity = 0;
            }
        }
        public static IEnumerator RotateTo(this IMyMotorStator rotor, float a, float v, float deadBandAngel = 0.5f)
        {
            var d = rotor.Displacement;
            rotor.Displacement = -d;
            rotor.Displacement = d;
            
            
            var radA = MathHelper.ToRadians(a);
            radA = MathHelper.Clamp(radA, rotor.LowerLimitRad, rotor.UpperLimitRad);
            var currentA = rotor.Angle;
            LimitRadiansPI(ref radA);
            LimitRadiansPI(ref currentA);
            var dir = radA < currentA ? -1 : 1;
            v *= dir;
            v *= (currentA + dir*4*Math.PI) > rotor.UpperLimitRad ||
                 (currentA + dir*4*Math.PI) < rotor.LowerLimitRad ? -1 : 1;
            while (Math.Abs(currentA - radA) > MathHelper.ToRadians(deadBandAngel))
            {
                rotor.Enabled = true;
                rotor.RotorLock = false;
                rotor.TargetVelocityRPM = v;
                
                yield return null;
                currentA = rotor.Angle; 
                LimitRadiansPI(ref currentA);
            }
            
            rotor.RotorLock = true;
            rotor.TargetVelocityRPM = 0;
        }
        #endregion

        #region IMyCargoContainer

        public static Dictionary<MyItemType, MyFixedPoint> GetItems(this IEnumerable<IMyCargoContainer> containers)
        {
            var retVal = new Dictionary<MyItemType, MyFixedPoint>();
            foreach (var cargo in containers)
            {
                var inv = new List<MyInventoryItem>();
                cargo.GetInventory().GetItems(inv);

                foreach (var item in inv)
                {
                    if (!retVal.ContainsKey(item.Type))
                    {
                        retVal.Add(item.Type, MyFixedPoint.Zero);
                    }
                        
                    retVal[item.Type] += item.Amount;
                }
            }

            return retVal;
        }

        #endregion
    }
}
