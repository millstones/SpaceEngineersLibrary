using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    class BatteryView : FreeCanvas
    {
        IEnumerable<IMyBatteryBlock> _battery;
        Text _input, _time;

        public BatteryView(IEnumerable<IMyBatteryBlock> battery, string group = "")
        {
            _battery = battery;
            _input = new Text(() =>
            {
                var retVal = _battery.Sum(x => (x.CurrentInput - x.CurrentOutput) * 1000000);
                return Math.Abs(retVal) < 1000 ? "" : retVal.ToStringPostfix(true) + "Wh";
            });
            _time = new Text(() =>
            {
                var input = _battery.Sum(x => x.CurrentInput - x.CurrentOutput);
                var target = input > 0
                    ? _battery.Sum(x => x.MaxStoredPower - x.CurrentStoredPower)
                    : _battery.Sum(x => x.CurrentStoredPower);

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
                //
                // var output = sec.Days > 0 ? $"{sec.Days:0}d:" : "";
                // output += sec.Hours > 0 ? $"{sec.Hours:00}h:" : "";
                // output += sec.Minutes > 0 ? $"{sec.Minutes:00}m:" : "";
                // output += $"{sec.Seconds:00}s";
                // return output;
            });
            var panel = new FreeCanvas() //, CreateArea(new Vector2(0, 0.1f), new Vector2(0.65f, 1)))
                    .Add(new ProgressBar(() =>
                        _battery.Sum(x => (double) x.CurrentStoredPower / x.MaxStoredPower) / _battery.Count()))
                    .Add(_input, CreateArea(new Vector2(0, 0.05f), new Vector2(1, 0.25f)))
                    .Add(_time, CreateArea(new Vector2(0, 0.6f), new Vector2(1, 0.8f)))
                    .Add(new Text(() =>
                        (_battery.Sum(x => x.CurrentStoredPower) * 1000000).ToStringPostfix(false) +
                        "Wh / " +
                        (_battery.Sum(x => x.MaxStoredPower) * 1000000).ToStringPostfix(false) +
                        "Wh"
                    ), CreateArea(new Vector2(0, 0.8f), new Vector2(1, 1)))
                ;


            Add(new Text(string.IsNullOrEmpty(group) ? "BATTERY" : group) {Border = true},
                CreateArea(Vector2.Zero, new Vector2(1, 0.1f)));
            Add(new BatteryModeSwitch(_battery.ToList(), false),
                CreateArea(new Vector2(0.65f, 0.1f), Vector2.One)
            );
            Add(panel,
                CreateArea(new Vector2(0, 0.1f), new Vector2(0.65f, 1)));
        }

        protected override void PreDraw()
        {
            var s = _battery.Sum(x => x.CurrentInput - x.CurrentOutput);

            if (s > 0)
            {
                _input.Color = Color.DarkGreen;
                _time.Color = Color.DarkGreen;
            }
            else if (s < 0)
            {
                _input.Color = Color.DarkRed;
                _time.Color = Color.DarkRed;
            }
            else
            {
                _input.Enabled = _time.Enabled = false;
            }

            base.PreDraw();
        }
    }
}