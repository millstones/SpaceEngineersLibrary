using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    abstract class Page : WPFContainerItem<WPFItem>
    {
        public string TitleId;
        protected Page(string titleId, string def="") : base(null, def)
        {
            TitleId = titleId;
        }

        public override void Draw(ref List<MySprite> sprites, ref IInteractive newInteractive, Func<string, float, Vector2> measureStringInPixels, float textScale,
            Vector2 arrowPos)
        {
            DrawBG(ref sprites);
            
            base.Draw(ref sprites, ref newInteractive, measureStringInPixels, textScale, arrowPos);
        }
    }

    class Page404 : Page
    {
        public Page404(string notFoundedPageName = "") : base("ERROR 404")
        {
            Add(new Text($"Page {notFoundedPageName} not found"));
        }
    }
}