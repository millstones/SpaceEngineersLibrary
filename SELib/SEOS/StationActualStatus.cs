using System;
using System.Collections.Immutable;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    struct StationActualStatus : ISerializer
    {
        public GridInfo GridInfo;
        public Product Deficit;
        public Product Surplus;
        public static StationActualStatus Default => new StationActualStatus
        {
            Deficit = default(Product), Surplus = default(Product), GridInfo = GridInfo.Default
        };

        public ImmutableArray<string> Serialize()
        {
            var retVal = ImmutableArray.CreateBuilder<string>();
            retVal.AddRange(GridInfo.Serialize());
            retVal.AddRange(Deficit.Serialize());
            retVal.AddRange(Surplus.Serialize());
            return retVal.ToImmutable();
        }

        public object Deserialize(ref ImmutableArray<string> str)
        {
            GridInfo.Deserialize(ref str);
            Deficit.Deserialize(ref str);
            Surplus.Deserialize(ref str);

            return this;
        }

        public bool Deserialize(MyIniValue iniValue, out object value)
        {
            value = Default;
            return true;
        }

        public override string ToString()
        {
            return string.Join("%", Serialize());
        }
    }
}