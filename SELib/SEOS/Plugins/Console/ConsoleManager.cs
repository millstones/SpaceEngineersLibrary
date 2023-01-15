using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    abstract class ConsoleManager
    {
        IEnumerable<IUserContent> _content;
        Repository<long, IMyCockpit> _inputs = new Repository<long, IMyCockpit>();
        Repository<long, Console> _consoles = new Repository<long, Console>();


        KeyValuePair<RectangleF, ContentPanel> _sysArea;
        KeyValuePair<RectangleF, Content> _userArea;
        KeyValuePair<RectangleF, MsgBox> _msgBoxArea;
        
        protected ConsoleStyle Style = ConsoleStyle.MischieviousGreen;

        string _status;
        protected ConsoleManager(IEnumerable<IUserContent> content)
        {
            _content = content;
            
            var sysArea = CreateArea(Vector2.Zero, new Vector2(1, 0.15f));
            var userArea = CreateArea(new Vector2(0, 0.15f), Vector2.One);
            var msgBoxArea = CreateArea(new Vector2(0.25f, 0.25f), new Vector2(0.75f, 0.75f));

            _sysArea = new KeyValuePair<RectangleF, ContentPanel>(sysArea, new CanvasSysPanel(this));
            _userArea = new KeyValuePair<RectangleF, Content>(userArea, new ContentText(new ReactiveProperty<string>("CREATE CONTENT !!")));
            _msgBoxArea = new KeyValuePair<RectangleF, MsgBox>(msgBoxArea, new MsgBox());

            _status = "INIT";
        }

        protected void UseDefaultUserContent(Content content) => _userArea = new KeyValuePair<RectangleF, Content>(_userArea.Key, content);
        
        IEnumerator _updateBlocksProcess, _drawProcess;
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

                var id =  block.GetId();
                //var id = string.IsNullOrEmpty(lcdResult.LcdNameId)? block.GetId().ToString() : lcdResult.LcdNameId;
                ids.Add(id);
                if (!_consoles.Contains(id))
                {
                    _consoles.Add(id, new Console(_content, surface, lcdResult.LcdNameId, lcdResult.SiteNameId, Style, _userArea, _sysArea, _msgBoxArea));
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

        DateTime _nextBlocksUpdate = DateTime.MinValue;
        IEnumerator UpdateBlocksProcess(IEnumerable<IMyTerminalBlock> blocks)
        {
            while (_nextBlocksUpdate > DateTime.Now)
            {
                yield return null;
                //_os.Program.Echo.Invoke($"Await update: {(nextUpdate - DateTime.Now).TotalSeconds:#.#}");
            }
            _nextBlocksUpdate = DateTime.Now + TimeSpan.FromSeconds(ConsolePluginSetup.UPDATE_SURFACES_LIST_PERIOD_SEC);
            
            var myTerminalBlocks = blocks as IMyTerminalBlock[] ?? blocks.ToArray();
            var blocksSurface = myTerminalBlocks.Where(block => block.CustomName.Contains(ConsolePluginSetup.SURFACE_MARK));
            var blocksSurfaceCtrl = myTerminalBlocks.Where(block => block.CustomName.Contains(ConsolePluginSetup.SURFACE_CONTROLLER_MARK));

            var step = FindControllers(blocksSurfaceCtrl);
            while (step.MoveNext())
            {
                _status = $"FIND CONTROLLERS. {(float) MathHelper.Lerp(0, 0.5, step.Current):P}";

                yield return null;
            }

            step = FindCanvases(blocksSurface);
            while (step.MoveNext())
            {
                _status = $"FIND SURFACE. {(float)MathHelper.Lerp(0.5, 1, step.Current):P}";
                yield return null;
            }

            _status = "";


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

        int _tickAwait = 10;
        Console _consolePointer;
        IEnumerator DrawProcess()
        {
            while (_tickAwait > 0)
            {
                yield return null;
                //_os.Program.Echo.Invoke($"Await draw: {ticks}");
                _tickAwait--;
            }
            _tickAwait = 10;

            for (var i = 0; i < _consoles.Ids.Length; i++)
            {
                _consolePointer = _consoles.Values[i];
                _consolePointer.Draw();
                _status = _status ?? $"Draw [{i + 1}/{_consoles.Values.Length}]";
                yield return null;
                _tickAwait--;
            }

            _status = "AWAIT";
            _consolePointer = null;
            yield break;
        }
        
        protected static RectangleF CreateArea(Vector2 leftUpPoint, Vector2 rightDownPoint)
        {
            rightDownPoint = Vector2.Clamp(rightDownPoint, Vector2.Zero, Vector2.One);
            leftUpPoint = Vector2.Clamp(leftUpPoint, Vector2.Zero, rightDownPoint);
            rightDownPoint = Vector2.Clamp(rightDownPoint, leftUpPoint, Vector2.One);

            return new RectangleF(leftUpPoint, rightDownPoint - leftUpPoint);
        }
        
        
        public void SwitchPage(string to, string onConsoleId = "")
        {
            if (string.IsNullOrEmpty(onConsoleId))
            {
                _consolePointer?.SwitchPage(to);
            }
            else
            {
                var id = _consoles.GetKeyFor(x => x.ConsoleId == onConsoleId, -1);
                if (id != -1) 
                    _consoles.GetOrDefault(id)?.SwitchPage(to);
            }
        }

        public void ShowMsg(string msg, string onConsoleId = "")
        {
            if (string.IsNullOrEmpty(onConsoleId))
            {
                foreach (var console in _consoles.Values)
                {
                    console.ShowMsgBox(msg);
                }
            }
            else
            {
                var id = _consoles.GetKeyFor(x => x.ConsoleId == onConsoleId, -1);
                if (id != -1) 
                    _consoles.GetOrDefault(id)?.ShowMsgBox(msg);
            }
        }

        class CanvasSysPanel : ContentPanel
        {
            public CanvasSysPanel(ConsoleManager consoleManager) : base(vertical: true)
            {
                Add(new ContentText(new ReactiveProperty<string>(() => consoleManager._consoles.Ids.Length.ToString()), alignment: TextAlignment.CENTER));
                Add(new ContentText(new ReactiveProperty<string>(() => consoleManager._status), alignment: TextAlignment.CENTER));
                Add(new ContentText(new ReactiveProperty<string>(() => consoleManager._inputs.Ids.Length.ToString()), alignment: TextAlignment.CENTER));
            }
        }
    }
}