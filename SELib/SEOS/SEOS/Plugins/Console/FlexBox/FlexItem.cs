using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRageMath;

namespace IngameScript
{ 
    enum FlexAlignItem
    {
        Start,
        End,
        Center,
        Stretch
    }
    enum FlexAlignContent
    {
        Start,
        End,
        Center,
        Stretch,
        SpaceBetween,
        SpaceAround
    }

    enum FlexDirection
    {
        Row,
        RowReverse,
        Column,
        ColumnRevers
    }

    enum JustifyContent
    {
        Start,
        End,
        Center,
        SpaceBetween,
        SpaceAround,
        SpaceEvery
    }

    struct FlexSize
    {
        static Vector2 MinSize = new Vector2(16, 9) * 1;
        uint _w, _h;
        Vector2? _size;

        public bool HasValue => _size.HasValue;

        // 0 - Auto size

        public Vector2 ResolveSize(Vector2 baseSize)
        {
            if (HasValue) return Get;
            
            var s = baseSize * new Vector2(_w, _h) * 0.01f;

            return new Vector2(Math.Max(MinSize.X, s.X), Math.Max(MinSize.Y, s.Y));
        }
        public Vector2 Get => _size.Value;

        public static FlexSize FromPixel(Vector2 size) => new FlexSize
        {
            _size = new Vector2(Math.Max(MinSize.X, size.X), Math.Max(MinSize.Y, size.Y)),
        };

        public static FlexSize FromRelative(Vector2I size) => new FlexSize
        {
            _w = Clamp(size.X),
            _h = Clamp(size.Y),
        };

        public static FlexSize FromPixel(float w, float h) => FromPixel(new Vector2(w, h));
        public static FlexSize FromRelative(int w, int h) => FromRelative(new Vector2I(w, h));
        public static FlexSize FullViewport => new FlexSize{_h = 100, _w = 100};
        public static FlexSize Minimum => new FlexSize{_size = MinSize};
        static uint Clamp(int v) => v <= 0 ? 100 : (uint) MathHelper.Clamp(v, 0, 100);
    }
    class FlexItem
    {
        //public bool Wrap;

        public FlexDirection Direction = FlexDirection.Row;
        public FlexAlignItem AlignItem = FlexAlignItem.Stretch;
        public FlexAlignContent AlignContent = FlexAlignContent.Start;
        public JustifyContent Justify = JustifyContent.SpaceAround;
        public FlexSize Size = FlexSize.FullViewport;
        public int Order = 0;
        public int Flex = 1;
        public bool Visible = true;

        readonly Content _content;
        Dictionary<int, List<FlexItem>> _items = new Dictionary<int, List<FlexItem>>();

        int _layer;
        public FlexItem()
        {
            
        }
        public FlexItem(Content content)
        {
            _content = content;
        }
        public FlexItem(FlexItem content)
        {
            Add(content);
        }

        public List<Content> Build(RectangleF viewport)
        {
            var retVal = new List<Content>();
            if (!Visible) return retVal;
            
            ToAlignItem(ref viewport);
            if (_items.Values.SelectMany(x => x).Any())
            {
                var vptItems = 
                    ToJustify(viewport, ToAlignContent(viewport, SetupFlexSize(viewport, this)));
                
                foreach (var item in vptItems.Keys)
                {
                    var vpt = vptItems[item];
                    retVal.AddRange(item.Build(vpt));
                }
                foreach (var content in retVal)
                {
                    content.Layer += _layer;
                }
            }
            if (_content != null)
            {
                _content.SetViewport(viewport);
                retVal.Add(_content);
            }
            
            return retVal;
        }

        public FlexItem Add(FlexItem itm)
        {
            if (!_items.ContainsKey(itm.Order))
                _items.Add(itm.Order, new List<FlexItem>());
            
            _items[itm.Order].Add(itm);

            itm._layer = _layer + 1;
            
            return this;
        }
        public FlexItem Add(Content content, int flex=1, int order = 1, FlexSize? size = null)
        {
            var itm = new FlexItem(content)
            {
                Flex = flex,
                Order = order,
                Size = size ?? FlexSize.FullViewport,
            };

            return Add(itm);
        }
        public void Remove(FlexItem item)
        {
            _items[item.Order].Remove(item);
        }
        void ToAlignItem(ref RectangleF viewport)
        {
            var size = Size.ResolveSize(viewport.Size);
            var pos = viewport.Position;
            switch (AlignItem)
            {
                case FlexAlignItem.Start:
                    break;
                case FlexAlignItem.End:
                    pos = viewport.Position + viewport.Size - size;
                    break;
                case FlexAlignItem.Center:
                    pos = viewport.Center - size / 2;
                    break;
                case FlexAlignItem.Stretch:
                    size = viewport.Size;
                    break;
                default:
                    throw new Exception("ArgumentOutOfRangeException");
            }

            //Size = FlexSize.FromPixel(size);
            
            viewport.Position = pos;
            viewport.Size = size;
        }
        Dictionary<FlexItem, Vector2> SetupFlexSize(RectangleF viewport, FlexItem line)
        {
            var flexItems = line._items.Values.SelectMany(x => x).ToList();

            var lineSize = viewport.Size;
            var freeSpace = lineSize - flexItems.Where(x=> x.Size.HasValue).Aggregate(Vector2.Zero, (a, b)=> a + b.Size.Get);

            var sumFlex = flexItems.Where(x=> !x.Size.HasValue).Sum(x => x.Flex);
            var normalSize = freeSpace / sumFlex;

            var retVal = new Dictionary<FlexItem, Vector2>();
            
            foreach (var item in flexItems)
            {
                Vector2 s;
                if (!item.Size.HasValue)
                {
                    if (line.Direction == FlexDirection.Column || line.Direction == FlexDirection.ColumnRevers)
                    {
                        s = item.Size.ResolveSize(new Vector2(lineSize.X, normalSize.Y * item.Flex));
                    }
                    else
                    {
                        s = item.Size.ResolveSize(new Vector2(normalSize.X * item.Flex, lineSize.Y));
                    }

                }
                else
                    s = item.Size.Get;
                
                retVal.Add(item, s);
            }

            return retVal;
        }

