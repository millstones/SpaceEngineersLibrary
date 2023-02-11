using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    class DroneRadarMasterRadio : Module
    {
        Dictionary<long, DroneActualStatus> _actualInfoOfDrone = new Dictionary<long, DroneActualStatus>();

        IEnumerator _pingTask;
        public override void Awake(IEnumerable<IMyTerminalBlock> blocks)
        {

        }

        public override void Start()
        {
            _pingTask = PingDronesTask();
        }

        public override void Tick(double dt)
        {
            _pingTask?.MoveNext();
        }
        IEnumerator PingDronesTask()
        {
            while (true)
            {
                var nextDate = DateTime.Now + TimeSpan.FromSeconds(30);
                while (DateTime.Now < nextDate)
                {
                    yield return null;
                }
                MessageBroker.Get<DroneActualStatus>(RadioLang.GetActualStatus, GlobalConst.GRID_GROUP_FLY_CARRIER_DRONE, (e) =>
                {
                    foreach (var pair in e)
                    {
                        if (!_actualInfoOfDrone.ContainsKey(pair.Key))
                            _actualInfoOfDrone.Add(pair.Key, pair.Value);
                        else
                            _actualInfoOfDrone[pair.Key] = pair.Value;
                    }
                });
            }
        }
    }
}