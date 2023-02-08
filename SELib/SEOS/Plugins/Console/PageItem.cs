using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using VRage.Game.GUI.TextPanel;
using VRageMath;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace IngameScript
{

    abstract class PageItem
    {
        public RectangleF? PixelViewport;

        public Color? BGColor, BorderColor;
        
        public bool Border;
        public bool Background;
        
        public bool Enabled = true;

        // MARGIN: x-Left, y-Right, W-Up, Z-Down
        public Vector4 Margin = new Vector4(1);
        public Alignment Alignment = Alignment.Center;
        public float? /*TextScale,*/ ImageScale, Rotation;
        //protected bool Hover;

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

            var itr = this as IInteractive;
            interactive = Enabled && itr != null && viewport.Contains(drawer.ArrowPosition)? itr : interactive;


            if (!Enabled || Background)
            {
                var bgColor = interactive == this
                    ? drawer.Style.FirstColor.Inverse()
                    : Enabled && BGColor.HasValue
                        ?BGColor.Value
                        : Enabled && itr != null
                            ? Color.Lighten(drawer.Style.FirstColor, 0.5)
                            : !Enabled 
                                ? Color.Darken(drawer.Style.FirstColor, 0.5).Alpha(0.1f)
                                : drawer.Style.FirstColor;
                
                DrawBackground(viewport, ref sprites, bgColor);
            }

            ToMargin(ref viewport);
            //ToStep(ref viewport, drawer.GridStep);

            var txt = this as Text;
            if (txt != null && interactive == this)
            {
                txt.Color = drawer.Style.FirstColor.Inverse().Inverse();
            }
            var childSprites = OnDraw(drawer, ref viewport, ref interactive);
            
            if (txt != null && interactive == this)
            {
                txt.Color = null;
            }
            
            
            sprites.AddRange(childSprites);

            if (Enabled && Border)
            {
                DrawBorder(viewport, ref sprites, drawer.Style.ThirdColor);
            }


            PostDraw();
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
            var p1 = viewport.Position + new Vector2(0, viewport.Height - w);
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

        void DrawBackground(RectangleF viewport, ref List<MySprite> sprites, Color color)
        {
            viewport.Size -= 2;
            viewport.Position += 1;
            sprites.Add(GetSprite("SquareSimple", viewport, color, 0, 1));
        }
    }

    abstract class Drawable : PageItem
    {
        public Color? Color;
    }
    class Text : Drawable, IText
    {
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
            return Enabled 
                ? new List<MySprite>{ GetText(drawer, _txt.Get(), viewport, Color ?? drawer.Style.SecondColor)}
                : new List<MySprite>();
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
    class Image : Drawable
    {
        string _texture;

        public Image(string texture)
        {
            _texture = texture;
        }
        protected override List<MySprite> OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref IInteractive interactive)
        {
            return Enabled
                ? new List<MySprite> {GetSprite(_texture, viewport, Color, Rotation ?? 0, ImageScale ?? 1)}
                : new List<MySprite>();
        }
    }
    class Link : FreeCanvas, IInteractive
    {
        public Action<IConsole> Select;
        public bool Pressed;
        public Link(Action<IConsole> @select)// : base(txt)
        {
            Border = false;
            Background = true;
            
            Select = @select;
        }

        public virtual void OnSelect(IConsole console)
        {
            Pressed = true;
            Select(console);
        }

        public virtual void OnEsc(IConsole console)
        {
            Pressed = false;
        }

        public virtual void OnInput(IConsole console, Vector3 dir)
        {}
    }

    class Button : Link
    {
        Drawable _content;
        
        public Button(Drawable content, Action<IConsole> @select) : base(@select)
        {
            _content = content;
            Select += console => Pressed = true;
        }

        protected override List<MySprite> OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref IInteractive interactive)
        {
            var retVal = base.OnDraw(drawer, ref viewport, ref interactive);
            if (Pressed)
            {
                BGColor = Color.Lighten(drawer.Style.Akcent, 0.2f);//drawer.Style.TrueColor;
                Border = true;
                _content.Color =  BGColor.Value.Inverse();
            }
            else
            {
                BGColor = Color.Darken(drawer.Style.Akcent, 0.2f);
                Border = false;
                _content.Color = null;
            }

            _content.Draw(drawer, ref viewport, ref retVal, ref interactive);

            retVal.AddRange(base.OnDraw(drawer, ref viewport, ref interactive));

            return retVal;
        }
    }

    class Switch<T> : Button
    {
        public readonly T Value;
        ReactiveProperty<T> _prop;

        public Switch(Drawable content, T value, ReactiveProperty<T> prop) : base(content, console =>prop.Set(value))
        {
            Value = value;
            _prop = prop;
        }

        public override void OnEsc(IConsole console)
        {
            //base.OnEsc(console);
        }
        
        protected override void PreDraw()
        {
            Pressed = _prop.Get().Equals(Value);
            base.PreDraw();
        }
    }

    class StringSwitch : Switch<string>
    {
        public StringSwitch(string value, ReactiveProperty<string> prop) : base(new Text(value), value, prop)
        { }
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
            if (!Enabled) return new List<MySprite>();
            
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
            var cinv = amount > _minimum 
                ? drawer.Style.Akcent //Color.FromNonPremultiplied(0xFF - c.R, 0xFF - c.G, 0xFF - c.B, c.A)
                : Color.DarkRed;
            
            var retVal = new List<MySprite>{GetSprite("SquareSimple", new RectangleF(position, size), cinv, 0, 1)};
            DrawBorder(viewport, ref retVal, cinv);
            
            _text.Draw(drawer, ref viewport, ref retVal, ref interactive);

            return retVal;
        }
    }
}