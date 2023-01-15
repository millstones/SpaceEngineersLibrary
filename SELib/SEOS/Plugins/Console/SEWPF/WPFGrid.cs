using System.Collections.Generic;
using System.Linq;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript.SEWPF
{
    sealed class WPFGrid : WPFItem
    {
        List<Row> _rows;
        
        public WPFGrid(string def, List<Row> rows) : base(def)
        {
            _rows = rows;
            
            OnResize(Canvas.Viewport);
        }


        protected override void OnResize(RectangleF viewport)
        {
            var count = _rows.Count;
            var rowSize = new Vector2(Canvas.Viewport.Width, Canvas.Viewport.Height/count);
            var rowPos = Canvas.Viewport.Position;
            for (var i = 0; i < count; i++)
            {
                var row = _rows[i];
                row.Resize(new RectangleF(rowPos + new Vector2(0, rowSize.Y * i), rowSize));
            }
        }

        public override void Draw(ref List<MySprite> sprites)
        {
            foreach (var row in _rows)
            {
                row.Draw(ref sprites);
            }
        }

        public void AddRow(params Row[] rows)
        {
            _rows.AddRange(rows);
            Resize(Canvas.Viewport);
        }

        public void RemoveRow(params Row[] rows)
        {
            foreach (var row in rows)
            {
                _rows.Remove(row);
            }

            Resize(Canvas.Viewport);
        }
        
        public void RemoveRow(params int[] rows)
        {
            _rows.RemoveIndices(rows.ToList());
            Resize(Canvas.Viewport);
        }
    }
    
    sealed class Row: WPFItem
    {
        List<Cell> _cells;
        public Row(string def, List<Cell> cells) : base(def)
        {
            _cells = cells;

            OnResize(Canvas.Viewport);
        }

        protected override void OnResize(RectangleF viewport)
        {
            var count = _cells.Count;
            var celSize = new Vector2(Canvas.Viewport.Width/count, Canvas.Viewport.Height);
            var celPos = Canvas.Viewport.Position;
            for (var i = 0; i < count; i++)
            {
                var cel = _cells[i];
                cel.Resize(new RectangleF(celPos + new Vector2(celSize.X * i, 0), celSize));
            }
        }

        public override void Draw(ref List<MySprite> sprites)
        {
            foreach (var cell in _cells)
            {
                cell.Draw(ref sprites);
            }
        }
        public void AddCell(params Cell[] cells)
        {
            _cells.AddRange(cells);
            OnResize(Canvas.Viewport);
        }

        public void RemoveCell(params Cell[] cells)
        {
            foreach (var row in cells)
            {
                _cells.Remove(row);
            }

            OnResize(Canvas.Viewport);
        }
        
        public void RemoveCell(params int[] cells)
        {
            _cells.RemoveIndices(cells.ToList());
            OnResize(Canvas.Viewport);
        }
    }

    sealed class Cell : WPFItem
    {
        WPFControl _control;

        public Cell(string def, WPFControl control) : base(def)
        {
            _control = control;
            
            OnResize(Canvas.Viewport);
        }

        protected override void OnResize(RectangleF viewport)
        {
            _control.Resize(Canvas.Viewport);
        }

        public override void Draw(ref List<MySprite> sprites)
        {
            _control.Draw(ref sprites);
        }
    }
}