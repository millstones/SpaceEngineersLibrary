using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript.SEWPF
{
    abstract class WPFControl : WPFItem
    {
        protected WPFControl(string def) : base(def)
        {
        }

        protected override void OnResize(RectangleF viewport)
        {
            var margin = Canvas.ContentDefinition.Margin ?? Vector4.Zero;
            var xShift = 0f;
            var yShift = 0f;
            
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (Canvas.ContentDefinition.Alignment)
            {
                case Alignment.Left:
                    xShift = viewport.X + margin.X;
                    break;
                case Alignment.Right:
                    xShift = viewport.X + viewport.Width - margin.Y;
                    break;
                case Alignment.Up:
                    yShift = viewport.Y - margin.Z;
                    break;
                case Alignment.Down:
                    yShift = viewport.Y + viewport.Height - margin.W;
                    break;
                case Alignment.UpLeft:
                    xShift = viewport.X + margin.X;
                    yShift = viewport.Y - margin.Z;
                    break;
                case Alignment.UpRight:
                    xShift = viewport.X + viewport.Width - margin.Y;
                    yShift = viewport.Y - margin.Z;
                    break;
                case Alignment.DownLeft:
                    xShift = viewport.X + margin.X;
                    yShift = viewport.Y + viewport.Height - margin.W;
                    break;
                case Alignment.DownRight:
                    xShift = viewport.X + viewport.Width - margin.Y;
                    yShift = viewport.Y + viewport.Height - margin.W;
                    break;
            }
            
            Canvas.Viewport.Position += new Vector2(xShift, yShift);
        }
    }

    class Text : WPFControl
    {
        object _txt;

        public Text(string def, string txt="") : base(def)
        {
            _txt = Canvas.ContentDefinition.Data ?? txt;
        }

        public override void Draw(ref List<MySprite> sprites)
        {
            var textScale = 0.75f;
            sprites.Add(GetText(_txt.ToString(), Canvas.Viewport, Canvas.ContentDefinition.Color ?? Style.SecondColor, textScale, Style.FontId));
        }
    }
}