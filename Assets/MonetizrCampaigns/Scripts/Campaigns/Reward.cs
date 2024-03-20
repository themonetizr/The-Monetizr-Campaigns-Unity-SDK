namespace Monetizr.Campaigns
{
    internal partial class ServerCampaign
    {
        [System.Serializable]
        public class Reward
        {
            //reward id
            public string id;

            //title: informative information
            public string title;

            //if this is false, then product is definitely with a price and requires payment it is not a giveaway
            public bool claimable;

            //not all giveaways can be delivered digitally, this would be a real product - this might be removed if it will make problems
            public bool requires_shipping_address;

            //to deliver any kind of giveaways an email is required
            public bool requires_email_address;

            public bool in_game_only;
        }
        
    }

}