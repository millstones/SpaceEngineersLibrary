using System;
using System.Collections.Generic;
using IngameScript;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
// avoiding ambiguity errors with mod compiler adding mod namespaces
using IMyProgrammableBlock = Sandbox.ModAPI.Ingame.IMyProgrammableBlock;
using UpdateFrequency = Sandbox.ModAPI.Ingame.UpdateFrequency;
using UpdateType = Sandbox.ModAPI.Ingame.UpdateType;
using MyGridProgram = Sandbox.ModAPI.Ingame.MyGridProgram;

namespace PB
{
    /*
    
    // Copy the DebugAPI class to your class and use it like shown here.
    public class Program : MyGridProgram
    {
        DebugAPI Debug;

        int YellowLengthId;
        const double YellowLengthDefault = 5;

        public Program()
        {
            Debug = new DebugAPI(this);

            Debug.PrintChat("Hello there.");

            // This allows local player to hold R and using mouse scroll, change that initial 5 by 0.05 per scroll. It will show up on HUD too when you do this.
            // Then the returned id can be used to retrieve this value.
            // For simplicity sake you should only call AddAdjustNumber() in the constructor here.
            Debug.DeclareAdjustNumber(out YellowLengthId, YellowLengthDefault, 0.05, DebugAPI.Input.R, "Yellow line length");

            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateType)
        {
            try
            {
                Debug.RemoveDraw();

                // various usage examples:


                float cellSize = Me.CubeGrid.GridSize;
                MatrixD pbm = Me.WorldMatrix;
                Vector3D somePoint = pbm.Translation + pbm.Up * (cellSize / 2);


                // draws a point at given 3D position, onTop set to true makes it see-through-walls.
                Debug.DrawPoint(somePoint, Color.Red, 0.5f, onTop: true);


                // example of using adjust number to tweak something in realtime and then get its value to use:
                double lineLength = Debug.GetAdjustNumber(YellowLengthId, YellowLengthDefault);
                Debug.DrawLine(somePoint, somePoint + pbm.Backward * lineLength, Color.Yellow, thickness: 0.25f, onTop: true);


                // various shape drawing examples
                Debug.DrawSphere(Me.WorldVolume, Color.SkyBlue, DebugAPI.Style.Wireframe);

                Debug.DrawMatrix(Me.CubeGrid.WorldMatrix, onTop: true);

                Debug.DrawAABB(Me.CubeGrid.WorldAABB, Color.Blue * 0.25f, DebugAPI.Style.SolidAndWireframe);

                Vector3D offset = Vector3D.Half * cellSize;
                BoundingBoxD gridLocalBB = new BoundingBoxD(Me.CubeGrid.Min * cellSize - offset, Me.CubeGrid.Max * cellSize + offset);
                MyOrientedBoundingBoxD obb = new MyOrientedBoundingBoxD(gridLocalBB, Me.CubeGrid.WorldMatrix);
                Debug.DrawOBB(obb, new Color(255, 0, 255));


                // self-explanatory
                Debug.DrawGPS("I'm here!", pbm.Translation + pbm.Backward * (cellSize / 2), Color.Blue);

                Debug.PrintHUD($"Time is now: {DateTime.Now.ToLongTimeString()}");
            }
            catch(Exception e)
            {
                // example way to get notified on error then allow PB to stop (crash)
                Debug.PrintChat($"{e.Message}\n{e.StackTrace}", font: DebugAPI.Font.Red);
                Me.CustomData = e.ToString();
                throw;
            }
        }
    }
    
    */
}