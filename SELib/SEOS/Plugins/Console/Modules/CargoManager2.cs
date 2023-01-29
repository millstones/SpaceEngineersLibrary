using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRageMath;
// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace IngameScript
{
    class CargoManager2 : Module, IPageProvider
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


        class CargoManagerPage : Page
        {
            public PageItem LeftPanel, RightPanel;

            public CargoManagerPage(CargoManager2 cargoManager) : base("Cargo storage manager")
            {
                Title = Id;

                RightPanel = new LinkDownList(0.5f)
                        .Add("Pages list:", console => { console.ShowMessageBox("CLICK"); })
                        .Add("Details", console => { })
                        .Add("Requests", console => { })
                        .Add("Limits", console => { })
                    ;
                var cargo = cargoManager._cargos;
                LeftPanel = new FlexiblePanel<PageItem>(true)
                        .Add(new StackPanel<PageItem>(true)
                            .Add(new InfoLine("TOTAL VOLUME:", () => cargo.TotalVolume(), "m^3"))
                            .Add(new InfoLine("EMPLOYED VOLUME:", () => cargo.EmployedVolume(), "m^3"))
                            .Add(new InfoLine("EMPLOYED MASS:", () => cargo.EmployedMass(), "kg"))
                            .Add(new InfoLine("FREE SPACE:",
                                () => cargo.TotalVolume() - cargo.EmployedVolume(),
                                "m^3")))

                        .Add(new ProgressBar(cargo.EmployedPercent) {Margin = new Vector4(10), Border = false})
                    ;

                Add(LeftPanel, CreateArea(new Vector2(0, 0.1f), new Vector2(0.5f, 1)));
                Add(RightPanel, CreateArea(new Vector2(0.5f, 0.1f), Vector2.One));
            }

            class InfoLine : FlexiblePanel<Text>
            {
                public InfoLine(string txt1, Func<double> val, string txt2)
                {
                    Add(new Text(txt1, 0.5f) {Alignment = Alignment.Left}, 5);
                    Add(new Text(val().ToStringPostfix(), 0.5f) {Border = true, Alignment = Alignment.Center}, 3);
                    Add(new Text(txt2, 0.5f) {Alignment = Alignment.Right}, 2);
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