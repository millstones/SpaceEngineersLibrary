using System;

namespace IngameScript
{
    abstract class SEMotor
    {
        readonly LimitsF _limSpeed;
        readonly bool _inversal;

        public bool IsLLimit => IsMinL || IsMaxL;
        public bool IsMaxL => (Math.Abs(L - (_inversal? LLimits.Min : LLimits.Max)) < DeadZone);
        public bool IsMinL => (Math.Abs(L - (_inversal? LLimits.Max : LLimits.Min)) < DeadZone);
        public abstract float L { get; }
        public readonly float DeadZone;

        public abstract LimitsF LLimits { get; }


        protected SEMotor(LimitsF limSpeed, bool inversal = false, float deadZone = 0.05f)
        {
            _limSpeed = limSpeed;
            _inversal = inversal;
            DeadZone = deadZone;
        }

        public void MoveBackward(bool fast)
        {
            var v = fast ? _limSpeed.Max : _limSpeed.Min;
            v = _inversal ? -v : v;
            Move(-v);
        }

        public void MoveForward(bool fast)
        {
            var v = fast ? _limSpeed.Max : _limSpeed.Min;
            v = _inversal ? -v : v;
            Move(v);
        }

        public abstract void Stop();
        public abstract void Move(float v);
    }
}