        Dictionary<FlexItem, RectangleF> ToAlignContent(RectangleF viewport, Dictionary<FlexItem, Vector2> sizeItems)
        {
            var retVal = new Dictionary<FlexItem, RectangleF>();
            var sumSize = sizeItems.Select(x => x.Value).Aggregate(Vector2.Zero, (a, b) => a + b);
            sumSize = Direction == FlexDirection.Column || Direction == FlexDirection.ColumnRevers
                    ? new Vector2(sumSize.X, viewport.Size.Y)
                    : new Vector2(viewport.Size.X, sumSize.Y)
                ;
            var freeSpace = viewport.Size - sumSize;
            freeSpace = Direction == FlexDirection.Column || Direction == FlexDirection.ColumnRevers
                    ? new Vector2(freeSpace.X, 0)
                    : new Vector2(0, freeSpace.Y)
                ;
            
            var pos = viewport.Position;
            var shift = Vector2.Zero;
            
            switch (AlignContent)
            {
                case FlexAlignContent.Start:
                    // остаеться как есть
                    break;
                case FlexAlignContent.End:
                    pos += freeSpace;
                    break;
                case FlexAlignContent.Center:
                    pos = viewport.Center - freeSpace / 2;
                    break;
                case FlexAlignContent.Stretch:
                    // применяеться в цикле ниже
                    break;
                case FlexAlignContent.SpaceBetween:
                    // нужен Wrap. Пока работает как Start
                    break;
                case FlexAlignContent.SpaceAround:
                    // нужен Wrap. Пока работает как Start
                    break;
                default:
                    throw new Exception("ArgumentOutOfRangeException");
            }

            pos = Direction == FlexDirection.Column || Direction == FlexDirection.ColumnRevers
                    ? new Vector2(pos.X, viewport.Position.Y)
                    : new Vector2(viewport.Position.X, pos.Y)
                ;
            foreach (var item in sizeItems.Keys)
            {
                var s = sizeItems[item];
                var size = AlignContent == FlexAlignContent.Stretch
                    ? Direction == FlexDirection.Column || Direction == FlexDirection.ColumnRevers
                        ? new Vector2(viewport.Width, s.Y)
                        : new Vector2(s.X, viewport.Height)
                    : s;
                
                retVal.Add(item, new RectangleF(pos, size));

                shift += size;
                shift = Direction == FlexDirection.Column || Direction == FlexDirection.ColumnRevers
                        ? new Vector2(0, shift.Y)
                        : new Vector2(shift.X, 0)
                    ;
                pos += shift;
            }

            return retVal;
        }

