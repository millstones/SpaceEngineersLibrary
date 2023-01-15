using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    class CargoManager : Module, ISEWPFContent
    {
        //public override Func<IMyTerminalBlock, bool> BlockFilter => b => b.GetType() == typeof(IMyCargoContainer);
        List<IMyCargoContainer> _cargos;

        public Page Page { get; }


        public override void Awake(IEnumerable<IMyTerminalBlock> blocks)
        {
            _cargos = blocks.OfType<IMyCargoContainer>().ToList();

            /*
            _page = new ContentPage[]
            {
                new CargoManagerPage("Cargo manager", this),
                new CargoManagerDetailPage("Cargo manager detail", this),
            };
            */
        }

        public override void Tick(double dt, IEnumerable<IMyTerminalBlock> blocks)
        {
            _cargos = blocks.OfType<IMyCargoContainer>().ToList();
        }

/*
        class CargoManagerPage : ContentPage
        {
            CargoManager _cargoManager;
            ReactiveProperty<double> _cargoAll;

            public override ReactiveProperty<NoteLevel> Note { get; }

            public CargoManagerPage(string nameId, CargoManager cargoManager) : base(nameId)
            {
                Vertical = true;
                
                _cargoManager = cargoManager;
                _cargoAll = new ReactiveProperty<double>(_cargoManager._cargos.InventoryPercent);
                
                Note = new ReactiveProperty<NoteLevel>(GetNoteLevel);

                var pbCargoWithText = new ContentPanel()
                        .Add(new ContentProgressBar(_cargoAll, c => c.SwitchPage("Cargo manager detail") ,  sizeNumber: 6))
                    ;

                Add(pbCargoWithText);
            }

            NoteLevel GetNoteLevel()
            {
                var cargoAmount = _cargoAll.Get();
                if (cargoAmount > 0.1f && cargoAmount < 0.65f) return NoteLevel.None;
                return NoteLevel.Waring;
            }
        }

        class CargoManagerDetailPage : ContentPage
        {
            CargoManager _cargoManager;
            ContentGrid _grid = new ContentGrid(4, false);

            public override ReactiveProperty<NoteLevel> Note => new ReactiveProperty<NoteLevel>(NoteLevel.None);

            public CargoManagerDetailPage(string nameId, CargoManager cargoManager) : base(nameId)
            {
                _cargoManager = cargoManager;
                Add(_grid);
            }

            protected override void PreDraw()
            {
                _grid.ClearChildren();
                //ClearChildren();

                var allItems = _cargoManager._cargos.GetItems();
                var allAmount = allItems.Values.Sum(x => x.RawValue);

                var rowItem = new List<Content>(allItems.Select(pair => new InvItemView(pair, allAmount)).ToList());

                _grid.AddItems(rowItem);
            }
            class InvItemView : ContentPanel
            {
                public InvItemView(KeyValuePair<MyItemType, MyFixedPoint> item, long allAmount)
                {
                    var texture = item.Key.ToString();
                    var name = texture.Split('/').Last();
                    var amount = (double)item.Value.RawValue / allAmount;

                    var header = new ContentPanel(vertical: true)
                            .Add(new ContentText(name))
                        ;
                    var center = new ContentPanel(vertical: true, sizeNumber: 3)
                            .Add(new ContentIconButton(texture, c => { c.ShowMsgBox(name); }, sizeNumber: 3))
                            .Add(new ContentProgressBar(new ReactiveProperty<double>(amount), sizeNumber: 2))
                        ;
                    var futter = new ContentPanel(vertical: true)
                            .Add(new ContentText((0.000001 * item.Value.RawValue).ToStringPostfix(), 3))
                            .Add(new ContentPanel(2))
                        //.Add(new ContentText(item.Value.RawValue.ToString(), sizeNumber: 5))
                        ;

                    Add(header);
                    Add(center);
                    Add(futter);
                }
            }
        }
        */
    }
}