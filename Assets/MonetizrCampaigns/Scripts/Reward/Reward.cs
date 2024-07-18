namespace Monetizr.SDK.Campaigns
{
    [System.Serializable]
    public class Reward
    {
        public string id;
        public string title;
        public bool claimable;
        public bool requires_shipping_address;
        public bool requires_email_address;
        public bool in_game_only;
    }

}