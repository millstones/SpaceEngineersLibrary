using System;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    struct ArrowClickEventArgs
    {
        public Console Console;
    }
class ConsoleInput : IDisposable
    {
        public IMyCockpit Cockpit;
        public bool IsEnableControl;
        public Action<ArrowClickEventArgs> OnClick;
        Console _forConsole;

        public ConsoleInput(IMyCockpit cockpit, Console forConsole)
        {
            Cockpit = cockpit;
            _forConsole = forConsole;
        }

        void SwitchControl()
        {
            IsEnableControl = !IsEnableControl;
        }

        void UpStep()
        {
            _arrowPos+=Vector2.UnitY*10;
        }

        void DownStep()
        {
            _arrowPos-=Vector2.UnitY*10;
        }

        void SwitchSelect()
        {
            
        }

        void Select()
        {
            OnClick?.Invoke(new ArrowClickEventArgs
            {
                Console = _forConsole
            });
        }

        void Deselect()
        {
            
        }

        Vector2 _arrowPos;

        public Vector2 ArrowPos()
        {
            var viewport = _forConsole.Viewport;
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
            var prefix = _forConsole.ConsoleId;
            if (argument == prefix + ConsolePluginSetup.SURFACE_CONTROLLER_SWITCH_CTRL_MARK)
            {
                SwitchControl();
            }
            else
            if (argument == prefix + ConsolePluginSetup.SURFACE_CONTROLLER_UP_CTRL_MARK)
            {
                UpStep();
            }
            else
            if (argument == prefix + ConsolePluginSetup.SURFACE_CONTROLLER_DOWN_CTRL_MARK)
            {
                DownStep();
            }
            else
            if (argument == prefix + ConsolePluginSetup.SURFACE_CONTROLLER_SWITCH_SELECT_CTRL_MARK)
            {
                SwitchSelect();
            }
        }

        public void Dispose()
        {
        }
    }
}