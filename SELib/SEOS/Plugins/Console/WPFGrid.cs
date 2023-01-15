using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    abstract class WPFContainerItem<T> : WPFItem where T : WPFItem
    {
        protected List<T> Container;
        protected WPFContainerItem(List<T> init = null, string def="") : base(def)
        {
            Container = init ?? new List<T>();
        }

        public void Add(params T[] itm)
        {
            Container.AddRange(itm);
            Resize(Canvas.Viewport);
        }
        public void Remove(params T[] itms)
        {
            foreach (var itm in itms)
            {
                Container.Remove(itm);
            }

            Resize(Canvas.Viewport);
        }
        public void Remove(params int[] itmInd)
        {
            Container.RemoveIndices(itmInd.ToList());
            Resize(Canvas.Viewport);
        }

        protected override void OnResize(RectangleF viewport)
        {
            foreach (var item in Container)
            {
                item.Resize(viewport);
            }
        }
        
        public override void SetStyle(ConsoleStyle style)
        {
            base.SetStyle(style);
            
            foreach (var item in Container)
            {
                item.SetStyle(style);
            }
        }

        public override void Draw(ref List<MySprite> sprites, ref IInteractive newInteractive, Func<string, float, Vector2> measureStringInPixels, float textScale,
            Vector2 arrowPos)
        {
            foreach (var row in Container)
            {
                row.Draw(ref sprites, ref newInteractive, measureStringInPixels, textScale, arrowPos);
            }
        }
    }
    class WPFGrid : WPFContainerItem<Row>
    {
        public WPFGrid(List<Row> rows, string def="") : base(rows, def)
        { }
        protected override void OnResize(RectangleF viewport)
        {
            var count = Container.Count;
            var rowSize = new Vector2(Canvas.Viewport.Width, Canvas.Viewport.Height/count);
            var rowPos = Canvas.Viewport.Position;
            for (var i = 0; i < count; i++)
            {
                var row = Container[i];
                row.Resize(new RectangleF(rowPos + new Vector2(0, rowSize.Y * i), rowSize));
            }
        }
    }
    
    // TODO Grid в Grid не вставлю. Может стоит ??
    sealed class Row: WPFContainerItem<WPFControl>
    {
        public Row(List<WPFControl> cells, string def="") : base(cells, def)
        { }

        protected override void OnResize(RectangleF viewport)
        {
            var count = Container.Count;
            var celSize = new Vector2(Canvas.Viewport.Width/count, Canvas.Viewport.Height);
            var celPos = Canvas.Viewport.Position;
            for (var i = 0; i < count; i++)
            {
                var cel = Container[i];
                cel.Resize(new RectangleF(celPos + new Vector2(celSize.X * i, 0), celSize));
            }
        }
    }
}