using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    class Surface : ISurface
    {
        public readonly string Id;
        IEnumerable<Page> _pages;
        
        Input _input;
        Display _display;
        
        IInteractive _lastInteractive;

        SysPage _sysPage;
        public int SpritesCount { get; private set; }

        public Surface(IEnumerable<Page> pages, IMyTextSurface surface, string id, Page startPage)
        {
            Id = id;
            _pages = pages;
            _display = new Display(surface, ConsoleStyle.MischieviousGreen);
            _sysPage = new DefaultSysPage(pages, startPage);
        }
        
        public void Tick()
        {
            _input?.Tick();
        }

        public void Message(string msg)
        {
            _input?.Message(msg);
        }

        public void SwitchPage(string id)
        {
            var page = _pages.FirstOrDefault(x => x.Id == id);
            if (page == null) 
                ShowMessageBox(new MsgBox(NoteLevel.Error, $"Page '{id}' NOT FOUND"), null, 3);
            else SwitchPage(page);
        }

        public void SwitchPage(Page page)
        {
            _sysPage.Switch(page);
        }

        public void SwitchPage<T>()
        {
            SwitchPage(typeof(T).Name);
        }

        public void ShowMessageBox(string msg, RectangleF? viewport = null, int closeSec = int.MaxValue)
        {
            ShowMessageBox(new MsgBox(NoteLevel.Info, msg), viewport, closeSec);
        }

        public void ShowMessageBox(FlexItem msg, RectangleF? viewport = null, int closeSec = int.MaxValue)
        {
            _sysPage.ShowMessageBox(msg, viewport, closeSec);
        }

        public void CloseMessageBox()
        {
            _sysPage.CloseMessageBox();
        }

        public void RemoveInput() => _input = null;

        public void ApplyInput(IMyCockpit controller)
        {
            if (controller == null) return;
            if (_input == null || _input.Cockpit != controller)
            {
                var clickDetect = 0;
                _input = new Input(controller, Id, _display.Viewport)
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

        public void Draw()
        {
            Vector2? arrowPos = null;

            if (_input != null && _input.IsEnableControl)
            {
                arrowPos = _input.ArrowPos;
            }

            _lastInteractive = _display.Draw(_sysPage, arrowPos);
            SpritesCount = _display.LastDrawnSpritesCount;
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
        RectangleF _viewport;

        public Input(IMyCockpit cockpit, string consoleId, RectangleF viewport)
        {
            Cockpit = cockpit;
            _consoleId = consoleId;
            _viewport = viewport;
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
            ArrowPos += 10 * new Vector2(dir.X, dir.Z);
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
        
        public Vector2 ArrowPos;

        public void Tick()
        {
            if (ConsolePluginSetup.ENABLE_AUTO_SWITCH_CONTROL_LCD)
            {
                IsEnableControl = Cockpit.IsUnderControl;
            }

            if (!IsEnableControl) return;

            var ri = Cockpit.RotationIndicator;
            var delta = new Vector2(ri.Y, ri.X);
            ArrowPos += delta * ConsolePluginSetup.MOUSE_SENSITIVITY;
            ArrowPos = Vector2.Clamp(ArrowPos, _viewport.Position, _viewport.Position + _viewport.Size);
            
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
}