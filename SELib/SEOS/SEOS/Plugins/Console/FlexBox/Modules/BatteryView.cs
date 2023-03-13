using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    class BatteryView : FlexItem
    {
        public BatteryView(IEnumerable<IMyBatteryBlock> battery, string group = "")
        {
            Direction = FlexDirection.Column;
            
            var batt = battery.ToList();
            
            var input1 = new Text(() =>
            {
                var retVal = batt.Sum(x => (x.CurrentInput - x.CurrentOutput) * 1000000);
                return Math.Abs(retVal) < 1000 ? "" : retVal.ToStringPostfix(true) + "Wh";
            });
            var time1 = new Text(() =>
            {
                var input = batt.Sum(x => x.CurrentInput - x.CurrentOutput);
                var target = input > 0
                    ? batt.Sum(x => x.MaxStoredPower - x.CurrentStoredPower)
                    : batt.Sum(x => x.CurrentStoredPower);

                var time = (double) Math.Abs(target / input);
                time = double.IsInfinity(time) ? 100000 : double.IsNaN(time) ? 0 : time;
                var sec = TimeSpan.FromHours(time);

                if (sec.Days < 10)
                {
                    if (sec.Days > 0) return $"{sec.Days:0}d";
                    if (sec.Hours > 0) return $"{sec.Hours:00}h";
                    if (sec.Minutes > 0) return $"{sec.Minutes:00}m";
                    if (sec.Seconds > 0) return $"{sec.Seconds:00}s";
                }

                return "";

            });
            var charge = new Text(() =>
                (batt.Sum(x => x.CurrentStoredPower) * 1000000).ToStringPostfix(false) +
                "Wh / " +
                (batt.Sum(x => x.MaxStoredPower) * 1000000).ToStringPostfix(false) +
                "Wh"
            );
            
            var panel = new FlexItem()
                    .Add(new ProgressBar(() =>
                        batt.Sum(x => (double) x.CurrentStoredPower / x.MaxStoredPower) / batt.Count()))
                    .Add(input1/*, CreateArea(new Vector2(0, 0.05f), new Vector2(1, 0.25f))*/)
                    .Add(time1/*, CreateArea(new Vector2(0, 0.6f), new Vector2(1, 0.8f))*/)
                    .Add(charge /*, CreateArea(new Vector2(0, 0.8f), new Vector2(1, 1))*/)
                ;


            Add(new Text(string.IsNullOrEmpty(group) ? "BATTERY" : group)
                //, CreateArea(Vector2.Zero, new Vector2(1, 0.1f))
                );
            Add(new BatteryModeSwitch(batt)
                //, CreateArea(new Vector2(0.65f, 0.1f), Vector2.One)
                );
            Add(panel
                //, CreateArea(new Vector2(0, 0.1f), new Vector2(0.65f, 1))
            );
        }

        // protected override List<MySprite> OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref IInteractive interactive)
        // {
        //     var s = _battery.Sum(x => x.CurrentInput - x.CurrentOutput);
        //
        //     if (s > 0)
        //     {
        //         _input.Color = drawer.Style.GoodAccent;
        //         _time.Color = drawer.Style.GoodAccent;
        //     }
        //     else if (s < 0)
        //     {
        //         _input.Color = drawer.Style.BadAccent;
        //         _time.Color = drawer.Style.BadAccent;
        //     }
        //     else
        //     {
        //         _input.Enabled = _time.Enabled = false;
        //     }
        //     
        //     return base.OnDraw(drawer, ref viewport, ref interactive);
        // }
    }

    class GeneratorView : FlexItem
    {
        List<IMyPowerProducer> _powerProducers;
        Text _output, _fuel;

        public GeneratorView(IEnumerable<IMyPowerProducer> power, string group = "")
        {
            _powerProducers = power.ToList();
            _output = new Text(() =>
            {
                var retVal = _powerProducers
                    .Sum(x => (x.CurrentOutput) * 1000000);
                return Math.Abs(retVal) < 1000 ? "" : retVal.ToStringPostfix(false) + "Wh";
            });
            
            _fuel = new Text(() =>
            {
                var inv = _powerProducers
                    .Where(x=> x.HasInventory)
                    .Select(x => x.GetInventory())
                    .ToList();
                var s = inv.Sum(x => (double)x.CurrentVolume.RawValue / x.MaxVolume.RawValue);

                return inv.Count > 0 ? $"Fuel: {s / inv.Count:P0}" : "";
            });
            
            var panel = new FlexItem()
                    .Add(new ProgressBar(() =>
                        _powerProducers.Sum(x => (double) x.CurrentOutput/ x.MaxOutput) / _powerProducers.Count, reversLogic:true))
                    .Add(_output/*, CreateArea(new Vector2(0.01f, 0.05f), new Vector2(0.99f, 0.25f))*/)
                    .Add(_fuel/*, CreateArea(new Vector2(0.01f, 0.6f), new Vector2(0.99f, 0.8f))*/)
                    /*
                    .Add(new Text(() =>
                        (_battery.Sum(x => x.CurrentStoredPower) * 1000000).ToStringPostfix(false) +
                        "Wh / " +
                        (_battery.Sum(x => x.MaxStoredPower) * 1000000).ToStringPostfix(false) +
                        "Wh"
                    ), CreateArea(new Vector2(0, 0.8f), new Vector2(1, 1)))
                    */
                ;


            Add(new Text(string.IsNullOrEmpty(group) ? "GENERATOR" : group)
                //, CreateArea(Vector2.Zero, new Vector2(1, 0.1f)));
            );
            //Add(new BatteryModeSwitch(_battery.ToList(), false),
            //    CreateArea(new Vector2(0.65f, 0.1f), Vector2.One));
            Add(panel
                //, CreateArea(new Vector2(0, 0.1f), new Vector2(0.65f, 1))
                );
            
            
        }
    }
}