using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    class ConsoleManager
    {
        IEnumerable<Page> _pages;
        Dictionary<long, IMyCockpit> _inputs = new Dictionary<long, IMyCockpit>();
        Dictionary<long, Surface> _surfaces = new Dictionary<long, Surface>();
        IEnumerator _updateBlocksProcess, _drawProcess;

        public int LastDrawnSprites => _surfaces.Values.Sum(x => x.SpritesCount);
        public int DrawerCount => _surfaces.Values.Count;

        public ConsoleManager(IEnumerable<IPageProvider> pages)
        {
            _pages = pages.Select(x=> x.Page);
        }

        public void Tick(IEnumerable<IMyTerminalBlock> blocks)
        {
            if (_updateBlocksProcess == null || !_updateBlocksProcess.MoveNext())
                _updateBlocksProcess = UpdateBlocksProcess(blocks);
            if (_drawProcess == null || !_drawProcess.MoveNext())
                _drawProcess = DrawProcess();

            foreach (var surface in _surfaces.Values)
            {
                surface.Tick();
            }
        }

        public void Message(string msg)
        {
            foreach (var drawer in _surfaces.Values)
            {
                drawer.Message(msg);
            }
        }

        DateTime _nextTimeForBlocksUpdate = DateTime.MinValue;

        IEnumerator UpdateBlocksProcess(IEnumerable<IMyTerminalBlock> blocks)
        {
            while (_nextTimeForBlocksUpdate > DateTime.Now)
            {
                ConsolePlugin.Logger.Log($"AWAIT ... {(_nextTimeForBlocksUpdate - DateTime.Now).TotalSeconds}");
                yield return null;
            }

            _nextTimeForBlocksUpdate =
                DateTime.Now + TimeSpan.FromSeconds(ConsolePluginSetup.SURFACES_LIST_UPDATE_PERIOD_SEC);

            var myTerminalBlocks = blocks as IMyTerminalBlock[] ?? blocks.ToArray();
            var blocksSurface =
                myTerminalBlocks.Where(block => block.CustomName.Contains(ConsolePluginSetup.SURFACE_MARK));
            var blocksSurfaceCtrl = myTerminalBlocks.Where(block =>
                block.CustomName.Contains(ConsolePluginSetup.SURFACE_CONTROLLER_MARK));

            var step = FindSurfaces(blocksSurface);
            while (step.MoveNext())
            {
                ConsolePlugin.Logger.Log($"FIND SURFACE. {(float)MathHelper.Lerp(1, 0.5, step.Current):P}");
                yield return null;
            }
            
            step = FindControllers(blocksSurfaceCtrl);
            while (step.MoveNext())
            {
                ConsolePlugin.Logger.Log($"FIND CONTROLLERS. {(float) MathHelper.Lerp(0.5, 1, step.Current):P}");
                yield return null;
            }



            foreach (var id in _surfaces.Keys)
            {
                var surface = _surfaces[id];
                
                if (_inputs.ContainsKey(id))
                    surface.ApplyInput(_inputs[id]);
                else
                    surface.RemoveInput();

            }

            yield break;
        }

        IEnumerator<float> FindSurfaces(IEnumerable<IMyTerminalBlock> blocks)
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
                if (!_surfaces.ContainsKey(id))
                {
                    var startPage = _pages.FirstOrDefault(x => x.Id == lcdResult.StartPageNameId);
                    var s = new Surface(_pages, surface, lcdResult.SurfaceNameId, startPage);
                    _surfaces.Add(id, s);
                }

                yield return (float) i / myTerminalBlocks.Length;
            }

            // remove unused
            foreach (var id in _surfaces.Keys)
            {
                if (ids.Contains(id)) continue;

                _surfaces.Remove(id);
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
                var id = _surfaces
                    .Where(x => x.Value.Id == ctrlResult)
                    .Select(x=> x.Key)
                    .ToList()
                    ;

                if (!id.Any() || _inputs.ContainsKey(id.First())) continue;

                _inputs.Add(id.First(), cockpit);

                yield return (float) i / myTerminalBlocks.Length;
            }
        }

        int _redrawAwait = 10;

        IEnumerator DrawProcess()
        {
            while (_redrawAwait > 0)
            {
                yield return null;
                //_os.Program.Echo.Invoke($"Await draw: {ticks}");
                _redrawAwait--;
            }

            _redrawAwait = 10;

            foreach (var surface in _surfaces.Values)
            {
                surface.Draw();
                //_status = _status ?? $"Draw [{i + 1}/{_consoles.Values.Length}]";
                yield return null;
                _redrawAwait--;
            }

            //_status = "AWAIT";
            yield break;
        }

        public void SwitchPage(string to, string onConsoleId)
        {
            var target = _surfaces.Values.FirstOrDefault(x => x.Id == onConsoleId);
            if (target == null)
            {
                foreach (var console in _surfaces.Values)
                {
                    console.SwitchPage(to);
                }
            }
        }

        public void ShowMsg(string msg, string onConsoleId)
        {
            var target = _surfaces.Values.FirstOrDefault(x => x.Id == onConsoleId);
            if (target == null)
            {
                foreach (var console in _surfaces.Values)
                {
                    console.ShowMessageBox(msg);
                }
            }
        }
    }
}