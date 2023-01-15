using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    abstract class Page : WPFItem
    {
        public string TitleId;
        List<WPFItem> WPFContent = new List<WPFItem>();


        protected Page(string titleId, string def="") : base(def)
        {
            TitleId = titleId;
        }

        protected override void OnResize(RectangleF viewport)
        {
            foreach (var wpfItem in WPFContent)
            {
                wpfItem.Resize(Canvas.Viewport);
            }
        }
        
        public override void SetStyle(ConsoleStyle style)
        {
            base.SetStyle(style);
            
            foreach (var wpfItem in WPFContent)
            {
                wpfItem.SetStyle(style);
            }
        }

        public override void Draw(ref List<MySprite> sprites, ref IInteractive newInteractive, Func<string, float, Vector2> measureStringInPixels, float textScale,
            Vector2 arrowPos)
        {
            DrawBG(ref sprites);
            
            foreach (var wpfItem in WPFContent)
            {
                wpfItem.Draw(ref sprites, ref newInteractive, measureStringInPixels, textScale, arrowPos);
            }
        }

        public void Add(WPFItem itm)
        {
            WPFContent.Add(itm);
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