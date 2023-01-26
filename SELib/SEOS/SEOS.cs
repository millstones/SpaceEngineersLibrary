using System;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRageMath;

namespace IngameScript
{
    class SEOS
    {
        public List<Plugin> Plugins = new List<Plugin>();
        public List<Module> Modules = new List<Module>();

        GridInfo _gridInfo;
        public MyGridProgram Program;
        public ILogger Logger { get; private set; }
        Func<List<IMyTerminalBlock>> BlockFinder;
        IEnumerable<IMyTerminalBlock> Blocks => BlockFinder();
        IMessageBroker MessageBroker;
        IStorage Storage;

        public SEOS(MyGridProgram program, string gridGroup = "", string gridName="")
        {
            _gridInfo = new GridInfo
            {
                Id = program.Me.EntityId,
                Name = gridName == "" ? program.Me.CubeGrid.CustomName : gridName,
                GroupName = gridGroup,
                CustomDate = "",
                IsStatic = program.Me.CubeGrid.IsStatic,
                Position = program.Me.CubeGrid.WorldMatrix.Translation
            };
            
            program.Me.CubeGrid.CustomName = _gridInfo.Name;
            Program = program;
            BlockFinder = () =>
            {
                var retVal = new List<IMyTerminalBlock>();
                program.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(retVal, block => block.IsSameConstructAs(program.Me));

                return retVal;
            };

            program.Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        void InitPlugins()
        {
            foreach (var item in Plugins)
            {
                item.Init(this);
            }
        }

        void InitModules()
        {
            foreach (var item in Modules)
            {
                item.Logger = Logger;
                item.Storage = Storage;
                item.MessageBroker = MessageBroker;
                item.GridInfo = _gridInfo;
                item.Awake(Blocks.Where(item.BlockFilter));
            }

            foreach (var item in Modules)
            {
                item.Start();
            }
        }
        void TickPlugins(double dt)
        {
            foreach (var item in Plugins)
            {
                item.Tick(dt);
            }
        }

        void TickModules(double dt)
        {
            _gridInfo.Position = Program.Me.CubeGrid.WorldMatrix.Translation;
            foreach (var item in Modules)
            {
                item.GridInfo = _gridInfo;
                item.Tick(dt, Blocks.Where(item.BlockFilter));
                item.Tick(dt);
            }
        }

        //RadioService _radio;
        public SEOS Build(ILogger logger)
        {
            Logger = logger;
            MessageBroker = new MessageBroker(Program.Me.EntityId, Logger);
            Storage = new PBCustomDataStorage(Program.Me);

            var antennas = Blocks.OfType<IMyRadioAntenna>().ToList();
            if (antennas.Any())
            {
                var antenna = antennas.First();
                antenna.EnableBroadcasting = true;
                var mb = new MessageBroker(this, new[] {_gridInfo.GroupName}, antenna)
                    {Serializers = _serializers};
                MessageBroker = mb;
            }

            try
            {
                InitPlugins();
                InitModules();
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Program.Runtime.UpdateFrequency = UpdateFrequency.None;
            }

            return this;
        }

        static DateTime lastTick;

        public void Tick()
        {
            PrintInfo();
            
            var dateTime = DateTime.Now;
            var dt = (dateTime - lastTick).TotalSeconds;
            try
            {
                TickPlugins(dt);
                TickModules(dt);
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Program.Runtime.UpdateFrequency = UpdateFrequency.None;
            }
            lastTick = dateTime;
        }

        Queue<double> _cpLoad = new Queue<double>();
        Queue<double> _ramLoad = new Queue<double>();
        double _cpMax, _ramMax;
        int i;
        void PrintInfo()
        {
            const int awaitTicks = 100;
            if (i < awaitTicks)
            {
                i++;
                return;
            }
            var rt = Program.Runtime;
            var cp = 0.01 * rt.LastRunTimeMs / ((float) 1 / 48);
            var ram = (double) rt.CurrentInstructionCount / rt.MaxInstructionCount;
            cp = double.IsInfinity(cp) || double.IsNaN(cp) ? 0 : cp;
            ram = double.IsInfinity(ram) || double.IsNaN(ram) ? 0 : ram;
            _cpLoad.Enqueue(cp);
            _ramLoad.Enqueue(ram);

            if (_cpLoad.Count <= awaitTicks) return;
            
            var cpAvg = _cpLoad.Average();
            var ramAvg = _ramLoad.Average();
            if (i < 5 * awaitTicks) i++;
            else
            {
                _cpMax = Math.Max(cpAvg, _cpMax);
                _ramMax = Math.Max(ramAvg, _ramMax);
            }
                
            _cpLoad.Dequeue();
            _ramLoad.Dequeue();
                
            var sb = new StringBuilder();

            sb.AppendLine($"GUZUN OS <Plugins: {Plugins.Count} Modules: {Modules.Count}>");
            sb.AppendLine($"PRC load: AWG:{cpAvg:P0} MAX:{_cpMax:P0}");
            sb.AppendLine($"RAM load: AWG:{ramAvg:P0} MAX:{_ramMax:P0}");
            Program.Echo(sb.ToString());
        }

        public void Message(string argument, UpdateType updateSource)
        {
            try
            {
                foreach (var item in Plugins)
                {
                    item.Message(argument, updateSource);
                }

                (MessageBroker as MessageBroker).Tick(argument);
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Program.Runtime.UpdateFrequency = UpdateFrequency.None;
            }
        }

        public void Save()
        {
            Storage.Save();
        }

        public SEOS AddPlugin<T>() where T : Plugin, new()
        {
            AddPlugin(new T());

            return this;
        }

        public SEOS AddPlugin<T>(T plugin) where T : Plugin
        {
            Plugins.Add(plugin);

            return this;
        }

        public SEOS AddModule<T>(UpdateFrequency frequency) where T : Module, new()
        {
            Modules.Add(new T());

            return this;
        }

        public SEOS UseBlockFinderDelegate(Func<List<IMyTerminalBlock>> finder)
        {
            BlockFinder = finder;

            return this;
        }
            
        Dictionary<string, object> _serializers = new Dictionary<string, object>();
        public SEOS UseSerializer<T>() where T : ISerializer, new()
        {
            _serializers.Add(typeof(T).Name, new T());
            return this;
        }
        public SEOS RenameMyBlocks()
        {
            var bloks = new List<IMyTerminalBlock>();
            if (BlockFinder != null) 
                bloks = BlockFinder();
            else 
                Program.GridTerminalSystem.GetBlocks(bloks);
            
            foreach (var block in bloks)
            {
                var substr = block.CustomName.Split('\'');
                for (int i = 0; i < substr.Length-1; i++)
                {
                    var s = substr[i];
                    if (string.IsNullOrEmpty(s)) continue;
                    block.CustomName = block.CustomName.Replace(s, "");
                }
                var add = $"'{_gridInfo.Name}' :";
                block.CustomName = block.CustomName.Replace("'", "");
                block.CustomName = block.CustomName.Replace(" :", "");
                block.CustomName = $"{add}{block.CustomName}";
            }
            
            return this;
        }
    }

