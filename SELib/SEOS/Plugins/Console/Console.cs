using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    interface IConsole
    {
        void ShowMsgBox(string msg);
        void SwitchPage(string to);
        void SwitchPage(ContentPage to);
    }
    class Console : IConsole
    {
        public string ConsoleId;
        IInteractive _lastInteractive;
        Content _currentContent;
        IMyTextSurface _surface;
        ConsoleStyle _style;
        KeyValuePair<RectangleF, Content> _userArea;
        KeyValuePair<RectangleF, MsgBox> _msgBoxArea;

        RectangleF _sysAreaRect;
        ContentPanel _sysAreaContent;

        ConsoleInput _input;
        RectangleF _viewport;
        Repository<string, Content> _userContent;
        public Console(IEnumerable<IUserContent> content, IMyTextSurface surface, string consoleId, string startPage, ConsoleStyle style, KeyValuePair<RectangleF, Content> userArea, KeyValuePair<RectangleF, ContentPanel> sysArea, KeyValuePair<RectangleF, MsgBox> msgBoxArea)
        {
            _surface = surface;
            ConsoleId = consoleId;
            _style = style;
            _userArea = userArea;
            _userContent = new Repository<string, Content>();
            _msgBoxArea = msgBoxArea;
            _sysAreaRect = sysArea.Key;
            _sysAreaContent = new ConsoleSysPanel(this).Add(sysArea.Value);

            SetupSurface();
            InitContent(content);
            SwitchPage(startPage);
        }
        
        void SetupSurface()
        {
            _surface.ContentType = ContentType.SCRIPT;
            _surface.Script = "";
            _surface.ScriptBackgroundColor = Color.Black; //_layout.Style.SecondColor;
            _surface.ScriptForegroundColor = Color.Black;
            
            _viewport = new RectangleF((_surface.TextureSize - _surface.SurfaceSize) / 2f, _surface.SurfaceSize);
            _viewport = new RectangleF(_viewport.Position + ConsolePluginSetup.SCREEN_BORDER_PX,
                _viewport.Size - ConsolePluginSetup.SCREEN_BORDER_PX * 2);
        }
        void InitContent(IEnumerable<IUserContent> content)
        {
            using (var frame = _surface.DrawFrame())
            {
                var sprites = new List<MySprite>();
                var bg = MySprite.CreateSprite("SquareTapered", _viewport.Center, _viewport.Size * 0.25f);
                var redCircle = new MySprite()
                {
                    Color = Color.Red,
                    Type = SpriteType.TEXTURE,
                    Data = "SquareTapered",
                    Position = _viewport.Center,
                    Size = _viewport.Size * 0.1f,
                    Alignment = TextAlignment.CENTER
                };

                foreach (var userContent in content)
                {
                    var viewport = _viewport;
                    var nodes = userContent.OnBuild();
                    foreach (var node in nodes)
                    {
                        new ContentText("Build '" + node.NameId + "' page")
                            .Draw(viewport, Vector2.PositiveInfinity,  ConsoleStyle.BlackWhiteRed, ref sprites, ref _lastInteractive,
                                MeasureStringInPixels, _surface.FontSize);
                        
                        BuildContent(node);
                    }
                }
                
                frame.AddRange(sprites);
            }
        }

        void BuildContent(ContentPage page)
        {
            _userContent.Add(page.NameId, page);
        }
        public void SwitchPage(string to)
        {
            _currentContent = _userContent.GetOrDefault(to);
        }

        public void SwitchPage(ContentPage to)
        {
            BuildContent(to);
            SwitchPage(to.NameId);
        }

        public void RemoveInput() => _input = null;
        public void ApplyInput(IMyCockpit controller)
        {
            if (controller == null) return;
            if (_input == null || _input.Cockpit != controller)
            {
                _input = new ConsoleInput(controller, this)
                {
                    OnClick = args =>
                    {
                        _lastInteractive?.OnClick(args.Console);
                    }
                };
            }
        }

        public void Draw()
        {
            IInteractive interactive = null;
            using (var frame = _surface.DrawFrame())
            {
                var spriteList = new List<MySprite>();

                var arrowPos = _input?.ArrowPos(_viewport) ?? Vector2.PositiveInfinity;
                    
                Draw(arrowPos, ref spriteList, ref interactive);

                if (_input!= null && _input.IsEnableControl)
                    DrawArrow(arrowPos, ref spriteList);

                frame.AddRange(spriteList);
            }
                
            if (_lastInteractive == interactive) return;

            _lastInteractive?.OnHoverEnable(false);
            _lastInteractive = interactive;
            _lastInteractive?.OnHoverEnable(true);
        }
        
        Vector2 MeasureStringInPixels(string txt, float scale) => _surface.MeasureStringInPixels(new StringBuilder(txt), _style.FontId, scale);
        
        void Draw(Vector2 arrowPos, ref List<MySprite> spriteList, ref IInteractive newInteractive)
        {
            var viewport = _viewport;
            var userContent = _currentContent ?? _userArea.Value;
            var textScale = _surface.FontSize;

            _sysAreaContent
                .Draw(CalcViewport(viewport, _sysAreaRect), arrowPos, _style, ref spriteList, ref newInteractive, MeasureStringInPixels, textScale);
            userContent
                .Draw(CalcViewport(viewport, _userArea.Key), arrowPos, _style, ref spriteList, ref newInteractive, MeasureStringInPixels, textScale);
            _msgBoxArea.Value
                .Draw(CalcViewport(viewport, _msgBoxArea.Key), arrowPos, _style, ref spriteList, ref newInteractive, MeasureStringInPixels, textScale);
        }

        public void Tick()
        {
            _input?.Tick();
            if (DateTime.Now > _msgAutoCloseTime) _msgBoxArea.Value.Hide();
        }

        public void Message(string msg)
        {
            _input?.Message(msg);
        }

        DateTime _msgAutoCloseTime;
        public void ShowMsgBox(string msg)
        {
            _msgAutoCloseTime =DateTime.Now + TimeSpan.FromSeconds(ConsolePluginSetup.MSG_SHOW_TIME_SEC);
            _msgBoxArea.Value.Show(msg);
        }

        RectangleF CalcViewport(RectangleF absViewport, RectangleF offset)
        {
            var position = absViewport.Position + offset.Position * absViewport.Size;
            var size = absViewport.Size * offset.Size;
                
            return new RectangleF(position, size);
        }
        
        void DrawArrow(Vector2? pos, ref List<MySprite> sprites)
        {
            /*
            var style =Style;
            var hsv =Style.FirstColor.ColorToHSV();
            hsv.X -= 360;
            hsv.X = Math.Abs(hsv.X);
            */
            sprites.Add
            (
                new MySprite
                {
                    Type = SpriteType.TEXTURE,
                    Data = "Triangle",
                    Position = pos,
                    Size = new Vector2(8, 12),
                    Color = Color.Lighten(_style.ThirdColor, 0.8), //hsv.HSVtoColor(),
                    RotationOrScale = -(float)Math.PI / 3f
                }
            );
        }
        class ConsoleSysPanel : ContentPanel
        {

            public ConsoleSysPanel(Console console)
            {
                Vertical = true;
                var nameInfoPanel = new ContentPanel();
                var iconInfoPanel = new ContentPanel(4);
                var lcdName = new ContentPanel(vertical: true)
                        .Add(new ContentText("LCD NAME", alignment: TextAlignment.LEFT))
                        .Add(new ContentText(new ReactiveProperty<string>(() => console.ConsoleId)
                            , alignment: TextAlignment.RIGHT))
                    ;
                var pageName = new ContentPanel(vertical: true)
                        .Add(new ContentText("PAGE", alignment: TextAlignment.LEFT))
                        .Add(new ContentText(new ReactiveProperty<string>(() => console._userContent.GetKeyFor(console._currentContent))
                            , alignment: TextAlignment.RIGHT))
                    ;

                nameInfoPanel
                    .Add(lcdName)
                    .Add(pageName)
                    ;
                
                Add(nameInfoPanel);
                Add(iconInfoPanel);
            }
        }
    }

    class MsgBox : ContentPanel
    {
        string _msg;
        public MsgBox()
        {
            Visible = false;
            
            var upPanel = new ContentPanel();
            var contentPanel = new ContentPanel(10)
                .Add(new ContentText(new ReactiveProperty<string>(() => _msg), alignment: TextAlignment.CENTER));

            Add(upPanel);
            Add(contentPanel);
        }

        public void Show(string msg)
        {
            _msg = msg;
            Visible = true;
        }

        public void Hide()
        {
            Visible = false;
        }
    }
}