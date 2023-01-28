using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    class EnergyManager : Module, IPageProvider
    {
        public Page Page { get; private set; }

        List<IMyPowerProducer> _allEnergy;

        List<IMyBatteryBlock> Battery => _allEnergy.OfType<IMyBatteryBlock>().ToList();
        List<IMyReactor> Reactors => _allEnergy.OfType<IMyReactor>().ToList();
        List<IMySolarPanel> SolarPanels => _allEnergy.OfType<IMySolarPanel>().ToList(); 

        public override void Awake(IEnumerable<IMyTerminalBlock> blocks)
        {
            _allEnergy = blocks.OfType<IMyPowerProducer>().ToList();
            
            Page = new EnergyManagerPage(this);
        }

        public override void Tick(double dt, IEnumerable<IMyTerminalBlock> blocks)
        {
            _allEnergy = blocks.OfType<IMyPowerProducer>().ToList();
        }

        class EnergyManagerPage : Page
        {
            public PageItem LeftPanel, RightPanel;
            public EnergyManagerPage(EnergyManager energy) : base("Energy manager")
            {
                Title = Id;
                RightPanel = new EnergyOverview();
                
                LeftPanel = new FlexiblePanel<PageItem>(true)
                        .Add(new StackPanel<PageItem>(true)
                            .Add(new BatteryView(energy))
                            )
                    ;
                
                Add(LeftPanel, CreateArea(new Vector2(0, 0.1f), new Vector2(0.5f, 1)));
                Add(RightPanel, CreateArea(new Vector2(0.5f, 0.1f), Vector2.One));
            }

            class EnergyOverview : FreeCanvas
            {
                
            }

            class BatteryView : FreeCanvas
            {
                public BatteryView(EnergyManager energy)
                {
                    
                }
            }
        }
    }
}