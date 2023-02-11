﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    class Surface
    {
        IEnumerable<Page> _pages;
        Repository<long, IMyCockpit> _inputs = new Repository<long, IMyCockpit>();
        Repository<long, Drawer> _consoles = new Repository<long, Drawer>();

        public int LastDrawnSprites => _consoles.Values.Sum(x => x.LastDrawSprites);
        public int DrawerCount => _consoles.Values.Count();

        class Drawer : IConsole, ISurfaceDrawer
        {
            class SysCanvas : FreeCanvas
            {
                Drawer _drawer;
                DateTime _msgAutoCloseTime;

                Page _currentPage;
                RectangleF _currentPageViewport = CreateArea(new Vector2(0, 0.05f), new Vector2(1, 0.95f));
                MsgBoxItem _currentMsgBoxItem;
                RectangleF? _currentMsgBoxViewport;
                public SysCanvas(Drawer drawer, Page startPage)
                {
                    _drawer = drawer;
                    _currentPage = startPage;
                    Background = false;

                    Add(new FlexiblePanel<PageItem>(true)
                        .Add(new Text("SETUP"))
                        .Add(new Menu("PAGES", (console, s) => console.SwitchPage(s)) 
                            .Add(drawer._surface._pages.Select(x=> x.Title).ToArray())
                        )
                        .Add(new Text("STORAGE"))
                        .Add(new Text("ENERGY"))
                        .Add(new Text(()=> DateTime.Now.ToLongTimeString())),
                        CreateArea(Vector2.Zero, new Vector2(1, 0.05f))
                    );
                    //Add(_currentPage, _currentPageViewport);
                    Add(new Text("page-navigation"), CreateArea(new Vector2(0, 0.95f), Vector2.One));
                    
                    SwitchPage(startPage);
                }

                protected override void PreDraw()
                {
                    if (DateTime.Now > _msgAutoCloseTime)
                        CloseMessageBox();

                    Enabled = _currentMsgBoxItem == null;
                    base.PreDraw();
                }

                protected override List<MySprite> OnDraw(ISurfaceDrawer drawer, ref RectangleF viewport, ref IInteractive interactive)
                {
                    var retVal = base.OnDraw(drawer, ref viewport, ref interactive);

                    var msbVpt = drawer.Viewport;
                    if (_currentMsgBoxViewport.HasValue)
                        msbVpt = _currentMsgBoxViewport.Value;
                    else
                    {
                        msbVpt.Position += msbVpt.Size * 0.25f/2;
                        msbVpt.Size *= 0.75f;
                    }
                    
                    _currentMsgBoxItem?.Draw(drawer, ref msbVpt, ref retVal, ref interactive);
                    
                    return retVal;
                }

                public void ShowMessageBox(string msg, RectangleF? viewport, int closeSec)
                {
                    ShowMessageBox(new NoteMsgBox(NoteLevel.None, msg), viewport, closeSec);
                }

                public void ShowMessageBox(MsgBoxItem msg, RectangleF? viewport, int closeSec)
                {
                    CloseMessageBox();

                    _msgAutoCloseTime = DateTime.Now + TimeSpan.FromSeconds(closeSec < 0? ConsolePluginSetup.MSG_SHOW_TIME_SEC : closeSec);
                    _currentMsgBoxItem = msg;
                    _currentMsgBoxViewport = viewport;
                    _drawer._lastInteractive = null;
                }

                public void CloseMessageBox()
                {
                    _currentMsgBoxItem = null;
                    _currentMsgBoxViewport = null;
                }
                public void SwitchPage(string id)
                {
                    var page = _drawer._surface.GetPage(id);
                    if (page == null) 
                        ShowMessageBox(new NoteMsgBox(NoteLevel.Error, $"Page '{id}' NOT FOUND"), null, 3);
                    else SwitchPage(page);
                }

                public void SwitchPage(Page page)
                {
                    Remove(_currentPage);
                    _currentPage = page;
                    Add(_currentPage, _currentPageViewport);
                }
            }
            public RectangleF Viewport { get; }
            public Vector2 GridStep { get; }

            public Vector2 ArrowPosition => _input?.ArrowPos(Viewport) ?? Vector2.PositiveInfinity;
            public string FontId => _panel.Font;
            public float FontSize => _panel.FontSize;
            public ConsoleStyle Style { get; } = ConsoleStyle.MischieviousGreen2;

            public string ConsoleId;
            public int LastDrawSprites;
            
            Surface _surface;
            IMyTextSurface _panel;

            Input _input;
            IInteractive _lastInteractive;
            SysCanvas _sysCanvas;

            public Drawer(Surface surface, IMyTextSurface panel, string consoleId, Page startPage)
            {
                ConsoleId = consoleId;
                _surface = surface;
                _panel = panel;

                Viewport = SetupSurface();
                GridStep = MeasureText(" ", FontId, FontSize)*0.5f;
                
                _sysCanvas = new SysCanvas(this, startPage);
            }

            RectangleF SetupSurface()
            {
                _panel.ContentType = ContentType.SCRIPT;
                //_panel.Script = "";
                _panel.ScriptBackgroundColor = Color.Darken(Style.FirstColor, 0.2); //_layout.Style.SecondColor;
                _panel.ScriptForegroundColor = Color.Black;

                var retVal = new RectangleF((_panel.TextureSize - _panel.SurfaceSize) / 2f, _panel.SurfaceSize);
                retVal.Position += ConsolePluginSetup.SCREEN_BORDER_PX;
                retVal.Size -= ConsolePluginSetup.SCREEN_BORDER_PX * 2;

                return retVal;
            }

            public Vector2 MeasureText(string txt, string fontId, float scale)=>
                _panel.MeasureStringInPixels(new StringBuilder(txt), fontId, scale);

            public void SwitchPage(string id) => _sysCanvas.SwitchPage(id);
            public void SwitchPage(Page page) => _sysCanvas.SwitchPage(page);

            public void ShowMessageBox(string msg, RectangleF? viewport=null, int closeSec = int.MaxValue)=> _sysCanvas.ShowMessageBox(msg,viewport, closeSec);
            public void ShowMessageBox(MsgBoxItem msg, RectangleF? viewport=null, int closeSec = int.MaxValue) => _sysCanvas.ShowMessageBox(msg, viewport, closeSec);
            public void CloseMessageBox()
            {
                _sysCanvas.CloseMessageBox();
            }

            public void Draw()
            {
                var viewport = Viewport;

                IInteractive interactive = null;
                using (var frame = _panel.DrawFrame())
                {
                    var sprites = new List<MySprite>();
                    
                    _sysCanvas.Draw(this, ref viewport, ref sprites, ref interactive);
                    /*
                    _currentPage.Draw(this, ref viewport, ref sprites, ref interactive);
                    _currentMsgBox?.Draw(this, ref msgBoxViewport, ref sprites, ref interactive);
*/
                    if (_input != null && _input.IsEnableControl)
                        DrawArrow(ref sprites);

                    frame.Clip((int) viewport.Position.X, (int) viewport.Position.Y, (int) viewport.Width,
                        (int) viewport.Height);
                    frame.AddRange(sprites);

                    LastDrawSprites = sprites.Count;
                }

                if (_lastInteractive == interactive) return;
                
                _lastInteractive = interactive;

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
                        Size = new Vector2(6, 9),
                        Color = Color.Lighten(Style.FirstColor, 0.8), //hsv.HSVtoColor(),
                        RotationOrScale = -(float) Math.PI / 3f
                    }
                );
            }

            public void Tick()
            {
                _input?.Tick();
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
                    var clickDetect = 0;
                    _input = new Input(controller, ConsoleId)
                    {
                        OnSelect = args =>
                        {
                            if (Math.Abs(args.Power) > 0.7)
                            {
                                clickDetect++;
                                if (clickDetect != 1) return;

                                if (_lastInteractive == null)
                                {
                                    if (args.Power < -0.7) CloseMessageBox();
                                }
                                else
                                {
                                    if (args.Power < -0.7) _lastInteractive?.OnEsc(this);
                                    if (args.Power > 0.7) _lastInteractive?.OnSelect(this);
                                }
                            }
                            else
                                clickDetect = 0;
                        },
                        OnInput = args =>
                        {
                            _lastInteractive?.OnInput(this, args.Dir);
                        }
                    };
                }
            }
        }

        struct SelectEventArgs
        {
            public double Power;
        }
        struct InputMoveEventArgs
        {
            public Vector3 Dir;
        }

        class Input : IDisposable
        {
            public IMyCockpit Cockpit;
            public bool IsEnableControl;
            public Action<SelectEventArgs> OnSelect;
            public Action<InputMoveEventArgs> OnInput;
            string _consoleId;

            public Input(IMyCockpit cockpit, string consoleId)
            {
                Cockpit = cockpit;
                _consoleId = consoleId;
            }

            void SwitchControl()
            {
                IsEnableControl = !IsEnableControl;
            }

            void InputMove(Vector3 dir)
            {
                OnInput?.Invoke(new InputMoveEventArgs
                {
                    Dir = dir
                });
                _arrowPos += 10 * new Vector2(dir.X, dir.Z);
            }
            void Select(double val = 1)
            {
                OnSelect?.Invoke(new SelectEventArgs
                {
                    Power = val
                });
            }

            void Deselect(double val = -1)
            {
                Select(val);
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
                
                var ri = Cockpit.RotationIndicator;
                var delta = new Vector2(ri.Y, ri.X);
                _arrowPos += delta * ConsolePluginSetup.MOUSE_SENSITIVITY;

                Select(Cockpit.RollIndicator);
                InputMove(Cockpit.MoveIndicator);
            }

            public void Message(string argument)
            {
                var prefix = _consoleId;
                if (argument == prefix + ConsolePluginSetup.SURFACE_CONTROLLER_SWITCH_CTRL_MARK)
                {
                    SwitchControl();
                }
                else if (argument == prefix + ConsolePluginSetup.SURFACE_CONTROLLER_UP_CTRL_MARK)
                {
                    InputMove(new Vector3(0, 1, 0));
                }
                else if (argument == prefix + ConsolePluginSetup.SURFACE_CONTROLLER_DOWN_CTRL_MARK)
                {
                    InputMove(new Vector3(0, -1, 0));
                }
                else if (argument == prefix + ConsolePluginSetup.SURFACE_CONTROLLER_RIGHT_CTRL_MARK)
                {
                    InputMove(new Vector3(1, 0, 0));
                }
                else if (argument == prefix + ConsolePluginSetup.SURFACE_CONTROLLER_LEFT_CTRL_MARK)
                {
                    InputMove(new Vector3(-1, 0, 0));
                }
                else if (argument == prefix + ConsolePluginSetup.SURFACE_CONTROLLER_SELECT_CTRL_MARK)
                {
                    Select();
                }
                else if (argument == prefix + ConsolePluginSetup.SURFACE_CONTROLLER_DESELECT_CTRL_MARK)
                {
                    Deselect();
                }
            }

            public void Dispose()
            {
            }
        }

        public Surface(IEnumerable<IPageProvider> pages)
        {
            _pages = pages.Select(x=> x.Page);
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
                DateTime.Now + TimeSpan.FromSeconds(ConsolePluginSetup.SURFACES_LIST_UPDATE_PERIOD_SEC);

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
                    var startPage = string.IsNullOrEmpty(lcdResult.StartPageNameId)
                        ? _pages.First()
                        : GetPage(lcdResult.StartPageNameId);
                    _consoles.Add(id,
                        new Drawer(this, surface, lcdResult.SurfaceNameId, startPage));
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
        
        IEnumerator<float> FindControllers(IEnumerable<IMyTerminalBlock> blocks)
        {
            var myTerminalBlocks = blocks.ToArray();

            for (var i = 0; i < myTerminalBlocks.Length; i++)
            {
                var block = myTerminalBlocks[i];
                var cockpit = block as IMyCockpit;
                if (cockpit == null)
                    throw new Exception($"Block name of {block.CustomName} is not cockpit");
                var ctrlResult = ConsoleNameParser.FindSubstring(ConsolePluginSetup.SURFACE_CONTROLLER_MARK, block.CustomName);

                if (string.IsNullOrEmpty(ctrlResult)) continue;
                var id = _consoles.GetKeyFor(x => x.ConsoleId == ctrlResult, -1);

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
            return _pages.FirstOrDefault(x => x.Id == id);
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