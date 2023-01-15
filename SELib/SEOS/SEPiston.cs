using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    class SEPiston : SEMotor
    {
        readonly IEnumerable<IMyPistonBase> _mover;

        public SEPiston(IEnumerable<IMyPistonBase> mover, LimitsF limSpeed, bool inversal = false, float deadZone = 0.1f)
            : base(limSpeed, inversal, deadZone)
        {
            _mover = mover;

            var myPistonBases = _mover as IMyPistonBase[] ?? _mover.ToArray();
            LLimits = new LimitsF
            {
                Max = myPistonBases.GetMaxPistonPosition(),
                Min = myPistonBases.GetMinPistonPosition()
            };
        }

        public override float L => _mover.GetCurrentPistonPosition();
        public override LimitsF LLimits { get; }

        public override void Stop()
        {
            _mover.SetVelocity(0);
        }

        public override void Move(float v)
        {
            _mover.Enable(true);
            _mover.SetVelocity(v);
        }
    }
}