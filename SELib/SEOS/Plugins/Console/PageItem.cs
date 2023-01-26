using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace IngameScript
{

    abstract class PageItem
    {
        public bool BackgroundVisible;
        public bool BorderVisible = true;

        // MARGIN: x-Left, y-Right, W-Up, Z-Down
        public Vector4 Margin;
        public Alignment Alignment;

        protected bool Highlighting;

        protected PageItem(Alignment alignment = Alignment.Center, Vector4? margin = null)
        {
            Alignment = alignment;
            Margin = margin ?? new Vector4(ConsolePluginSetup.PADDING_PX);
        }

        public static RectangleF CreateArea(Vector2 leftUpPoint, Vector2 rightDownPoint)
        {
            rightDownPoint = Vector2.Clamp(rightDownPoint, Vector2.Zero, Vector2.One);
            leftUpPoint = Vector2.Clamp(leftUpPoint, Vector2.Zero, rightDownPoint);
            rightDownPoint = Vector2.Clamp(rightDownPoint, leftUpPoint, Vector2.One);

            return new RectangleF(leftUpPoint, rightDownPoint - leftUpPoint);
        }

        public virtual void Draw(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites, ref IInteractive interactive)
        {
            var bgColor = drawer.Style.FirstColor;
            if (BackgroundVisible)
                DrawBackground(viewport, ref sprites, Highlighting ? Color.Lighten(bgColor,0.1) : bgColor);

            if (BorderVisible)
                DrawBorder(viewport, ref sprites, drawer.Style.ThirdColor);

            GetViewport(ref viewport);

            var intv = this as IInteractive;
            if (intv != null && viewport.Contains(drawer.ArrowPosition))
                interactive = intv;
        }

        void GetViewport(ref RectangleF viewport)
        {
            var xShift = 0f;
            var yShift = 0f;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (Alignment)
            {
                case Alignment.Left:
                    xShift = viewport.X + Margin.X;
                    break;
                case Alignment.Right:
                    xShift = viewport.X + viewport.Width - Margin.Y;
                    break;
                case Alignment.Up:
                    yShift = viewport.Y - Margin.Z;
                    break;
                case Alignment.Down:
                    yShift = viewport.Y + viewport.Height - Margin.W;
                    break;
                case Alignment.UpLeft:
                    xShift = viewport.X + Margin.X;
                    yShift = viewport.Y - Margin.Z;
                    break;
                case Alignment.UpRight:
                    xShift = viewport.X + viewport.Width - Margin.Y;
                    yShift = viewport.Y - Margin.Z;
                    break;
                case Alignment.DownLeft:
                    xShift = viewport.X + Margin.X;
                    yShift = viewport.Y + viewport.Height - Margin.W;
                    break;
                case Alignment.DownRight:
                    xShift = viewport.X + viewport.Width - Margin.Y;
                    yShift = viewport.Y + viewport.Height - Margin.W;
                    break;
            }

            viewport.Position += new Vector2(xShift, yShift);
        }

        protected void AlignmentToStep(ref RectangleF viewport, Vector2 step)
        {
            viewport.Position = AlignmentToStep(viewport.Position, step);
            viewport.Size = AlignmentToStep(viewport.Size, step);
        }

        Vector2 AlignmentToStep(Vector2 val, Vector2 step)
        {
            var s = val / step;
            s.X = (int) Math.Round(s.X);
            s.Y = (int) Math.Round(s.Y);
            s.X = s.X == 0 ? 1 : s.X;
            s.Y = s.Y == 0 ? 1 : s.Y;

            return s * step;
        }

        protected MySprite GetText(ISurfaceDrawer drawer, string text, RectangleF viewport, bool autoSize,
            Color? color = null)
        {
            var scale = drawer.FontScale;
            var textSize = drawer.MeasureText(text, drawer.FontId, scale);

            if (autoSize)
            {
                if (textSize.X > viewport.Width)
                {
                    scale = viewport.Width / textSize.X;
                }

                if (textSize.Y > viewport.Height)
                {
                    scale = viewport.Height / textSize.Y;
                }

                textSize = drawer.MeasureText(text, drawer.FontId, scale);
            }


            var alt = Alignment == Alignment.Left
                ? TextAlignment.LEFT
                : Alignment == Alignment.Right
                    ? TextAlignment.RIGHT
                    : TextAlignment.CENTER;

            if (textSize.X > viewport.Size.X)
                alt = TextAlignment.LEFT;

            var pos = alt == TextAlignment.LEFT
                ? viewport.Center - new Vector2(viewport.Width / 2, textSize.Y / 2f)
                : alt == TextAlignment.RIGHT
                    ? viewport.Center - new Vector2(-viewport.Width / 2, textSize.Y / 2f)
                    : viewport.Center - new Vector2(0, textSize.Y / 2f);

            return new MySprite
            {
                Type = SpriteType.TEXT,
                Data = text,
                Size = viewport.Size,
                Position = pos,
                Color = color ?? drawer.Style.SecondColor,
                FontId = drawer.FontId,
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

        void DrawBorder(RectangleF viewport, ref List<MySprite> sprites, Color color)
        {
            var w = ConsolePluginSetup.PADDING_PX;
            var xLine = new Vector2(viewport.Width, w);
            var yLine = new Vector2(w, viewport.Height);
            var p1 = viewport.Position + new Vector2(0, viewport.Height - w);
            var p2 = viewport.Position + new Vector2(viewport.Width - w, 0);
            sprites.AddRange(new[]
            {
                GetSprite("SquareSimple", new RectangleF(viewport.Position, xLine), color),
                GetSprite("SquareSimple", new RectangleF(viewport.Position, yLine), color),
                GetSprite("SquareSimple", new RectangleF(p1, xLine), color),
                GetSprite("SquareSimple", new RectangleF(p2, yLine), color),
            });

        }

        void DrawBackground(RectangleF viewport, ref List<MySprite> sprites, Color color)
        {
            sprites.Add(GetSprite("SquareSimple", viewport, color));
        }
    }

    class Text : PageItem, IInteractive
    {
        bool _autoSize;
        Color? _color;
        Action<IConsole> _click;
        ReactiveProperty<string> _txt;

        public Text(string txt, bool autoSize = true, Color? color = null, Action<IConsole> click = null)
        {
            _autoSize = autoSize;
            _color = color;
            _click = click;
            _txt = new ReactiveProperty<string>(txt);
        }

        public Text(Func<string> txt, bool autoSize = true, Color? color = null, Action<IConsole> click = null)
        {
            _autoSize = autoSize;
            _color = color;
            _click = click;
            _txt = new ReactiveProperty<string>(txt);
        }

        public override void Draw(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites, ref IInteractive interactive)
        {
            base.Draw(drawer, ref viewport, ref sprites, ref interactive);
            sprites.Add(GetText(drawer, _txt.Get(), viewport, _autoSize, _color));
        }

        public void OnSelect(IConsole console, double power)
        {
            if (power > 0.7) 
                _click?.Invoke(console);
        }

        public void OnInput(IConsole console, Vector3 dir)
        {
            
        }

        public void OnHoverEnable(bool hover)
        {
            if (_click == null) return;

            Highlighting = hover;
        }
    }
}