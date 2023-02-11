using System.Collections.Generic;
using System.Collections.Immutable;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript
{
    struct DroneActualStatus : ISerializer
    {
        public GridInfo GridInfo;
        public float energy;
        public float cargo;

        public static DroneActualStatus Default => new DroneActualStatus
        {
            GridInfo = default(GridInfo),
            energy = -1,
            cargo = -1,
        };
        public ImmutableArray<string> Serialize()
        {
            return ImmutableArray.Create(cargo.ToString(), energy.ToString(), GridInfo.ToString());
        }

        public object Deserialize(ref ImmutableArray<string> str)
        {
            cargo = float.Parse(str[0]);
            energy = float.Parse(str[1]);
            str = str.RemoveRange(0, 2);
            GridInfo.Deserialize(ref str);

            return this;
        }

        public bool Deserialize(MyIniValue iniValue, out object value)
        {
            // X3 !!!!
            value = Default;
            var lines = new List<string>();
            iniValue.GetLines(lines);
            return true; //lines.Count == 1 && Deserialize(lines[0], out value);
        }
        
        public override string ToString()
        {
            return string.Join("%", Serialize());
        }
    }
}