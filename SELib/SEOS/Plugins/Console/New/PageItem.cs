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

        protected MySprite GetText(IDrawSurface surface, string text, RectangleF viewport, Color color)
        {
            var scale = surface.FontScale;
            var textSize = surface.MeasureText(text, surface.FontId, scale);
            if (textSize.X > viewport.Width)
            {
                scale = viewport.Width / textSize.X;
            }
            if (textSize.Y > viewport.Height)
            {
                scale = viewport.Height / textSize.Y;
            }
            textSize = surface.MeasureText(text, surface.FontId, scale);
            /*
            if (Vertical)
            {
                return alignment == TextAlignment.LEFT
                    ? viewport.Center - new Vector2(0, viewport.Height / 2f)
                    : alignment == TextAlignment.RIGHT
                        ? viewport.Center + new Vector2(0, viewport.Height / 2f)
                        : viewport.Center;
            }
            */
            var alt = _alignment == Alignment.Left
                ? TextAlignment.LEFT
                : _alignment == Alignment.Right
                    ? TextAlignment.RIGHT
                    : TextAlignment.CENTER;
            
            var pos = alt == TextAlignment.LEFT
                ? viewport.Center - new Vector2(viewport.Width / 2, textSize.Y / 2f)
                : alt == TextAlignment.RIGHT
                    ? viewport.Center - new Vector2(-viewport.Width / 2, textSize.Y / 2f)
                    : viewport.Center - new Vector2(0, textSize.Y / 2f);

            return
                new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = text,
                    Size = viewport.Size,
                    Position = pos,
                    Color = color,
                    FontId = surface.FontId,
                    RotationOrScale = scale,
                    Alignment = alt,
                };
        }

        protected MySprite GetSprite(string texture, RectangleF viewport, Color color, float rotation = 0)
        {
            return new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = texture,
                Size = viewport.Size,
                Position = viewport.Center,
                Color = color,
                RotationOrScale = rotation,
                Alignment = TextAlignment.CENTER,
            };
        }

        protected void SizeOfPadding(ref RectangleF rect)
        {
            rect.Position += ConsolePluginSetup.PADDING_PX;
            rect.Size -= ConsolePluginSetup.PADDING_PX * 2;
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
            surface.AddFrameSprites(new List<MySprite>(){GetText(surface, _txt.Get(), viewport, Color.White)});
        }
    }
}