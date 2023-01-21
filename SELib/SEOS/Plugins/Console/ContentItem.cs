using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    abstract class Content
    {
        public bool Vertical;
        public bool Visible = true;
        public int SizeNumber;
        TextAlignment _alignment;
        
        protected Content(int sizeNumber, TextAlignment alignment)
        {
            SizeNumber = (int) MyMath.Clamp(sizeNumber, 1, int.MaxValue);
            _alignment = alignment;
        }
        
        protected MySprite GetText(string text, string fontId, float scale, RectangleF viewport, Color color,
            Func<string, float, Vector2> textMeasure)
        {
            var textSize = textMeasure(text, scale);
            if (textSize.X > viewport.Width)
            {
                scale = viewport.Width / textSize.X;
            }
            if (textSize.Y > viewport.Height)
            {
                scale = viewport.Height / textSize.Y;
            }
            textSize = textMeasure(text, scale);
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
            var pos = _alignment == TextAlignment.LEFT
                ? viewport.Center - new Vector2(viewport.Width / 2, textSize.Y / 2f)
                : _alignment == TextAlignment.RIGHT
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
                    FontId = fontId,
                    RotationOrScale = scale,
                    Alignment = _alignment,
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
        
        public void Draw(RectangleF viewport, Vector2 arrowPos, ConsoleStyle style, ref List<MySprite> sprites,
            ref IInteractive interactive,Func<string, float, Vector2> textMeasure, float textScale)
        {
            if (! Visible) return;
            
            SizeOfPadding(ref viewport);
            
            var newInteractive = this as IInteractive;
            if (newInteractive != null && viewport.Contains(arrowPos))
            {
                interactive = newInteractive;
            }

            PreDraw();
            OnDraw(ref viewport, arrowPos, style, ref sprites, ref interactive, textMeasure, textScale);
        }

        protected virtual void PreDraw() {}
        protected abstract void OnDraw(ref RectangleF viewport, Vector2 arrowPos, ConsoleStyle style,
            ref List<MySprite> sprites,
            ref IInteractive interactive, Func<string, float, Vector2> textMeasure, float textScale);
    }

    class ContentPanel : Content
    {
        protected bool Bordered, Fill;
        List<Content> _children = new List<Content>();

        public ContentPanel(int sizeNumber=1, bool vertical=false, TextAlignment alignment = TextAlignment.CENTER, bool border = true, bool fill = true) 
            : base(sizeNumber, alignment)
        {
            Bordered = border;
            Fill = fill;
            Vertical = vertical;
        }
        
        public ContentPanel Add(Content node)
        {
            node.Vertical = Vertical;
            _children.Add(node);
            return this;
        }

        public void ClearChildren()
        {
            _children.Clear();
        }

        protected override void OnDraw(ref RectangleF viewport, Vector2 arrowPos, ConsoleStyle style,
            ref List<MySprite> sprites,
            ref IInteractive interactive, Func<string, float, Vector2> textMeasure, float textScale)
        {
            if (Bordered)
            {
                sprites.Add(GetSprite("SquareHollow", viewport, style.ThirdColor));
                SizeOfPadding(ref viewport);
            }
            if (Fill)
            {
                sprites.Add(GetSprite("SquareSimple", viewport, style.FirstColor));
            }
            SizeOfPadding(ref viewport);
            foreach (var item in _children)
            {
                var vp = GetViewport(viewport, item);

                item.Draw(vp, arrowPos, style, ref sprites, ref interactive, textMeasure, textScale);
            }
        }
        
        RectangleF GetViewport(RectangleF parentViewport, Content item)
        {
            // TODO: еще нужно учитывать свойство _alignment !!

            var sizeNumber = item.SizeNumber;
            var sumSizeNumber = _children.Sum(x => x.SizeNumber);
            var preNums = _children.GetRange(0, _children.IndexOf(item)).Sum(x => x.SizeNumber);

            var kSize = (float) sizeNumber / sumSizeNumber;
            var dSize = parentViewport.Size * preNums / sumSizeNumber;

            var size = item.Vertical
                ? new Vector2((parentViewport.Size.X * kSize) - 2, parentViewport.Size.Y)
                : new Vector2(parentViewport.Size.X, (parentViewport.Size.Y * kSize) - 2);

            var position = item.Vertical
                ? new Vector2((parentViewport.Position.X + (dSize.X)) + 1, parentViewport.Position.Y)
                : new Vector2(parentViewport.Position.X, (parentViewport.Position.Y + (dSize.Y)) + 1);

            return new RectangleF(position, size);
        }
    }

    class ContentText : Content
    {
        ReactiveProperty<string> _prop;
        public ContentText(ReactiveProperty<string> prop,int sizeNumber=1, TextAlignment alignment = TextAlignment.CENTER) 
            : base(sizeNumber, alignment)
        {
            _prop = prop;
        }
        public ContentText(string txt,int sizeNumber=1, TextAlignment alignment = TextAlignment.CENTER) 
            : base(sizeNumber, alignment)
        {
            _prop = new ReactiveProperty<string>(txt);
        }

        protected override void OnDraw(ref RectangleF viewport, Vector2 arrowPos, ConsoleStyle style,
            ref List<MySprite> sprites,
            ref IInteractive interactive, Func<string, float, Vector2> textMeasure, float textScale)
        {
            sprites.Add(GetText(_prop.Get(), style.FontId, textScale, viewport, style.ThirdColor, textMeasure));
        }
    }
    class ContentImage : Content
    {
        ReactiveProperty<string> _prop;
        Color? _color;

        public ContentImage(ReactiveProperty<string> prop, Color? color = null, int sizeNumber=1) 
            : base(sizeNumber, TextAlignment.CENTER)
        {
            _prop = prop;
            _color = color;
        }
        public ContentImage(string texture, Color? color = null, int sizeNumber=1) 
            : base(sizeNumber, TextAlignment.CENTER)
        {
            _prop = new ReactiveProperty<string>(texture);
            _color = color;
        }

        protected override void OnDraw(ref RectangleF viewport, Vector2 arrowPos, ConsoleStyle style,
            ref List<MySprite> sprites,
            ref IInteractive interactive, Func<string, float, Vector2> textMeasure, float textScale)
        {
            var l = Math.Min(viewport.Size.X, viewport.Size.Y);
            viewport.Size = new Vector2(l);
            sprites.Add(GetSprite(_prop.Get(), viewport, _color ?? Color.White));
        }
    }

    class ContentGrid : ContentPanel
    {
        int _columns;

        public ContentGrid(int columns, bool gridVisible)
        {
            _columns = columns;
            Bordered = gridVisible;
        }
        
        public ContentGrid AddItems(List<Content> items)
        {
            var i = 0;
            while (i < items.Count)
            {
                var rowItem = new List<Content>();
                for (var j = 0; j < _columns; j++)
                {
                    var value = i >= items.Count
                        ? new ContentPanel(border: false, fill: false)
                        : items[i];

                    rowItem.Add(value);

                    i++;
                }

                Add(new GridRow(rowItem, Bordered));
            }

            return this;
        }

        class GridRow : ContentPanel
        {
            public GridRow(IEnumerable<Content> rowItems, bool gridVisible)
            {
                Vertical = true;
                Bordered = gridVisible;

                foreach (var item in rowItems)
                {
                    Add(item);
                }
            }
        }
    }

    abstract class InteractiveContent : ContentPanel, IInteractive
    {
        Action<IConsole> _click;
        Func<bool> _enabled;
        bool _isHover;

        protected InteractiveContent(Action<IConsole> click, Func<bool> enabled, int sizeNumber=1, bool vertical = false, TextAlignment alignment = TextAlignment.CENTER) : base(sizeNumber, vertical, alignment)
        {
            _click = click ?? (args => { });
            _enabled = enabled ?? (() => true);
        }

        public virtual void OnClick(IConsole console) => _click.Invoke(console);

        public void OnHoverEnable(bool hover)=> _isHover = hover;

        protected override void OnDraw(ref RectangleF viewport, Vector2 arrowPos, ConsoleStyle style,
            ref List<MySprite> sprites,
            ref IInteractive interactive, Func<string, float, Vector2> textMeasure, float textScale)
        {
            if (!_enabled())
            {
                style.FirstColor = Color.Darken(style.FirstColor, 0.5);
                style.SecondColor = Color.Darken(style.SecondColor, 0.5);
                style.ThirdColor = Color.Darken(style.ThirdColor, 0.5);
            }
            else
            {
                if (_isHover)
                {
                    style.FirstColor = Color.Lighten(style.FirstColor, 0.5);
                    style.ThirdColor = Color.Darken(style.ThirdColor, 0.5);
                }
            }
            base.OnDraw(ref viewport, arrowPos, style, ref sprites, ref interactive, textMeasure, textScale);
        }
    }

    class ContentProgressBar : InteractiveContent
    {
        ReactiveProperty<double> _prop;
        double _minimum;
        Color? _color;

        public ContentProgressBar(ReactiveProperty<double> prop, Action<IConsole> click = null, Func<bool> enabled = null, Color? color = null, double minimum = 0.1, int sizeNumber=1, bool vertical = false)
            : base(click, enabled, sizeNumber, vertical)
        {
            _prop = prop;
            _minimum = minimum;
            _color = color;
        }

        protected override void OnDraw(ref RectangleF viewport, Vector2 arrowPos, ConsoleStyle style,
            ref List<MySprite> sprites,
            ref IInteractive interactive, Func<string, float, Vector2> textMeasure, float textScale)
        {
            base.OnDraw(ref viewport, arrowPos, style, ref sprites, ref interactive, textMeasure, textScale);
            
            var amount = _prop.Get();
            var progress = Math.Max(amount, _minimum);
            
            SizeOfPadding(ref viewport);
            
            var size = Vertical
                    ? new Vector2(viewport.Size.X, (float) (viewport.Size.Y * progress))
                    : new Vector2((float) (viewport.Size.X * progress), viewport.Size.Y)
                ;
            
            var position = Vertical
                    ? new Vector2(viewport.Position.X, viewport.Position.Y + viewport.Size.Y - size.Y)
                    : new Vector2(viewport.Position.X, viewport.Position.Y)
                ;

            
            var color = _color ?? style.SecondColor;
            sprites.Add(GetSprite("SquareSimple", new RectangleF(position, size), color));
            sprites.Add(GetText(amount.ToString("P0"), style.FontId, textScale, viewport, style.ThirdColor, textMeasure));
        }
    }

    class ContentTextButton : InteractiveContent
    {
        public ContentTextButton(ReactiveProperty<string> prop, Action<IConsole> click, Func<bool> enabled = null, int sizeNumber = 1, bool vertical = false, TextAlignment alignment = TextAlignment.LEFT) 
            : base(click, enabled, sizeNumber, vertical)
        {
            Add(new ContentText(prop, sizeNumber, alignment));
        }
        public ContentTextButton(string text, Action<IConsole> click, Func<bool> enabled = null, int sizeNumber = 1, bool vertical = false, TextAlignment alignment = TextAlignment.LEFT) 
            : base(click, enabled, sizeNumber, vertical)
        {
            Add(new ContentText(new ReactiveProperty<string>(text), sizeNumber, alignment));
        }
    }

    class ContentIconButton : InteractiveContent
    {
        public ContentIconButton(ReactiveProperty<string> prop, Action<IConsole> click, Func<bool> enabled = null, int sizeNumber = 1) 
            : base(click, enabled, sizeNumber)
        {
            Add(new ContentImage(prop));
        }
        public ContentIconButton(string texture ,Action<IConsole> click, Func<bool> enabled = null, int sizeNumber = 1) 
            : base(click, enabled, sizeNumber)
        {
            Add(new ContentImage(texture));
        }
    }

    abstract class ContentPage : ContentPanel
    {
        public readonly string NameId;
        public abstract ReactiveProperty<NoteLevel> Note { get; }
        protected ContentPage(string nameId)
        {
            NameId = nameId;
        }
    }

}