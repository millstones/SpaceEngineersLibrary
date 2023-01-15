using System;
using System.Collections;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    class CNCSystem
    {
        readonly IMyShipController _controller;
        readonly IMyTerminalBlock _zeroBlock;
        readonly CNCMotor _addYRotor;
        readonly CNCMotor _mainRotor, _yMotor, _zMotor, _toolRotor;
        readonly ILogger _logger;
        
        MatrixD _grid;
        CNCHelpSensor _helpSensor;
        CNCTool _tool;
        CNCCargo _cargo;
        //readonly float _angleToolShift;
        
        public CNCSystem(IMyShipController controller, IMyMotorStator zeroBlock, IMyFunctionalBlock tool, IMySensorBlock helpSensor, IEnumerable<IMyCargoContainer> cargo,
            IMyMotorStator addYRotor, IMyMotorStator xRotor, IEnumerable<IMyPistonBase> yMover, IEnumerable<IMyPistonBase> zMover, ILogger logger)
        {
            _controller = controller;
            _zeroBlock = zeroBlock;
            
            CreateMatrix();
            _tool = new CNCTool(tool);
            _helpSensor = new CNCHelpSensor(helpSensor);
            _cargo = new CNCCargo(cargo);

            var moverLimSpeed = new LimitsF {Max = 1, Min = .5f};
            var rotorLimSpeed = new LimitsF {Max = .5f, Min = .1f};
            _addYRotor = new CNCRotor(new SERotor(addYRotor, rotorLimSpeed), (m)=> _tool.GetAngle(_grid));
            _mainRotor = new CNCRotor(new SERotor(zeroBlock, rotorLimSpeed), (m) =>
            {
                var p = _tool.GetPosition(_grid);
                return AngelBetweenLocalVectorAndZero(new Vector2((float)p.X, (float)p.Y));
            });

            _yMotor = new CNCPiston(new SEPiston(yMover, moverLimSpeed), (m) => (float)_tool.GetPosition(_grid).Y);
            _zMotor = new CNCPiston(new SEPiston(zMover, new LimitsF {Max = 1, Min = .05f}, true), (m) => (float)_tool.GetPosition(_grid).Z);
            _toolRotor = new CNCRotor(new SERotor(xRotor, rotorLimSpeed, true), (m)=> _tool.GetAngle(_grid));
            _logger = logger;

            Task.EveryTick(() =>
                {
                    AxisInfo("X", null, _toolRotor);
                    AxisInfo("Y",_yMotor, null);
                    AxisInfo("Z", _zMotor, _mainRotor);
                    ToolInfo();
                })
                .Run();

            //(_logger as DefaultLogger).Debug.DrawMatrix(Grid, 3, seconds: 50);
        }

        public void Init()
        {
            CreateMatrix();
        }

        void CreateMatrix()
        {
            var localGrid = _controller.CreateHorizontalMatrix(_zeroBlock);
            _grid = MatrixD.CreateWorld(localGrid.Translation, localGrid.Down, localGrid.Forward);
        }

        void AxisInfo(string name, CNCMotor motor, CNCMotor rotor)
        {
            _logger.Log($"AXIS {name}:");
            if (rotor != null)
                _logger.Log($"    angel   : {rotor.PV:000}° ---> {rotor.SP:000}°");
            if (motor != null)
                _logger.Log($"    position: {motor.PV:00.00} ---> {motor.SP:00.00}");
        }

        void ToolInfo()
        {
            _logger.Log($"TOOL: {_tool.GetPosition(_grid).ToString("00.00")} : {_toolRotor.PV:000}°");
        }

        public void Stop()
        {
            _mainRotor.Stop();
            _tool.Stop();
        }

        public void Test(string cmd)
        {
            var z = 0;
            var a = -180f;
            /*
            // [x] [y] [z] [a]
            var cmdA = cmd.Split(' ');
            var x = cmdA.Length > 1? float.Parse(cmdA[1]) : 0;
            var y = cmdA.Length > 2? float.Parse(cmdA[2]) : 0;
            var z = cmdA.Length > 3? float.Parse(cmdA[3]) : 0;
            var a = cmdA.Length > 4? float.Parse(cmdA[4]) : 0;
            
            var vecSP = new Vector2(x, y);
            */
            var vec = _helpSensor.GetPithRange(_grid).Max;
            var vecSP = new Vector2(vec.X, vec.Y);
            vecSP = vecSP == Vector2.Zero ? new Vector2((float)_grid.Up.X, (float)_grid.Up.Y) : vecSP;
            var angle = AngelBetweenLocalVectorAndZero(vecSP);
            var yMove = vecSP.Length();

            _mainRotor.Start(angle, true);
            _toolRotor.Start(a, true);
            _yMotor.Start((float)yMove, true);
            //_zMotor.Start((float)z, true);
            
            /*
            var v = Grid.Body2WorldPosition(new Vector3D(vecSP, z));
            var t = Grid.Body2WorldPosition(_tool.Position);
            (_logger as DefaultLogger).Debug.DrawSphere(new BoundingSphereD(v, 1), Color.Red, DebugAPI.Style.Wireframe, seconds: 50);
            (_logger as DefaultLogger).Debug.DrawSphere(new BoundingSphereD(t, 1), Color.Green, DebugAPI.Style.Wireframe, seconds: 50);
            */

            //_tool.Start();
        }

        float AngelBetweenLocalVectorAndZero(Vector2 vec)
        {
            //var toolPosNormal = Vector2.Normalize(new Vector2((float)Grid.Up.X, (float)Grid.Up.Y));
            //var vecNormal = Vector2.Normalize(vec);
            //var aX = (float)Math.Acos(Vector2.Dot(toolPosNormal,vecNormal));


            var aX = Math.Atan2(vec.X, vec.Y);

            aX = MathHelper.ToDegrees(aX);
            
            aX = Math.Abs(vec.X) < 0.01f ? 0 : aX;
            aX = Math.Abs(vec.Y) < 0.01f ? 180 : aX;
            
            if (vec.X < 0)
                aX = -aX;

            return (float)aX;
        }

        public void Start()
        {
            WorkProcess().ToTask().Run();
        }

        IEnumerator WorkProcess()
        {
            const float maxX = 10f;
            const float minY = 10f;
            const float maxY = 20f;
            const float maxZ = -300f;
            const float sizeTool = 1f;
            // Получить рабочий направление поворота платформы
            var workDir = _grid.GetClosestDirection(_tool.GetPosition(_grid));
            // выставить платформу в НОЛЬ
            var starA = 0f;
            switch (workDir)
            {
                case Base6Directions.Direction.Backward:
                    starA = 180;
                    break;
                case Base6Directions.Direction.Left:
                    starA = -90;
                    break;
                case Base6Directions.Direction.Right:
                    starA = 90;
                    break;
                default:
                    starA = 0;
                    break;
            }

            _mainRotor.Start(starA, true);
            while (_mainRotor.IsWork)
            {
                yield return null;
            }
            
            // Создать рабочие оси координат
            // TODO ХЗ
            
            //
            var step = 0;
            var steps = Math.Abs((2 * maxX * (minY - maxY)) / sizeTool);

            for (var x = -maxX; x < maxX; x+= sizeTool)
            {
                for (var y = minY; y < maxY; y+= sizeTool)
                {
                    step++;
                    // Подводим инструмент к начальной точки прохода
                    var vecSP = new Vector2(x, y);
                    vecSP = vecSP == Vector2.Zero ? new Vector2((float)_grid.Up.X, (float)_grid.Up.Y) : vecSP;
                    var angle = AngelBetweenLocalVectorAndZero(vecSP);
                    var yMove = vecSP.Length();
                    
                    _mainRotor.Start(angle, true);
                    _yMotor.Start(yMove, true);

                    while (_mainRotor.IsWork)
                    {
                        _logger.Log($"Шахта #{step}/{steps}");
                        _logger.Log($"Подводим инструмент к начальной точки прохода");
                        yield return null;
                    }
                    
                    // Доворачиваем бур
                    _tool.Enable();
                    _toolRotor.Start(-90, true);

                    while (_toolRotor.IsWork)
                    {
                        _logger.Log($"Шахта #{step}/{steps}");
                        _logger.Log($"Доворачиваем бур");
                        yield return null;
                    }

                    // Бурим проход
                    _tool.Enable();
                    _zMotor.Start(maxZ, false);

                    while (_zMotor.IsWork)
                    {
                        _logger.Log($"Шахта #{step}/{steps}");
                        _logger.Log($"Бурим проход");
                        if (_cargo.IsFull)
                        {
                            while (_cargo.IsFull)
                            {
                                _logger.Log($"Контейнер полон");
                                _tool.Stop();
                                _zMotor.Stop();
                                yield return null;
                            }

                            _tool.Enable();
                            _zMotor.Start(maxZ, false);
                        }
                        yield return null;
                    }
                    // Поднимаем бур
                    _tool.Stop();
                    _zMotor.Start(0, true);

                    while (_zMotor.IsWork)
                    {
                        _logger.Log($"Шахта #{step}/{steps}");
                        _logger.Log($"Поднимаем бур");
                        yield return null;
                    }
                }
            }
        }
    }
}