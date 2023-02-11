using System;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    class CNCRotor : CNCMotor
    {
        public CNCRotor(SEMotor motor, Func<CNCMotor, float> getPV) : base(motor, getPV)
        {
        }
        

        protected override float Clamp(float value)
        {
            Extension.LimitDegreesPI(ref value);
            //value = value < 0 ? 360 - value : value;
            return value;//_internalPositionLimits.Clamp(value);
        }
    }
}