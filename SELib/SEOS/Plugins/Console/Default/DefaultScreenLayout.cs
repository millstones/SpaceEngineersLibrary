using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    class DefaultScreenConsoleManager : ConsoleManager
    {
        public DefaultScreenConsoleManager(IEnumerable<IUserContent> content) : base(content)
        {
            //Style = ConsoleStyle.BlackWhiteRed;


            var userPanel = new ContentPanel()
                .Add(new ContentText(ConsolePluginSetup.LOGO))
                .Add(new ContentText(new ReactiveProperty<string>(() => DateTime.Now.ToLongTimeString())))
                .Add(new ContentIconButton("Danger", (c) => c.SwitchPage("Cargo manager")))
                ;
            
            UseDefaultUserContent(userPanel);
        }
    }
}