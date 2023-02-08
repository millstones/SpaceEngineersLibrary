using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    class BatteryModeSwitch : SwitchPanel<ChargeMode>
    {
        public BatteryModeSwitch(IReadOnlyCollection<IMyBatteryBlock> battery, bool vertical) : base(vertical)
        {
            var getSet = new ReactiveProperty<ChargeMode>(() => battery.GetMode() ?? ChargeMode.Auto, battery.SetMode);
            
            Add(new Switch<ChargeMode>(new Text("AUTO"), ChargeMode.Auto, getSet));
            Add(new Switch<ChargeMode>(new Text("DISCHARGE"), ChargeMode.Discharge, getSet));
            Add(new Switch<ChargeMode>(new Text("RECHARGE"), ChargeMode.Recharge, getSet));
        }
    }
}