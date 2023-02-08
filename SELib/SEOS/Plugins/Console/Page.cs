using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    abstract class Page : FreeCanvas
    {
        public string Id;
        public string Title = ConsolePluginSetup.LOGO;

        Text TitleText;
        protected Page(string id)
        {
            Id = id;
            Border = true;
            Background = true;

            TitleText = new Text(() => Title) {Alignment = Alignment.Center, /*Margin = new Vector4(4)*/Border = true};
            AddTitle();
        }

        public override void Clear()
        {
            base.Clear();
           AddTitle();
        }

        void AddTitle()
        {
            Add(TitleText, CreateArea(Vector2.Zero, new Vector2(1, 0.1f)));
        }
    }

    abstract class MsgBoxItem : FreeCanvas, IInteractive
    {
        protected MsgBoxItem()
        {
            Border = true;
            Background = true;
        }

        protected MsgBoxItem(PageItem content)
        {
            Border = true;
            Background = true;

            Add(content);
            //Add(new Link("X", console => console.CloseMessageBox()) {Border = true},
            //    CreateArea(new Vector2(0.95f, 0), new Vector2(1, 0.05f)));
        }

        public void OnSelect(IConsole console)
        {
        }

        public void OnEsc(IConsole console)
        {
            console.CloseMessageBox();
            //PixelPosition = PixelSize = null;
        }

        public void OnInput(IConsole console, Vector3 dir)
        {
        }

        public void OnHoverEnable(bool hover)
        {
        }
        protected IConsole Console;
        public void Show(IConsole console, RectangleF? viewport = null, int closeSec = int.MaxValue)
        {
            Enabled = true;
            Console = console;
            Console.ShowMessageBox(this, viewport, closeSec);
        }
        public void Close()
        {
            Enabled = false;
            Console?.CloseMessageBox();
        }
        
    }
    class MsgBoxItem<T> : MsgBoxItem
    {
        public Action<T> OnClose;

        public MsgBoxItem(PageItem content) : base(content)
        {
            Enabled = false;
        }
        
        public void Close(T result)
        {
            OnClose?.Invoke(result);
            Close();
        }
    }

    class NoteMsgBox : MsgBoxItem
    {
        float[] sArray = {1, 1.1f, 1.25f, 1.4f, 1.4f, 1.1f};
        int i;
        Image _image;
        public NoteMsgBox(NoteLevel level, string msg)
        {

            var title = "INFO";
            var texture = "Arrow";
            Color? c=null;
            var r = 0f;
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (level)
            {
                case NoteLevel.Info:
                    r = 1;
                    break;
                case NoteLevel.Waring:
                    title = "WARING";
                    texture = "Danger";
                    c = Color.Yellow;
                    break;
                case NoteLevel.Error:
                    title = "ERROR !!!";
                    texture = "Cross";
                    c = Color.Red;
                    break;
            }

            _image = new Image(texture) {Rotation = r};
            
            Add(new Text(title) {Border = true, Color = c}, CreateArea(Vector2.Zero, new Vector2(1, 0.1f)));
            Add(_image, CreateArea(new Vector2(0.4f, 0.2f), new Vector2(0.6f, 0.4f)));
            Add(new Text(msg) {Color = c}, CreateArea(new Vector2(0.1f, 0.45f), new Vector2(0.9f)));
            //Add(new Link("X", console => console.CloseMessageBox()) {Border = true},
            //    CreateArea(new Vector2(0.95f, 0), new Vector2(1, 0.05f)));
        }

        protected override void PreDraw()
        {
            i++;
            if (i >= sArray.Length) i = 0;
            _image.ImageScale = sArray[i];
            
            base.PreDraw();
        }
    }
}