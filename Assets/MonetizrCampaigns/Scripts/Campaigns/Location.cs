using System.Collections.Generic;

namespace Monetizr.SDK
{
    internal partial class ServerCampaign
    {
        [System.Serializable]
        public class Location
        {
            public string country;

            public List<Region> regions = new List<Region>();
        }
        
    }

}