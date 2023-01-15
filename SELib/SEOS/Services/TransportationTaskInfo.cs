using System;
using System.Collections.Immutable;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    struct TransportationTaskInfo : ISerializer, IEquatable<TransportationTaskInfo>
    {
        public GridInfo From, To;
        public Product ProductRequest;

        public static TransportationTaskInfo Default => new TransportationTaskInfo
        {
            From = GridInfo.Default, To = GridInfo.Default, ProductRequest = default(Product)
        };
    
        public bool Equals(TransportationTaskInfo other)
        {
            return From.Equals(other.From) && To.Equals(other.To) && ProductRequest.Equals(other.ProductRequest);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TransportationTaskInfo && Equals((TransportationTaskInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = From.GetHashCode();
                hashCode = (hashCode * 397) ^ To.GetHashCode();
                hashCode = (hashCode * 397) ^ ProductRequest.GetHashCode();
                return hashCode;
            }
        }

        public ImmutableArray<string> Serialize()
        {
            var retVal = ImmutableArray.CreateBuilder<string>();
            retVal.AddRange(To.Serialize());
            retVal.AddRange(ProductRequest.Serialize());
            
            return retVal.ToImmutable();
        }

        public object Deserialize(ref ImmutableArray<string> str)
        {
            From.Deserialize(ref str);
            To.Deserialize(ref str);
            ProductRequest.Deserialize(ref str);

            return this;
        }

        public bool Deserialize(MyIniValue iniValue, out object value)
        {
            value = Default;
            return true;
        }
    }
}