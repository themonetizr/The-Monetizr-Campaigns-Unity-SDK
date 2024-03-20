namespace Monetizr.SDK.Minigames
{
    internal abstract class MonetizrGameParentBase : PanelController
    {
        internal abstract void OnOpenDone(int id);
        internal abstract void OnCloseDone(int id);
    }

}