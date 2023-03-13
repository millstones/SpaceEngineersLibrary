using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    class EnergyManager : Module, IPageProvider
    {
        Action _onSorting;
        public Page Page { get; private set; }

        Dictionary<string, List<IMyPowerProducer>> _allEnergy;

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
            _onSorting?.Invoke();
        }

        public class EnergyManagerPage : Page
        {
            public EnergyManagerPage(EnergyManager energy) : base("E N E R G Y")
            {

                var page = new TabPanel(
                    new EnergyOverview(energy), 
                    new EnergyGeneration(energy),
                    new EnergyStorage(energy));

                Add(page);
            }

            class EnergyOverview : Page
            {
                EnergyManager _energy;

                public EnergyOverview(EnergyManager energy) : base("OVERVIEW")
                {
                    _energy = energy;

                    _energy._onSorting += () =>
                    {
                        //RemoveAll();
                        Build();
                    };
                }

                void Build()
                {
                    var currentOut = 0f;
                    var maxOut = 0f;
                    var fCurrentOut = 0f;
                    var fMaxOut = 0f;

                    foreach (var value in _energy._allEnergy.Values)
                    {
                        foreach (var producer in value)
                        {
                            var @out = producer.CurrentOutput;
                            var @in = producer.MaxOutput;
                            currentOut += @out;
                            maxOut += @in;

                            if (producer.HasInventory)
                            {
                                fCurrentOut += @out;
                                fMaxOut += @in;
                            }
                        }
                    }

                    var panel = new FlexItem()
                            .Add(new Text("All power generators") /*, new RectangleF(0, 0, 1, 0.1f)*/)
                            .Add(new ProgressBar(() => currentOut / maxOut, reversLogic: true))
                            //, CreateArea(new Vector2(0, 0.1f), new Vector2(1, 0.4f)))
                            .Add(new Text("Fuel generators"))
                            //, CreateArea(new Vector2(0, 0.4f), new Vector2(1, 0.5f)))
                            .Add(new ProgressBar(() => fCurrentOut / fMaxOut, reversLogic: true))
                            //, CreateArea(new Vector2(0, 0.5f), new Vector2(1, 0.8f)))
                            .Add(new Text("Other generators"))
                            //, CreateArea(new Vector2(0, 0.8f), new Vector2(1, 0.9f)))
                            .Add(new ProgressBar(() => (currentOut - fCurrentOut) / (fMaxOut - fMaxOut),
                                reversLogic: true))
                            //,CreateArea(new Vector2(0, 0.9f), new Vector2(1, 1.2f)))
                        ;


                    Add(panel);
                }
            }

            class EnergyGeneration : Page
            {
                EnergyManager _energy;

                public EnergyGeneration(EnergyManager energy) : base("GENERATION")
                {
                    _energy = energy;

                    _energy._onSorting += () =>
                    {
                        //RemoveAll();
                        Build();
                    };
                }

                void Build()
                {
                    var batViewList = new List<GeneratorView>();
                    foreach (var @group in _energy._allEnergy.Where(x => x.Value.Any()))
                    {
                        var bats = @group.Value; //.OfType<IMyPowerProducer>().ToList();
                        if (!bats.Any()) continue;
                        batViewList.Add(new GeneratorView(bats, @group.Key));
                    }

                    var rowLen = 4;
                    var collumns = batViewList.Count / rowLen;
                    collumns += (batViewList.Count % rowLen) > 0 ? 1 : 0;

                    var num = 0;
                    for (var i = 0; i < collumns; i++)
                    {
                        var row = new FlexItem();
                        for (var j = 0; j < rowLen; j++)
                        {
                            row.Add(num < batViewList.Count
                                ? batViewList[num]
                                : new FlexItem());

                            num++;
                        }

                        Add(row);
                    }
                }
            }

            class EnergyStorage : Page
            {
                EnergyManager _energy;

                public EnergyStorage(EnergyManager energy) : base("STORAGE")
                {
                    _energy = energy;

                    _energy._onSorting += () =>
                    {
                        //RemoveAll();
                        Build();
                    };
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
                        var row = new FlexItem();
                        for (var j = 0; j < rowLen; j++)
                        {
                            row.Add(num < batViewList.Count
                                ? batViewList[num]
                                : new FlexItem());

                            num++;
                        }

                        Add(row);
                    }
                }
            }
        }

    }
}