using System;
using System.Collections.Immutable;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Serialization;

namespace IngameScript
{
    interface ISerializer
    {
        ImmutableArray<string> Serialize();
        /// <summary>
        /// Возвращает длинну масива сериализованных данных. 0 - в случае неудачной десереализации
        /// </summary>
        object Deserialize(ref ImmutableArray<string> str);
        bool Deserialize(MyIniValue iniValue, out object value);
    }
}