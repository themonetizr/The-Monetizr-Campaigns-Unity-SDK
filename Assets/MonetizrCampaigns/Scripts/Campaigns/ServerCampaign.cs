﻿using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Networking;
using Monetizr.SDK.Utils;
using Monetizr.SDK.VAST;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Monetizr.SDK.Campaigns
{
    [System.Serializable]
    public class ServerCampaign
    {
        private static readonly Dictionary<AssetsType, System.Type> AssetsSystemTypes = new Dictionary<AssetsType, System.Type>()
        {
            { AssetsType.BrandLogoSprite, typeof(Sprite) },
            { AssetsType.BrandBannerSprite, typeof(Sprite) },
            { AssetsType.BrandRewardLogoSprite, typeof(Sprite) },
            { AssetsType.BrandRewardBannerSprite, typeof(Sprite) },
            { AssetsType.SurveyURLString, typeof(String) },
            { AssetsType.VideoFilePathString, typeof(String) },
            { AssetsType.BrandTitleString, typeof(String) },
            { AssetsType.TinyTeaserTexture, typeof(Texture2D) },
            { AssetsType.TinyTeaserSprite, typeof(Sprite) },
            { AssetsType.Html5PathString, typeof(String) },
            { AssetsType.TiledBackgroundSprite, typeof(Sprite) },
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
            { AssetsType.LeaderboardBannerSprite, typeof(Sprite) },
        };

        [System.NonSerialized] internal string vastAdParameters = "";
        [System.NonSerialized] internal VastHelper.VastSettings vastSettings = new VastHelper.VastSettings();
        [System.NonSerialized] private Dictionary<AssetsType, object> assetsDict = new Dictionary<AssetsType, object>();
        [System.NonSerialized] public bool isLoaded = true;
        [System.NonSerialized] public string loadingError = "";
        [System.NonSerialized] public SettingsDictionary<string, string> serverSettings = new SettingsDictionary<string, string>();
        [System.NonSerialized] public string openRtbRawResponse = "";

        public string id;
        public string brand_id;
        public string application_id;
        public string device_ip;
        public string title;
        public string content;
        public int progress;
        public int reward;
        public string dar_tag;
        public string panel_key;
        public bool testmode;
        public List<Reward> rewards = new List<Reward>();
        public List<Asset> assets = new List<Asset>();
        public string end_date;
        public string adm;
        public string verifications_vast_node;

        public bool hasMadeEarlyBidRequest = false;
        public CampaignType campaignType = CampaignType.None;
        public float campaignTimeoutStart;

        public ServerCampaign () { }

        public ServerCampaign(string id, string darTag, SettingsDictionary<string,string> defaultServerSettings)
        {
            this.id = id;
            dar_tag = darTag;
            serverSettings = defaultServerSettings;
        }

        public bool HasTimeoutPassed ()
        {
            if (Time.time >= campaignTimeoutStart + 120f) return true;
            return false;
        }

        internal bool TryGetAssetInList(List<string> types, out Asset asset)
        {
            foreach (var t in types)
            {
                if (TryGetAssetInList(t, out asset))
                    return true;
            }

            asset = null;
            return false;
        }

        internal bool TryGetAssetInList(string type, out Asset asset)
        {
            asset = assets.Find(a => a.type == type);
            return asset != null;
        }

        public void RemoveAssetsByTypeFromList(string type)
        {
            assets.RemoveAll((a) => a.type == type);
        }

        internal bool TryGetSpriteAsset(string spriteTitle, out Sprite res)
        { 
            int index = assets.FindIndex(a => a.title == spriteTitle);

            if (index >= 0)
            {
                res = assets[index].spriteAsset;
                return true;
            }

            res = null;
            return false;
        }

        internal void SetAsset<T>(AssetsType t, object asset)
        {
            if (assetsDict.ContainsKey(t))
            {
                MonetizrLogger.PrintWarning($"An item {t} already exist in the campaign {id}");
            }

            MonetizrManager.HoldResource(asset);
            assetsDict[t] = asset;
        }

        internal bool HasAsset(AssetsType t)
        {
            return assetsDict.ContainsKey(t);
        }

        internal bool TryGetAsset<T>(AssetsType t, out T res)
        {
            res = default(T);
            
            if (AssetsSystemTypes[t] != typeof(T))
            {
                MonetizrLogger.PrintError($"AssetsType {t} and {typeof(T)} do not match!");
                return false;
            }

            if (!assetsDict.ContainsKey(t)) return false;
            res = (T)Convert.ChangeType(assetsDict[t], typeof(T));
            return true;
        }
        
        internal T GetAsset<T>(AssetsType t)
        {
            if (AssetsSystemTypes[t] != typeof(T)) throw new ArgumentException($"AssetsType {t} and {typeof(T)} do not match!");
            if (!assetsDict.ContainsKey(t)) return default(T);
            return (T)Convert.ChangeType(assetsDict[t], typeof(T));
        }

        internal string GetCampaignPath(string fname)
        {
            return $"{Application.persistentDataPath}/{this.id}/{fname}";
        }

        internal async Task AssignAssetTextures(Asset asset, AssetsType texture, AssetsType sprite, bool isOptional = false)
        {
            if (asset.url == null || asset.url.Length == 0)
            {
                MonetizrLogger.PrintWarning($"Resource {texture} {sprite} has no url in path!");
                this.isLoaded = false;
                return;
            }

            string path = Application.persistentDataPath + "/" + this.id;

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            string fname = Path.GetFileName(asset.url);
            string fpath = path + "/" + fname;
            asset.localFullPath = fpath;
            byte[] data = null;

            if (!File.Exists(fpath))
            {
                data = await MonetizrHttpClient.DownloadAssetData(asset.url);

                if (data == null)
                {
                    MonetizrLogger.PrintWarning($"Loading {asset.url} failed!");

                    if (!isOptional)
                    {
                        MonetizrLogger.PrintError($"Campaign loading will fail, because asset is required!");

                        this.loadingError = $"Nothing downloaded with a path {asset.url}";
                        this.isLoaded = false;
                    }
                    return;
                }

                File.WriteAllBytes(fpath, data);
            }
            else
            {
                data = File.ReadAllBytes(fpath);
            }

#if TEST_SLOW_LATENCY
            await Task.Delay(1000);
#endif

            Texture2D tex = new Texture2D(0, 0);
            tex.LoadImage(data);
            tex.wrapMode = TextureWrapMode.Clamp;

            if (texture != AssetsType.Unknown) SetAsset<Texture2D>(texture, tex);

            Sprite s = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            asset.spriteAsset = s;
            
            if (sprite != AssetsType.Unknown)
            { 
                SetAsset<Sprite>(sprite, s);
            }
        }

        internal async Task PreloadAssetToCache(Asset asset, AssetsType fileString, bool required = true)
        {
            if (string.IsNullOrEmpty(asset.url))
            {
                MonetizrLogger.PrintWarning($"Malformed URL for {fileString} {this.id}");
                return;
            }

            string path = Application.persistentDataPath + "/" + this.id;
            string fname = string.IsNullOrEmpty(asset.fname) ? Path.GetFileName(asset.url) : $"{asset.fname}.{asset.fext}";
            path = string.IsNullOrEmpty(asset.fpath) ? $"{path}" : $"{path}/{asset.fpath}";
            string fpath = $"{path}/{fname}";
            string zipFolder = null;
            string fileToCheck = fpath;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (fname.Contains("zip"))
            {
                zipFolder = path;
                fileToCheck = zipFolder + "/index.html";

                MonetizrLogger.Print($"archive: {zipFolder} {fileToCheck} {File.Exists(fileToCheck)}");
            }

            byte[] data = null;
            MonetizrLogger.Print($"PreloadAssetToCache: {fname} {fileToCheck}");

            if (!File.Exists(fileToCheck))
            {
                MonetizrLogger.Print($"Downloading archive {asset.url}");
                data = await MonetizrHttpClient.DownloadAssetData(asset.url);

                if (data == null)
                {
                    MonetizrLogger.PrintWarning($"Nothing downloaded with an url {asset.url}!");

                    if (required)
                    {
                        this.isLoaded = false;
                        this.loadingError = $"Nothing downloaded with an url {asset.url}";
                    }
                    return;
                }

                MonetizrLogger.Print($"WriteAllBytes to {fpath} size: {data.Length}");
                File.WriteAllBytes(fpath, data);

                if (zipFolder != null)
                {
                    MonetizrLogger.Print("Extracting to: " + zipFolder);
                    var unzipResult = MonetizrUtils.ExtractAllToDirectory(fpath, zipFolder);
                    File.Delete(fpath);

                    if (!unzipResult)
                    {
                        if (required)
                        {
                            this.isLoaded = false;
                            this.loadingError = $"Zip {fpath} extracting failed!";
                        }
                        return;
                    }
                }
            }

            if (zipFolder != null) fpath = fileToCheck;
            if (!string.IsNullOrEmpty(asset.mainAssetName)) fpath = $"{path}/{asset.mainAssetName}";
            MonetizrLogger.Print($"Resource {fileString} {fpath}");
            asset.localFullPath = fpath;
            SetAsset<string>(fileString, fpath);
        }

        internal async Task LoadCampaignAssets()
        {
            MonetizrLogger.Print($"Campaign path: {Application.persistentDataPath}/{id}");

            foreach (var asset in assets)
            {
                MonetizrLogger.Print($"Loading asset type:{asset.type} title:{asset.title} url:{asset.url}");

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

                    case "leaderboard_banner":
                        await AssignAssetTextures(asset, AssetsType.Unknown, AssetsType.LeaderboardBannerSprite, true);
                        break;

                    case "survey":
                        if (!string.IsNullOrEmpty(asset.survey_content))
                        {
                            SetAsset<string>(AssetsType.SurveyURLString, asset.survey_content);
                        }
                        else
                        {
                            SetAsset<string>(AssetsType.SurveyURLString, asset.url);
                        }
                        break;

                    case "video":
                        asset.fpath = MonetizrUtils.ConvertCreativeToFname(asset.url);
                        asset.fname = "video";
                        asset.fext = MonetizrUtils.ConvertCreativeToExt("", asset.url);
                        asset.mainAssetName = $"index.html";
                        await PreloadAssetToCache(asset, AssetsType.Html5PathString, true);
                        await PreloadVideoPlayer(asset);
                        break;

                    case "text":
                            SetAsset<string>(AssetsType.BrandTitleString, asset.title);
                        break;

                    case "html":
                        asset.fpath = MonetizrUtils.ConvertCreativeToFname(asset.url);
                        asset.fname = "video";
                        asset.fext = MonetizrUtils.ConvertCreativeToExt("", asset.url);
                        asset.mainAssetName = $"index.html";
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

                    case "image":
                        await AssignAssetTextures(asset, AssetsType.Unknown, AssetsType.Unknown, true);
                        break;
                }
            }

            if (!HasTeaserAsset())
            {
                await SetDefaultTeaser();
            }
        }

        internal async Task PreloadVideoPlayerForProgrammatic(Asset asset)
        {
            string videoPlayerURL = MonetizrUtils.GetVideoPlayerURL(this);
            string zipFolder = GetCampaignPath($"{asset.fpath}");
            string indexPath = $"{zipFolder}/index.html";
            MonetizrLogger.Print($"{zipFolder}");
            
            if (!Directory.Exists(zipFolder))
            {
                Directory.CreateDirectory(zipFolder);
            }

            string playerUrl = serverSettings.GetParam("openrtb.player_url", videoPlayerURL);
            byte[] data = await MonetizrHttpClient.DownloadAssetData(playerUrl);

            if (data == null)
            {
                MonetizrLogger.PrintError("Can't download video player for programmatic");
                return;
            }

            File.WriteAllBytes(zipFolder + "/html.zip", data);
            MonetizrUtils.ExtractAllToDirectory(zipFolder + "/html.zip", zipFolder);
            File.Delete(zipFolder + "/html.zip");

            if (!File.Exists(indexPath))
            {
                MonetizrLogger.PrintError($"Main html for video player {indexPath} doesn't exist");
            }
        }

        internal async Task PreloadVideoPlayer(Asset asset)
        {
            string videoPlayerURL = MonetizrUtils.GetVideoPlayerURL(this);
            string campPath = Application.persistentDataPath + "/" + id;
            string zipFolder = campPath + "/" + asset.fpath;
            string indexPath = $"{zipFolder}/index.html";
            MonetizrLogger.Print($"{campPath} {zipFolder}");
            
            if (!Directory.Exists(zipFolder))
            {
                this.isLoaded = false;
                this.loadingError = $"Folder for video player {zipFolder} doesn't exist";
            }

            byte[] data = await MonetizrHttpClient.DownloadAssetData(videoPlayerURL);

            if (data == null)
            {
                this.isLoaded = false;
                this.loadingError = $"Can't download video player";
                return;
            }

            File.WriteAllBytes(zipFolder + "/html.zip", data);
            MonetizrUtils.ExtractAllToDirectory(zipFolder + "/html.zip", zipFolder);
            File.Delete(zipFolder + "/html.zip");
            
            if (!File.Exists(indexPath))
            {
                this.isLoaded = false;
                this.loadingError = $"Main html for video player {indexPath} doesn't exist";
            }
        }

        internal void EmbedVastParametersIntoVideoPlayer(Asset asset)
        {
            string fpath = GetCampaignPath(asset.fpath);
            string videoPath = $"{fpath}/video.mp4";
            string indexPath = $"{fpath}/{asset.mainAssetName}";

            MonetizrLogger.Print("VastAdParameters: " + vastAdParameters);
            MonetizrLogger.Print("OpenRTBResponse: " + openRtbRawResponse);
            MonetizrLogger.Print("Vast Verification Node: " + verifications_vast_node);

            var str = File.ReadAllText(indexPath);
            str = str.Replace("\"${MON_VAST_COMPONENT}\"", $"{vastAdParameters}");

            if (!string.IsNullOrEmpty(openRtbRawResponse))
            {
                openRtbRawResponse = "`" + openRtbRawResponse + "`";
                str = str.Replace("\"${VAST_RESPONSE}\"", openRtbRawResponse);
            }

            if (!string.IsNullOrEmpty(verifications_vast_node))
            {
                verifications_vast_node = "`" + verifications_vast_node + "`";
                str = str.Replace("\"${VAST_VERIFICATIONS}\"", verifications_vast_node);
            }

            MonetizrLogger.Print("Final HTML: " + str);
            if (!File.Exists(videoPath)) str = str.Replace("video.mp4", asset.url);
            File.WriteAllText(indexPath, str);
        }

        internal static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }
        
        internal string DumpsVastSettings(TagsReplacer vastTagsReplacer)
        {
            string res = JsonUtility.ToJson(vastSettings); 
            var campaignSettingsJson = $",\"campaignSettings\":{DumpCampaignSettings(vastTagsReplacer)}";
            res = res.Insert(res.Length - 1, campaignSettingsJson);      
            MonetizrLogger.Print($"VAST Settings: {res}");
            return res;
        }

        internal string DumpCampaignSettings(TagsReplacer tagsReplacer)
        {
            string result = string.Join(",", serverSettings.Select(kvp =>
            {
                var v = tagsReplacer == null ? kvp.Value : tagsReplacer.ReplaceAngularMacros(kvp.Value);
                return $"\"{kvp.Key}\":\"{v}\"";
            }));

            return $"{{{result}}}";
        }

        internal bool IsCampaignActivate()
        {
            if (MonetizrManager.Instance.missionsManager.GetActiveMissionsNum(this) == 0) return false;

            var serverMaxAmount = serverSettings.GetIntParam("amount_of_teasers");
            var currentAmount = MonetizrManager.Instance.localSettings.GetSetting(id).amountTeasersShown;
            bool hasNoTeasers = currentAmount > serverMaxAmount;
            var serverMaxNotificationsAmount = serverSettings.GetIntParam("amount_of_notifications");
            var currentNotificationsAmount = MonetizrManager.Instance.localSettings.GetSetting(id).amountNotificationsShown;
            bool hasNoNotifications = currentNotificationsAmount > serverMaxNotificationsAmount;

            if (hasNoNotifications && hasNoTeasers) return false;
            return true;
        }

        public bool AreConditionsTrue(Dictionary<string, string> mConditions)
        {
            var settings = MonetizrManager.Instance.localSettings.GetSetting(id).settings;
            if (settings == null || settings.dictionary.Count == 0) return false;
            return mConditions.All(c => settings[c.Key] == c.Value);
        }

        internal void PostCampaignLoad()
        {
            if (string.IsNullOrEmpty(content))
            {
                MonetizrLogger.PrintError("CampaignID: " + id + " content is empty.");
                return;
            }

            Dictionary<string, string> cd = MonetizrUtils.ParseContentString(content);
            serverSettings = new SettingsDictionary<string, string>(cd);
            MonetizrLogger.Print("CampaignID: " + id + "\n" + "Parsed Content: " + MonetizrUtils.PrintDictionaryValuesInOneLine(cd));
        }

        private bool HasTeaserAsset ()
        {
            if (!HasAsset(AssetsType.TinyTeaserSprite) && !HasAsset(AssetsType.TeaserGifPathString) && !HasAsset(AssetsType.BrandRewardLogoSprite))
            {
                MonetizrLogger.Print("CampaignID: " + id + " has no teaser texture. Loading default.");
                return false;
            }

            return true;
        }

        public async Task<bool> SetDefaultTeaser ()
        {
            Asset defaultTeaser = new Asset();
            defaultTeaser.type = "tiny_teaser_gif";
            defaultTeaser.url = "https://cdn.monetizr.com/px_track/d957e956-319e-46f6-971d-b6c10d16e449.gif";
            await PreloadAssetToCache(defaultTeaser, AssetsType.TeaserGifPathString, true);
            return true;
        }
        
    }

}