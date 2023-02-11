using System;
using System.Collections.Immutable;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript
{
    public struct GridInfo : ISerializer, IEquatable<GridInfo>
    {
        public long Id;
        public string GroupName, Name, CustomDate;
        public bool IsStatic;
        public Vector3D Position;
        public string FulName => string.Join("-", GroupName, Name, Id);

        public static GridInfo Default => new GridInfo
        {
            Id = -1,
            Name = "default",
            Position = Vector3D.Zero,
            CustomDate = "not define",
            GroupName = "not define",
            IsStatic = false,
        };

        public ImmutableArray<string> Serialize()
        {
            return ImmutableArray.Create(new[] {GroupName, Name, Id.ToString(), Position.ToString()});
        }

        public object Deserialize(ref ImmutableArray<string> str)
        {
            if (!Vector3D.TryParse(str[3], out Position)) return null;
                
            GroupName = str[0];
            Name = str[1];
            Id = long.Parse(str[2]);
            str = str.RemoveRange(0, 4);

            return this;
        }

        public bool Deserialize(MyIniValue iniValue, out object value)
        {
            value = new GridInfo {Id = -1, Name = "NOT IMPLEMENT METHOD !!!!"};
            return false;
        }

        public bool Equals(GridInfo other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is GridInfo && Equals((GridInfo) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        
        public override string ToString()
        {
            return string.Join("%", Serialize());
        }
    }
}