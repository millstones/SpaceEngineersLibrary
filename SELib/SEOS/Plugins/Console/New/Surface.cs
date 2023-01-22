using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript.New
{
    class Surface
    {
        IEnumerable<IConsolePage> _pages;
        Repository<long, IMyCockpit> _inputs = new Repository<long, IMyCockpit>();
        Repository<long, SurfaceView> _consoles = new Repository<long, SurfaceView>();

        class SurfaceView : IConsole, IDrawSurface
        {
            public RectangleF Viewport { get; }
            public Vector2 GridStep { get; }

            public Vector2 ArrowPosition => _input?.ArrowPos(Viewport) ?? Vector2.PositiveInfinity;
            public string FontId => _panel.Font;
            public float FontScale => _panel.FontSize;


            public string ConsoleId;
            Surface _surface;
            IMyTextSurface _panel;
            Page _currentPage;
            MessageBox _currentMsgBox;

            Input _input;
            IInteractive _lastInteractive;

            public SurfaceView(Surface surface, IMyTextSurface panel, string consoleId, Page startPage)
            {
                ConsoleId = consoleId;
                _surface = surface;
                _panel = panel;
                _currentPage = startPage;

                Viewport = SetupSurface();
                GridStep = MeasureText(" ", FontId, FontScale);
            }

            RectangleF SetupSurface()
            {
                _panel.ContentType = ContentType.SCRIPT;
                _panel.Script = "";
                _panel.ScriptBackgroundColor = Color.Black; //_layout.Style.SecondColor;
                _panel.ScriptForegroundColor = Color.Black;

                var retVal = new RectangleF((_panel.TextureSize - _panel.SurfaceSize) / 2f, _panel.SurfaceSize);
                retVal = new RectangleF(retVal.Position + ConsolePluginSetup.SCREEN_BORDER_PX,
                    retVal.Size - ConsolePluginSetup.SCREEN_BORDER_PX * 2);

                return retVal;
            }

            List<MySprite> _frameSprites = new List<MySprite>();

            public void AddFrameSprites(List<MySprite> sprites)
            {
                _frameSprites.AddRange(sprites);
            }

            public Vector2 MeasureText(string txt, string fontId, float scale)=>
                _panel.MeasureStringInPixels(new StringBuilder(txt), fontId, scale);

            public void SwitchPage(string id)
            {
                SwitchPage(_surface.GetPage(id));
            }

            public void SwitchPage(Page page)
            {
                _currentPage = page;
            }

            DateTime _msgAutoCloseTime;

            public void ShowMessageBox(string msg)
            {
                ShowMessageBox(new MessageBox(msg));
            }

            public void ShowMessageBox(MessageBox msg)
            {
                _msgAutoCloseTime = DateTime.Now + TimeSpan.FromSeconds(ConsolePluginSetup.MSG_SHOW_TIME_SEC);
                _currentMsgBox = msg;
            }

            public void Draw()
            {
                var viewport = Viewport;
                var msgBoxViewport = new RectangleF(viewport.Position + Viewport.Size / 20,
                    viewport.Size - Viewport.Size / 10);

                _currentPage.Draw(this, ref viewport);


                IInteractive interactive = null;
                using (var frame = _panel.DrawFrame())
                {
                    var spriteList = new List<MySprite>();

                    _currentPage.Draw(this, ref viewport);
                    _currentMsgBox.Draw(this, ref msgBoxViewport);

                    if (_input != null && _input.IsEnableControl)
                        DrawArrow(ref spriteList);

                    frame.AddRange(spriteList);
                }

                if (_lastInteractive == interactive) return;

                _lastInteractive?.OnHoverEnable(false);
                _lastInteractive = interactive;
                _lastInteractive?.OnHoverEnable(true);
            }

            void DrawArrow(ref List<MySprite> sprites)
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
                        Position = ArrowPosition,
                        Size = new Vector2(8, 12),
                        Color = Color.Lighten(Color.White, 0.8), //hsv.HSVtoColor(),
                        RotationOrScale = -(float) Math.PI / 3f
                    }
                );
            }

            public void Tick()
            {
                _input?.Tick();
                if (DateTime.Now > _msgAutoCloseTime) _currentMsgBox = null;

            }

            public void Message(string msg)
            {
                _input?.Message(msg);
            }

            public void RemoveInput() => _input = null;

            public void ApplyInput(IMyCockpit controller)
            {
                if (controller == null) return;
                if (_input == null || _input.Cockpit != controller)
                {
                    _input = new Input(controller, this)
                    {
                        OnClick = args => { _lastInteractive?.OnClick(args.Surface); }
                    };
                }
            }
        }

        struct ArrowClickEventArgs
        {
            public SurfaceView Surface;
        }

        class Input : IDisposable
        {
            public IMyCockpit Cockpit;
            public bool IsEnableControl;
            public Action<ArrowClickEventArgs> OnClick;
            SurfaceView _mySurface;

            public Input(IMyCockpit cockpit, SurfaceView mySurface)
            {
                Cockpit = cockpit;
                _mySurface = mySurface;
            }

            void SwitchControl()
            {
                IsEnableControl = !IsEnableControl;
            }

            void UpStep()
            {
                _arrowPos += Vector2.UnitY * 10;
            }

            void DownStep()
            {
                _arrowPos -= Vector2.UnitY * 10;
            }

            void SwitchSelect()
            {

            }

            void Select()
            {
                OnClick?.Invoke(new ArrowClickEventArgs
                {
                    Surface = _mySurface
                });
            }

            void Deselect()
            {

            }

            Vector2 _arrowPos;

            public Vector2 ArrowPos(RectangleF viewport)
            {
                _arrowPos = Vector2.Clamp(_arrowPos, viewport.Position, viewport.Position + viewport.Size);
                return _arrowPos;
            }

            public void Tick()
            {
                if (ConsolePluginSetup.ENABLE_AUTO_SWITCH_CONTROL_LCD)
                {
                    IsEnableControl = Cockpit.IsUnderControl;
                }

                if (!IsEnableControl) return;

                {
                    var ri = Cockpit.RotationIndicator;
                    var delta = new Vector2(ri.Y, ri.X);
                    _arrowPos += delta * ConsolePluginSetup.MOUSE_SENSITIVITY;
                }
                {
                    var ri = Cockpit.RollIndicator;
                    if (ri > 0.3) Select();
                    if (ri < -0.3) Deselect();
                }
                {
                    var mi = Cockpit.MoveIndicator;
                    if (mi.Z > 0.3) UpStep();
                    if (mi.Z < -0.3) DownStep();
                }
            }

            public void Message(string argument)
            {
                var prefix = _mySurface.ConsoleId;
                if (argument == prefix + ConsolePluginSetup.SURFACE_CONTROLLER_SWITCH_CTRL_MARK)
                {
                    SwitchControl();
                }
                else if (argument == prefix + ConsolePluginSetup.SURFACE_CONTROLLER_UP_CTRL_MARK)
                {
                    UpStep();
                }
                else if (argument == prefix + ConsolePluginSetup.SURFACE_CONTROLLER_DOWN_CTRL_MARK)
                {
                    DownStep();
                }
                else if (argument == prefix + ConsolePluginSetup.SURFACE_CONTROLLER_SWITCH_SELECT_CTRL_MARK)
                {
                    SwitchSelect();
                }
            }

            public void Dispose()
            {
            }
        }

        public Surface(IEnumerable<IConsolePage> pages)
        {
            _pages = pages;
        }

        IEnumerator _updateBlocksProcess, _drawProcess;

        public void Tick(IEnumerable<IMyTerminalBlock> blocks)
        {

            if (_updateBlocksProcess == null || !_updateBlocksProcess.MoveNext())
                _updateBlocksProcess = UpdateBlocksProcess(blocks);
            if (_drawProcess == null || !_drawProcess.MoveNext())
                _drawProcess = DrawProcess();


            foreach (var console in _consoles.Values)
            {
                console.Tick();
            }
        }

        public void Message(string msg)
        {
            foreach (var console in _consoles.Values)
            {
                console.Message(msg);
            }
        }

        DateTime _nextTimeForBlocksUpdate = DateTime.MinValue;

        IEnumerator UpdateBlocksProcess(IEnumerable<IMyTerminalBlock> blocks)
        {
            while (_nextTimeForBlocksUpdate > DateTime.Now)
            {
                yield return null;
                //_os.Program.Echo.Invoke($"Await update: {(nextUpdate - DateTime.Now).TotalSeconds:#.#}");
            }

            _nextTimeForBlocksUpdate =
                DateTime.Now + TimeSpan.FromSeconds(ConsolePluginSetup.UPDATE_SURFACES_LIST_PERIOD_SEC);

            var myTerminalBlocks = blocks as IMyTerminalBlock[] ?? blocks.ToArray();
            var blocksSurface =
                myTerminalBlocks.Where(block => block.CustomName.Contains(ConsolePluginSetup.SURFACE_MARK));
            var blocksSurfaceCtrl = myTerminalBlocks.Where(block =>
                block.CustomName.Contains(ConsolePluginSetup.SURFACE_CONTROLLER_MARK));

            var step = FindControllers(blocksSurfaceCtrl);
            while (step.MoveNext())
            {
                //_status = $"FIND CONTROLLERS. {(float) MathHelper.Lerp(0, 0.5, step.Current):P}";

                yield return null;
            }

            step = FindCanvases(blocksSurface);
            while (step.MoveNext())
            {
                //_status = $"FIND SURFACE. {(float)MathHelper.Lerp(0.5, 1, step.Current):P}";
                yield return null;
            }

            //_status = "";


            foreach (var consoleId in _consoles.Ids)
            {
                var console = _consoles.GetOrDefault(consoleId);

                var controller = _inputs.GetOrDefault(consoleId);
                if (controller == null)
                    console.RemoveInput();
                else
                    console.ApplyInput(controller);
            }

            yield break;
        }

        IEnumerator<float> FindCanvases(IEnumerable<IMyTerminalBlock> blocks)
        {
            var myTerminalBlocks = blocks.ToArray();

            var ids = new List<long>();
            for (var i = 0; i < myTerminalBlocks.Length; i++)
            {
                var block = myTerminalBlocks[i];
                var lcdResult = ConsoleNameParser.ParseLcd(block.CustomName);
                var surface = block as IMyTextSurface ??
                              (block as IMyTextSurfaceProvider)?.GetSurface(lcdResult.SurfaceInd);

                if (surface == null)
                    throw new Exception($"Block name of {block.CustomName} is not surface[ind:{lcdResult.SurfaceInd}]");

                var id = block.GetId();
                //var id = string.IsNullOrEmpty(lcdResult.LcdNameId)? block.GetId().ToString() : lcdResult.LcdNameId;
                ids.Add(id);
                if (!_consoles.Contains(id))
                {
                    _consoles.Add(id,
                        new SurfaceView(this, surface, lcdResult.SurfaceNameId, GetPage(lcdResult.StartPageNameId)));
                }

                yield return (float) i / myTerminalBlocks.Length;
            }

            // remove unused
            foreach (var consolesId in _consoles.Ids)
            {
                if (ids.Contains(consolesId)) continue;

                _consoles.Remove(consolesId);
            }
        }

        // 'name [SURFACE_MARK]' - текстовая панель с первой попавшийся страницей
        // 'name [SURFACE_MARK@id]' - текстовая панель с первой попавшийся страницей, id - id (имя) для управления
        // 'name [SURFACE_MARK-s]' - текстовая панель . s-имя страницы
        // 'name [SURFACE_MARK-n]' - текстовая панель многопанельного терм. блока с первой попавшийся страницей. n-номер текстовой панели
        // 'name [SURFACE_MARK-n-s]' - текстовая панель многопанельного терм. блока. n-номер текстовой панели, s-имя страницы
        IEnumerator<float> FindControllers(IEnumerable<IMyTerminalBlock> blocks)
        {
            var myTerminalBlocks = blocks.ToArray();

            for (var i = 0; i < myTerminalBlocks.Length; i++)
            {
                var block = myTerminalBlocks[i];
                var cockpit = block as IMyCockpit;
                if (cockpit == null)
                    throw new Exception($"Block name of {block.CustomName} is not cockpit");
                var ctrlResult = ConsoleNameParser.ParseLcdController(block.CustomName);

                if (ctrlResult.ForLcdNameId == "") continue;
                var id = _consoles.GetKeyFor(x => x.ConsoleId == ctrlResult.ForLcdNameId, -1);

                if (id == -1) continue;

                _inputs.Add(id, cockpit);

                yield return (float) i / myTerminalBlocks.Length;
            }
        }

        int _tickAwait = 10;

        IEnumerator DrawProcess()
        {
            while (_tickAwait > 0)
            {
                yield return null;
                //_os.Program.Echo.Invoke($"Await draw: {ticks}");
                _tickAwait--;
            }

            _tickAwait = 10;

            foreach (var view in _consoles.Values)
            {
                view.Draw();
                //_status = _status ?? $"Draw [{i + 1}/{_consoles.Values.Length}]";
                yield return null;
                _tickAwait--;
            }

            //_status = "AWAIT";
            yield break;
        }

        Page GetPage(string id)
        {
            return _pages.FirstOrDefault(x => x.Page.Id == id)?.Page
                   ?? new Page404(id);
        }

        public void SwitchPage(string to, string onConsoleId)
        {
            var target = _consoles.Values.FirstOrDefault(x => x.ConsoleId == onConsoleId);
            if (target == null)
            {
                foreach (var console in _consoles.Values)
                {
                    console.SwitchPage(to);
                }
            }
        }

        public void ShowMsg(string msg, string onConsoleId)
        {
            var target = _consoles.Values.FirstOrDefault(x => x.ConsoleId == onConsoleId);
            if (target == null)
            {
                foreach (var console in _consoles.Values)
                {
                    console.ShowMessageBox(msg);
                }
            }
        }
    }
}