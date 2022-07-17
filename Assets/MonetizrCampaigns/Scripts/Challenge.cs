using System;
using System.Collections.Generic;

//"id" vs "campaign_id"?
//application_id should be bundle id? but maybe not

//not necessary in asset
//"campaign_id":"8ff82e4b-0d13-46a4-a91c-684c3e0d0e70",
//"brand_id":"d250d29e-8488-4a2f-b0b3-a1e2953ac2c4",
//"application_id":"d10d793a-e937-4622-a79f-68cbc01a97ad",

/*   [{"id":"8ff82e4b-0d13-46a4-a91c-684c3e0d0e70",
       "brand_id":"d250d29e-8488-4a2f-b0b3-a1e2953ac2c4",
       "application_id":"d10d793a-e937-4622-a79f-68cbc01a97ad",
       "title":"Charmin video","content":"<p>Watch video by Charmin to get 2 Energy boosts</p>",
       "country":null,"state":"","city":"","age_from":1,"age_to":130,
       "assets":[{"id":"8e16d2ae-c436-4038-8b47-9884da5a8ffe",
                   "campaign_id":"8ff82e4b-0d13-46a4-a91c-684c3e0d0e70",
                   "brand_id":"d250d29e-8488-4a2f-b0b3-a1e2953ac2c4",
                   "application_id":"d10d793a-e937-4622-a79f-68cbc01a97ad",
                   "title":"Survey",
                   "type":"survey",
                   "url":"https://wss.pollfish.com/link/cfb1a09e-8128-42ce-a313-20ed5de162d4"},
*/

namespace Monetizr.Campaigns
{
    [System.Serializable]
    internal class ServerCampaign
    {
        public string id;
        public string brand_id;
        public string application_id;
        public string title;
        public string content;
        public int progress;
        public int reward;
        public string dar_tag;
        public List<Reward> rewards = new List<Reward>();
        public List<Asset> assets = new List<Asset>();
        [System.NonSerialized] public Dictionary<string, string> additional_params;

        public string GetParam(string p)
        {
            if (!additional_params.ContainsKey(p))
                return "";

            return additional_params[p];
        }

        public int GetIntParam(string p)
        {
            if (!additional_params.ContainsKey(p))
                return 0;

            int result = 0;
            string val = additional_params[p];

            if (!Int32.TryParse(val, out result))
            {
                return 0;
            }

            return result;
        }

        [System.Serializable]
        public class Reward
        {
            //title: informative information
            public string title;

            //if this is false, then product is definitely with a price and requires payment it is not a giveaway
            public bool claimable;

            //not all giveaways can be delivered digitally, this would be a real product - this might be removed if it will make problems
            public bool requires_shipping_address;

            //to deliver any kind of giveaways an email is required
            public bool requires_email_address;
        }

        [System.Serializable]
        public class Asset
        {
            public string id;
            public string type;
            public string title;
            public string url;
        }
    }
}