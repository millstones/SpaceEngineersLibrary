using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    interface IStorage
    {
        void IniAndSave(MyIniKey key, ObjectReactiveProperty property);
        void ForceIni(MyIniKey key);
        void ForceSave(MyIniKey key);
        void Save();
    }
    
    class PBCustomDataStorage : IStorage
    {
        MyIni _ini;
        IMyTerminalBlock _blockData;
        Dictionary<MyIniKey, ObjectReactiveProperty> _props = new Dictionary<MyIniKey, ObjectReactiveProperty>();
        //Dictionary<MyIniKey, KeyValuePair<object,  ReactiveProperty<object, object>>> _getters = new Dictionary<MyIniKey, KeyValuePair<object,  ReactiveProperty<object, object>>>();
        public PBCustomDataStorage(IMyTerminalBlock blockData)
        {
            _blockData = blockData;
            _ini = new MyIni();
            _ini.TryParse(blockData.CustomData);
        }

        public void IniAndSave(MyIniKey key, ObjectReactiveProperty property)
        {
            if (_props.ContainsKey(key)) return;

            //_setters.Add(key, o => property.Set((TProp)o));
            //_getters.Add(key, new KeyValuePair<object, ReactiveProperty<object, object>>(obj, property);
            _props.Add(key, property);

            var iniValue = GetValue(key);
            //set( _serializers.Deserialize<TProp>(iniValue));
        }

        public void ForceIni(MyIniKey key)
        {
            if (!_props.ContainsKey(key)) return;
            var iniValue = GetValue(key);
            
            _props[key].Set(iniValue);
        }

        MyIniValue GetValue(MyIniKey key)
        {
            var iniValue = _ini.Get(key);
            if (iniValue.IsEmpty)
            {
                ForceSave(key);
            }
            //throw new Exception($"Key {key} not found in storage for initialization");

            return iniValue;
        }

        public void ForceSave(MyIniKey key)
        {
            if (_props.ContainsKey(key))
            {
                var property = _props[key].Get();
                //var s = _serializers.Serialize(property.GetType(), property);
                // _ini.Set(key, s);
            }
        }

        public void Save()
        {
            _blockData.CustomData = _ini.ToString();
        }
    }
}