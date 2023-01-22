using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace IngameScript.New
{
    abstract class PageItem
    {
        public Vector2 PosShift, SizeScale;
        public Vector2 MinSize;
        Alignment _alignment = Alignment.Center;
        Vector4 _margin = Vector4.Zero;

        protected PageItem(Vector2? minSize=null, Vector2? posShift=null, Vector2? sizeScale=null)
        {
            MinSize = minSize ?? Vector2.One;
            PosShift = posShift ?? Vector2.Zero;
            SizeScale = sizeScale ?? Vector2.One;
        }
        public static RectangleF CreateArea(Vector2 leftUpPoint, Vector2 rightDownPoint)
        {
            rightDownPoint = Vector2.Clamp(rightDownPoint, Vector2.Zero, Vector2.One);
            leftUpPoint = Vector2.Clamp(leftUpPoint, Vector2.Zero, rightDownPoint);
            rightDownPoint = Vector2.Clamp(rightDownPoint, leftUpPoint, Vector2.One);

            return new RectangleF(leftUpPoint, rightDownPoint - leftUpPoint);
        }
        public void Draw(IDrawSurface surface, ref RectangleF viewport)
        {
            var xShift = 0f;
            var yShift = 0f;
            
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (_alignment)
            {
                case Alignment.Left:
                    xShift = viewport.X + _margin.X;
                    break;
                case Alignment.Right:
                    xShift = viewport.X + viewport.Width - _margin.Y;
                    break;
                case Alignment.Up:
                    yShift = viewport.Y - _margin.Z;
                    break;
                case Alignment.Down:
                    yShift = viewport.Y + viewport.Height - _margin.W;
                    break;
                case Alignment.UpLeft:
                    xShift = viewport.X + _margin.X;
                    yShift = viewport.Y - _margin.Z;
                    break;
                case Alignment.UpRight:
                    xShift = viewport.X + viewport.Width - _margin.Y;
                    yShift = viewport.Y - _margin.Z;
                    break;
                case Alignment.DownLeft:
                    xShift = viewport.X + _margin.X;
                    yShift = viewport.Y + viewport.Height - _margin.W;
                    break;
                case Alignment.DownRight:
                    xShift = viewport.X + viewport.Width - _margin.Y;
                    yShift = viewport.Y + viewport.Height - _margin.W;
                    break;
            }
            
            viewport.Position += new Vector2(xShift, yShift) + PosShift;
            viewport.Size *= SizeScale;
            
            viewport.Position = AlignmentToStep(viewport.Position, surface.GridStep);
            viewport.Size = AlignmentToStep(viewport.Size, surface.GridStep);

            OnDraw(surface, ref viewport);
        }

        Vector2 AlignmentToStep(Vector2 val, Vector2 step)
        {
            var s = val / step;
            s.X = (int)Math.Round(s.X);
            s.Y = (int)Math.Round(s.Y);
            s.X = s.X == 0 ? 1 : s.X;
            s.Y = s.Y == 0 ? 1 : s.Y;
            
            return s * step; 
        }

        protected abstract void OnDraw(IDrawSurface surface, ref RectangleF viewport);
    }

    class Text : PageItem
    {
        ReactiveProperty<string> _txt;

        public Text(string txt)
        {
            _txt = new ReactiveProperty<string>(txt);
        }

        public Text(Func<string> txt)
        {
            _txt = new ReactiveProperty<string>(txt);
        }
        protected override void OnDraw(IDrawSurface surface, ref RectangleF viewport)
        {
            throw new NotImplementedException();
        }
    }
}