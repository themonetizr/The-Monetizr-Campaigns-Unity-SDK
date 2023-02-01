using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Networking;

namespace Monetizr.Campaigns
{
    public class KevelHelper
    {
        [Serializable]
        public class JsonData
        {
            public User user;
            public Decision decisions;
            public CandidateRetrieval candidateRetrieval;
        }

        [Serializable]
        public class User
        {
            public string key;
        }

        [Serializable]
        public class Decision
        {
            public Div1 div1;
        }

        [Serializable]
        public class Div1
        {
            public int adId;
            public int creativeId;
            public int flightId;
            public int campaignId;
            public int advertiserId;
            public int priorityId;
            public string clickUrl;
            public string impressionUrl;
            public Content[] contents;
            public int height;
            public int width;
            public object[] events;
            public int candidatesFoundCount;
        }

        [Serializable]
        public class Content
        {
            public string type;
            public Data data;
            public string body;
            public string customTemplate;
        }

        [Serializable]
        public class Data
        {
            public int height;
            public int width;
            public string externalUrl;
            public string ctMediaUrl;
            public string ctMediaDuration;
        }

        [Serializable]
        public class CandidateRetrieval
        {
            public Div1 div1;
        }


        internal KevelHelper(MonetizrClient monetizrClient)
        {
        }

        internal async Task GetCampaign(List<ServerCampaign> campList)
        {
            string content = "{ \"placements\": [ { \"divName\": \"div1\", \"networkId\": \"11272\", \"siteId\": \"1240593\", \"adTypes\": [16, 3] }]}";

                        
            string uri = $"https://e-11272.adzerk.net/api/v2";

            Debug.Log($"Requesting VAST campaign with url {uri}");

            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
                       

            string res = null;

            using (UnityWebRequest webRequest = UnityWebRequest.Put(uri, content))
            {
                webRequest.method = "POST";
                webRequest.SetRequestHeader("Content-Type", "application/json");

                await webRequest.SendWebRequest();


                res = webRequest.downloadHandler.text;
            }

            //res = "{\"JsonData\":" + res + "}";


            //var dict = Json.Deserialize(res) as JsonData;

            var data = JsonUtility.FromJson<JsonData>(res);
            /*XmlNodeList elemList = xmlDoc.GetElementsByTagName("Creative");
            for (int i = 0; i < elemList.Count; i++)
            {
                Debug.Log($"{i}------{elemList[i].InnerXml}");
            }*/

            /*VAST vastData = null;

            var ser = new XmlSerializer(typeof(VAST));

            using (var reader = new StringReader(res))
            {
                vastData = (VAST)ser.Deserialize(reader);
            }


            //Debug.Log(v.Ad[0].Item.GetType());

            ServerCampaign serverCampaign = await PrepareServerCampaign(vastData);

            if (serverCampaign != null)
                campList.Add(serverCampaign);*/

        }
    }
}