    public enum NoteLevel
    {
        None = 0,
        Info = 1,
        Waring = 2,
        Error = 3
    }

    public interface ILogger
    {
        DebugAPI Debug { get; }
        void Log(string msg);
        void Log(NoteLevel msgType, string msg);
        void Log(Exception e);
    }

    public class DefaultLogger : ILogger
    {
        const int MAX_LINES = 100;
        IMyTerminalBlock _logStorage;
        Action<string> _echo;
        public DebugAPI Debug { get; }

        int _counter;
        public DefaultLogger(MyGridProgram prg)
        {
            _logStorage = prg.Me;
            _echo = prg.Echo;

            Debug = new DebugAPI(prg);
            Debug.RemoveAll();
            Debug.PrintChat("Debug is enable", senderColor: Color.Pink);
            
            Clear();
        }
        void Clear()
        {
            if (_logStorage != null)
                _logStorage.CustomData = $"Created '{DateTime.Now}'" + "\n";;
        }

        public void Log(string msg)
        {
            Log(NoteLevel.Info, msg);
        }

        public void Log(NoteLevel msgType, string msg)
        {
            if (msgType == NoteLevel.Info)
            {
                _echo(msg);
                return;
            }
            if (_logStorage == null) return;
            
            if (_counter > MAX_LINES)
            {
                var lines = _logStorage.CustomData.Split('\n').ToList();
                lines.RemoveAt(1);
                var newLog = lines.Aggregate("", (current, line) => current + (line + '\n'));
                _logStorage.CustomData = newLog;
            }
            
            _logStorage.CustomData += $"[{++_counter}] " + msg + "\n";
            PrintChat(msgType, msg);
        }
        
        void PrintChat(NoteLevel msgType, string msg)
        {
            var color = msgType == NoteLevel.Error
                ? Color.Red
                : msgType == NoteLevel.Waring
                    ? Color.Yellow
                    : Color.White;

            Debug?.PrintChat(msg, senderColor: color );
        }

        public void Log(Exception e)
        {
            Log(NoteLevel.Error, e.ToString());
        }
    }
}
