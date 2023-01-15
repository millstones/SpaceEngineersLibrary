using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    class SERotor : SEMotor
    {
        readonly IMyMotorStator _mover;

        public SERotor(IMyMotorStator mover, LimitsF limSpeed, bool inversal = false, float deadZone = 0.1f) : base(limSpeed, inversal, deadZone)
        {
            _mover = mover;
            LLimits = new LimitsF
            {
                Max = _mover.LowerLimitDeg,
                Min = _mover.UpperLimitDeg
            };
        }

        public override float L => _mover.Angle;
        public override LimitsF LLimits { get; }
        public override void Stop()
        {
            _mover.RotorLock = true;
            _mover.TargetVelocityRPM = 0;
        }

        public override void Move(float v)
        {
            _mover.Enable(true);
            _mover.RotorLock = false;
            _mover.TargetVelocityRPM = v;
        }
    }
}