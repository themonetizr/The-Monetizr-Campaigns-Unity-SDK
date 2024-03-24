using Monetizr.SDK.Utils;
using UnityEngine;

namespace Monetizr.SDK.Campaigns
{
    internal partial class ServerCampaign
    {
        [System.Serializable]
        public class Asset
        {
            public string id;
            public string type;
            public string title;
            public string url;
            public string survey_content;
            public string fname;
            public string fext;
            public string fpath;
            public string mainAssetName;
            public string localFullPath;
            
            public Sprite spriteAsset;
            public string mediaType;

            internal Asset() { }

            internal static bool ValidateAssetJson(string json)
            {
                if (string.IsNullOrEmpty(json))
                    return false;

                if (!MonetizrUtils.ValidateJson(json))
                    return false;

                var d = MonetizrUtils.ParseJson(json);

                if (d.Count == 0)
                    return false;

                return d.ContainsKey("id") && d.ContainsKey("type") && d.ContainsKey("title") && d.ContainsKey("url");
            }

            internal Asset(string json, bool isVideo)
            {
                var d = MonetizrUtils.ParseJson(json);

                id = d["id"];
                type = d["type"];
                title = d["title"];
                url = d["url"];
                survey_content = d["survey_content"];

                if(isVideo)
                    InitializeVideoPaths();
            }

            internal void InitializeVideoPaths()
            {
                fpath = MonetizrUtils.ConvertCreativeToFname(url);
                fname = "video";
                fext = MonetizrUtils.ConvertCreativeToExt("", url);
                mainAssetName = $"index.html";
            }

            public Asset Clone()
            {
                return (Asset)this.MemberwiseClone();
            }

            public override string ToString()
            {
                return $"Id: {id}, Type: {type}, Title: {title}, URL: {url}, Survey Content: {survey_content}";
            }
            
        }
        
    }

}