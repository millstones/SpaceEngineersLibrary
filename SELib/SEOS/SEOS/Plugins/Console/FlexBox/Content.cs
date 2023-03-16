using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    abstract class Content
    {
        public int Layer = Layers.Content;
        public bool Enabled = true;
        public Color? Color { get; set; }

        RectangleF? _viewport;

        public bool ContainsPoint(Vector2 point)
        {
            return _viewport.HasValue && _viewport.Value.Contains(point);
        }

        public void SetViewport(RectangleF viewport) => _viewport = viewport;
        public abstract void Draw(ref List<MySprite> sprites, bool isSelect, IDrawer drawer);

        protected RectangleF GetOrThrowViewport()
        {
            if (_viewport.HasValue) return _viewport.Value;
            
            throw new Exception("Viewport is not set");
        }
    }

    class Rect : Content
    {
        public bool IsFill;
        public override void Draw(ref List<MySprite> sprites, bool isSelect, IDrawer drawer)
        {
            var c = Color ?? drawer.Style.ContentColor;
            var vpt = GetOrThrowViewport();
            var w = ConsolePluginSetup.PADDING_PX;
            var xLine = new Vector2(vpt.Width, w);
            var yLine = new Vector2(w, vpt.Height);
            var p1 = vpt.Position + new Vector2(0, vpt.Height - w);
            var p2 = vpt.Position + new Vector2(vpt.Width - w, 0);
            if (IsFill)
                sprites.Add(Fill(vpt, c));
            else
                sprites.AddRange(new List<MySprite>
                {
                    Fill(new RectangleF(vpt.Position, xLine), c),
                    Fill(new RectangleF(vpt.Position, yLine), c),
                    Fill(new RectangleF(p1, xLine), c),
                    Fill(new RectangleF(p2, yLine), c)
                });
        }
        MySprite Fill(RectangleF viewport, Color? color)
        {
            return new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Size = viewport.Size,
                Position = viewport.Center,
                Color = color,
                Alignment = TextAlignment.CENTER,
            };
        }
    }
    class Text : Content
    {
        public Alignment Align = Alignment.Center;
        public float? Scale;
        
        Func<string> _get;

        public Text(Func<string> get)
        {
            _get = get;
        }
        public Text(string txt)
        {
            _get = () => txt;
        }

        public override void Draw(ref List<MySprite> sprites, bool isSelect, IDrawer drawer)
        {
            var vpt = GetOrThrowViewport();
            
            vpt.Size -= 4 * ConsolePluginSetup.PADDING_PX;
            vpt.Position += 2 * ConsolePluginSetup.PADDING_PX;

            var text = _get();
            var textSize = drawer.MeasureText(text, Scale ?? 1);
            var scaleTxt = vpt.Size / textSize;

            var scale = Scale ?? Math.Min(Math.Min(scaleTxt.X, scaleTxt.Y), 1);

            textSize = drawer.MeasureText(text, scale);

            var alt = TextAlignment.CENTER;

            switch (Align)
            {
                case Alignment.DownRight:
                case Alignment.UpRight:
                    alt = TextAlignment.RIGHT;
                    break;
                case Alignment.UpLeft:
                case Alignment.DownLeft:
                    alt = TextAlignment.LEFT;
                    break;
            }


            if (textSize.X > vpt.Size.X)
                alt = TextAlignment.LEFT;

            var pos = alt == TextAlignment.LEFT
                ? vpt.Center - new Vector2(vpt.Width / 2, textSize.Y / 2f)
                : alt == TextAlignment.RIGHT
                    ? vpt.Center - new Vector2(-vpt.Width / 2, textSize.Y / 2f)
                    : vpt.Center - new Vector2(0, textSize.Y / 2f);


            sprites.Add(new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = text,
                    Position = pos,
                    Size = vpt.Size,
                    Color = Color ?? drawer.Style.ContentColor,
                    FontId = drawer.Style.FontId,
                    RotationOrScale = scale,
                    Alignment = alt
                }
            );
        }
    }
    class Image : Content
    {
        public  Alignment Align = Alignment.Center;
        
        Func<string> _texture;
        Func<float> _rotation;


        public Image(Func<string> texture, Func<float> rotation = null)
        {
            _texture = texture;
            _rotation = rotation ?? (() => 0);
        }
        
        public override void Draw(ref List<MySprite> sprites, bool isSelect, IDrawer drawer)
        {
            var vpt = GetOrThrowViewport();
            var texture = _texture();

            var size = new Vector2(Math.Min(vpt.Width, vpt.Height));
            var pos = vpt.Center - size / 2;
            switch (Align)
            {
                case Alignment.CenterLeft:
                    pos = new Vector2(0, pos.Y);
                    break;
                case Alignment.CenterUp:
                    pos = new Vector2(pos.X, 0);
                    break;
                case Alignment.UpLeft:
                    pos = vpt.Position;
                    break;
                case Alignment.UpRight:
                    pos = new Vector2(vpt.Position.X + vpt.Width - size.X, 0);
                    break;
                case Alignment.DownLeft:
                    pos = new Vector2(0, vpt.Position.Y + vpt.Height - size.Y);
                    break;
                case Alignment.DownRight:
                    pos = new Vector2(vpt.Position.X + vpt.Width - size.X,
                        vpt.Position.Y + vpt.Height - size.Y);
                    break;
            }

            vpt = new RectangleF(pos, size);

            sprites.Add(new MySprite
                {
                    Type = SpriteType.TEXTURE,
                    Data = texture,
                    Size = vpt.Size, // * (s ?? 1),
                    Position = vpt.Center,
                    Color = Color,
                    RotationOrScale = _rotation(),
                    Alignment = TextAlignment.CENTER,
                }
            );
        }
    }
    class Panel : Content
    {
        Rect _bg,_br;

        public Panel()
        {
            _bg = new Rect {Layer = Layers.BG + Layer, IsFill = true, Color = Color};
            _br = new Rect {Layer = Layers.Decals + Layer, IsFill = false};
        }
        
        public override void Draw(ref List<MySprite> sprites, bool isSelect, IDrawer drawer)
        {
            var vpt = GetOrThrowViewport();
            
            _bg.Color = Color ?? drawer.Style.BGColor;
            _bg.Enabled = Enabled;
            _bg.SetViewport(vpt);
            
            _br.Color = drawer.Style.Accent;
            _br.Enabled = Enabled;
            _br.SetViewport(vpt);
            
            _bg.Draw(ref sprites, isSelect, drawer);
            _br.Draw(ref sprites, isSelect, drawer);
            
            // foreach (var content in _contents)
            // {
            //     content.SetViewport(vpt);
            //     content.Draw(ref sprites, isSelect, drawer);
            // }
        }
    }

    class Link : Content, IInteractive
    {
        Action<ISurface> _click;
        Text _text;

        public Link(string text, Action<ISurface> click)
        {
            _text = new Text(text) {Layer = Layers.Content + Layer};
            _click = click;
        }
        public Link(Func<string> text, Action<ISurface> click)
        {
            _text = new Text(text){Layer = Layers.Content + Layer};
            _click = click;
        }
        
        public override void Draw(ref List<MySprite> sprites, bool isSelect, IDrawer drawer)
        {
            var vpt = GetOrThrowViewport();
            
            var c = Color ?? drawer.Style.BGColor;
            if (isSelect)
                c = VRageMath.Color.Lighten(c, 0.8);
            
            _text.Color = c;
            _text.Enabled = Enabled;
            _text.SetViewport(vpt);
        }

        public void OnSelect(ISurface surface)
        {
            _click?.Invoke(surface);
        }

        public void OnEsc(ISurface surface)
        {}

        public void OnInput(ISurface surface, Vector3 dir)
        {}
    }
    
    class Button : Content, IInteractive
    {
        Action<ISurface> _click;
        Panel _panel;
        Content _content;
        
        public Button(Action<ISurface> click, Color? color = null)
        {
            Build(click, new Rect{Color = color}, color);
        }

        public Button(Action<ISurface> click, string txt, Color? color = null)
        {
            Build(click, new Text(txt) {Color = color}, color);
        }

        public Button(Action<ISurface> click, Image image)
        {
            Build(click, image, null);
        }
        void Build(Action<ISurface> click, Content content, Color? color)
        {
            _click = click;
            Color = color;
            _content = content;
            _panel = new Panel{Color = color, Enabled = Enabled, Layer = Layers.BG + Layer};

            _content.Layer += Layer;
        }
        public override void Draw(ref List<MySprite> sprites, bool isSelect, IDrawer drawer)
        {
            var vpt = GetOrThrowViewport();
            
            var c = Color ?? drawer.Style.BGColor;
            if (isSelect)
                c = VRageMath.Color.Lighten(c, 0.15).Alpha(0.2f);
            
            _panel.Color = c;
            _panel.Enabled = Enabled;
            _panel.SetViewport(vpt);
            
            _content.Enabled = Enabled;
            _content.SetViewport(vpt);
            
            _panel.Draw(ref sprites, isSelect, drawer);
            _content.Draw(ref sprites, isSelect, drawer);
        }

        public void OnSelect(ISurface surface)
        {
            _click?.Invoke(surface);
        }

        public void OnEsc(ISurface surface)
        {}

        public void OnInput(ISurface surface, Vector3 dir)
        {}
    }
    
    class Switch<T> : Content, IInteractive
    {
        public T Value { get; private set; }
        ReactiveProperty<T> _getSet;
        Action<ISurface, T> _stateChange;
        Content _on, _off;

        public Switch(T state, ReactiveProperty<T> getSet, Content on, Content off=null, Action<ISurface, T> stateChange = null)
        {
            _getSet = getSet;
            _stateChange = stateChange;
            Build(state, on, off, getSet, stateChange);
        }
        public Switch(T state, ReactiveProperty<T> getSet, Action<ISurface, T> stateChange = null)
        {
            Build(state, new Text(state.ToString()), null, getSet, stateChange);
        }

        void Build(T state, Content on, Content off, ReactiveProperty<T> getSet, Action<ISurface, T> stateChange = null)
        {
            Value = state;
            _on = on;
            _off = off;
            _getSet = getSet;
            _stateChange = stateChange;

            _on.Layer = Layers.Content + Layer;
            if (_off != null)
                _off.Layer = _on.Layer;
        }

        public override void Draw(ref List<MySprite> sprites, bool isSelect, IDrawer drawer)
        {
            var current = _getSet.Get();
            if (_off != null)
            {
                if (current.Equals(Value))
                {
                    InternalDraw(ref sprites, isSelect, drawer, _on);
                }
                else
                {
                    InternalDraw(ref sprites, isSelect, drawer, _off);
                }
            }
            else
            {
                var c = current.Equals(Value)
                        ? VRageMath.Color.Lighten(drawer.Style.ContentColor, 0.35)
                        : VRageMath.Color.Darken(drawer.Style.ContentColor, 0.35)
                    ;
                InternalDraw(ref sprites, isSelect, drawer, _on, c);
            }
        }

        void InternalDraw(ref List<MySprite> sprites, bool isSelect, IDrawer drawer, Content content, Color? color = null)
        {
            var vpt = GetOrThrowViewport();
            
            if (color.HasValue)
                content.Color = color;

            content.Enabled = Enabled;
            content.SetViewport(vpt);
            content.Draw(ref sprites, isSelect, drawer);
        }

        public void OnSelect(ISurface surface)
        {
            _getSet.Set(Value);
            _stateChange?.Invoke(surface, Value);
        }

        public void OnEsc(ISurface surface)
        { }

        public void OnInput(ISurface surface, Vector3 dir)
        { }
    }
    
    class ProgressBar : Content
    {
        public bool Vertical;
        
        double _min, _max;
        bool _reversLogic;
        ReactiveProperty<double> _amount;

        Rect _rect;
        Text _text;

        public ProgressBar(Func<double> amount, double min = 0.1, double max = 0.9, bool reversLogic = false)
        {
            _min = min;
            _max = max;
            _reversLogic = reversLogic;
            _amount = new ReactiveProperty<double>(amount);
            
            _rect = new Rect {Layer = Layers.BG + Layer};
            _text = new Text(() => _amount.Get().ToString("P0")) {Layer = Layers.Content + Layer};
        }

        public override void Draw(ref List<MySprite> sprites, bool isSelect, IDrawer drawer)
        {
            var vpt = GetOrThrowViewport();
            var amount = _amount.Get();
            var c = GetColor(drawer.Style, amount);
            _rect.Enabled = Enabled;
            _rect.Color = c;
            _rect.SetViewport(vpt);
            _rect.Draw(ref sprites, isSelect, drawer);
            
            vpt.Size -= 4 * ConsolePluginSetup.PADDING_PX;
            vpt.Position += 2 * ConsolePluginSetup.PADDING_PX;
            var progress = Math.Max(amount, _min);

            var size = Vertical
                    ? new Vector2(vpt.Size.X, (float) (vpt.Size.Y * progress))
                    : new Vector2((float) (vpt.Size.X * progress), vpt.Size.Y)
                ;
            
            var position = Vertical
                    ? new Vector2(vpt.Position.X, vpt.Position.Y + vpt.Size.Y - size.Y)
                    : new Vector2(vpt.Position.X, vpt.Position.Y)
                ;

            sprites.Add(Filling(new RectangleF(position, size), drawer.Style.Accent));

            _text.Enabled = Enabled;
            _text.Color = c == (Color ?? drawer.Style.Accent) ? (Color?) null : c;
            _text.SetViewport(vpt);
            _text.Draw(ref sprites, isSelect, drawer);
        }

        Color GetColor(ConsoleStyle style, double v)
        {
            var normal = Color ?? style.Accent;
            var c =normal;
            if (_reversLogic)
            {
                if (v > _max) c = style.BadAccent ;
                else 
                if (v < _min) c = style.GoodAccent ;
            }
            else
            {
                if (v > _max) c = style.GoodAccent ;
                else 
                if (v < _min) c = style.BadAccent ;
            }

            return c != style.GoodAccent
                ? (_rect.Color.HasValue && _rect.Color == c)
                    ? normal
                    : c
                : style.GoodAccent;
        }
        
        MySprite Filling(RectangleF viewport, Color? color)
        {
            return new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Size = viewport.Size,
                Position = viewport.Center,
                Color = color,
                Alignment = TextAlignment.CENTER,
            };
        }
    }
}