﻿using System;
using System.Collections;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    class ConsolePlugin : Plugin
    {
        public new static ILogger Logger => INSTANCE._os.Logger;
        static ConsolePlugin INSTANCE;
        
        ConsoleManager _consoleManager;
        List<IPageProvider> _pages = new List<IPageProvider>();

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
            //_os.AddModule<EnergyManager>(UpdateFrequency.Update10);
            //_os.AddModule<CargoManager>(UpdateFrequency.Update10);
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
                var page = module as IPageProvider;
                if (page != null)
                    RegisterPage(page);

                yield return null;
                _os.Program.Echo.Invoke($"Console plugin init ...");
            }
            
            // Parse Custom dates
            // ...
            //

            _consoleManager = new ConsoleManager(_pages);

            // Update
            while (!_isPause)
            {
                var blocks = new List<IMyTerminalBlock>();
                _os.Program.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks,
                    block => block.CustomName.Contains(ConsolePluginSetup.SURFACE_MARK));

                _consoleManager.Tick(blocks);
                _os.Program.Echo.Invoke($"Console plugin. Drawn {_consoleManager.LastDrawnSprites} sprites on {_consoleManager.DrawerCount} surfaces");
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

        public static void RegisterPage(IPageProvider pageProvider)
        {
            ThrowIfNotCreate();
            INSTANCE._pages.Add(pageProvider);
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