using System;
using System.Collections;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
class ConsolePlugin : Plugin
    {
        static ConsolePlugin INSTANCE;
        
        ConsoleManager _consoleManager;
        SysLayoutPage _sysPage;
        List<ISEWPFContent> _pages = new List<ISEWPFContent>();

        SEOS _os;
        IEnumerator _updateProcess;
        bool _isPause = false;
        public override void Init(SEOS os)
        {
            base.Init(os);

            INSTANCE = this;
            INSTANCE._os = os;
            INSTANCE._updateProcess = UpdateProcess();
            INSTANCE.AddSubmodules();
        }

        void AddSubmodules()
        {
            _os.AddModule<CargoManager>(UpdateFrequency.Update100);
        }

        public override void Message(string argument, UpdateType updateSource)
        {
            _consoleManager?.Message(argument);
        }
        
        public override void Tick(double dt)
        {
            _updateProcess?.MoveNext();
        }

        IEnumerator UpdateProcess()
        {
            // Init
            foreach (var module in _os.Modules)
            {
                var page = module as ISEWPFContent;
                if (page != null)
                    RegisterPage(page);

                yield return null;
                _os.Program.Echo.Invoke($"Console plugin init ...");
            }
            
            // Parse Custom dates
            // ...
            //
            
            _consoleManager = new ConsoleManager(_pages, _sysPage ?? new DefaultSysLayoutPage());

            var tick = 0;
            // Update
            while (!_isPause)
            {
                var blocks = new List<IMyTerminalBlock>();
                _os.Program.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks,
                    block => block.CustomName.Contains(ConsolePluginSetup.SURFACE_MARK));

                _consoleManager.Tick(blocks);
                _os.Program.Echo.Invoke($"Console plugin tick {tick}");
                tick++;
                yield return null;
            }
        }

        public override void Save()
        {
        }
        
        static void ThrowIfNotCreate()
        {
            if (INSTANCE == null)
                throw new Exception("Console plugin not create");
        }

        public static void RegisterPage(ISEWPFContent page)
        {
            ThrowIfNotCreate();
            INSTANCE._pages.Add(page);
        }

        public static void UseCanvas(SysLayoutPage sysPage)
        {
            ThrowIfNotCreate();
            INSTANCE._sysPage = sysPage;
        }
        
        public static void SwitchPage(string to, string onConsoleId = "")
        {
            ThrowIfNotCreate();
            INSTANCE._consoleManager.SwitchPage(to, onConsoleId);
        }
        public static void ShowMsg(string msg, string onConsoleId = "")
        {
            ThrowIfNotCreate();
            INSTANCE._consoleManager.ShowMsg(msg, onConsoleId);
        }
    }
}