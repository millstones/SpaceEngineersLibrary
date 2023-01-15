using System;

namespace IngameScript
{
    class ReactiveProperty<TProp>
    {
        Action<TProp> _set;
        Func<TProp> _get;

        public ReactiveProperty(Func<TProp> get, Action<TProp> set = null)
        {
            _set = set ?? (e => { });
            _get = get; 
        }

        public ReactiveProperty(TProp get)
        {
            _get = () => get;
        }

        public TProp Get() => _get();
        public void Set(TProp val) => _set(val);
    }

    class ObjectReactiveProperty : ReactiveProperty<object>
    {
        public ObjectReactiveProperty(Func<object> get, Action<object> set = null) : base(get, set ?? (e => { }))
        { }
    }
}