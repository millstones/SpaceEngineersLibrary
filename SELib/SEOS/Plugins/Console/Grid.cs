using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.GUI.TextPanel;
using VRageMath;
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

namespace IngameScript
{
    class FreeCanvas : PageItem
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

        protected override void OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites, ref IInteractive interactive)
        {
            foreach (var pageItem in Items)
            {
                pageItem.Key.Visible = Visible;
                pageItem.Key.Enabled = Enabled;

                var vpt = viewport;
                var scale = pageItem.Value;
                vpt.Position += scale.Position * vpt.Size;
                vpt.Size *= scale.Size;

                pageItem.Key.Draw(drawer, ref vpt, ref sprites, ref interactive);
            }
        }
    }
    class FlexiblePanel<T> : PageItem, IEnumerable<T> where T : PageItem
    {
        protected List<KeyValuePair<T, int>> Items = new List<KeyValuePair<T, int>>();
        bool _vertical;

        public FlexiblePanel(bool vertical = false)
        {
            _vertical = vertical;
        }
        public FlexiblePanel<T> Add(T item, int size=1)
        {
            Items.Add(new KeyValuePair<T, int>(item, size));
            return this;
        }

        protected override void OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites, ref IInteractive interactive)
        {
            foreach (var item in Items)
            {
                item.Key.Visible = Visible;
                item.Key.Enabled = Enabled;

                var vpt = GetViewport(viewport, item);
                item.Key.Draw(drawer, ref vpt, ref sprites, ref interactive);
            }
        }
        RectangleF GetViewport(RectangleF parentViewport, KeyValuePair<T, int> item)
        {
            var children = Items;//.Keys.ToList();
            
            var sizeNumber = item.Value;
            var sumSizeNumber = Items.Sum(x => x.Value);
            var preNums = children.GetRange(0, children.IndexOf(item)).Sum(x=> x.Value);

            var kSize = (float) sizeNumber / sumSizeNumber;
            var dSize = parentViewport.Size * preNums / sumSizeNumber;

            var size = !_vertical
                ? new Vector2((parentViewport.Size.X * kSize) /*- 2*/, parentViewport.Size.Y)
                : new Vector2(parentViewport.Size.X, (parentViewport.Size.Y * kSize) /*- 2*/);

            var position = !_vertical
                ? new Vector2((parentViewport.Position.X + (dSize.X)) /*+ 1*/, parentViewport.Position.Y)
                : new Vector2(parentViewport.Position.X, (parentViewport.Position.Y + (dSize.Y)) /*+ 1*/);

            return new RectangleF(position, size);
        }

        public IEnumerator<T> GetEnumerator() => Items.Select(x=> x.Key).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    class StackPanel<T> : PageItem where T : PageItem
    {
        bool _vertical;
        List<KeyValuePair<T, int>> _items = new List<KeyValuePair<T, int>>();
        
        public StackPanel(bool vertical = false)
        {
            _vertical = vertical;
        }

        public StackPanel<T> Add(T item, int steps = 1)
        {
            _items.Add(new KeyValuePair<T, int>(item, steps));
            return this;
        }
        protected override void OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites,
            ref IInteractive interactive)
        {
            var gridStep = drawer.GridStep;
            var posShift = Vector2.Zero;
            foreach (var menuItem in _items)
            {
                var vpt = viewport;
                vpt.Position += posShift;
                var size = menuItem.Value * gridStep;

                vpt.Size = _vertical? new Vector2(vpt.Size.X, size.Y) : new Vector2(size.X, vpt.Size.Y);
                
                menuItem.Key.Visible = Visible;
                menuItem.Key.Draw(drawer, ref vpt, ref sprites, ref interactive);
                
                posShift += _vertical? new Vector2(0, size.Y) : new Vector2(size.X, 0);
            }
        }
    }
    
    class Menu : StackPanel<Link>
    {
        float? _textScale;

        public Menu(float? textScale=null) : base(true)
        {
            _textScale = textScale;
            Border = true;
        }

        public new Menu Add(Link item, int steps = 1)
        {
            
            throw new NotSupportedException();
            /*
            item.TextScale = _textScale;
            base.Add(item);
            return this;
            */
        }
        
        public Menu Add(string item, Action<IConsole> click)
        {
            base.Add(new Link(item, click, _textScale));
            return this;
        }
    }
}