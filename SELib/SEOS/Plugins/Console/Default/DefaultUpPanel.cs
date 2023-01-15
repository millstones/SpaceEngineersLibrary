using System;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    class DefaultUpContentPanel : ContentPanel
    {
        ContentPanel _info = new ContentPanel();
        ContentPanel _tray = new ContentPanel();
        public DefaultUpContentPanel(Console console)
        {
            
            _info
                .Add(new ContentText(new ReactiveProperty<string>(()=> $"Arrow pos: {console}")))
                /*
                .Add(new ContentText(()=> console == null
                    ? "CONSOLE INPUT CONTROLLER NOT FOUND"
                    : console.ArrowPosition.ToString(), border: false ))
                    */
                ;
            _tray
                .Add(new ContentIconButton(new ReactiveProperty<string>("IconEnergy"), (c) => { }, () => true))
                .Add(new ContentIconButton(new ReactiveProperty<string>("IconHydrogen"), (c) => { }, () => true))
                .Add(new ContentIconButton(new ReactiveProperty<string>("IconOxygen"), (c) => { }, () => true))
                .Add(new ContentImage(new ReactiveProperty<string>("No Entry")))
                .Add(new ContentText(new ReactiveProperty<string>(DateTime.Now.ToLongTimeString)))
                ;

            Add(_info);
            Add(_tray);
        }
    }
}