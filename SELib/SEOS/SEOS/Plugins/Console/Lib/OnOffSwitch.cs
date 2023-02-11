using System;
using System.Collections.Generic;

namespace IngameScript
{
    class OnOffSwitch : SwitchPanel<bool>
    {
        public OnOffSwitch(ReactiveProperty<bool> prop, bool vertical) : base(vertical)
        {
            Add(new Switch<bool>(new Text("ON"), true, prop));
            Add(new Switch<bool>(new Text("OFF"), false, prop));
        }
    }

    class StringSwitcher : SwitchPanel<string>
    {
        ReactiveProperty<string> _prop;

        public StringSwitcher(ReactiveProperty<string> prop, bool vertical) : base(vertical)
        {
            _prop = prop;
        }

        public StringSwitcher Add(params string[] item)
        {
            foreach (var s in item)
            {
                base.Add(new StringSwitch(s, _prop));
            }

            return this;
        }
    }
}