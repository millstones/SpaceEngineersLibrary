using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript.New
{
    abstract class PageItemContainer : PageItem
    {
        protected List<PageItem> Items = new List<PageItem>();
        protected override void OnDraw(IDrawSurface surface, ref RectangleF viewport)
        {
            foreach (var item in Items)
            {
                item.Draw(surface, ref viewport);
            }
        }

        public virtual void Remove(PageItem item)
        {
            Items.Remove(item);
        }
    }
    class Grid : PageItemContainer
    {
        public void Add(PageItem item, Vector2 position, Vector2 sizeScale)
        {
            item.PosShift = position;
            item.SizeScale = sizeScale;
            
            Items.Add(item);
        }
    }

    class DockGrid : PageItemContainer
    {
        Dictionary<Alignment, List<PageItem>> _items = new Dictionary<Alignment, List<PageItem>>{{Alignment.Center, new List<PageItem>()}};

        public void Add(PageItem item, Alignment alignment= Alignment.Center)
        {
            var viewport = new RectangleF(Vector2.Zero, Vector2.One);
            
            foreach (var itm in Items)
            {
                var alt = GetAlignmentFor(itm);
                var minSize = itm.MinSize;

                Vector2 posShift;
                Vector2 sizeScale;
                switch (alt)
                {
                    case Alignment.Left:
                        posShift = viewport.Position;
                        sizeScale = new Vector2(minSize.X, viewport.Size.Y);
                        
                        viewport.Position += new Vector2(minSize.X, 0);
                        viewport.Size -= new Vector2(minSize.X, 0);
                        break;
                    case Alignment.Right:
                        posShift = viewport.Position + viewport.Size.X - minSize.X;
                        sizeScale = new Vector2(minSize.X, viewport.Size.Y);
                        
                        //viewport.Position += new Vector2(gridStep.X, 0);
                        viewport.Size -= new Vector2(minSize.X, 0);
                        break;
                    case Alignment.Up:
                        posShift = viewport.Position;
                        sizeScale = new Vector2(viewport.Size.X, minSize.Y);
                        
                        viewport.Position += new Vector2(0, minSize.Y);
                        viewport.Size -= new Vector2(0, minSize.Y);
                        break;
                    case Alignment.Down:
                        posShift = viewport.Position + viewport.Size.Y - minSize.Y;
                        sizeScale = new Vector2(viewport.Size.X, minSize.Y);
                        
                        //viewport.Position += new Vector2(0, gridStep.Y);
                        viewport.Size -= new Vector2(0, minSize.Y);
                        break;
                    case Alignment.CenterLeft:
                        var lCount = _items[alt].Count;
                        var w = viewport.Size.X / lCount;
                        posShift = viewport.Position;
                        sizeScale = new Vector2(w, viewport.Size.Y);
                        
                        viewport.Position += new Vector2(w, 0);
                        viewport.Size -= new Vector2(w, 0);
                        break;
                    default:
                        var count = _items.Count(x => x.Key== Alignment.Center || 
                                                      x.Key == Alignment.CenterUp ||
                                                      x.Key == alt);
                        var h = viewport.Size.Y / count;
                        posShift = viewport.Position;
                        sizeScale = new Vector2(viewport.Size.X, h);
                        
                        viewport.Position += new Vector2(0, h);
                        viewport.Size -= new Vector2(0, h);
                        break;
                }

                itm.PosShift = posShift;
                itm.SizeScale = sizeScale;
            }

            if (!_items.ContainsKey(alignment))
                _items.Add(alignment, new List<PageItem>());
            _items[alignment].Add(item);
            item.PosShift = viewport.Position;
            item.SizeScale = viewport.Size;
            Items.Add(item);
        }

        Alignment GetAlignmentFor(PageItem item)
        {
            Alignment retVal;
            if (_items.Any(x => x.Value.Contains(item)));
            {
                retVal = _items.First(x => x.Value.Contains(item)).Key;
            }

            return retVal;
        }

        public override void Remove(PageItem item)
        {
            var alt = GetAlignmentFor(item);
            _items[alt].Remove(item);
            base.Remove(item);
        }
    }
}