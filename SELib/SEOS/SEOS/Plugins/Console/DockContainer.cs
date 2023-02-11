namespace IngameScript
{
/*
    abstract class DockContainer<T>: PageItem where T : PageItem
    {
        Alignment _alignment;
    
        List<T> _items = new List<T>();

        protected DockContainer(Alignment alignment)
        {
            _alignment = alignment;
        }
    
        public DockContainer<T> Add(T item)
        {
            _items.Add(item);
            return this;
        }

        public override void Draw(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites, ref IInteractive interactive)
        {
            base.Draw(drawer, ref viewport, ref sprites, ref interactive);
            
            var count = _items.Count;
            var step = drawer.GridStep;
            var alt = _alignment;
    
            float s;
            Vector2 size, shiftVector;
    
            switch (alt)
            {
                case Alignment.Left:
                case Alignment.Right:
                    s = Math.Max(step.X, viewport.Size.X / count);
                    s = step.X;
                    size = new Vector2(s, viewport.Size.Y);
    
                    shiftVector = new Vector2(s, 0);
                    break;
                case Alignment.Up:
                case Alignment.Down:
                case Alignment.DownLeft:
                case Alignment.DownRight:
                    s = Math.Max(step.Y, viewport.Size.Y / count);
                    s = step.Y;
                    size = new Vector2(viewport.Size.X, s);
    
                    shiftVector = new Vector2(0, s);
                    break;
                case Alignment.CenterLeft:
                    s = Math.Max(step.X, viewport.Size.X / count);
                    s = step.X;
                    size = new Vector2(s, viewport.Size.Y);
    
                    shiftVector = new Vector2(s, 0);
                    break;
                default:
                    s = Math.Max(step.Y, viewport.Size.Y / count);
                    s = step.Y;
                    size = new Vector2(viewport.Size.X, s);
    
                    shiftVector = new Vector2(0, s);
                    break;
            }
    
            // ConsolePlugin.Logger.Log(NoteLevel.Waring, $"=======================================");
            // ConsolePlugin.Logger.Log(NoteLevel.Waring, $"P:{viewport.Position} S:{viewport.Size}");
            var rect = new RectangleF(viewport.Position, size);
            for (var j = 0; j < count - 1; j++)
            {
                _items[j].Draw(drawer, ref rect, ref sprites, ref interactive);

                rect.Position += shiftVector;
            }
    
            rect = new RectangleF(rect.Position, viewport.Size - (rect.Position - viewport.Position));
            // ConsolePlugin.Logger.Log(NoteLevel.Waring, $"P:{rect.Position} S:{rect.Size}");
            // ConsolePlugin.Logger.Log(NoteLevel.Waring, $"=======================================");
            //Items.Last().Build(SurfaceDrawer, ref rect);
    
            //Add(_containers.Last(), rect);

            _items.LastOrDefault()?.Draw(drawer, ref rect, ref sprites, ref interactive);
        }
    }

    class DockRow : DockContainer<PageItem>
    {
        public DockRow(Alignment alignment) : base(alignment)
        { }
    }

    class DockGrid : DockContainer<DockRow>
    {
        public DockGrid(Alignment alignment) : base(alignment)
        { }
    }
    */
}