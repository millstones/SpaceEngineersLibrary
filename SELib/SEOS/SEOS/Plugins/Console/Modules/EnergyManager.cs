using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.VisualScripting.Utils;
using VRageMath;

namespace IngameScript
{
    class EnergyManager : Module, IPageProvider
    {
        Action OnSorting;
        public Page Page { get; private set; }
        
        Dictionary<string, List<IMyPowerProducer>> _allEnergy;

        // List<T> PowerProducerOfGroup<T>(string group)
        // {
        //     // if (group == "")
        //     //     return _allEnergy.OfType<T>().ToList();
        //
        //     return !_allEnergy.ContainsKey(@group) ? new List<T>() : _allEnergy[@group].OfType<T>().ToList();
        // }
        //List<IMyBatteryBlock> Battery => _allEnergy.OfType<IMyBatteryBlock>().ToList();
        //List<IMyReactor> Reactors => _allEnergy.OfType<IMyReactor>().ToList();
        //List<IMySolarPanel> SolarPanels => _allEnergy.OfType<IMySolarPanel>().ToList();

        public override void Awake(IEnumerable<IMyTerminalBlock> blocks)
        {
            Page = new EnergyManagerPage(this);
        }

        DateTime _nextSorting;
        IEnumerator _groupFinder;

        public override void Tick(double dt, IEnumerable<IMyTerminalBlock> blocks)
        {
            if (_groupFinder == null || !_groupFinder.MoveNext() && _nextSorting < DateTime.Now)
                _groupFinder = SortOfGroups(blocks.OfType<IMyPowerProducer>());
        }

        IEnumerator SortOfGroups(IEnumerable<IMyPowerProducer> producers)
        {
            _nextSorting = DateTime.Now + TimeSpan.FromSeconds(ConsolePluginSetup.POWER_PRODUCER_UPDATE_PERIOD_SEC);
            var all = new Dictionary<string, List<IMyPowerProducer>>();
            
            foreach (var producer in producers)
            {
                if (producer == null) continue;
                
                var g = ConsoleNameParser.FindSubstring(ConsolePluginSetup.GROUP_MARK, producer.CustomName);
                //if (!string.IsNullOrEmpty(g))
                {
                    if (!all.ContainsKey(g)) all.Add(g, new List<IMyPowerProducer>());
                    
                    all[g].Add(producer);
                }
                
                yield return null;
            }

            var remove = new List<string>();
            foreach (var pair in all)
            {
                if (!pair.Value.Any()) remove.Add(pair.Key);
                yield return null;
            }

            foreach (var rem in remove)
            {
                all.Remove(rem);
                yield return null;
            }
            
            _allEnergy = all;
            OnSorting?.Invoke();
        }

        class EnergyManagerPage : Page
        {
            EnergyManager _energy;

            FlexiblePanel<PageItem> _content;
            string _currentPage = "OVERVIEW";
            public EnergyManagerPage(EnergyManager energy) : base("Energy manager")
            {
                _energy = energy;
                Title = Id;

                var getCurrentPage = new ReactiveProperty<string>(
                    () => _currentPage, 
                    pages =>
                    {
                        SwitchContent(pages);
                        _currentPage = pages;
                    }
                    );

                var navigation = new StringSwitcher(getCurrentPage, true)
                        .Add("OVERVIEW", "GENERATION", "STORAGE");

                Add(navigation, CreateArea(new Vector2(0.0f, 0.1f), new Vector2(1, 0.175f)));
            }
            void SwitchContent(string content)
            {
                Remove(_content);
                
                switch (content)
                {
                    case "GENERATION":
                        _content = new EnergyGeneration(_energy);
                        break;
                    case "STORAGE":
                        _content = new EnergyStorage(_energy);
                        break;
                    default:
                        _content = new EnergyOverview(_energy);
                        break;
                }

                Add(_content, CreateArea(new Vector2(0f, 0.175f), Vector2.One));
            }
        }

        class EnergyOverview : FlexiblePanel<PageItem>
        {
            public EnergyOverview(EnergyManager energy) : base(false)
            {

            }
        }
        class EnergyStorage : FlexiblePanel<PageItem>
        {
            readonly EnergyManager _energy;

            public EnergyStorage(EnergyManager energy) : base(false)
            {
                _energy = energy;
                
                _energy.OnSorting += () =>
                {
                    Clear();
                    Build();
                };
                
                Build();
            }

            void Build()
            {
                var batViewList = new List<BatteryView>();
                foreach (var @group in _energy._allEnergy.Where(x => x.Value.Any()))
                {
                    var bats = @group.Value.OfType<IMyBatteryBlock>().ToList();
                    if (!bats.Any()) continue;
                    batViewList.Add(new BatteryView(bats, @group.Key));
                }

                var rowLen = 4;
                var collumns = batViewList.Count / rowLen;
                collumns += (batViewList.Count % rowLen) > 0 ? 1 : 0;

                var num = 0;
                for (var i = 0; i < collumns; i++)
                {
                    var row = new FlexiblePanel<PageItem>(true);
                    for (var j = 0; j < rowLen; j++)
                    {
                        row.Add(num < batViewList.Count
                            ? batViewList[num]
                            : new FreeCanvas());

                        num++;
                    }

                    Add(row);
                }
            }
        }

        class EnergyGeneration : FlexiblePanel<PageItem>
        {
            public EnergyGeneration(EnergyManager energy) : base(false)
            {

            }
        }
    }
}