using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace IngameScript
{
    abstract class BaseProductController : Module
    {
        protected List<Product> Products = new List<Product>();

        Product GetSurplus()
        {
            var p = Products.Where(x=> x.Amount > 0).ToArray();
            if (!p.Any()) return default(Product);
            {
                var maxMass = p.Max(x =>x.Amount);
                return Products.First(x => x.Amount == maxMass);
            }
        }

        Product GetDeficit()
        {
            var p = Products.Where(x=> x.Amount < 0).ToArray();
            if (!p.Any()) return default(Product);
            {
                var minMass = p.Min(x =>x.Amount);
                return Products.First(x => x.Amount == minMass);
            }
        }

        public override void Start()
        {
            MessageBroker.Post(RadioLang.GetActualStatus, GetActualStatusCallback);
        }
        
        StationActualStatus GetActualStatusCallback()
        {
            return new StationActualStatus
            {
                Deficit = GetDeficit(),
                Surplus = GetSurplus(),
                GridInfo = GridInfo
            };
        }
    }
    class ProductController : BaseProductController
    {
        public override void Awake(IEnumerable<IMyTerminalBlock> blocks)
        {
            Products.Add(new Product{Type = MyItemType.MakeOre("Iron"), Amount = -100000});
        }
    }

    class ProductController2 : BaseProductController
    {
        public override void Awake(IEnumerable<IMyTerminalBlock> blocks)
        {
            Products.Add(new Product {Type = MyItemType.MakeOre("Iron"), Amount = 10000000});
        }
    }
}