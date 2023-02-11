using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    interface ISelectable
    {
        bool IsSelected { get; set; }
        bool IsCurrent { get; set; }
        void Increase();
        void Decrease();
    }
    
    public abstract class ConsoleItm
    {
        public string FontId = "White";
        public Color Color = Color.Red;
        public abstract string GetContent();

        public MySprite Sprite()
        {
            return new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = GetContent(),
                Color = Color,
                FontId = FontId,
                RotationOrScale = 0.75f,
                Alignment = TextAlignment.LEFT,
            };
        }
    }

    class ConsoleButton : ConsoleItm, ISelectable
    {
        string _text;

        readonly Action _action;

        public ConsoleButton(string text, Action action)
        {
            _text = $"[{text}]";
            _action = action;
        }
        public override string GetContent() => _text;
        
        public bool IsSelected
        {
            get
            {
                return false;
            }
            set
            {
                if (value)
                    _action();
            }
        }
        public bool IsCurrent { get; set; }
        public void Increase(){}
        public void Decrease(){}
    }
    abstract class ConsoleSimpleValue<TObj, TProp> : ConsoleItm
    {
        protected TObj _obj;
        protected Func<TObj, TProp> _getter;
        protected TProp Property => _getter(_obj);

        protected ConsoleSimpleValue(TObj obj, Func<TObj, TProp> get)
        {
            _obj = obj;
            _getter = get;
        }
        
        public override string GetContent() => Property.ToString();
    }
    abstract class ConsoleChangeValue<TObj, TProp> : ConsoleSimpleValue<TObj, TProp>, ISelectable
    {
        Action<TProp> _setter;
        protected new TProp Property
        {
            get { return _getter(_obj); }
            set { _setter(value); }
        }
        protected ConsoleChangeValue(TObj obj, Func<TObj, TProp> get, Action<TProp> set) : base(obj, get)
        {
            _setter = set;
        }

        public bool IsSelected { get; set; }
        public bool IsCurrent { get; set; }
        public abstract void Increase();
        public abstract void Decrease();
    }
    /*
    abstract class ConsoleValue<TObj, TProp> : ConsoleItm, ISelectable
    {
        public bool IsSelected { get; set; }
        public bool IsCurrent { get; set; }
        
        TObj _obj;
        Func<TObj, TProp> _getter;
        Action<TProp> _setter = prop => { };

        protected TProp Property
        {
            get { return _getter(_obj); }
            set { _setter(value); }
        }

        protected ConsoleValue(TObj obj, Func<TObj, TProp> get, Action<TProp> set = null)
        {
            _obj = obj;
            _getter = get;
            if (set != null)
                _setter = set;
        }
        
        public abstract void Increase();
        public abstract void Decrease();
        public override string GetContent() => Property.ToString();
    }
    */

    class Label : ConsoleSimpleValue<object, string> 
    {
        public Label(object obj) : base(obj, o => o.ToString())
        { }
    }

    class Label<T> : ConsoleSimpleValue<T, string> 
    {
        public Label(T obj, Func<T, string> get) : base(obj, get)
        { }
        public Label(T obj, Func<T, object> get) : base(obj, arg => get(arg).ToString())
        { }
    }

    class LabelChangeableBool<T> : ConsoleChangeValue<T, bool> 
    {
        public LabelChangeableBool(T obj, Func<T, bool> get, Action<bool> set) : base(obj, get, set)
        { }
        public override void Increase() => Property = !Property;
        public override void Decrease() => Increase();
    }

    class LabelChangeableInt<T> : ConsoleChangeValue<T, int> 
    {
        public LabelChangeableInt(T obj, Func<T, int> get, Action<int> set) : base(obj, get, set)
        { }
        public override void Increase() => Property += 1;
        public override void Decrease() => Property -= 1;
    }
    class LabelChangeableSingle<T> : ConsoleChangeValue<T, float>
    {
        float _minStep, _maxStep;
        public LabelChangeableSingle(T obj, Func<T, float> get, Action<float> set, float minStep = 0.001f, float maxStep = 0.001f) : base(obj, get, set)
        {
            _minStep = minStep;
            _maxStep = maxStep;
        }

        public override void Increase()
        {
            // TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }

        public override void Decrease()
        {
            // TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }
        public override string GetContent()
        {
            return Property.ToString("#.####");
        }
    }
    class LabelChangeableVector3<T> : ConsoleChangeValue<T, Vector3D>
    {
        float _minStep = 0.001f;
        float _maxStep = 0.001f;
        public LabelChangeableVector3(T obj, Func<T, Vector3D> get, Action<Vector3D> set) : base(obj, get, set)
        {
        }

        public override void Increase()
        {
            // TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }

        public override void Decrease()
        {
            // TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }

        public override string GetContent()
        {
            return Property.ToString("#.##");
        }
    }
    
    public class ConsoleLine
    {
        public List<ConsoleItm> Items { get; } = new List<ConsoleItm>();

        public void AddItm(ConsoleItm itm) => Items.Add(itm);
        public void AddItm(params ConsoleItm[] itm) => Items.AddRange(itm);
    }

    public abstract class ConsolePage
    {
        public bool Root;
        public SurfaceDrawer Drawer;
        public abstract string NameId { get; }
        List<ConsoleLine> _systemLines = new List<ConsoleLine>();

        protected ConsolePage()
        {
            _systemLines.Add(new ConsoleLine{Items = { new Label(NameId) }});
        }
        protected abstract IEnumerable<ConsoleLine> OnOpen();
        public IEnumerable<ConsoleLine> Open()
        {
            var retVal = new List<ConsoleLine>();
            retVal.AddRange(_systemLines);
            retVal.AddRange(OnOpen());
            return retVal;
        }
        protected void SwitchPage(ConsolePage page)
        {
            if (!page.Root)
                page._systemLines.Add
                (
                    new ConsoleLine()
                    {
                        Items =
                        {
                            new ConsoleButton("<-- back",() =>
                            {
                                SwitchPage(this);
                            })
                        }
                    }
                );
            Drawer.SwitchPage(page);
        }
    }

    public class SurfaceDrawer
    {
        public readonly IMyTextSurface Surface;
        RectangleF _viewport;
        
        public IEnumerable<ConsoleLine> Lines = new List<ConsoleLine>();
        LinkedList<ISelectable> _selectableItems = new LinkedList<ISelectable>();
        LinkedListNode<ISelectable> _currentCursorPosition;
        
        public SurfaceDrawer(IMyTextSurface surface, ConsolePage page)
        {
            Surface = surface;

            Surface.ContentType = ContentType.SCRIPT;
            Surface.Script = "";
            Surface.BackgroundColor = Color.DarkGray;
            _viewport = new RectangleF((Surface.TextureSize - Surface.SurfaceSize) / 2f, Surface.SurfaceSize);
            
            SwitchPage(page);
        }

        public void SwitchPage(ConsolePage page)
        {
            foreach (var item in _selectableItems)
            {
                item.IsCurrent = item.IsSelected = false;
            }
            _selectableItems.Clear();
            
            page.Drawer = this;
            Lines = page.Open();
            
            foreach (var line in Lines)
            {
                var itms = line.Items.OfType<ISelectable>();
                foreach (var itm in itms)
                {
                    _selectableItems.AddLast(itm);
                }
            }

            if (!_selectableItems.Any()) return;
            
            _currentCursorPosition = _selectableItems.First;
            _currentCursorPosition.Value.IsCurrent = true;
        }

        public void MoveForward()
        {
            if (_currentCursorPosition.Value.IsSelected)
            {
                _currentCursorPosition.Value.Increase();
                return;
            }
            if (_currentCursorPosition.Next == null) return;
                
            _currentCursorPosition.Value.IsCurrent = false;
            _currentCursorPosition.Value.IsSelected = false;
            _currentCursorPosition = _currentCursorPosition.Next;
            _currentCursorPosition.Value.IsCurrent = true;
        }
        public void MoveBackward()
        {
            if (_currentCursorPosition.Value.IsSelected)
            {
                _currentCursorPosition.Value.Decrease();
                return;
            }
            if (_currentCursorPosition.Previous == null) return;
                
            _currentCursorPosition.Value.IsCurrent = false;
            _currentCursorPosition.Value.IsSelected = false;
            _currentCursorPosition = _currentCursorPosition.Previous;
            _currentCursorPosition.Value.IsCurrent = true;
        }
        public void Select()
        {
            _currentCursorPosition.Value.IsSelected = !_currentCursorPosition.Value.IsSelected;
        }
        public IEnumerable<MySprite> DrawFrame(Vector2 startPosition)
        {
            var retVal = new List<MySprite>();
            var position = startPosition + _viewport.Position;

            var bgSprite = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "Grid",
                Position = position,
                Size = _viewport.Size,
                Color = Color.Black,
                Alignment = TextAlignment.LEFT
            };
            retVal.Add(bgSprite);
            
                foreach (var line in Lines)
                {
                    var size = new Vector2();
                    foreach (var itm in line.Items)
                    {
                        var contentSprite = itm.Sprite();
                        contentSprite.Position = position;
                        
                        size = Surface.MeasureStringInPixels(
                            new StringBuilder(contentSprite.Data + " "), contentSprite.FontId,
                            contentSprite.RotationOrScale);

                        //---------------------------------

                        var borderSprite = new MySprite
                        {
                            Type = SpriteType.TEXTURE,
                            Data = "SquareSimple",
                            Position = position + new Vector2(0, 16),
                            Size = size - new Vector2(3, 3),
                            Color = Color.Wheat,
                            Alignment = TextAlignment.LEFT
                        };

                        //-----------------------------------
                        //---------------------------------
                        var fillSprite = new MySprite
                        {
                            Type = SpriteType.TEXTURE,
                            Data = "White screen",
                            Position = position + new Vector2(0, 16),
                            Size = size - new Vector2(6, 6),
                            Color = Color.Black.Alpha(0.4f),
                            Alignment = TextAlignment.LEFT
                        };

                        //-----------------------------------

                        var selectable = itm as ISelectable;
                        if (selectable != null && selectable.IsCurrent)
                        {
                            if (selectable.IsSelected)
                                retVal.Add(borderSprite);
                            retVal.Add(fillSprite);
                        }

                        retVal.Add(contentSprite);

                        position.X += size.X;
                    }

                    position.X = 0;
                    position.Y += size.Y;
                }

            return retVal;
        }
    }
    public class Console
    {
        readonly IMessageBroker _msgBroker;
        List<SurfaceDrawer> _drawers = new List<SurfaceDrawer>();
        public Console(IMessageBroker msgBroker)
        {
            _msgBroker = msgBroker;
        }

        public void AddSurface(IMyTextSurface surface, ConsolePage page, string fwdCmd="", string bkwCmd="", string selCmd="")
        {
            page.Root = true;
            var drawer = new SurfaceDrawer(surface, page);
            if (!string.IsNullOrEmpty(selCmd))
                _msgBroker.Post(selCmd, drawer.Select);
            if (!string.IsNullOrEmpty(fwdCmd))
                _msgBroker.Post(fwdCmd, drawer.MoveForward);
            if (!string.IsNullOrEmpty(bkwCmd))
                _msgBroker.Post(bkwCmd, drawer.MoveBackward);
            
            _drawers.Add(drawer);
        }

        DateTime _messageDrawTime;
        public void Draw()
        {
            var drawTime = DateTime.Now;
            foreach (var drawer in _drawers)
            {
                var frame = drawer.Surface.DrawFrame();
                var position = new Vector2(0, 0);
                var size = drawer.Surface.MeasureStringInPixels(
                    new StringBuilder(" "), 
                    msgFontId,
                    msgScale);

                position += new Vector2(0, size.Y);
                frame.AddRange(drawer.DrawFrame(position));
                if (!string.IsNullOrEmpty(_lastMsg))
                {
                    if (_messageDrawTime == DateTime.MinValue)
                        _messageDrawTime = drawTime;
                    
                    if ((drawTime - _messageDrawTime).TotalSeconds < 1)
                        frame.AddRange(Message(drawer.Surface, Vector2.Zero, _lastMsg));
                    else
                    {
                        _lastMsg = "";
                        _messageDrawTime = DateTime.MinValue;
                    }
                }
                frame.Dispose();
            }
        }

        const string msgFontId = "Red";
        const float msgScale = 0.75f;

        IEnumerable<MySprite> Message(IMyTextSurface surface, Vector2 position, string content)
        {

            var size = surface.MeasureStringInPixels(
                new StringBuilder(content + " "), msgFontId, msgScale);
            var borderSprite = new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = position + new Vector2(0, 16),
                Size = size - new Vector2(3, 3),
                Color = Color.Wheat,
                Alignment = TextAlignment.LEFT
            };
            var fillSprite = new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "White screen",
                Position = position + new Vector2(0, 16),
                Size = size - new Vector2(6, 6),
                Color = Color.Red.Alpha(0.4f),
                Alignment = TextAlignment.LEFT
            };
            var contentSprite = new MySprite
            {
                Type = SpriteType.TEXT,
                Data = content,
                Position = position,
                Size = size - new Vector2(6, 6),
                Color = Color.Black,
                Alignment = TextAlignment.LEFT,
                FontId = msgFontId,
                RotationOrScale = msgScale,
            };

            return new[] {borderSprite, fillSprite, contentSprite};
        }

        string _lastMsg;
        public void Show(string msg)
        {
            _lastMsg = msg;
        }
    }
}