using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    class CNCPiston : CNCMotor
    {
        public CNCPiston(SEMotor motor,  Func<CNCMotor, float> getPV = null) : base(motor, getPV)
        {
        }
        protected override float Clamp(float value)
        {
            return value;
        }
    }
}