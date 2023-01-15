using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    public enum FlyMode
    {
        Free, Docking
    }
    class PathBuilder
    {
        readonly IMyCubeBlock _relative;
        readonly IMyRemoteControl _rc;

        public struct PathPoint
        {
            public MatrixD target;
            public FlyMode mode;
            public double distanceDeadZone;
        }
        struct Patch : IEnumerator<PathPoint>
        {
            List<PathPoint> _points;
            
            IMyCubeBlock _relative;
            int _i;

            public Patch(IEnumerable<PathPoint> points, IMyCubeBlock relative)
            {
                _points = new List<PathPoint>(points);
                _relative = relative;
                _i = _points.Count-1;
                Current = _points[_i];
            }
            
            public void Dispose()
            {
                _points.Clear();
            }

            public bool MoveNext()
            {
                if (_i < 0) return false;
                
                Current = _points[_i];
                var distance = _relative.Distance2Body(Current.target.Translation);
                if (distance.Length() < Current.distanceDeadZone)
                    _i--;

                return true;
            }
            
            public void Reset()
            {
                _i = _points.Count-1;
                Current = _points[_i];
            }

            public PathPoint Current { get; private set; }

            object IEnumerator.Current => Current;
        }
        
        List<PathPoint> _current = new List<PathPoint>();

        public PathBuilder(IMyCubeBlock relative, IMyRemoteControl rc)
        {
            _relative = relative;
            _rc = rc;
        }
        public PathBuilder New()
        {
            if (_current.Any()) throw new Exception("Path contains points");
            _current = new List<PathPoint>();
            return this;
        }

        public PathBuilder AddDocking(MatrixD target)
        {
            var horTargetM = GetTargetMatrix(target);
            var fw = target.Forward;
            var up = horTargetM.Up;
            _current.AddRange(new []
            {
                new PathPoint
                {
                    mode = FlyMode.Docking,
                    target = target,
                    distanceDeadZone = 1,
                },
                new PathPoint
                {
                    mode = FlyMode.Free,
                    target = MatrixD.CreateWorld(target.Translation + fw * 50, fw, up),
                    distanceDeadZone = 5,
                },
                new PathPoint
                {
                    mode = FlyMode.Free,
                    target = MatrixD.CreateWorld(target.Translation + fw * 100 + up * 10, fw, up),
                    distanceDeadZone = 5,
                }
            });

            return this;
        }
        public PathBuilder AddUnDocking(MatrixD target)
        {
            var horTargetM = GetTargetMatrix(target);
            var fw = target.Forward;
            var up = horTargetM.Up;
            _current.AddRange(new []
            {
                new PathPoint
                {
                    mode = FlyMode.Free,
                    target = MatrixD.CreateWorld(target.Translation + fw * 100 + up * 10, fw, up),
                    distanceDeadZone = 5,
                },
                new PathPoint
                {
                    mode = FlyMode.Docking,
                    target = MatrixD.CreateWorld(target.Translation + fw * 50, fw, up),
                    distanceDeadZone = 5,
                },
            });
            return this;
        }

        MatrixD GetTargetMatrix(MatrixD m)
        {
            var retVal = m;
            var grav = _rc.GetTotalGravity();
            if (Math.Abs(grav.LengthSquared()) > 0.1f)
            {
                retVal = _rc.CreateHorizontalMatrix(_relative.WorldMatrix.Translation);
            }

            return retVal;
        }
        public PathBuilder AddFree(Vector3D target, double distanceDeadZone = 5)
        {
            _current.Add(new PathPoint
            {
                mode = FlyMode.Free,
                target = MatrixD.CreateWorld(target),
                distanceDeadZone = distanceDeadZone
            });

            return this;
        }
        public PathBuilder AddFree(params PathPoint[] points)
        {
            _current.AddRange(points);
            return this;
        }

        public IEnumerator<PathPoint> Build()
        {
            var retVal = new Patch(_current, _relative);
            _current.Clear();

            return retVal;
        }
    }
}