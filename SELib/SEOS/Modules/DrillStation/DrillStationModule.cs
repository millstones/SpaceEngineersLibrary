using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript
{
    /// <summary>
    /// Скрипт управления добычей.
    /// Совершает последовательное бурение породы, проходя снячало вдоль оси X, далее смещается по оси Y на ширину буровой головки, возвращается по оси X, и т.д. до максимально возможного по оси Y.
    /// После чего заглубляется в породу нв ша 'Z_DEPTH_STEP' и продулывает все движения по X и Y в обратном направлении.
    /// Бурение происходит до максимального заглубления по оси Z.
    /// Пауза в бурении - при наполнении контейнера с маркером '[CARGO]' до 100%. Продолжает бурить когда '[CARGO]' снизиться до 80%
    /// На текстовой панеле прог. блока отображается некоторая информация о процессе.
    /// 
    /// 1. Установить буровую головку (буры) на 3х осях.
    /// 2. Указать размер буровой головки в осях X и Y (Параметр '_toolSize'. По умолчанию 6х6 м)
    /// 3. Указать шаг прохода в породу (Параметр 'Z_DEPTH_STEP'. По умолчанию 1 м)
    /// 4. Поршни в имени должны содержать маркеры '[X]' '[Y]' '[Z]' в соответствии с осями
    /// 5. Прог. блок должени иметь маркер '[DRILL CONTROLLER]'.
    ///
    /// Комманды:
    /// 1. 'Start' - запускает бурение с текуще точки
    /// 1. 'StartAs [x] [y] [z]' - запускает бурение с точки x, y, z
    /// 2. 'Stop' - останавленвает бурение
    /// 3. 'To [x] [y] [z]' - перемещает буровую головку в точку x, y, z
    /// </summary>
    class DrillStationModule : Module
    {
        Vector2 _toolSize = new Vector2(6);
        const float Z_DEPTH_STEP = 1;

        CNCPiston X, Y, Z;
        List<IMyShipDrill> _drills;
        List<IMyCargoContainer> _cargo;

        IMyTextSurface _info;
        public override void Awake(IEnumerable<IMyTerminalBlock> blocks)
        {
            var myTerminalBlocks = blocks as IMyTerminalBlock[] ?? blocks.ToArray();
            _drills = myTerminalBlocks.OfType<IMyShipDrill>().ToList();
            _cargo = myTerminalBlocks.OfType<IMyCargoContainer>().Where(x => x.CustomName.Contains("[CARGO]")).ToList();
            _info = myTerminalBlocks.OfType<IMyProgrammableBlock>().First(x => x.CustomName.Contains("[DRILL CONTROLLER]")).GetSurface(0);

            _info.WriteText("Init CNCDrill");
            
            var xPistons = myTerminalBlocks.OfType<IMyPistonBase>().Where(x => x.CustomName.Contains("[X]"));
            var yPistons = myTerminalBlocks.OfType<IMyPistonBase>().Where(x => x.CustomName.Contains("[Y]"));
            var zPistons = myTerminalBlocks.OfType<IMyPistonBase>().Where(x => x.CustomName.Contains("[Z]"));

            Logger.Log(NoteLevel.Waring, $"{xPistons.Count()} {yPistons.Count()} {zPistons.Count()}");
            
            X = new CNCPiston(new SEPiston(xPistons, new LimitsF() {Min = 1, Max = 5}));
            
            Y = new CNCPiston(new SEPiston(yPistons, new LimitsF() {Min = 1, Max = 5}));
            
            Z = new CNCPiston(new SEPiston(zPistons, new LimitsF() {Min = 0.5f, Max = 1}));

            MessageBroker.Post("Start", StartDrilling);
            MessageBroker.Post<string>("StartAs", s => StartDrilling(s));
            MessageBroker.Post<string>("To", To);
            MessageBroker.Post("Stop", StopDrilling);

            Task.EveryTick(() =>
            {
                var xState = X.IsWork ? "WORK" : "WAIT";
                var x = $"X: {X.PV:00.00} -> {X.SP:00.00} || STATE: {xState}";
                
                var yState = Y.IsWork ? "WORK" : "WAIT";
                var y = $"Y: {Y.PV:00.00} -> {Y.SP:00.00} || STATE: {yState}";
                
                var zState = Z.IsWork ? "WORK" : "WAIT";
                var z = $"Z: {Z.PV:00.00} -> {Z.SP:00.00} || STATE: {zState}";

                _info.WriteText(new StringBuilder()
                    .AppendLine(x)
                    .AppendLine(y)
                    .AppendLine(z)
                    .AppendLine($"Cargo: {_cargo.InventoryPercent():P}")
                );
            })
                .Run();
        }

        MyIniKey _xStepKey = new MyIniKey("drill-station", "x-pos");
        MyIniKey _yStepKey = new MyIniKey("drill-station", "y-pos");
        MyIniKey _zStepKey = new MyIniKey("drill-station", "z-pos");
        MyIniKey _moveDir = new MyIniKey("drill-station", "is-fwd");

       
        bool _isFwdDir;
        public override void Start()
        {
            var x = 0f;
            var y = 0f;
            var z = 0f;
            Storage.IniAndSave(_xStepKey, new ObjectReactiveProperty(() => X.PV, o => x = (float)o));
            Storage.IniAndSave(_yStepKey, new ObjectReactiveProperty(() => Y.PV, o => y = (float)o));
            Storage.IniAndSave(_zStepKey, new ObjectReactiveProperty(() => Z.PV, o => z = (float)o));
            Storage.IniAndSave(_moveDir, new ObjectReactiveProperty(() => _isFwdDir, o => _isFwdDir = (bool)o));
            
            StartDrilling(new Vector3(x, y, z));
        }

        void StartDrilling()
        {
            StartDrilling(new Vector3(X.PV, Y.PV, Z.PV));
        }
        void StartDrilling(string args)
        {
            var argsArr = args.Split(' ');
            var x = argsArr.Length > 1? float.Parse(argsArr[1]) : 0;
            var y = argsArr.Length > 2? float.Parse(argsArr[2]) : 0;
            var z = argsArr.Length > 3? float.Parse(argsArr[3]) : 0;
            
            StartDrilling(new Vector3(x, y, z));
        }

        void To(string args)
        {
            var argsArr = args.Split(' ');
            var x = args.Length > 1? float.Parse(argsArr[1]) : 0;
            var y = args.Length > 2? float.Parse(argsArr[2]) : 0;
            var z = args.Length > 3? float.Parse(argsArr[3]) : 0;
            
            _operation = new Operation()
            {
                Name = $"To point {x}-{y}",
                Process = ToPoint(new Vector3(x, y, z), true),
            };
        }

        void StartDrilling(Vector3 from)
        {
            _operation = new Operation()
            {
                Name = $"Start drilling from point {from.X}-{from.Y}-{from.Z}",
                Process = WorkFromPoint(from),
            };
        }

        IEnumerator<float> ToPoint(Vector3 pos, bool fast)
        {
            var d = Math.Abs(X.PV - pos.X) + Math.Abs(Y.PV - pos.Y) + Math.Abs(Z.PV - pos.Z);
            d = Math.Max(d, float.MinValue);
            Z.Start(pos.Z, fast);
            while (Z.IsWork)
            {
                yield return 1 -(Math.Abs(Z.PV - Z.SP) / d);
            }

            X.Start(pos.X, fast);
            Y.Start(pos.Y, fast);

            while (X.IsWork || Y.IsWork)
            {
                yield return 1 -((Math.Abs(X.PV - X.SP) + Math.Abs(Y.PV - Y.SP)) / d);
            }
        }

        IEnumerator<float> WorkStep(Vector3 to)
        {
            var step = ToPoint(to, false);
            while (step.MoveNext())
            {
                if (_cargo.InventoryPercent() > 0.99)
                {
                    _drills.Enable(false);
                    while (_cargo.InventoryPercent() > 0.8)
                    {
                        _info.WriteText($"Cargo is FULL {_cargo.InventoryPercent():P}. Await", true);
                        yield return step.Current;
                    }
                    _drills.Enable(true);
                }
                yield return step.Current;
            }
        }

        IEnumerator<float> WorkFromPoint(Vector3 start)
        {
            var step = ToPoint(new Vector3(X.PV, Y.PV, start.Z), true);

            // Z to start
            while (step.MoveNext())
            {
                yield return step.Current;
            }

            step = ToPoint(start, true);
            // X Y to start
            while (step.MoveNext())
            {
                yield return step.Current;
            }
            
            _drills.Enable(true);
            
            var yStep = (int)(start.Y / _toolSize.Y);
            var zStep = (int)(start.Z / Z_DEPTH_STEP);

            while (!Z.Motor.IsMaxL)
            {
                if (_isFwdDir)
                {
                    while (!Y.Motor.IsMaxL)
                    {
                        var nextPoint = new Vector3(
                            X.Motor.LLimits.Max,
                            Y.Motor.LLimits.Min + (yStep * _toolSize.Y),
                            Z.Motor.LLimits.Min + (zStep * Z_DEPTH_STEP));
                        step = WorkStep(nextPoint);
                        while (step.MoveNext())
                        {
                            yield return step.Current;
                        }

                        Storage.Save();
                        yStep++;

                        nextPoint = new Vector3(
                            X.Motor.LLimits.Max,
                            Y.Motor.LLimits.Min + (yStep * _toolSize.Y),
                            Z.Motor.LLimits.Min + (zStep * Z_DEPTH_STEP));
                        step = WorkStep(nextPoint);
                        while (step.MoveNext())
                        {
                            yield return step.Current;
                        }

                        Storage.Save();

                        nextPoint = new Vector3(
                            X.Motor.LLimits.Min,
                            Y.Motor.LLimits.Min + (yStep * _toolSize.Y),
                            Z.Motor.LLimits.Min + (zStep * Z_DEPTH_STEP));
                        step = WorkStep(nextPoint);
                        while (step.MoveNext())
                        {
                            yield return step.Current;
                        }

                        Storage.Save();
                        yStep++;

                        nextPoint = new Vector3(
                            X.Motor.LLimits.Min,
                            Y.Motor.LLimits.Min + (yStep * _toolSize.Y),
                            Z.Motor.LLimits.Min + (zStep * Z_DEPTH_STEP));
                        step = WorkStep(nextPoint);
                        while (step.MoveNext())
                        {
                            yield return step.Current;
                        }
                    }

                    _isFwdDir = false;
                    Storage.Save();
                    yStep = 0;
                    zStep++;
                }

                while (!Y.Motor.IsMinL)
                {
                    var nextPoint = new Vector3(
                        X.Motor.LLimits.Max,
                        Y.Motor.LLimits.Max - (yStep * _toolSize.Y),
                        Z.Motor.LLimits.Min + (zStep * Z_DEPTH_STEP));
                    step = WorkStep(nextPoint);
                    while (step.MoveNext())
                    {
                        yield return step.Current;
                    }
                    
                    Storage.Save();
                    yStep++;
                    
                    nextPoint = new Vector3(
                        X.Motor.LLimits.Max,
                        Y.Motor.LLimits.Max - (yStep * _toolSize.Y),
                        Z.Motor.LLimits.Min + (zStep * Z_DEPTH_STEP));
                    step = WorkStep(nextPoint);
                    while (step.MoveNext())
                    {
                        yield return step.Current;
                    }
                    
                    Storage.Save();
                    
                    nextPoint = new Vector3(
                        X.Motor.LLimits.Min,
                        Y.Motor.LLimits.Max - (yStep * _toolSize.Y),
                        Z.Motor.LLimits.Min + (zStep * Z_DEPTH_STEP));
                    step = WorkStep(nextPoint);
                    while (step.MoveNext())
                    {
                        yield return step.Current;
                    }
                    
                    Storage.Save();
                    yStep++;
                        
                    nextPoint = new Vector3(
                        X.Motor.LLimits.Min,
                        Y.Motor.LLimits.Max - (yStep * _toolSize.Y),
                        Z.Motor.LLimits.Min + (zStep * Z_DEPTH_STEP));
                    step = WorkStep(nextPoint);
                    while (step.MoveNext())
                    {
                        yield return step.Current;
                    }
                }
                
                _isFwdDir = true;
                Storage.Save();
                yStep = 0;
                zStep++;
            }
            
            _drills.Enable(false);
        }
        
        void StopDrilling()
        {
            _operation = new Operation()
            {
                Name = $"Stop",
                Process = null,
            };
            
            X.Stop();
            Y.Stop();
            Z.Stop();
        }

        Operation? _operation;
        
        public override void Tick(double dt)
        {
            Logger.Log($"tick {dt}");
            if (!_operation.HasValue)
            {
                _info.WriteText("Await", true);
                return;
            }
            
            
            _operation.Value.Process?.MoveNext();
            _info.WriteText(_operation.Value.Name + " " + _operation.Value.Process?.Current.ToString("P"), true);
        }

        struct Operation
        {
            public string Name;
            public IEnumerator<float> Process;
        }
    }
}