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
        void SwitchPage(Page to);
    }
    
    class Console : IConsole
    {
        public string ConsoleId;
        public RectangleF Viewport => _sysLayoutPage.Viewport;
        IInteractive _lastInteractive;
        //Page _currentContent;
        IMyTextSurface _surface;
        SysLayoutPage _sysLayoutPage;
        

        ConsoleInput _input;
        //RectangleF _viewport;
        Repository<string, Page> _userContent;
        public Console(IMyTextSurface surface, SysLayoutPage sysLayoutPage, IEnumerable<ISEWPFContent> content, string consoleId, string startPage)
        {
            _surface = surface;
            _sysLayoutPage = sysLayoutPage;
            ConsoleId = consoleId;
            _userContent = new Repository<string, Page>();

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
            
            var viewport = new RectangleF((_surface.TextureSize - _surface.SurfaceSize) / 2f, _surface.SurfaceSize);
            viewport = new RectangleF(viewport.Position + ConsolePluginSetup.SCREEN_BORDER_PX,
                viewport.Size - ConsolePluginSetup.SCREEN_BORDER_PX * 2);
            
            _sysLayoutPage.Resize(viewport);
        }
        void InitContent(IEnumerable<ISEWPFContent> content)
        {
            using (var frame = _surface.DrawFrame())
            {
                var sprites = new List<MySprite>();

                foreach (var userContent in content)
                {
                    BuildContent(userContent.Page);
                    /*
                    foreach (var node in nodes)
                    {
                        new ContentText("Build '" + node.NameId + "' page")
                            .Draw(viewport, Vector2.PositiveInfinity,  ConsoleStyle.BlackWhiteRed, ref sprites, ref _lastInteractive,
                                MeasureStringInPixels, _surface.FontSize);
                        
                        
                    }
                    */
                }
                
                frame.AddRange(sprites);
            }
        }

        void BuildContent(Page page)
        {
            page.SetStyle(_sysLayoutPage.Style);
            _userContent.Add(page.TitleId, page);
        }
        public void SwitchPage(string to)
        {
            _sysLayoutPage.SwitchUserContent(_userContent.GetOrDefault(to) ?? new Page404(to));
        }

        public void SwitchPage(Page to)
        {
            BuildContent(to);
            SwitchPage(to.TitleId);
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

                var arrowPos = _input?.ArrowPos() ?? Vector2.PositiveInfinity;
                    
                _sysLayoutPage.Draw(ref spriteList, ref interactive, MeasureStringInPixels, _surface.FontSize, arrowPos);

                if (_input!= null && _input.IsEnableControl)
                    DrawArrow(arrowPos, ref spriteList);

                frame.AddRange(spriteList);
            }
                
            if (_lastInteractive == interactive) return;

            _lastInteractive?.OnHoverEnable(false);
            _lastInteractive = interactive;
            _lastInteractive?.OnHoverEnable(true);
        }
        
        Vector2 MeasureStringInPixels(string txt, float scale) => _surface.MeasureStringInPixels(new StringBuilder(txt), _sysLayoutPage.Style.FontId, scale);

        public void Tick()
        {
            _input?.Tick();
            if (DateTime.Now > _msgAutoCloseTime) _sysLayoutPage.HideMsgBox();
        }

        public void Message(string msg)
        {
            _input?.Message(msg);
        }

        DateTime _msgAutoCloseTime;
        public void ShowMsgBox(string msg)
        {
            _msgAutoCloseTime =DateTime.Now + TimeSpan.FromSeconds(ConsolePluginSetup.MSG_SHOW_TIME_SEC);
            _sysLayoutPage.ShowMsgBox(msg);
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
                    Color = Color.Lighten(_sysLayoutPage.Style.ThirdColor, 0.8), //hsv.HSVtoColor(),
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
                        .Add(new ContentText(new ReactiveProperty<string>(() => console._sysLayoutPage.TitleId)
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