        Dictionary<FlexItem, RectangleF> ToJustify(RectangleF viewport, Dictionary<FlexItem, RectangleF> sizeItems)
        {
            var retVal = new Dictionary<FlexItem, RectangleF>();
            var fillSize = sizeItems.Values.Select(x => x).Aggregate(Vector2.Zero, (a, b) => a + b.Size);
            var freeSpace = (Direction == FlexDirection.Column || Direction == FlexDirection.ColumnRevers)
                    ? new Vector2(0, viewport.Size.Y - fillSize.Y)
                    : new Vector2(viewport.Size.X - fillSize.X, 0)
                ;
            var pos = viewport.Position;
            var space = Vector2.Zero;
            
            switch (Justify)
            {
                case JustifyContent.Start:
                    break;
                case JustifyContent.End:
                    pos += freeSpace;
                    break;
                case JustifyContent.Center:
                    pos += freeSpace / 2;
                    break;
                case JustifyContent.SpaceBetween:
                    space = freeSpace / (sizeItems.Count - 1);
                    break;
                case JustifyContent.SpaceAround:
                    space = freeSpace / (sizeItems.Count);
                    pos += space / 2;
                    break;
                case JustifyContent.SpaceEvery:
                    space = freeSpace / (sizeItems.Count+1);
                    pos += space;
                    break;
                default:
                    throw new Exception("ArgumentOutOfRangeException");
            }
            
            foreach (var item in sizeItems.Keys)
            {
                var size = sizeItems[item].Size;
                retVal.Add(item, new RectangleF(pos, size));

                switch (Direction)
                {
                    case FlexDirection.Row:
                    case FlexDirection.RowReverse:
                        pos.X += space.X;
                        pos.Y = viewport.Position.Y;
                        size.Y = 0;
                        space.Y = 0;
                        break;
                    case FlexDirection.Column:
                    case FlexDirection.ColumnRevers:
                        pos.Y += space.Y;
                        pos.X = viewport.Position.X;
                        size.X = 0;
                        space.X = 0;
                        break;
                    default:
                        throw new Exception("ArgumentOutOfRangeException");
                }
                pos += size + space;
            }

            return retVal;
        }
    }
    
    class TabPanel : FlexItem
    {
        FlexItem _buttonsPanel, _tabPanel;
        Dictionary<string, FlexItem> _tabs = new Dictionary<string, FlexItem>();
        Dictionary<FlexItem, FlexItem> _buttons = new Dictionary<FlexItem, FlexItem>();

        string _current;
        public TabPanel(params Page[] pages)
        {
            Direction = FlexDirection.Column;
            AlignItem = FlexAlignItem.Stretch;
            AlignContent = FlexAlignContent.Stretch;
            Justify = JustifyContent.SpaceAround;
            
            _buttonsPanel = new FlexItem
            {
                AlignItem = FlexAlignItem.Center, 
                Direction = FlexDirection.Row, 
                Justify = JustifyContent.Start,
                Size = FlexSize.FromRelative(100, 10)
            };
            _tabPanel = new FlexItem
            {
                AlignItem = FlexAlignItem.Stretch, 
                Direction = FlexDirection.Column, 
                Justify = JustifyContent.Center,
                Size = FlexSize.FromRelative(100, 90)
            };
            
            base.Add(_buttonsPanel);
            base.Add(_tabPanel);
            
            if (pages != null && pages.Any())
            {
                foreach (var page in pages) Add(page);
                Switch(pages[0].Id);
            }
        }

        public new TabPanel Add(FlexItem page) => Add(page, page.GetType().Name);

        public TabPanel Add(FlexItem page, string id)
        {
            _tabs.Add(id, page);
            _tabPanel.Add(page);
            
            var bt = new Switch<string>(id, new ReactiveProperty<string>(() => _current), (s, i) => Switch(i));
            var button = new FlexItem(bt) {AlignItem = FlexAlignItem.Center};
            _buttons.Add(page, button);
            _buttonsPanel.Add(button);
            return this;
        }

        public new TabPanel Remove(FlexItem page)
        {
            var id = page.GetType().Name;
            if (_tabs.ContainsKey(id))
            {
                Remove(id);
            }
            return this;
        }
        public TabPanel Remove(string pageId)
        {
            if (_tabs.ContainsKey(pageId))
            {
                var page = _tabs[pageId];
                _tabs.Remove(pageId);
                _tabPanel.Remove(page);
                
                _buttonsPanel.Remove(_buttons[page]);
                _buttons.Remove(page);
            }
            return this;
        }

        public void Switch(string id)
        {
            if (!_tabs.ContainsKey(id)) return;
            
            if (!string.IsNullOrEmpty(_current))
            {
                _tabs[_current].Visible = false;
            }
            
            _current = id;
            
            _tabs[_current].Visible = true;
        }
        
        public void Switch(FlexItem page)
        {
            if (_tabs.ContainsValue(page))
                Switch(_tabs.First(x=> x.Value == page).Key);
        }
    }
    
    class Page : FlexItem
    {
        public readonly string Id;

        public Page(string titleId = "") : base(new Panel{Layer = Layers.BG})
        {
            Direction = FlexDirection.Column;
            AlignItem = FlexAlignItem.Stretch;

            Id = string.IsNullOrEmpty(titleId)? GetType().Name : titleId;

            var title = new FlexItem(new Panel{Layer = Layers.BG}) {Size = FlexSize.FromRelative(100, 10)};
            title.Add(new Text(Id));
            Add(title);
            //Add(new Text(Id), size: FlexSize.FromRelative(100, 10));
        }
    }

    class MsgBox : Page
    {
        public MsgBox(NoteLevel level, string msg) : base(level.ToString())
        {
            Direction = FlexDirection.Column;
            Justify = JustifyContent.Center;
            AlignItem = FlexAlignItem.Stretch;

            Add(new FlexItem(new Text(msg)) {AlignItem = FlexAlignItem.Center});
        }
    }
}