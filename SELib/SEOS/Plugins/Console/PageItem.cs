using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace IngameScript
{

    abstract class PageItem
    {
        public bool Border;
        public bool Background;

        public bool Visible = true;
        public bool Enabled = true;

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

        protected abstract void OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites,
            ref IInteractive interactive);
        public void Draw(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites, ref IInteractive interactive)
        {
            if (!Visible) return;
            
            sprites.Add(MySprite.CreateClipRect(new Rectangle(
                (int) viewport.Position.X, (int) viewport.Position.Y,
                (int) viewport.Width, (int) viewport.Height)));

            
            if (Background)
            {
                DrawBackground(viewport, ref sprites, drawer.Style.FirstColor);
            }

            
            if (Border)
                DrawBorder(ref viewport, ref sprites, drawer.Style.ThirdColor);

            ToMargin(ref viewport);
            //ToStep(ref viewport, drawer.GridStep);
            OnDraw(drawer, ref viewport, ref sprites, ref interactive);

            sprites.Add(MySprite.CreateClearClipRect());
        }

        void ToMargin(ref RectangleF viewport)
        {
            viewport.Position += new Vector2(Margin.X, Margin.Z);
            viewport.Size -= new Vector2(Margin.X + Margin.Y, Margin.Z + Margin.W);
        }

        void ToStep(ref RectangleF viewport, Vector2 step)
        {
            var center = AlignmentToStep(viewport.Center, step) + step/2;
            var d = center - viewport.Center;
            viewport.Position += d;
            //viewport.Size = AlignmentToStep(viewport.Size, step);
            viewport.Size -= 2*d;
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

        protected MySprite GetSprite(string texture, RectangleF viewport, Color? color, float rotation = 0)
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

        protected void DrawBorder(ref RectangleF viewport, ref List<MySprite> sprites, Color color)
        {
            var w = ConsolePluginSetup.PADDING_PX;
            var xLine = new Vector2(viewport.Width, w);
            var yLine = new Vector2(w, viewport.Height);
            var p1 = viewport.Position + new Vector2(0, viewport.Height - w-2); // TODO: '-2' ХЗ почему если не вычесть не отображается линия нижняя
            var p2 = viewport.Position + new Vector2(viewport.Width - w, 0);
            sprites.AddRange(new[]
            {
                GetSprite("SquareSimple", new RectangleF(viewport.Position, xLine), color),
                GetSprite("SquareSimple", new RectangleF(viewport.Position, yLine), color),
                GetSprite("SquareSimple", new RectangleF(p1, xLine), color),
                GetSprite("SquareSimple", new RectangleF(p2, yLine), color),
            });

            viewport.Position += w;
            viewport.Size -= 2*w;
        }

        protected void DrawBackground(RectangleF viewport, ref List<MySprite> sprites, Color color)
        {
            sprites.Add(GetSprite("SquareSimple", viewport, color));
        }
    }

    abstract class InteractivePageItem : PageItem, IInteractive
    {
        protected Action<IConsole> Select;
        protected Action Deselect;


        protected InteractivePageItem(Action<IConsole> @select=null)
        {
            Select = @select;
        }
        public void OnSelect(IConsole console, double power)
        {
            if (Enabled && power > 0.7) 
                Select.Invoke(console);
            if (Enabled && power < -0.7)
                OnDeselect();
        }

        public void OnInput(IConsole console, Vector3 dir)
        {
            
        }

        public void OnHoverEnable(bool hover)
        {
            if (Select == null || !Enabled) return;

            Highlighting = hover;
        }

        public virtual void OnDeselect()
        {
            Deselect?.Invoke();
        }
        
        protected override void OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites,
            ref IInteractive interactive)
        {
            if (viewport.Contains(drawer.ArrowPosition))
                interactive = this;
            
            if (Background)
            {
                DrawBackground(viewport, ref sprites,
                    Highlighting ? drawer.Style.FirstColor.Invert() : Color.Lighten(drawer.Style.FirstColor, 0.1));
            }

            DrawInternal(drawer, ref viewport, ref sprites, ref interactive);
            
            if (!Enabled)
                DrawBackground(viewport, ref sprites, Color.Black.Alpha(0.6f));
        }

        protected abstract void DrawInternal(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites,
            ref IInteractive interactive);
    }

    class Text : PageItem
    {
        public Color? Color;
        ReactiveProperty<string> _txt;
        public float? TextScale;
        public Text(string txt, float? scale = null, Color? color = null)
        {
            Build(new ReactiveProperty<string>(txt), scale, color);
        }

        public Text(Func<string> txt, float? scale = null, Color? color = null)
        {
            Build(new ReactiveProperty<string>(txt), scale, color);
        }

        void Build(ReactiveProperty<string> txt, float? scale, Color? color)
        {
            Color = color;
            _txt = txt;
            TextScale = scale;
        }

        protected override void OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites, ref IInteractive interactive)
        {
            sprites.Add(GetText(drawer, _txt.Get(), viewport, TextScale, Color ?? drawer.Style.SecondColor));
        }
        MySprite GetText(ISurfaceDrawer drawer, string text, RectangleF viewport, float? size = null, Color? color = null)
        {
            viewport.Size -= 1;
            var scale = drawer.FontScale;
            var textSize = drawer.MeasureText(text, drawer.FontId, scale);
            if (size.HasValue)
            {
                scale *= size.Value;
                textSize = drawer.MeasureText(text, drawer.FontId, scale);
            }
            else
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
                Color = color,
                FontId = drawer.FontId,
                RotationOrScale = scale,
                Alignment = alt,
            };
        }
    }

    class Link : InteractivePageItem
    {
        Text _text;
        public Link(string txt, Action<IConsole> @select, float? scale = null, Color? color = null) : base(@select) 
        {
            _text = new Text(txt, scale, color);
        }
        protected override void DrawInternal(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites, ref IInteractive interactive)
        {
            _text.Draw(drawer, ref viewport, ref sprites, ref interactive);
        }
    }
    class Image : PageItem
    {
        readonly string _texture;
        public Color? Color;
        public float Rotation;

        public Image(string texture, int gridStepSize, float rotation = 0, Color? color=null)
        {
            _texture = texture;
            Rotation = rotation;
            Color = color;
        }
        protected override void OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites,
            ref IInteractive interactive)
        {
            sprites.Add(GetSprite(_texture, viewport, Color, Rotation));
        }
    }
    class ProgressBar : PageItem
    {
        double _minimum;
        bool _vertical;
        ReactiveProperty<double> _amount;
        Text _text;
        public ProgressBar(Func<double> amount, double minimum = 0.1, bool vertical = true)
        {
            _minimum = minimum;
            _vertical = vertical;
            _amount = new ReactiveProperty<double>(amount);
            
            _text = new Text(() => amount().ToString("P0"));
        }

        protected override void OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites, ref IInteractive interactive)
        {
            var amount = _amount.Get();
            var progress = Math.Max(amount, _minimum);

            var size = _vertical
                    ? new Vector2(viewport.Size.X, (float) (viewport.Size.Y * progress))
                    : new Vector2((float) (viewport.Size.X * progress), viewport.Size.Y)
                ;
            
            var position = _vertical
                    ? new Vector2(viewport.Position.X, viewport.Position.Y + viewport.Size.Y - size.Y)
                    : new Vector2(viewport.Position.X, viewport.Position.Y)
                ;

            var c = drawer.Style.SecondColor;
            var cinv = Color.FromNonPremultiplied(0xFF - c.R, 0xFF - c.G, 0xFF - c.B, c.A);
            sprites.Add(GetSprite("SquareSimple", new RectangleF(position, size), cinv));
            DrawBorder(ref viewport, ref sprites, cinv);
            
            _text.Draw(drawer, ref viewport, ref sprites, ref interactive);
        }
    }
}