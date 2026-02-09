using Monetizr.SDK.Utils;
using UnityEngine;

namespace Monetizr.SDK.Campaigns
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
    }
}