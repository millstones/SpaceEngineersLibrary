using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    class DroneStantonMasterRadio : Module
    {
        //IRadioService _radio;
        List<IMyShipConnector> _connector;
        public override void Awake(IEnumerable<IMyTerminalBlock> blocks)
        {
            _connector = blocks.OfType<IMyShipConnector>().ToList();
        }

        public override void Start()
        {
            //_radio = ServiceProvider.Get<IRadioService>();
            MessageBroker.Post(RadioLang.GetFreeUnloadConnector, GetFreeConnectorCallback);
            MessageBroker.Post(RadioLang.GetFreeLoadConnector, GetFreeConnectorCallback);
        }

        MatrixD GetFreeConnectorCallback(long from)
        {
            var freeConnector = _connector.First();
            //MessageBroker.SendMessage<MatrixD>(cmd, from, freeConnector.WorldMatrix);

            return freeConnector.WorldMatrix;
        }
    }
}