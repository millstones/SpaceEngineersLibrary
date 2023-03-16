﻿using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    class BatteryModeSwitch : FlexItem
    {
        public BatteryModeSwitch(IReadOnlyCollection<IMyBatteryBlock> battery)
        {
            Direction = FlexDirection.Column;
            
            var getSet = new ReactiveProperty<ChargeMode>(() => battery.GetMode() ?? ChargeMode.Auto, battery.SetMode);

            Add(new Switch<ChargeMode>(ChargeMode.Auto, getSet));
            Add(new Switch<ChargeMode>(ChargeMode.Discharge, getSet));
            Add(new Switch<ChargeMode>(ChargeMode.Recharge, getSet));
        }
    }
}