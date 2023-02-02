using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.GUI.TextPanel;
using VRageMath;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace IngameScript
{

    abstract class PageItem
    {
        public RectangleF? PixelViewport;
        
        public bool Border;
        public bool Background;
        
        public bool Enabled = true;

        // MARGIN: x-Left, y-Right, W-Up, Z-Down
        //public Vector4 Margin;
        public Alignment Alignment = Alignment.Center;
        public Func<float> /*TextScale,*/ ImageScale, Rotation;
        protected bool Highlighting;

        public static RectangleF CreateArea(Vector2 leftUpPoint, Vector2 rightDownPoint)
        {
            rightDownPoint = Vector2.Clamp(rightDownPoint, Vector2.Zero, Vector2.One);
            leftUpPoint = Vector2.Clamp(leftUpPoint, Vector2.Zero, rightDownPoint);
            rightDownPoint = Vector2.Clamp(rightDownPoint, leftUpPoint, Vector2.One);

            return new RectangleF(leftUpPoint, rightDownPoint - leftUpPoint);
        }

        protected virtual void PreDraw(){}
        protected virtual void PostDraw(){}
        protected abstract List<MySprite> OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref IInteractive interactive);
        public void Draw(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites, ref IInteractive interactive)
        {
            PixelViewport = viewport;
            PreDraw();

            if (!Enabled)
            {
                DrawBackground(viewport, ref sprites, drawer.Style.FirstColor.Alpha(0.6f));
            }
            
            var itr = this as IInteractive;
            interactive = Enabled && itr != null && viewport.Contains(drawer.ArrowPosition)? itr : interactive;

            if (Background)
            {
                var c = itr != null ? Color.Lighten(drawer.Style.FirstColor, 0.1) : drawer.Style.FirstColor;
                c = Highlighting && Enabled ? Color.FromNonPremultiplied(0xFF - c.R, 0xFF - c.G, 0xFF - c.B, c.A) : c;

                DrawBackground(viewport, ref sprites, c);
            }

            ToMargin(ref viewport);
            //ToStep(ref viewport, drawer.GridStep);

            var childSprites = OnDraw(drawer, ref viewport, ref interactive);

            sprites.AddRange(childSprites);

            if (Border)
            {
                DrawBorder(viewport, ref sprites, drawer.Style.ThirdColor);
            }

            PostDraw();
            
            //sprites.Add(MySprite.CreateClearClipRect());
        }

        void ToMargin(ref RectangleF viewport)
        {
            //viewport.Position += new Vector2(Margin.X, Margin.Z);
            //viewport.Size -= new Vector2(Margin.X + Margin.Y, Margin.Z + Margin.W);
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

        protected MySprite GetSprite(string texture, RectangleF viewport, Color? color, float rotation, float scale)
        {
            return new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = texture,
                Size = viewport.Size * scale,// * (s ?? 1),
                Position = viewport.Center,
                Color = color,
                RotationOrScale = rotation,// r ?? 0,
                Alignment = TextAlignment.CENTER,
            };
        }

        protected void DrawBorder(RectangleF viewport, ref List<MySprite> sprites, Color color)
        {
            var w = ConsolePluginSetup.PADDING_PX;
            var xLine = new Vector2(viewport.Width, w);
            var yLine = new Vector2(w, viewport.Height);
            var p1 = viewport.Position + new Vector2(0, viewport.Height - w); // TODO: '-2' ХЗ почему если не вычесть не отображается линия нижняя
            var p2 = viewport.Position + new Vector2(viewport.Width - w, 0);
            sprites.AddRange(new[]
            {
                GetSprite("SquareSimple", new RectangleF(viewport.Position, xLine), color, 0, 1),
                GetSprite("SquareSimple", new RectangleF(viewport.Position, yLine), color, 0, 1),
                GetSprite("SquareSimple", new RectangleF(p1, xLine), color, 0, 1),
                GetSprite("SquareSimple", new RectangleF(p2, yLine), color, 0, 1),
            });

            /*
            viewport.Position += w;
            viewport.Size -= 2*w;
            */
        }

        protected void DrawBackground(RectangleF viewport, ref List<MySprite> sprites, Color color)
        {
            viewport.Size -= 2;
            viewport.Position += 1;
            sprites.Add(GetSprite("SquareSimple", viewport, color, 0, 1));
        }
    }

    class Text : PageItem, IText
    {
        public Color? Color;
        ReactiveProperty<string> _txt;
        public float? FontSize { get; set; }

        public Text(string txt)
        {
            _txt = new ReactiveProperty<string>(txt);
        }

        public Text(Func<string> txt)
        {
            _txt = new ReactiveProperty<string>(txt);
        }

        protected override List<MySprite> OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref IInteractive interactive)
        {
            return new List<MySprite>{ GetText(drawer, _txt.Get(), viewport, Color ?? drawer.Style.SecondColor)};
        }

        MySprite GetText(ISurfaceDrawer drawer, string text, RectangleF viewport, Color? color)
        {
            viewport.Size -= 1;
            var fSize = FontSize ?? drawer.FontSize;
            var textSize = drawer.MeasureText(text, drawer.FontId, fSize);
            var scaleTxt = viewport.Size / textSize;

            var scale = Math.Min(Math.Min(scaleTxt.X, scaleTxt.Y), fSize);

            textSize = drawer.MeasureText(text, drawer.FontId, scale);

            FontSize = scale;
            //TextScale = ()=>scale;
            
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

    class Image : PageItem
    {
        string _texture;
        public Color? Color;

        public Image(string texture)
        {
            _texture = texture;
        }
        protected override List<MySprite> OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref IInteractive interactive)
        {
            return new List<MySprite> {GetSprite(_texture, viewport, Color, Rotation?.Invoke() ?? 0, ImageScale?.Invoke() ?? 1)};
        }
    }
    class Link : Text, IInteractive
    {
        Action<IConsole> _select;
        public Link(string txt, Action<IConsole> @select) : base(txt)
        {
            Border = false;
            Background = true;
            
            _select = @select;
        }

        public Link(Func<string> txt, Action<IConsole> @select) : base(txt)
        {
            Border = false;
            Background = true;
            
            _select = @select;
        }

        public void OnSelect(IConsole console) => _select(console);

        public void OnEsc(IConsole console)
        {
        }

        public void OnInput(IConsole console, Vector3 dir)
        {

        }

        public void OnHoverEnable(bool hover)
        {
            Highlighting = hover;
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

        protected override List<MySprite> OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref IInteractive interactive)
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
            
            var retVal = new List<MySprite>{GetSprite("SquareSimple", new RectangleF(position, size), cinv, 0, 1)};
            DrawBorder(viewport, ref retVal, cinv);
            
            _text.Draw(drawer, ref viewport, ref retVal, ref interactive);

            return retVal;
        }
    }
}