namespace Monetizr.Campaigns
{
    internal abstract class MonetizrGameParentBase : PanelController
    {
        internal abstract void OnOpenDone(int id);
        internal abstract void OnCloseDone(int id);
    }

}