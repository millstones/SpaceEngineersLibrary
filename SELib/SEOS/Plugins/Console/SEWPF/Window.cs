using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript.SEWPF
{
    class Window : WPFItem
    {
        public string Title;
        List<WPFItem> WPFContent = new List<WPFItem>();
        

        public Window(string title, string def) : base(def)
        {
            Title = title;
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

        public override void Draw(ref List<MySprite> sprites)
        {
            DrawBG(ref sprites);
            
            foreach (var wpfItem in WPFContent)
            {
                wpfItem.Draw(ref sprites);
            }
        }

        public void Add(WPFItem itm)
        {
            WPFContent.Add(itm);
        }
        
    }
}