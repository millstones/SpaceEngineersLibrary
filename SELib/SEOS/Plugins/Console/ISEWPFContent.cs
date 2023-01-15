namespace IngameScript
{
    interface ISEWPFContent 
    {
        Page Page { get; }
    }
    interface IInteractive
    {
        void OnClick(IConsole console);
        void OnHoverEnable(bool hover);
    }
}