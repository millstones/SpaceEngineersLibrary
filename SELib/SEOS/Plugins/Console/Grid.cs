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
        public bool GridVisible = true;

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

        public override void Draw(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites, ref IInteractive interactive)
        {
            base.Draw(drawer, ref viewport, ref sprites, ref interactive);
            
            foreach (var pageItem in Items)
            {
                pageItem.Key.BorderVisible = GridVisible;
                    
                var vpt = viewport;
                var scale = pageItem.Value;
                vpt.Position += scale.Position * vpt.Size;
                vpt.Size *= scale.Size;

                sprites.Add(MySprite.CreateClipRect(new Rectangle(
                    (int) viewport.Position.X, (int) viewport.Position.Y,
                    (int) viewport.Width, (int) viewport.Height)));

                    
                pageItem.Key.Draw(drawer, ref vpt, ref sprites, ref interactive);

                sprites.Add(MySprite.CreateClearClipRect());
            }
        }
    }

    class SizableRow: PageItem
    {
        List<KeyValuePair<PageItem, int>> _items = new List<KeyValuePair<PageItem, int>>();
        bool _vertical;
        
        public SizableRow(bool vertical = false)
        {
            _vertical = vertical;
        }
        public SizableRow Add(PageItem item, int size=1)
        {
            _items.Add(new KeyValuePair<PageItem, int>(item, size));
            return this;
        }

        public override void Draw(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites, ref IInteractive interactive)
        {
            base.Draw(drawer, ref viewport, ref sprites, ref interactive);
            
            foreach (var item in _items)
            {
                sprites.Add(MySprite.CreateClipRect(new Rectangle(
                    (int) viewport.Position.X, (int) viewport.Position.Y,
                    (int) viewport.Width, (int) viewport.Height)));


                var vpt = GetViewport(viewport, item);
                item.Key.Draw(drawer, ref vpt, ref sprites, ref interactive);

                sprites.Add(MySprite.CreateClearClipRect());
            }
            
            sprites.Add(GetText(drawer, _items.Count.ToString(), viewport, true, Color.Red));
        }
        RectangleF GetViewport(RectangleF parentViewport, KeyValuePair<PageItem, int> item)
        {
            var children = _items;//.Keys.ToList();
            
            var sizeNumber = item.Value;
            var sumSizeNumber = _items.Sum(x => x.Value);
            var preNums = children.GetRange(0, children.IndexOf(item)).Sum(x=> x.Value);

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
    }

    class DockCanvas : FreeCanvas
    {
        Alignment _alignment;
    
        List<FreeCanvas> _containers = new List<FreeCanvas>();
    
        public DockCanvas(Alignment alignment)
        {
            _alignment = alignment;
        }
    
        public DockCanvas Add(FreeCanvas item)
        {
            _containers.Add(item);
            return this;
        }
    
        // void Resize(Vector2 step, RectangleF viewport)
        // {
        //     var count = Items.Count;
        //
        //     var alt = _alignment;
        //
        //     float s;
        //     Vector2 size, shiftVector;
        //
        //     switch (alt)
        //     {
        //         case Alignment.Left:
        //         case Alignment.Right:
        //             s = Math.Max(step.X, viewport.Size.X / count);
        //             size = new Vector2(s, viewport.Size.Y);
        //
        //             shiftVector = new Vector2(s, 0);
        //             break;
        //         case Alignment.Up:
        //         case Alignment.Down:
        //         case Alignment.DownLeft:
        //         case Alignment.DownRight:
        //             s = Math.Max(step.Y, viewport.Size.Y / count);
        //             size = new Vector2(viewport.Size.X, s);
        //
        //             shiftVector = new Vector2(0, s);
        //             break;
        //         case Alignment.CenterLeft:
        //             s = Math.Max(step.X, viewport.Size.X / count);
        //             size = new Vector2(s, viewport.Size.Y);
        //
        //             shiftVector = new Vector2(s, 0);
        //             break;
        //         default:
        //             s = Math.Max(step.Y, viewport.Size.Y / count);
        //             size = new Vector2(viewport.Size.X, s);
        //
        //             shiftVector = new Vector2(0, s);
        //             break;
        //     }
        //
        //     // ConsolePlugin.Logger.Log(NoteLevel.Waring, $"=======================================");
        //     // ConsolePlugin.Logger.Log(NoteLevel.Waring, $"P:{viewport.Position} S:{viewport.Size}");
        //     var rect = new RectangleF(viewport.Position, size);
        //     for (var j = 0; j < count - 1; j++)
        //     {
        //         Items[j].Build(SurfaceDrawer, ref rect);
        //
        //         rect.Position += shiftVector;
        //         
        //         
        //         ConsolePlugin.Logger.Log(NoteLevel.Waring, $"P:{rect.Position} S:{rect.Size}");
        //     }
        //
        //     rect = new RectangleF(rect.Position, viewport.Size - (rect.Position - viewport.Position));
        //     // ConsolePlugin.Logger.Log(NoteLevel.Waring, $"P:{rect.Position} S:{rect.Size}");
        //     // ConsolePlugin.Logger.Log(NoteLevel.Waring, $"=======================================");
        //     Items.Last().Build(SurfaceDrawer, ref rect);
        // }

        public override void Draw(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites, ref IInteractive interactive)
        {
            base.Draw(drawer, ref viewport, ref sprites, ref interactive);
            
            var count = _containers.Count;
            var step = drawer.GridStep;
            var alt = _alignment;
    
            float s;
            Vector2 size, shiftVector;
    
            switch (alt)
            {
                case Alignment.Left:
                case Alignment.Right:
                    s = Math.Max(step.X, viewport.Size.X / count);
                    size = new Vector2(s, viewport.Size.Y);
    
                    shiftVector = new Vector2(s, 0);
                    break;
                case Alignment.Up:
                case Alignment.Down:
                case Alignment.DownLeft:
                case Alignment.DownRight:
                    s = Math.Max(step.Y, viewport.Size.Y / count);
                    size = new Vector2(viewport.Size.X, s);
    
                    shiftVector = new Vector2(0, s);
                    break;
                case Alignment.CenterLeft:
                    s = Math.Max(step.X, viewport.Size.X / count);
                    size = new Vector2(s, viewport.Size.Y);
    
                    shiftVector = new Vector2(s, 0);
                    break;
                default:
                    s = Math.Max(step.Y, viewport.Size.Y / count);
                    size = new Vector2(viewport.Size.X, s);
    
                    shiftVector = new Vector2(0, s);
                    break;
            }
    
            // ConsolePlugin.Logger.Log(NoteLevel.Waring, $"=======================================");
            // ConsolePlugin.Logger.Log(NoteLevel.Waring, $"P:{viewport.Position} S:{viewport.Size}");
            var rect = new RectangleF(viewport.Position, size);
            for (var j = 0; j < count - 1; j++)
            {
                Add(_containers[j], new RectangleF(shiftVector, size / viewport.Size));
                //_containers[j]..Build(SurfaceDrawer, ref rect);
    
                rect.Position += shiftVector;
                
                //ConsolePlugin.Logger.Log(NoteLevel.Waring, $"P:{rect.Position} S:{rect.Size}");
            }
    
            rect = new RectangleF(rect.Position, viewport.Size - (rect.Position - viewport.Position));
            // ConsolePlugin.Logger.Log(NoteLevel.Waring, $"P:{rect.Position} S:{rect.Size}");
            // ConsolePlugin.Logger.Log(NoteLevel.Waring, $"=======================================");
            //Items.Last().Build(SurfaceDrawer, ref rect);
    
            //Add(_containers.Last(), rect);
            Add(_containers.Last(), new RectangleF(rect.Position - viewport.Position, rect.Size / viewport.Size));
        }
    }
}