using System.Collections.Immutable;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    public struct Product : ISerializer
    {
        public MyItemType Type;
        public int Amount; //  <0 - требуеться; >0 избыток

        public ImmutableArray<string> Serialize()
        {
            return ImmutableArray.Create(new[]
            {
                Type.ToString(), Amount.ToString()
            });
        }
        public object Deserialize(ref ImmutableArray<string> str)
        {
            MyDefinitionId dId; 
            var result = MyDefinitionId.TryParse(str[0], out dId);
            
            Type = result ? (MyItemType) dId : default(MyItemType);
            Amount = int.Parse(str[1]);
            str = str.RemoveRange(0, 2);

            return this;
        }

        public bool Deserialize(MyIniValue iniValue, out object value)
        {
            value = default(Product);
            return false;
        }

        public override string ToString()
        {
            return string.Join("%", Serialize());
        }
    }
}