using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    class CarrierController : Module
    {
        Engine _engine;
        MainConnector _connector;
        MainCargoContainer _cargoContainer;
        
        PathBuilder _pathBuilder;

        DroneActualStatus _status;
        
        IEnumerator _workLoop;

        public override void Awake(IEnumerable<IMyTerminalBlock> blocks)
        {
            var myTerminalBlocks = blocks as IMyTerminalBlock[] ?? blocks.ToArray();
            var shipController = myTerminalBlocks.OfType<IMyRemoteControl>().First();
            
            _connector = new MainConnector(myTerminalBlocks, MessageBroker);
            _cargoContainer = new MainCargoContainer(myTerminalBlocks, MessageBroker, Debug);
            _engine = new Engine(myTerminalBlocks, _connector.Relative, Debug);
            
            _pathBuilder = new PathBuilder(_connector.Relative, shipController);
            
            MessageBroker.Post("set autopilot", EnableAutopilot);
            MessageBroker.Post("reset autopilot", DisableAutopilot);
            MessageBroker.Post(RadioLang.GetActualStatus, () => _status);
        }

        public override void Start()
        {
            _workLoop = WorkLoop();
        }

        void UpdateStatus(string status)
        {
            Logger.Log(NoteLevel.Info, $"STATUS : {status}");
        }

        IEnumerator<GridInfo> AwaitRequestNearestBasePosition()
        {
            UpdateStatus("Request base position");
            
            var s = StationActualStatus.Default;
            var next = false;

            MessageBroker.Get<StationActualStatus>(RadioLang.GetActualStatus, GlobalConst.GRID_GROUP_STATION,
                (e) =>
                {
                    var keyValuePairs = e as KeyValuePair<long, StationActualStatus>[] ?? e.ToArray();
                    if (!keyValuePairs.Any())
                    {
                        EmergencyStop("Stations list is empty.");
                    }
                    else
                    {
                        s = keyValuePairs.First().Value;
            
                        foreach (var pair in keyValuePairs)
                        {
                            var d = _connector.Relative.Distance2Body(pair.Value.GridInfo.Position);
                            var dMin = _connector.Relative.Distance2Body(s.GridInfo.Position);
                            if (d.LengthSquared() < dMin.LengthSquared())
                                s = pair.Value;
                        }

                        next = true;
                        UpdateStatus($"Nearest base -> {s.GridInfo.FulName}");
                    }
                });
            
            var exitDate = DateTime.Now + MessageBroker.AwaitAnswerTime + TimeSpan.FromSeconds(1);
            while (!next && DateTime.Now < exitDate)
            {
                yield return GridInfo.Default;
                if (DateTime.Now > exitDate)
                    EmergencyStop("Time out");
            }

            UpdateStatus($"Nearest base distance={_connector.Relative.Distance2Body(s.GridInfo.Position).Length()}.");
            yield return s.GridInfo;
        }
        IEnumerator<GridInfo> AwaitRequestBasePosition(GridInfo grid)
        {
            UpdateStatus("Request base position");

            var retVal = GridInfo.Default;
            var next = false;

            MessageBroker.Get<StationActualStatus>(RadioLang.GetActualStatus, grid.Id, (s) =>
            {
                next = true;
                retVal = s.GridInfo;
            });

            var exitDate = DateTime.Now + MessageBroker.AwaitAnswerTime + TimeSpan.FromSeconds(1);

            while (!next && exitDate > DateTime.Now)
            {
                yield return GridInfo.Default;
                if (DateTime.Now > exitDate)
                    EmergencyStop("Time out");
            }

            UpdateStatus($"Complete. Distance={(_connector.Relative.WorldMatrix.Translation - retVal.Position).Length()}. Await...");

            yield return retVal;
        }
        IEnumerator AwaitMoveToBasePosition(GridInfo grid)
        {
            UpdateStatus($"Move to base {grid.FulName}");
            
            _engine.Path = _pathBuilder
                .New()
                .AddFree(grid.Position, 1000)
                .Build();

            _engine.SetAutopilot();
            while (!_engine.IsMoveDone)
            {
                yield return null;
            }
        }
        IEnumerator<MatrixD> AwaitDocking(GridInfo grid)
        {
            UpdateStatus("Docking");
            
            Logger.Log(NoteLevel.Info, $"Request free unloading connector to {grid.Id}");
            var retVal = default(MatrixD);
            var next = false;
                
            MessageBroker.Get<MatrixD>(RadioLang.GetFreeUnloadConnector, grid.Id, d =>
            {
                Logger.Log(NoteLevel.Info, "Accept free unloading connector");

                _engine.Path = _pathBuilder
                    .New()
                    .AddDocking(d)
                    .Build();

                next = true;
                retVal = d;
            });
            
            var exitDate = DateTime.Now + MessageBroker.AwaitAnswerTime + TimeSpan.FromSeconds(1);
            while (!next && exitDate > DateTime.Now)
            {
                yield return default(MatrixD);
                if (DateTime.Now > exitDate)
                    EmergencyStop("Time out");
            }
            
            _engine.SetAutopilot();
            while (!_engine.IsMoveDone && !_connector.IsConnectable())
            {
                yield return default(MatrixD);
            }

            if (!_connector.IsConnectable())
                EmergencyStop("Docking failed. Connector not found");

            while (_connector.IsConnectable())
            {
                _connector.TryConnect();
                yield return default(MatrixD);
            }

            yield return retVal;
        }
        IEnumerator AwaitLoading(long toId, Product product)
        {
            UpdateStatus("Loading");

            var transfer = _cargoContainer.Transfer(toId, product.Type,Math.Abs(product.Amount), false);

            var exitDate = DateTime.Now + TimeSpan.FromSeconds(30);
            while (transfer.MoveNext() && exitDate > DateTime.Now)
            {
                Debug.PrintChat($"Load {transfer.Current}");
                yield return null;
                if (DateTime.Now > exitDate)
                    Logger.Log(NoteLevel.Waring, "Time out loading product");
            }
        }
        IEnumerator AwaitUnloading(long toId, Product product)
        {
            UpdateStatus("Unloading");

            var transfer = _cargoContainer.Transfer(toId, product.Type, Math.Abs(product.Amount), true);

            var exitDate = DateTime.Now + TimeSpan.FromSeconds(30);
            while (transfer.MoveNext() && exitDate > DateTime.Now)
            {
                Debug.PrintChat($"Unload {transfer.Current}");
                yield return null;
                if (DateTime.Now > exitDate)
                    Logger.Log(NoteLevel.Waring, "Time out unloading product");
            }
        }

        DateTime _lastRequestNewTask;
        IEnumerator<TransportationTaskInfo> AwaitNextTask()
        {
            UpdateStatus("Await next task");

            var exitDate = _lastRequestNewTask + TimeSpan.FromSeconds(30);
            while (DateTime.Now < exitDate)
            {
                yield return TransportationTaskInfo.Default;
            }
            _lastRequestNewTask = DateTime.Now;
            
            var retVal = TransportationTaskInfo.Default;
            var next = false;
            var stations = new List<StationActualStatus>();
            MessageBroker.Get<StationActualStatus>(RadioLang.GetActualStatus, GlobalConst.GRID_GROUP_STATION,
                (e =>
                {
                    stations = e.Select(x => x.Value).ToList();
                    next = true;
                }));

            while (!next)
            {
                yield return retVal;
            }

            if (stations.Count < 2) 
                yield break;
            
            var deficit = stations
                .Where(x=> !x.Deficit.Type.Equals(default(MyItemType)))
                .Min(x => x.Deficit.Amount);
            if (deficit == 0)
            {
                yield break;
            }

            StationActualStatus from;
            var to = stations.First(x => x.Deficit.Amount == deficit);

            if (stations.Any(x => !x.Surplus.Type.Equals(default(MyItemType)) && x.Surplus.Type == to.Deficit.Type))
            {
                @from = stations.Find(x => x.Surplus.Type == to.Deficit.Type);
            }
            else
                yield break;

            if (@from.GridInfo.Id == to.GridInfo.Id)
                yield return TransportationTaskInfo.Default;
            else
                yield return new TransportationTaskInfo
                {
                    From = @from.GridInfo,
                    To = to.GridInfo,
                    ProductRequest = to.Deficit,
                };
        }

        IEnumerator AwaitUndocking(MatrixD dock)
        {
            UpdateStatus("Undocking");

            _engine.SetAutopilot();
            var exitDate = DateTime.Now + TimeSpan.FromSeconds(5);
            while (_connector.IsConnected())
            {
                _connector.Disconnect();

                yield return null;
                
                if (DateTime.Now >= exitDate)
                    EmergencyStop("Disconnection from connector failed");
            }

            _engine.Path = _pathBuilder
                .New()
                .AddUnDocking(dock)
                .Build();

            _engine.SetAutopilot();
            while (!_engine.IsMoveDone)
            {
                yield return null;
            }

        }

        bool CheckTransportationTask()
        {
            return true;
        }

        void CheckEnergy()
        {
            
        }

        public override void Tick(double dt)
        {
            _status = new DroneActualStatus
            {
                cargo = 100,
                energy = 100,
                GridInfo = GridInfo,
            };
            
            _engine.Tick();

            if (_pause) return;
            
            if (_workLoop == null)
                _workLoop = WorkLoop();
            
            
            _workLoop.MoveNext();
        }

        IEnumerator WorkLoop()
        {
            GridInfo nearestGrid;
            MatrixD undockingMatrix;
            // 1 
            {
                var task = AwaitRequestNearestBasePosition();
                while (task.MoveNext())
                {
                    yield return null;
                }

                nearestGrid = task.Current;
                if (nearestGrid.Equals(GridInfo.Default))
                    EmergencyStop("Invalid target base position");
            }
            // 2
            {
                var task = MoveToGrid(nearestGrid);
                while (task.MoveNext())
                {
                    yield return null;
                }

                undockingMatrix = task.Current;
            }

            var dockedGrid = nearestGrid;
            var currentTransportationTask = TransportationTaskInfo.Default;

            while (true)
            {
                GridInfo targetGrid;
                if (currentTransportationTask.Equals(TransportationTaskInfo.Default))
                    // 1
                {
                    var task = AwaitNextTask();
                    while (task.MoveNext())
                    {
                        yield return null;
                    }

                    currentTransportationTask = task.Current;
                    targetGrid = currentTransportationTask.From;
                }
                else
                    targetGrid = currentTransportationTask.To;

                //2
                if (dockedGrid.Id == currentTransportationTask.To.Id)
                {
                    if (_cargoContainer.Contains(currentTransportationTask.ProductRequest.Type,
                        Math.Abs(currentTransportationTask.ProductRequest.Amount)))
                    {
                        // 2.1
                        var nextOperation = AwaitUnloading(targetGrid.Id, currentTransportationTask.ProductRequest);
                        while (nextOperation.MoveNext())
                        {
                            yield return null;
                        }
                        
                        currentTransportationTask = TransportationTaskInfo.Default;
                        continue;
                    }
                    targetGrid = currentTransportationTask.From;
                }
                // 3
                if (dockedGrid.Id == currentTransportationTask.From.Id)
                {
                    if (!_cargoContainer.Contains(currentTransportationTask.ProductRequest.Type,
                        Math.Abs(currentTransportationTask.ProductRequest.Amount)))
                    {
                        // 3.1
                        var task = AwaitLoading(targetGrid.Id, currentTransportationTask.ProductRequest);
                        while (task.MoveNext())
                        {
                            yield return null;
                        }
                    }
                    targetGrid = currentTransportationTask.To;
                }
                
                // 4
                {
                    var task = AwaitUndocking(undockingMatrix);
                    while (task.MoveNext())
                    {
                        yield return null;
                    }
                }
                // 5
                {
                    var task = MoveToGrid(targetGrid);
                    while (task.MoveNext())
                    {
                        yield return null;
                    }

                    undockingMatrix = task.Current;
                    dockedGrid = targetGrid;
                }
            }
        }

        IEnumerator<MatrixD> MoveToGrid(GridInfo grid)
        {
            MatrixD retVal;
            // 1
            {
                var task = AwaitRequestBasePosition(grid);
                while (task.MoveNext() )
                {
                    yield return default(MatrixD);
                }
            }
            // 2 
            {
                var task = AwaitMoveToBasePosition(grid);
                while (task.MoveNext())
                {
                    yield return default(MatrixD);
                }
            }
            
            // 3
            {
                var task = AwaitDocking(grid);
                while (task.MoveNext())
                {
                    yield return default(MatrixD);
                }

                retVal = task.Current;
            }

            yield return retVal;
        }

        void EnableAutopilot()
        {
            _pause = false;
            _engine.SetAutopilot();
        }

        bool _pause = true;
        void DisableAutopilot()
        {
            _engine.ResetAutopilot();
            _pause = true;
        }
        void EmergencyStop(string e)
        {
            _engine.ResetAutopilot();
            _pause = true;
            _workLoop = null;
            Logger.Log(NoteLevel.Waring, e);
        }
    }
}