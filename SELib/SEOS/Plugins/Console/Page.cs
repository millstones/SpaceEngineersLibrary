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
        
        protected Page(string id)
        {
            Id = id;
            Border = true;
            Background = true;
            
            Add(new Text(() => Title)
                {
                    Alignment = Alignment.Center,
                    //Margin = new Vector4(4),
                    Border = true
                },
                    CreateArea(Vector2.Zero, new Vector2(1, 0.1f)))
                ;
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
    }
    class MsgBoxItem<T> : MsgBoxItem
    {
        public Action<T> OnClose;

        public MsgBoxItem(string msg)
        {
            Enabled = false;
        }

        public MsgBoxItem(PageItem content) : base(content)
        {
            Enabled = false;
        }

        public void Show()
        {
            Enabled = true;
        }

        public void Close(T result)
        {
            Enabled = false;
            OnClose?.Invoke(result);
        }
    }

    class NoteMsgBox : MsgBoxItem
    {
        float s;
        float[] sArray = {1, 1.1f, 1.25f, 1.4f, 1.25f, 1.1f, 1, 0.9f, 0.75f, 0.6f, 0.75f, 0.9f};
        int i;
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
            
            Add(new Text(title) {Border = true, Color = c}, CreateArea(Vector2.Zero, new Vector2(1, 0.1f)));
            Add(new Image(texture){Border = true, Rotation = () => r, Scale = () => s},
                CreateArea(new Vector2(0.4f, 0.2f), new Vector2(0.6f, 0.4f)));
            Add(new Text(msg) {Color = c}, CreateArea(new Vector2(0.1f, 0.45f), new Vector2(0.9f)));
            //Add(new Link("X", console => console.CloseMessageBox()) {Border = true},
            //    CreateArea(new Vector2(0.95f, 0), new Vector2(1, 0.05f)));
        }

        protected override void PreDraw()
        {
            i++;
            if (i >= sArray.Length) i = 0;
            s = sArray[i];
            
            base.PreDraw();
        }
    }
}