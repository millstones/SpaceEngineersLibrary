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
            
            Page = new CargoManagerPage("Cargo manager", this);
        }


        public override void Tick(double dt, IEnumerable<IMyTerminalBlock> blocks)
        {
            _cargos = blocks.OfType<IMyCargoContainer>().ToList();
        }


        class CargoManagerPage : Page
        {
            CargoManager2 _cargoManager;
            ReactiveProperty<double> _cargoAll;

            public CargoManagerPage(string nameId, CargoManager2 cargoManager) : base(nameId)
            {
                _cargoManager = cargoManager;
                _cargoAll = new ReactiveProperty<double>(_cargoManager._cargos.InventoryPercent);
                
                var pbCargoWithText1 = new Text(() => _cargoManager._cargos.InventoryPercent().ToString(), click:
                    console =>
                    {
                        dock.Add(pbCargoWithText2);
                    });
                //var pbCargoWithText2 = new Text(() => DateTime.Now.ToLongTimeString());
                var pbCargoWithText3 = new Text(ConsolePluginSetup.LOGO);

                dock = new SizableRow()
                        .Add(pbCargoWithText1)
                        .Add(pbCargoWithText2)
                        .Add(pbCargoWithText3)
                    ;
                Add(dock, new RectangleF(new Vector2(0.25f, 0.1f), Vector2.One * 0.5f));

                pbCargoWithText1.BackgroundVisible = true;
                pbCargoWithText1.BorderVisible = true;
                
                //Add(pbCargoWithText1, CreateArea(Vector2.Zero, Vector2.One*0.8f)); 
                Add(pbCargoWithText1, CreateArea(new Vector2(0, 0.5f), new Vector2(0.5f, 1))); 
                // Add(pbCargoWithText3, CreateArea(Vector2.One*0.5f, Vector2.One)); 
            }

            Text pbCargoWithText2 => new Text(() => DateTime.Now.ToLongTimeString());
            SizableRow dock;

            NoteLevel GetNoteLevel()
            {
                var cargoAmount = _cargoAll.Get();
                if (cargoAmount > 0.1f && cargoAmount < 0.65f) return NoteLevel.None;
                return NoteLevel.Waring;
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