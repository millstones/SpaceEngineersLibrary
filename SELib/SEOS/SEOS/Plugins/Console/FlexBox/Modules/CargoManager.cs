using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    class CargoManager : Module, IPageProvider
    {
        //public override Func<IMyTerminalBlock, bool> BlockFilter => b => b.GetType() == typeof(IMyCargoContainer);
        List<IMyCargoContainer> _cargos;
        public Page Page { get; private set; }

        public override void Awake(IEnumerable<IMyTerminalBlock> blocks)
        {
            _cargos = blocks.OfType<IMyCargoContainer>().ToList();

            Page = new CargoManagerPage(this);
        }


        public override void Tick(double dt, IEnumerable<IMyTerminalBlock> blocks)
        {
            _cargos = blocks.OfType<IMyCargoContainer>().ToList();
        }


        public class CargoManagerPage : Page
        {
            FlexItem _panel = new FlexItem();

            public CargoManagerPage(CargoManager cargoManager) : base("C A R G O")
            {
                var getSet = new ReactiveProperty<string>(() => "", s => ConsolePlugin.ShowMsg(s));
                var rightPanel = new FlexItem{Direction = FlexDirection.Column} //{Scale = () => 0.5f}
                        .Add(new Text("Pages list:"))
                        .Add(new Text("Pages list:"))
                        .Add(new Text("Pages list:"))
                        .Add(new Text("Pages list:"))

                    // .Add(new SwitchString("Details", getSet ))
                    // .Add(new SwitchString("Requests", getSet ))
                    // .Add(new SwitchString("Limits", getSet ))
                    ;
                var cargo = cargoManager._cargos;
                var leftPanel = new FlexItem {Direction = FlexDirection.Column}
                        .Add(new FlexItem() {Direction = FlexDirection.Column} 
                            .Add(new InfoLine("TOTAL VOLUME:", () => cargo.TotalVolume(), "m^3"))
                            // .Add(new InfoLine("EMPLOYED VOLUME:", () => cargo.EmployedVolume(), "m^3"))
                            // .Add(new InfoLine("EMPLOYED MASS:", () => cargo.EmployedMass(), "kg"))
                            // .Add(new InfoLine("FREE SPACE:", () => cargo.TotalVolume() - cargo.EmployedVolume(), "m^3"))
                            )
                        .Add(new ProgressBar(cargo.EmployedPercent) {Vertical = true})
                    ;

                _panel
                    .Add(leftPanel)
                    .Add(rightPanel)
                    ;

                Add(_panel);
            }

            class InfoLine : FlexItem
            {
                public InfoLine(string txt1, Func<double> val, string txt2)
                {
                    //Add(new Text(txt1) {Align = Alignment.CenterLeft}, 5);
                    //Add(new Text(val().ToStringPostfix(false)) {Align = Alignment.Center}, 3);
                    //Add(new Text(txt2) {Align = Alignment.CenterRight}, 2);
                }
            }

        }

        // class CargoManagerDetailPage : ContentPage
        // {
        //     CargoManager2 _cargoManager;
        //     ContentGrid _grid = new ContentGrid(4, false);
        //
        //     public override ReactiveProperty<NoteLevel> Note => new ReactiveProperty<NoteLevel>(NoteLevel.None);
        //
        //     public CargoManagerDetailPage(string nameId, CargoManager2 cargoManager) : base(nameId)
        //     {
        //         _cargoManager = cargoManager;
        //         Add(_grid);
        //     }
        //
        //     protected override void PreDraw()
        //     {
        //         _grid.ClearChildren();
        //         //ClearChildren();
        //
        //         var allItems = _cargoManager._cargos.GetItems();
        //         var allAmount = allItems.Values.Sum(x => x.RawValue);
        //
        //         var rowItem = new List<Content>(allItems.Select(pair => new InvItemView(pair, allAmount)).ToList());
        //
        //         _grid.AddItems(rowItem);
        //     }
        //
        //     class InvItemView : ContentPanel
        //     {
        //         public InvItemView(KeyValuePair<MyItemType, MyFixedPoint> item, long allAmount)
        //         {
        //             var texture = item.Key.ToString();
        //             var name = texture.Split('/').Last();
        //             var amount = (double) item.Value.RawValue / allAmount;
        //
        //             var header = new ContentPanel(vertical: true)
        //                     .Add(new ContentText(name))
        //                 ;
        //             var center = new ContentPanel(vertical: true, sizeNumber: 3)
        //                     .Add(new ContentIconButton(texture, c => { c.ShowMsgBox(name); }, sizeNumber: 3))
        //                     .Add(new ContentProgressBar(new ReactiveProperty<double>(amount), sizeNumber: 2))
        //                 ;
        //             var futter = new ContentPanel(vertical: true)
        //                     .Add(new ContentText((0.000001 * item.Value.RawValue).ToStringPostfix(), 3))
        //                     .Add(new ContentPanel(2))
        //                 //.Add(new ContentText(item.Value.RawValue.ToString(), sizeNumber: 5))
        //                 ;
        //
        //             Add(header);
        //             Add(center);
        //             Add(futter);
        //         }
        //     }
        // }
    }
}