using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Sandbox.ModAPI.Ingame;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    public class MainConnector
    {
        public enum Commands
        {
            GetOtherConnector,
        }
        
        IMyShipConnector _connector;
        
        public MainConnector(IEnumerable<IMyTerminalBlock> blocks, IMessageBroker messageBroker)
        {
            var terminalBlocks = blocks as IMyTerminalBlock[] ?? blocks.ToArray();
            var connectors = terminalBlocks.OfType<IMyShipConnector>().ToArray();
            if (connectors.Length == 0)
                throw new Exception("Connectors not found");
            if (connectors.Length > 1)
            {
                var c = connectors.Where(x =>
                    x.CustomName.Contains("main connector") || x.CustomName.Contains("Main connector")).ToArray();
                if (c.Length == 0)
                    throw new Exception("Not found 'M(m)ain connector'");
                if (c.Length > 1)
                    throw new Exception("Multiple 'M(m)ain connector'");

                _connector = c[0];
            }
            else
                _connector = connectors[0];
            
            messageBroker.Post(Commands.GetOtherConnector, () => _connector.OtherConnector);
        }

        public void TryConnect() => _connector.Connect();
        public void Disconnect() => _connector.Disconnect();
        public bool IsConnected() => _connector.Status == MyShipConnectorStatus.Connected;
        public bool IsConnectable() => _connector.Status == MyShipConnectorStatus.Connectable;
        public IMyCubeBlock Relative => _connector;
    }

    public class MainCargoContainer
    {
        IMyCargoContainer _container;
        IMessageBroker _messageBroker;
        readonly DebugAPI _debug;

        public MainCargoContainer(IEnumerable<IMyTerminalBlock> blocks, IMessageBroker messageBroker, DebugAPI debug)
        {
            var terminalBlocks = blocks as IMyTerminalBlock[] ?? blocks.ToArray();
            var cargos = terminalBlocks.OfType<IMyCargoContainer>().ToArray();
            if (cargos.Length == 0)
                throw new Exception("Cargo container not found");
            if (cargos.Length > 1)
            {
                var c = cargos.Where(x =>
                    x.CustomName.Contains("main cargo container") || x.CustomName.Contains("Main cargo container")).ToArray();
                if (c.Length == 0)
                    throw new Exception("Not found 'M(m)ain cargo container'");
                if (c.Length > 1)
                    throw new Exception("Multiple 'M(m)ain connector'");
                
                _container = c[0];
            }
            else
                _container = cargos[0];
            
            _messageBroker = messageBroker;
            _debug = debug;
        }

        public bool Contains(MyItemType type, MyFixedPoint amount)
        {
            _debug.PrintHUD($"Contains product: {type}-{amount}");
            return _container.GetInventory().ContainItems(amount, type);
        }
        public IEnumerator<int> Transfer(long toId, MyItemType product, MyFixedPoint amount, bool fromMe)
        {
            var next = false;
            IMyShipConnector other = null;
            _messageBroker.Get<IMyShipConnector>(MainConnector.Commands.GetOtherConnector, pairs =>
            {
                //var a = pairs.Where(x => x.Key == toId).ToList();

                if (pairs.Any())
                {
                    var s = pairs.Select(x => x.Value).ToList();
                    if (s.Count > 1 || s.Count == 0)
                        throw new Exception("Internal error");

                    other = s.FirstOrDefault();
                    next = true;
                }
                else
                    throw new Exception("Internal error");
            });

            while (!next)
            {
                yield return 0;
            }
            
            if (other == null)
                throw new Exception($"Other connector is NULL");

            if (!other.HasInventory || !_container.GetInventory().CanTransferItemTo(other.GetInventory(), product))
            {
                throw new Exception($"Receiver for cargo {product.SubtypeId} {amount} not found");
            }

            IMyInventory from = other.GetInventory(), to = _container.GetInventory();
            if (fromMe)
            {
                to = from;
                from = _container.GetInventory();
            }
            
            var items = new List<MyInventoryItem>();
            from.GetItems(items, item => item.Type == product);
            
            var allTransItemCount  = MyFixedPoint.Zero;//items.Aggregate(MyFixedPoint.Zero, (current, item) => current + item.Amount);

            const float V2M = 1;
            
            while (allTransItemCount < amount && items.Count != 0)
            {
                var removeList = new List<MyInventoryItem>();
                foreach (var item in items)
                {
                    var itmForTrans = (amount - allTransItemCount);
                    var freeSpace = (to.MaxVolume - to.CurrentVolume) * 1000 * V2M;
                    var transItemCount = item.Amount > itmForTrans ? itmForTrans : item.Amount;
                    if (freeSpace < itmForTrans)
                        transItemCount = freeSpace;
                    else
                        removeList.Add(item);

                    to.TransferItemFrom(from, item, transItemCount);

                    allTransItemCount += transItemCount;
                
                    yield return allTransItemCount.ToIntSafe();
                }

                foreach (var item in removeList)
                {
                    items.Remove(item);
                }
            }
            
            yield return allTransItemCount.ToIntSafe();
        }
    }
}