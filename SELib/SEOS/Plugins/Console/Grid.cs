using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.GUI.TextPanel;
using VRageMath;
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

namespace IngameScript
{
    abstract class PageItemContainer : PageItem, IEnumerable<PageItem>
    {
        public abstract IEnumerator<PageItem> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected override void PreDraw()
        {
            var thisIsIText = this as IText;
            if (thisIsIText != null && !thisIsIText.FontSize.HasValue)
            {
                thisIsIText.FontSize = this.OfType<IText>().Min(x => x.FontSize);
            }
            
            foreach (var pageItem in this)
            {
                if (thisIsIText !=null && pageItem is IText)
                {
                    var itext = ((IText) pageItem);
                    if (!itext.FontSize.HasValue)
                        itext.FontSize = thisIsIText.FontSize;
                    else 
                    if (thisIsIText.FontSize.HasValue)
                        itext.FontSize = Math.Min(itext.FontSize.Value, thisIsIText.FontSize.Value);
                }
                
                //if (Scale?.Invoke() != null) 
                //if (pageItem.TextScale == null) 
                //    pageItem.TextScale = TextScale;
                pageItem.Enabled = Enabled;
            }
        }
    }
    class FreeCanvas : PageItemContainer
    {
        protected Dictionary<PageItem, RectangleF> Items = new Dictionary<PageItem, RectangleF>();

        public FreeCanvas Add(PageItem item, RectangleF? viewport = null)
        {
            Items.Add(item, viewport ?? new RectangleF(Vector2.Zero, Vector2.One));
            return this;
        }
        public void Remove(PageItem item)
        {
            Items.Remove(item);
        }

        public override IEnumerator<PageItem> GetEnumerator() => Items.Select(x => x.Key).GetEnumerator();

