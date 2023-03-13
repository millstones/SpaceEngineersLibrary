using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.ExternalLib
{
    public class OtherClasses
    {
        /// <summary>
        ///     Overrides all the gyros on the ship and sets them to these specific speeds.
        /// </summary>
        /// <param name="pitch_speed"></param>
        /// <param name="yaw_speed"></param>
        /// <param name="roll_speed"></param>
        /// <param name="gyro_list"></param>
        /// <param name="b_WorldMatrix"></param>
        public void ApplyGyroOverride(double pitch_speed, double yaw_speed, double roll_speed,
            List<IMyGyro> gyro_list, MatrixD b_WorldMatrix)
        {
            var rotationVec =
                new Vector3D(-pitch_speed, yaw_speed, roll_speed); //because keen does some weird stuff with signs 
            var relativeRotationVec = Vector3D.TransformNormal(rotationVec, b_WorldMatrix);
            var hasDetected = false;

            foreach (var thisGyro in gyro_list)
                if (thisGyro.IsWorking)
                {
                    var gyroMatrix = thisGyro.WorldMatrix;
                    var transformedRotationVec =
                        Vector3D.TransformNormal(relativeRotationVec, Matrix.Transpose(gyroMatrix));

                    thisGyro.Pitch = (float) transformedRotationVec.X;
                    thisGyro.Yaw = (float) transformedRotationVec.Y;
                    thisGyro.Roll = (float) transformedRotationVec.Z;
                    thisGyro.GyroOverride = true;
                }
                else if (!hasDetected)
                {
                    // Warning:\nGyro damage detected, recomputing.
                    hasDetected = true;
                }
        }
                    public static Vector3D VectorProjection(Vector3D a, Vector3D b) //proj a on b    
            {
                var projection = a.Dot(b) / b.LengthSquared() * b;
                return projection;
            }

            public static int
                VectorCompareDirection(Vector3D a, Vector3D b) //returns -1 if vectors return negative dot product 
            {
                var check = a.Dot(b);
                if (check < 0)
                    return -1;
                return 1;
            }

            public static double VectorSignedAngleBetween(Vector3D current, Vector3D target, Vector3D axisOfRotation, bool requireProjection = false)
            {
                Vector3D current_adjusted = current;

                if (requireProjection)
                {
                    current_adjusted = ProjectPointOnPlane(axisOfRotation, Vector3D.Zero, current);
                }

                double angle = Math.Acos(MathHelper.Clamp(target.Dot(current_adjusted), -1, 1));
                Vector3D cross = target.Cross(current_adjusted);
                if (axisOfRotation.Dot(cross) < 0)
                {
                    angle = -angle;
                }

                return angle;
            }

            public static double VectorAngleBetween(Vector3D a, Vector3D b) //returns radians 
            {
                if (a.LengthSquared() == 0 || b.LengthSquared() == 0)
                    return 0;
                return Math.Acos(MathHelper.Clamp(a.Dot(b) / a.Length() / b.Length(), -1, 1));
            }

            public static Vector3D NearestPointOnLine(Vector3D linePoint, Vector3D lineDirection, Vector3D point)
            {
                var lineDir = Vector3D.Normalize(lineDirection);
                var v = point - linePoint;
                var d = v.Dot(lineDir);
                return linePoint + lineDir * d;
            }

            public static Vector3D ProjectPointOnPlane(Vector3D planeNormal, Vector3D planePoint, Vector3D point)
            {
                double distance;
                Vector3D translationVector;

                //First calculate the distance from the point to the plane:
                distance = SignedDistancePlanePoint(planeNormal, planePoint, point);

                //Reverse the sign of the distance
                distance *= -1;

                //Get a translation vector
                translationVector = SetVectorLength(planeNormal, distance);

                //Translate the point to form a projection
                return point + translationVector;
            }


            //Get the shortest distance between a point and a plane. The output is signed so it holds information
            //as to which side of the plane normal the point is.
            public static double SignedDistancePlanePoint(Vector3D planeNormal, Vector3D planePoint, Vector3D point)
            {
                return Vector3D.Dot(planeNormal, point - planePoint);
            }


            //create a vector of direction "vector" with length "size"
            public static Vector3D SetVectorLength(Vector3D vector, double size)
            {
                //normalize the vector
                var vectorNormalized = Vector3D.Normalize(vector);

                //scale the vector
                return vectorNormalized *= size;
            }
        // double AlignWithGravity(Waypoint waypoint, bool requireYawControl)
        // {
        //     if (waypoint.RequireRotation)
        //     {
        //         var referenceBlock = systemsAnalyzer.currentHomeLocation.shipConnector;
        //
        //
        //         var referenceOrigin = referenceBlock.GetPosition();
        //         
        //         var targetDirection = -waypoint.forward;
        //         var gravityVecLength = targetDirection.Length();
        //         if (targetDirection.LengthSquared() == 0)
        //         {
        //             foreach (var thisGyro in systemsAnalyzer.gyros) thisGyro.SetValue("Override", false);
        //             return -1;
        //         }
        //
        //         //var block_WorldMatrix = referenceBlock.WorldMatrix;
        //         var block_WorldMatrix = Matrix.CreateWorld(referenceOrigin,
        //             referenceBlock.WorldMatrix.Up, //referenceBlock.WorldMatrix.Forward,
        //             -referenceBlock.WorldMatrix.Forward //referenceBlock.WorldMatrix.Up
        //         );
        //
        //         var referenceForward = block_WorldMatrix.Forward;
        //         var referenceLeft = block_WorldMatrix.Left;
        //         var referenceUp = block_WorldMatrix.Up;
        //
        //         anglePitch =
        //             Math.Acos(MathHelper.Clamp(targetDirection.Dot(referenceForward) / gravityVecLength, -1, 1)) -
        //             Math.PI / 2;
        //         Vector3D planetRelativeLeftVec = referenceForward.Cross(targetDirection);
        //         angleRoll = PID.VectorAngleBetween(referenceLeft, planetRelativeLeftVec);
        //         angleRoll *= PID.VectorCompareDirection(PID.VectorProjection(referenceLeft, targetDirection),
        //             targetDirection); //ccw is positive 
        //         if (requireYawControl)
        //             //angleYaw = 0;
        //             angleYaw = Math.Acos(MathHelper.Clamp(waypoint.auxilleryDirection.Dot(referenceLeft), -1, 1)) - Math.PI / 2;
        //         else
        //             angleYaw = 0;
        //         //shipIOHandler.Echo("Angle Yaw: " + IOHandler.RoundToSignificantDigits(angleYaw, 2).ToString());
        //
        //
        //         anglePitch *= -1;
        //         angleRoll *= -1;
        //
        //
        //         //shipIOHandler.Echo("Pitch angle: " + Math.Round((anglePitch / Math.PI * 180), 2).ToString() + " deg");
        //         //shipIOHandler.Echo("Roll angle: " + Math.Round((angleRoll / Math.PI * 180), 2).ToString() + " deg");
        //
        //         //double rawDevAngle = Math.Acos(MathHelper.Clamp(targetDirection.Dot(referenceForward) / targetDirection.Length() * 180 / Math.PI, -1, 1));
        //         var rawDevAngle = Math.Acos(MathHelper.Clamp(targetDirection.Dot(referenceForward), -1, 1)) * 180 /
        //                           Math.PI;
        //         rawDevAngle -= 90;
        //
        //         //shipIOHandler.Echo("Angle: " + rawDevAngle.ToString());
        //
        //
        //         var rollSpeed = rollPID.Control(angleRoll);
        //         var pitchSpeed = pitchPID.Control(anglePitch);
        //         double yawSpeed = 0;
        //         if (requireYawControl) yawSpeed = yawPID.Control(angleYaw);
        //
        //         //---Set appropriate gyro override  
        //         if (!errorState)
        //             //do gyros
        //             systemsController.ApplyGyroOverride(pitchSpeed, yawSpeed, -rollSpeed, systemsAnalyzer.gyros,
        //                 block_WorldMatrix);
        //         return rawDevAngle;
        //     }
        //
        //     return -1;
        // }
        // double AlignWithWaypoint(Waypoint waypoint)
        // {
        //
        //     MatrixD stationConnectorWorldMatrix = Matrix.CreateWorld(systemsAnalyzer.currentHomeLocation.stationConnectorPosition, systemsAnalyzer.currentHomeLocation.stationConnectorForward,
        //         (-systemsAnalyzer.currentHomeLocation.stationConnectorLeft).Cross(systemsAnalyzer.currentHomeLocation.stationConnectorForward));
        //
        //     var referenceGrid = Me.CubeGrid;
        //
        //     Vector3D waypointForward = HomeLocation.localDirectionToWorldDirection(waypoint.forward, systemsAnalyzer.currentHomeLocation);
        //     Vector3D waypointRight = HomeLocation.localDirectionToWorldDirection(waypoint.auxilleryDirection, systemsAnalyzer.currentHomeLocation);
        //
        //
        //     var targetDirection = waypointForward;
        //     
        //
        //
        //     var referenceOrigin = referenceGrid.GetPosition();
        //
        //
        //
        //     var block_WorldMatrix = Matrix.CreateWorld(referenceOrigin,
        //         referenceGrid.WorldMatrix.Up, //referenceBlock.WorldMatrix.Forward,
        //         -referenceGrid.WorldMatrix.Forward //referenceBlock.WorldMatrix.Up
        //     );
        //
        //     var referenceForward = block_WorldMatrix.Forward;
        //     var referenceLeft = block_WorldMatrix.Left;
        //     var referenceUp = block_WorldMatrix.Up;
        //
        //     anglePitch = Math.Acos(MathHelper.Clamp(targetDirection.Dot(referenceForward), -1, 1)) - Math.PI / 2;
        //     //anglePitch *= PID.VectorCompareDirection(targetDirection, referenceForward);
        //
        //
        //     Vector3D relativeLeftVec = referenceForward.Cross(targetDirection);
        //     angleRoll = PID.VectorAngleBetween(referenceLeft, relativeLeftVec);
        //     angleRoll *= PID.VectorCompareDirection(PID.VectorProjection(referenceLeft, targetDirection),
        //         targetDirection); //ccw is positive 
        //                           //angleRoll *= PID.VectorCompareDirection(PID.VectorProjection(referenceLeft, targetDirection),
        //                           //    targetDirection); //ccw is positive 
        //
        //                           Vector3D waypointUp = (-waypointRight).Cross(waypointForward);
        //     angleYaw = Math.Acos(MathHelper.Clamp((-waypointUp).Dot(referenceLeft), -1, 1)) - Math.PI / 2;
        //     //angleYaw *= PID.VectorCompareDirection(PID.VectorProjection(referenceLeft, targetDirection), targetDirection);
        //
        //     anglePitch *= -1;
        //     angleRoll *= -1;
        //
        //     //shipIOHandler.Echo("Pitch angle: " + Math.Round((anglePitch / Math.PI * 180), 2).ToString() + " deg");
        //     //shipIOHandler.Echo("Roll angle: " + Math.Round((angleRoll / Math.PI * 180), 2).ToString() + " deg");
        //     //shipIOHandler.Echo("Yaw angle: " + Math.Round((angleYaw / Math.PI * 180), 2).ToString() + " deg");
        //
        //     //double rawDevAngle = Math.Acos(MathHelper.Clamp(targetDirection.Dot(referenceForward) / targetDirection.Length() * 180 / Math.PI, -1, 1));
        //     var rawDevAngle = Math.Acos(MathHelper.Clamp(targetDirection.Dot(referenceForward), -1, 1)) * 180 /
        //                         Math.PI;
        //     rawDevAngle -= 90;
        //
        //     //shipIOHandler.Echo("Angle: " + rawDevAngle.ToString());
        //
        //
        //     var rollSpeed = rollPID.Control(angleRoll) * 1;
        //     var pitchSpeed = pitchPID.Control(anglePitch) * 1;
        //     double yawSpeed = yawPID.Control(angleYaw) * 1;
        //
        //     //---Set appropriate gyro override  
        //     if (!errorState)
        //         //do gyros
        //         systemsController.ApplyGyroOverride(pitchSpeed, yawSpeed, -rollSpeed, systemsAnalyzer.gyros,
        //             block_WorldMatrix);
        //     return rawDevAngle;
        //
        // }
    }
}