﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UnityEngine;

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
        private static readonly Dictionary<AssetsType, System.Type> AssetsSystemTypes = new Dictionary<AssetsType, System.Type>()
        {
            { AssetsType.BrandLogoSprite, typeof(Sprite) },
            { AssetsType.BrandBannerSprite, typeof(Sprite) },
            { AssetsType.BrandRewardLogoSprite, typeof(Sprite) },
            { AssetsType.BrandRewardBannerSprite, typeof(Sprite) },
            { AssetsType.SurveyURLString, typeof(String) },
            //{ AssetsType.VideoURLString, typeof(String) },
            { AssetsType.VideoFilePathString, typeof(String) },
            { AssetsType.BrandTitleString, typeof(String) },
            { AssetsType.TinyTeaserTexture, typeof(Texture2D) },
            { AssetsType.TinyTeaserSprite, typeof(Sprite) },
            //{ AssetsType.Html5ZipURLString, typeof(String) },
            { AssetsType.Html5PathString, typeof(String) },
            //{ AssetsType.HeaderTextColor, typeof(Color) },
            //{ AssetsType.CampaignTextColor, typeof(Color) },
           // { AssetsType.CampaignHeaderTextColor, typeof(Color) },
            { AssetsType.TiledBackgroundSprite, typeof(Sprite) },
            //{ AssetsType.CampaignBackgroundColor, typeof(Color) },
            { AssetsType.CustomCoinSprite, typeof(Sprite) },
            { AssetsType.CustomCoinString, typeof(String) },
            { AssetsType.LoadingScreenSprite, typeof(Sprite) },
            { AssetsType.TeaserGifPathString, typeof(String) },
            { AssetsType.RewardSprite, typeof(Sprite) },
            { AssetsType.IngameRewardSprite, typeof(Sprite) },
            { AssetsType.UnknownRewardSprite, typeof(Sprite) },
            { AssetsType.MinigameSprite1, typeof(Sprite) },
            { AssetsType.MinigameSprite2, typeof(Sprite) },
            { AssetsType.MinigameSprite3, typeof(Sprite) },

        };

        public string id;
        public string brand_id;
        public string application_id;
        public string title;
        public string content;
        public int progress;
        public int reward;
        public string dar_tag;
        public string panel_key;
        public bool testmode;
        public List<Reward> rewards = new List<Reward>();
        public List<Asset> assets = new List<Asset>();
        public List<Location> locations = new List<Location>();

        [System.NonSerialized]
        private Dictionary<AssetsType, object> assetsDict = new Dictionary<AssetsType, object>();

        [System.NonSerialized]
        public bool isLoaded = true;

        [System.NonSerialized]
        public SettingsDictionary<string, string> serverSettings = new SettingsDictionary<string, string>();

        [System.Serializable]
        public class Location
        {
            public string country;

            public List<Region> regions = new List<Region>();
        }

        [System.Serializable]
        public class Region
        {
            public string region;
        }


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

            public override string ToString()
            {
                return $"Id: {id}, Type: {type}, Title: {title}, URL: {url}, Survey Content: {survey_content}";
            }
        }

        internal void SetAsset<T>(AssetsType t, object asset)
        {
            if (assetsDict.ContainsKey(t))
            {
                Log.PrintWarning($"An item {t} already exist in the campaign {id}");
                return;
            }

            //Log.Print($"Adding asset {asset} into {t}");

            MonetizrManager.HoldResource(asset);

            assetsDict.Add(t, asset);
        }

        internal bool HasAsset(AssetsType t)
        {
            return assetsDict.ContainsKey(t);
        }

        internal T GetAsset<T>(AssetsType t)
        {
            if (AssetsSystemTypes[t] != typeof(T))
                throw new ArgumentException($"AssetsType {t} and {typeof(T)} do not match!");

            if (!assetsDict.ContainsKey(t))
                //throw new ArgumentException($"Requested asset {t} doesn't exist in challenge!");
                return default(T);

            return (T)Convert.ChangeType(assetsDict[t], typeof(T));
        }

        internal Sprite LoadSpriteFromCache(string campaignId, string assetUrl)
        {
            string fname = Path.GetFileName(assetUrl);
            string fpath = Application.persistentDataPath + "/" + campaignId + "/" + fname;

            if (!File.Exists(fpath))
                return null;

            byte[] data = File.ReadAllBytes(fpath);

            Texture2D tex = new Texture2D(0, 0);
            tex.LoadImage(data);
            tex.wrapMode = TextureWrapMode.Clamp;

            return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        }

        /// <summary>
        /// Helper function to download and assign graphics assets
        /// </summary>
        internal async Task AssignAssetTextures(ServerCampaign.Asset asset, AssetsType texture, AssetsType sprite, bool isOptional = false)
        {
            if (asset.url == null || asset.url.Length == 0)
            {
                Debug.LogWarning($"Resource {texture} {sprite} has no url in path!");
                this.isLoaded = false;
                return;
            }

            string path = Application.persistentDataPath + "/" + this.id;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fname = Path.GetFileName(asset.url);
            string fpath = path + "/" + fname;

            asset.localFullPath = fpath;

            //Debug.Log(fpath);

            byte[] data = null;

            if (!File.Exists(fpath))
            {
                data = await DownloadHelper.DownloadAssetData(asset.url);

                if (data == null)
                {
                    if (!isOptional)
                        this.isLoaded = false;

                    return;
                }

                File.WriteAllBytes(fpath, data);

                //Log.Print("saving: " + fpath);
            }
            else
            {
                data = File.ReadAllBytes(fpath);

                if (data == null)
                {
                    if (!isOptional)
                        this.isLoaded = false;

                    return;
                }

                //Log.Print("reading: " + fpath);
            }

#if TEST_SLOW_LATENCY
            await Task.Delay(1000);
#endif

            Texture2D tex = new Texture2D(0, 0);
            tex.LoadImage(data);
            tex.wrapMode = TextureWrapMode.Clamp;

            if (texture != AssetsType.Unknown)
                SetAsset<Texture2D>(texture, tex);

            Sprite s = null;
            if (sprite != AssetsType.Unknown)
            {
                s = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

                SetAsset<Sprite>(sprite, s);
            }

            //campaign.SetAssetUrl(sprite, asset.url);

            bool texStatus = tex != null;
            bool spriteStatus = s != null;

            //Debug.Log($"Adding texture:{texture}={texStatus} sprite:{sprite}={spriteStatus} into:{ech.campaign.id}");
        }

        internal async Task PreloadAssetToCache(ServerCampaign.Asset asset, /*AssetsType urlString,*/ AssetsType fileString, bool required = true)
        {
            if (asset.url == null || asset.url.Length == 0)
            {
                Debug.LogWarning($"Malformed URL for {fileString} {this.id}");
                return;
            }

            string path = Application.persistentDataPath + "/" + this.id;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fname = string.IsNullOrEmpty(asset.fname) ? Path.GetFileName(asset.url) : $"{asset.fname}.{asset.fext}";
            string fpath = string.IsNullOrEmpty(asset.fpath) ? $"{path}/{fname}" : $"{path}/{asset.fpath}/{fname}";
            string zipFolder = null;
            string fileToCheck = fpath;

            Log.Print("PreloadAssetToCache: " + fname);

            if (fname.Contains("zip"))
            {
                zipFolder = path + "/" + fname.Replace(".zip", "");
                fileToCheck = zipFolder + "/index.html";

                Log.Print($"archive: {zipFolder} {fileToCheck} {File.Exists(fileToCheck)}");
            }

            byte[] data = null;

            if (!File.Exists(fileToCheck))
            {
                Log.Print($"Downloading archive {asset.url}");

                data = await DownloadHelper.DownloadAssetData(asset.url);

                if (data == null)
                {
                    Log.Print("Nothing downloaded! Data == null");

                    if (required)
                        this.isLoaded = false;

                    return;
                }

                Log.Print($"WriteAllBytes to {fpath} size: {data.Length}");

                File.WriteAllBytes(fpath, data);

                if (zipFolder != null)
                {
                    Log.Print("Extracting to: " + zipFolder);

                    if (Directory.Exists(zipFolder))
                        DeleteDirectory(zipFolder);

                    //if (!Directory.Exists(zipFolder))
                    Directory.CreateDirectory(zipFolder);

                    ZipFile.ExtractToDirectory(fpath, zipFolder);

                    File.Delete(fpath);
                }


                //Log.Print("saving: " + fpath);
            }

            if (zipFolder != null)
                fpath = fileToCheck;

            if (!string.IsNullOrEmpty(asset.mainAssetName))
            {
                fpath = $"{path}/{asset.mainAssetName}";
            }

            Log.Print($"Resource {fileString} {fpath}");

            //Log.Print("zip path to: " + fpath);

            asset.localFullPath = fpath;

            //ech.SetAsset<string>(urlString, asset.url);
            SetAsset<string>(fileString, fpath);
        }

        internal async Task LoadCampaignAssets()
        {
            foreach (var asset in assets)
            {
                Debug.Log($"Loading {asset.type}");

                switch (asset.type)
                {
                    case "icon":
                        await AssignAssetTextures(asset, AssetsType.Unknown, AssetsType.BrandLogoSprite);

                        break;
                    case "banner":
                        await AssignAssetTextures(asset, AssetsType.Unknown, AssetsType.BrandBannerSprite);

                        break;
                    case "logo":
                        await AssignAssetTextures(asset, AssetsType.Unknown, AssetsType.BrandRewardLogoSprite);

                        break;
                    case "reward_banner":
                        await AssignAssetTextures(asset, AssetsType.Unknown, AssetsType.BrandRewardBannerSprite);

                        break;

                    case "tiny_teaser":
                        await AssignAssetTextures(asset, AssetsType.TinyTeaserTexture, AssetsType.TinyTeaserSprite);

                        break;

                    case "custom_coin_icon":
                        await AssignAssetTextures(asset, AssetsType.Unknown, AssetsType.CustomCoinSprite, true);

                        break;

                    case "loading_screen":

                        await AssignAssetTextures(asset, AssetsType.Unknown, AssetsType.LoadingScreenSprite, true);

                        break;

                    case "reward_image":

                        await AssignAssetTextures(asset, AssetsType.Unknown, AssetsType.RewardSprite, true);

                        break;

                    case "ingame_reward_image":

                        await AssignAssetTextures(asset, AssetsType.Unknown, AssetsType.IngameRewardSprite, true);

                        break;

                    case "unknown_reward_image":

                        await AssignAssetTextures(asset, AssetsType.Unknown, AssetsType.UnknownRewardSprite, true);

                        break;


                    case "minigame_asset1":

                        await AssignAssetTextures(asset, AssetsType.Unknown, AssetsType.MinigameSprite1, true);

                        break;

                    case "minigame_asset2":

                        await AssignAssetTextures(asset, AssetsType.Unknown, AssetsType.MinigameSprite2, true);

                        break;

                    case "minigame_asset3":

                        await AssignAssetTextures(asset, AssetsType.Unknown, AssetsType.MinigameSprite3, true);

                        break;

                    //------------------

                    case "survey":

                        if (!string.IsNullOrEmpty(asset.survey_content))
                            SetAsset<string>(AssetsType.SurveyURLString, asset.survey_content);
                        else
                            SetAsset<string>(AssetsType.SurveyURLString, asset.url);

                        break;
                    case "video":
                        await PreloadAssetToCache(asset, AssetsType.VideoFilePathString, true);

                        break;

                    case "text":
                            SetAsset<string>(AssetsType.BrandTitleString, asset.title);

                        break;

                    case "html":
                        await PreloadAssetToCache(asset, AssetsType.Html5PathString, true);

                        break;

                    case "tiny_teaser_gif":
                        await PreloadAssetToCache(asset, AssetsType.TeaserGifPathString, true);

                        break;


                    case "tiled_background":
                        await AssignAssetTextures(asset, AssetsType.Unknown, AssetsType.TiledBackgroundSprite, true);

                        break;

                    case "custom_coin_title":
                        SetAsset<string>(AssetsType.CustomCoinString, asset.title);

                        break;



                }

            }
        }

        internal static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                //File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }



        internal bool IsCampaignInsideLocation(IpApiData locData)
        {
            //no location data
            if (locData == null)
                return true;

            //whole world
            if (locations.Count == 0)
                return true;

            var country = locations.Find(l => l.country == locData.country_code);

            //outside desired country
            if (country == null)
                return false;

            //no regions defined
            if (country.regions == null)
                return true;

            if (country.regions.Count == 0)
                return true;

            var region = country.regions.Find(r => r.region == locData.region_code);

            //region found
            return region != null;
        }
    }
}