        protected override List<MySprite> OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref IInteractive interactive)
        {
            var retVal = new List<MySprite>();
            //var contentSize = viewport.Size;
            foreach (var pageItem in Items)
            {
                var vpt = viewport;
                var scale = pageItem.Value;
                vpt.Position += scale.Position * vpt.Size;
                vpt.Size *= scale.Size;

                pageItem.Key.Draw(drawer, ref vpt, ref retVal, ref interactive);
                
                //contentSize = Vector2.Max(vpt.Size, contentSize);
            }

            //PixelSize = contentSize;

            return retVal;
        }
    }
    class FlexiblePanel<T> : PageItemContainer, IText where T : PageItem
    {
        public float? FontSize { get; set; }
        protected List<KeyValuePair<T, int>> Items = new List<KeyValuePair<T, int>>();
        bool _vertical;


        public FlexiblePanel(bool vertical)
        {
            //TextScale = null;
            _vertical = vertical;
        }

        public FlexiblePanel<T> Add(T item, int size=1)
        {
            //item.TextScale = TextScale;
            //if (item.TextScale == null) item.TextScale = TextScale;
            Items.Add(new KeyValuePair<T, int>(item, size));
            return this;
        }

        protected override List<MySprite> OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref IInteractive interactive)
        {
            var retVal = new List<MySprite>();

            foreach (var item in Items)
            {
                var vpt = GetViewport(viewport, item);
                item.Key.Draw(drawer, ref vpt, ref retVal, ref interactive);
            }
            
            return retVal;
        }

        RectangleF GetViewport(RectangleF parentViewport, KeyValuePair<T, int> item)
        {
            var children = Items; //.Keys.ToList();

            var sizeNumber = item.Value;
            var sumSizeNumber = Items.Sum(x => x.Value);
            var preNums = children.GetRange(0, children.IndexOf(item)).Sum(x => x.Value);

            var kSize = (float) sizeNumber / sumSizeNumber;
            var dSize = parentViewport.Size * preNums / sumSizeNumber;

            var size = _vertical
                ? new Vector2((parentViewport.Size.X * kSize) /*- 2*/, parentViewport.Size.Y)
                : new Vector2(parentViewport.Size.X, (parentViewport.Size.Y * kSize) /*- 2*/);

            var position = _vertical
                ? new Vector2((parentViewport.Position.X + (dSize.X)) /*+ 1*/, parentViewport.Position.Y)
                : new Vector2(parentViewport.Position.X, (parentViewport.Position.Y + (dSize.Y)) /*+ 1*/);

            return new RectangleF(position, size);
        }

        public override IEnumerator<PageItem> GetEnumerator() => Items.Select(x=> x.Key).GetEnumerator();
    }
    class StackPanel<T> : PageItemContainer, IText where T : PageItem
    {
        public float? FontSize { get; set; }
        bool _vertical;
        List<KeyValuePair<T, int>> _items = new List<KeyValuePair<T, int>>();


        public StackPanel(bool vertical)
        {
            _vertical = vertical;
        }

        public StackPanel<T> Add(T item, int steps = 1)
        {
            //if (item.TextScale == null) item.TextScale = TextScale;
            _items.Add(new KeyValuePair<T, int>(item, steps));
            return this;
        }

        protected override List<MySprite> OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref IInteractive interactive)
        {
            var retVal = new List<MySprite>();

            var gridStep = drawer.GridStep;// * (Scale?.Invoke() ?? 1);
            var posShift = Vector2.Zero;
            foreach (var menuItem in _items)
            {
                var vpt = viewport;
                vpt.Position += posShift;
                var size = menuItem.Value * gridStep;

                vpt.Size = _vertical? new Vector2(size.X, vpt.Size.Y) : new Vector2(vpt.Size.X, size.Y);
                
                menuItem.Key.Draw(drawer, ref vpt, ref retVal, ref interactive);
                
                posShift += _vertical? new Vector2(size.X, 0) : new Vector2(0, size.Y);
            }

            return retVal;
        }

        public override IEnumerator<PageItem> GetEnumerator() => _items.Select(x => x.Key).GetEnumerator();
    }
    
    class LinkDownList : StackPanel<Link>
    {
        public new LinkDownList Add(Link item, int steps = 1)
        {
            throw new NotSupportedException();
        }

        public LinkDownList Add(string item, Action<IConsole> click)
        {
            base.Add(new Link(item, click));
            return this;
        }

        public LinkDownList() : base(false)
        {
        }
    }

    class Menu : Text, IInteractive
    {
        Action<IConsole, string> _click;
        LinkDownList _downList;
        MsgBoxItem<string> _msgBox;

        RectangleF? GetViewport()
        {
            if (!PixelViewport.HasValue) return null;
            
            var vpt = PixelViewport.Value;
            return new RectangleF(
                vpt.Position + new Vector2(0, vpt.Height),
                new Vector2(vpt.Width, vpt.Height * _downList.Count()));

        }
        public void OnSelect(IConsole console)
        {
            _downList.FontSize = FontSize;
            if (_msgBox == null)
            {
                _msgBox = new MsgBoxItem<string>(_downList) /*{Scale = Scale}*/;
                _msgBox.OnClose += s => { if (!string.IsNullOrEmpty(s)) _click(console, s); };
            }
            
            _msgBox.Show(console, GetViewport());
        }

        public void OnEsc(IConsole console)
        {
            _msgBox.Close("");
            _msgBox = null;
        }

        public void OnInput(IConsole console, Vector3 dir)
        {
            
        }

        public void OnHoverEnable(bool hover)
        {
            Highlighting = true;
        }

        public Menu(string txt, Action<IConsole, string> click) : base(txt)
        {
            _click = click;
            _downList = new LinkDownList();
        }

        public Menu(Func<string> txt, Action<IConsole, string> click) : base(txt)
        {
            _click = click;
            _downList = new LinkDownList();
        }

        public Menu Add(params string[] items)
        {
            foreach (var item in items)
            {
                _downList.Add(item, console => _msgBox?.Close(item));
            }

            return this;
        }
    }
}