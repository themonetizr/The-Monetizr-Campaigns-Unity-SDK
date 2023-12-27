using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Monetizr.Campaigns
{
    //TODO: This class must be improved
    internal class MissionsManager
    {
        internal List<Mission> missions => serializedMissions.GetMissions();

        private MissionsSerializeManager serializedMissions = new MissionsSerializeManager();


        internal void CleanRewardsClaims()
        {
            serializedMissions.Reset();
        }

        internal void SaveClaimedReward(Mission m)
        {
            //serializedMissions.SaveClaimed(m);
        }

        internal void Save(Mission m)
        {
            //serializedMissions.SaveClaimed(m);
        }

        internal void SaveAll()
        {
            serializedMissions.SaveAll();
        }

        internal void CleanUp()
        {
            CleanRewardsClaims();

            //missions.Clear();

            //DestroyTinyMenuTeaser();
        }

        internal void CleanUserDefinedMissions()
        {
            //if (missionsDescriptions == null)
            //    missionsDescriptions = new List<MissionUIDescription>();

            missions.RemoveAll((e) => { return e.isSponsored == false; });
        }


        public void AddMission(Mission m)
        {
            missions.Add(m);
        }

        internal Mission FindMissionInCache(int id, MissionType mt, string ch, ulong reward)
        {
            foreach (var m in missions)
            {
                if (m.type == mt &&
                    m.campaignId == ch &&
                    m.id == id &&
                    m.apiKey == MonetizrManager.Instance.GetCurrentAPIkey() &&
                    m.reward == reward)
                    return m;
            }

            return null;
        }

        internal bool CheckFullCampaignClaim(Mission _m)
        {
            foreach (var m in missions)
            {
                if (m.campaignId == _m.campaignId && m.isClaimed != ClaimState.Claimed)
                    return false;
            }

            return true;
        }

        Mission prepareVideoMission(MissionType mt, ServerCampaign campaign)
        {
            bool hasSomething = campaign.HasAsset(AssetsType.Html5PathString) ||
                                campaign.HasAsset(AssetsType.VideoFilePathString);
            

            if (!hasSomething && !campaign.serverSettings.GetBoolParam("programmatic",false))
            {
                Log.PrintWarning($"campaign {campaign} has no video asset!");
                return null;
            }
            
            return new Mission()
            {
                //rewardType = RewardType.Coins,
                type = MissionType.VideoReward,
                //reward = reward,
                progress = 1.0f,
                isDisabled = false,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
                hasVideo = true,
                isRewardIngame = true,
            };
        }

        Mission prepareDoubleMission(MissionType mt, ServerCampaign campaign)
        {
            RewardType rt = RewardType.Coins;

            return new Mission()
            {
                //rewardType = rt,
                startMoney = MonetizrManager.gameRewards[rt].GetCurrencyFunc(),
                type = MissionType.MutiplyReward,
                //reward = reward,
                progress = 0.0f,
                isDisabled = false,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
                hasVideo = false,
                isRewardIngame = true,
            };
        }

        Mission PrepareSurveyMission(MissionType mt, ServerCampaign campaign)
        {
            string url = campaign.GetAsset<string>(AssetsType.SurveyURLString);

            //if (string.IsNullOrEmpty(url))
            //    return null;

            return new Mission()
            {
                //rewardType = RewardType.Coins,
                type = mt,
                //reward = reward,
                isDisabled = false, //survey is disabled from start
                surveyUrl = url,
                //delaySurveyTimeSec = 30,//86400,
                progress = 0.0f,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
                hasVideo = false,
                isRewardIngame = true,
            };
        }

        Mission PrepareGiveawayMission(MissionType mt, ServerCampaign campaign)
        {
            //if no claimable reward in campaign - no give away missions
            var claimableReward = campaign.rewards.Find((ServerCampaign.Reward obj) => { return obj.claimable == true; });

            if (claimableReward == null)
                return null;

            RewardType rt = RewardType.Coins;

            return new Mission()
            {
                //rewardType = rt,
                startMoney = MonetizrManager.gameRewards[rt].GetCurrencyFunc(),
                type = mt,
                //reward = reward,
                progress = 0.0f,
                isDisabled = false,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
                hasVideo = false,
                isRewardIngame = false,
            };
        }

        Mission prepareVideoGiveawayMission(MissionType mt, ServerCampaign campaign)
        {
            bool hasHtml = campaign.HasAsset(AssetsType.Html5PathString);
            bool hasVideo = campaign.HasAsset(AssetsType.VideoFilePathString);

            bool video = !(campaign.serverSettings.GetParam("email_giveaway_mission_without_video") == "true");

            if (video && !hasHtml && !hasVideo)
            {
                return null;
            }

            //if no claimable reward in campaign - no give away missions
            //var claimableReward = MonetizrManager.Instance.GetCampaign(campaign).rewards.Find((ServerCampaign.Reward obj) => { return obj.claimable == true; });

            //if (claimableReward == null)
            //    return null;



            RewardType rt = RewardType.Coins;

            return new Mission()
            {
                //rewardType = rt,
                startMoney = MonetizrManager.gameRewards[rt].GetCurrencyFunc(),
                type = mt,
                //reward = reward,
                progress = 1.0f,
                isDisabled = false,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
                hasVideo = video,
                isRewardIngame = false,
            };
        }

        Mission prepareMission(MissionType mt, ServerCampaign campaign, bool hasVideo, bool isRewardInGame)
        {
            return new Mission()
            {
                type = mt,
                progress = 1.0f,
                isDisabled = false,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
                hasVideo = hasVideo,
                isRewardIngame = isRewardInGame,
            };
        }

        //TODO: make separate classes for each mission type
        Mission prepareNewMission(int id, ServerCampaign campaign, MissionDescription md)
        {
            MissionType mt = md.missionType;
            Mission m = null;

            switch (mt)
            {
                case MissionType.MutiplyReward: m = prepareDoubleMission(mt, campaign); break;
                case MissionType.VideoReward: m = prepareVideoMission(mt, campaign); break;
                case MissionType.SurveyReward: m = PrepareSurveyMission(mt, campaign); break;
                //case MissionType.TwitterReward: m = prepareTwitterMission(mt, campaign); break;
                // case MissionType.GiveawayWithMail: m = prepareGiveawayMission(mt, campaign, reward); break;
                case MissionType.VideoWithEmailGiveaway: m = prepareVideoGiveawayMission(mt, campaign); break;
                case MissionType.MinigameReward:
                case MissionType.MemoryMinigameReward:
                case MissionType.ActionReward:
                case MissionType.CodeReward:
                    m = prepareMission(mt, campaign, false, true); break;

            }

            if (m == null)
                return null;


            m.rewardType = md.rewardCurrency;
            m.startMoney = MonetizrManager.gameRewards[m.rewardType].GetCurrencyFunc();
            m.reward = md.reward;
            m.state = MissionUIState.Visible;
            m.id = id;
            m.isSponsored = true;
            m.isClaimed = ClaimState.NotClaimed;
            m.campaignId = campaign.id;
            m.apiKey = MonetizrManager.Instance.GetCurrentAPIkey();

            m.sdkVersion = MonetizrManager.SDKVersion;

            //if(string.IsNullOrEmpty(m.surveyUrl))

            if (!md.hasUnitySurvey)
                m.surveyUrl = md.surveyUrl;

            m.surveyId = md.surveyId;

            m.serverId = md.id;
            m.rewardPercent = md.rewardPercent;
            m.autoStartAfter = md.autoStartAfter;
            m.alwaysHiddenInRC = md.alwaysHiddenInRC;
            m.openRtbRequestForProgrammatic = md.openRtbRequestForProgrammatic;


            return m;
        }



        internal Action ClaimAction(Mission m, Action<bool> __onComplete, Action updateUIDelegate)
        {
            var onComplete = __onComplete;

            var nextMission = missions.Find(_m => m.serverId == _m.autoStartAfter);

            if (nextMission != null && nextMission != m)
            {
                //Log.Print($"======{nextMission.serverId}");

                onComplete = (bool skipped) =>
                {

                    //Log.Print($"======{nextMission.serverId} {skipped}");

                    //launch previous on complete
                    if (!skipped)
                    {
                        ClaimAction(nextMission, __onComplete, updateUIDelegate).Invoke();
                    }
                };
            }

            switch (m.type)
            {
                case MissionType.SurveyReward: return SurveyClaimAction(m, onComplete, updateUIDelegate);
                case MissionType.MinigameReward:
                case MissionType.MemoryMinigameReward: return MinigameClaimAction(m, onComplete, updateUIDelegate);
                case MissionType.VideoWithEmailGiveaway: return GetEmailGiveawayClaimAction(m, onComplete, updateUIDelegate);
                case MissionType.VideoReward: return VideoClaimAction(m, onComplete, updateUIDelegate);
                case MissionType.MutiplyReward: return MutiplyRewardClaimAction(m, onComplete, updateUIDelegate);
                case MissionType.ActionReward: return ActionRewardClaimAction(m, onComplete, updateUIDelegate);
                case MissionType.CodeReward: return CodeRewardClaimAction(m, onComplete, updateUIDelegate);
            }

            return null;
        }

        internal Action MutiplyRewardClaimAction(Mission m, Action<bool> onComplete, Action updateUIDelegate)
        {
            return () =>
            {
                MonetizrManager.Instance.OnClaimRewardComplete(m, false, onComplete, updateUIDelegate);

                //MonetizrManager.ShowSurvey(onSurveyComplete, m);

                //MonetizrManager.ShowMinigame(onMinigameComplete, PanelId.CarMemoryGame, m);

                //MonetizrManager.ShowMinigame(onMinigameComplete, m);
                //OnVideoPlayPress(m, onVideoComplete);

            };
        }

        internal Action VideoClaimAction(Mission m, Action<bool> onComplete, Action updateUIDelegate)
        {
            Action<bool> onVideoComplete = (bool isSkipped) =>
            {
                MonetizrManager.Instance.OnClaimRewardComplete(m, isSkipped, onComplete, updateUIDelegate);
            };

            return () =>
            {
                //MonetizrManager.ShowSurvey(onSurveyComplete, m);

                //MonetizrManager.ShowMinigame(onMinigameComplete, PanelId.CarMemoryGame, m);

                //MonetizrManager.ShowMinigame(onMinigameComplete, m);
                OnVideoPlayPress(m, onVideoComplete);

            };
        }

        internal Action MinigameClaimAction(Mission m, Action<bool> onComplete, Action updateUIDelegate)
        {
            Action<bool> onMinigameComplete = (bool isSkipped) =>
            {
                MonetizrManager.Instance.OnClaimRewardComplete(m, isSkipped, onComplete, updateUIDelegate);
            };

            return () =>
            {
                //MonetizrManager.ShowSurvey(onSurveyComplete, m);

                //MonetizrManager.ShowMinigame(onMinigameComplete, PanelId.CarMemoryGame, m);

                MonetizrManager.ShowMinigame(onMinigameComplete, m);

            };
        }

        internal Action ActionRewardClaimAction(Mission m, Action<bool> onComplete, Action updateUIDelegate)
        {
            Action<bool> onActionComplete = (bool isSkipped) =>
            {
                MonetizrManager.Instance.OnClaimRewardComplete(m, isSkipped, onComplete, updateUIDelegate);
            };

            return () =>
            {
                MonetizrManager.ShowActionView(onActionComplete, m);
            };
            //#endif
        }

        internal Action CodeRewardClaimAction(Mission m, Action<bool> onComplete, Action updateUIDelegate)
        {
            Action<bool> onCodeComplete = (bool isSkipped) =>
            {
                MonetizrManager.Instance.OnClaimRewardComplete(m, isSkipped, onComplete, updateUIDelegate);
            };

            return () =>
            {
                MonetizrManager.ShowCodeView(onCodeComplete, m);
            };
            //#endif
        }


        internal Action SurveyClaimAction(Mission m, Action<bool> onComplete, Action updateUIDelegate)
        {
            Action<bool> onSurveyComplete = (bool isSkipped) =>
            {
                MonetizrManager.Instance.OnClaimRewardComplete(m, isSkipped, onComplete, updateUIDelegate);
            };

            //#if UNITY_EDITOR_WIN
            //            return () => onSurveyComplete.Invoke(false);
            //#else

            return () =>
            {
                if (!m.hasUnitySurvey)
#if UNITY_EDITOR_WIN
                {
                    Log.Print($"Completing survey {m.surveyUrl} on windows");
                    onSurveyComplete.Invoke(false);
                }
#else
                    MonetizrManager.ShowSurvey(onSurveyComplete, m);
#endif
                else
                    MonetizrManager.ShowUnitySurvey(onSurveyComplete, m);

                /*MonetizrManager.ShowNotification((bool isSkipped) => { if(!isSkipped) MonetizrManager.ShowSurvey(onSurveyComplete, m); },
                           m,
                           PanelId.SurveyNotification);*/
            };
            //#endif
        }

        internal Action GetEmailGiveawayClaimAction(Mission m, Action<bool> onComplete, Action updateUIDelegate)
        {
            MonetizrManager.temporaryRewardTypeSelection = MonetizrManager.RewardSelectionType.Product;

            bool needToPlayVideo = m.hasVideo;

#if UNITY_EDITOR_WIN
            needToPlayVideo = false;
#endif

            if (m.isVideoShown)
                needToPlayVideo = false;

            /*Action<bool> _onComplete = (bool isSkipped) =>
            {
                //OnClaimRewardComplete(m, isSkipped, AddNewUIMissions);
                //MonetizrManager.Instance.OnClaimRewardComplete(m, isSkipped, updateUIDelegate);
            };*/

            //MonetizrManager.analytics.TrackEvent("Claim pressed", m);

            Action<bool> onVideoComplete = (bool isVideoSkipped) =>
            {
                /*OnClaimRewardComplete(m, false);*/

                if (MonetizrManager.claimForSkippedCampaigns)
                    isVideoSkipped = false;

                if (isVideoSkipped)
                {
                    //MonetizrManager.analytics.TrackEvent("Video skipped", m);

                    onComplete?.Invoke(isVideoSkipped);
                    return;
                }

                if (m.campaignServerSettings.GetParam("watch_video_only_once") == "true")
                    m.isVideoShown = true;

                MonetizrManager.ShowEnterEmailPanel(
                    (bool isMailSkipped) =>
                    {
                        if (isMailSkipped)
                        {
                            //MonetizrManager.analytics.TrackEvent("Email enter skipped", m);

                            MonetizrManager.Analytics.TrackEvent(m, m.adPlacement, MonetizrManager.EventType.ButtonPressSkip);

                            onComplete?.Invoke(isMailSkipped);
                            return;
                        }

                        MonetizrManager.WaitForEndRequestAndNotify(onComplete, m, updateUIDelegate);

                    },
                    m,
                    PanelId.GiveawayEmailEnterNotification);

            };

            //show video, then claim rewards if it's completed
            return () =>
            {
                //Log.Print($"----- {needToPlayVideo}");

                if (needToPlayVideo)
                {
                    OnVideoPlayPress(m, onVideoComplete);
                }
                else
                {
                    onVideoComplete(false);
                }

            };


        }

        internal void OnVideoPlayPress(Mission m, Action<bool> onComplete)
        {

            /*#if UNITY_EDITOR_WIN
                        onComplete?.Invoke(false);
                        return;
            #endif*/

            //var htmlPath = m.campaign.GetAsset<string>(AssetsType.Html5PathString);

            //if (htmlPath != null)
            //{
            MonetizrManager.ShowHTML5((bool isSkipped) => { onComplete(isSkipped); }, m);
            //}
            //else
            //{
            //    Log.PrintV("No HTML5 path for the video player");
            //}
        }

        internal void UpdateMissionsRewards(RewardType rt, MonetizrManager.GameReward reward)
        {
            foreach (var m in missions)
            {
                if (m.rewardType == rt)
                {
                    m.reward = (ulong)(reward.maximumAmount * m.rewardPercent);
                }
            }
        }

        internal static Sprite GetMissionRewardImage(Mission m)
        {

            if (!string.IsNullOrEmpty(m.rewardAssetName))
            {
                if (m.campaign.TryGetSpriteAsset(m.rewardAssetName, out var s))
                {
                    //Log.Print($"GetMissionRewardImage: {m.id} {m.rewardAssetName}");
                    return s;
                }
            }

            Sprite rewardIcon = MonetizrManager.gameRewards[m.rewardType].icon;

            if (m.rewardType == RewardType.Coins &&
                m.campaign.TryGetAsset<Sprite>(AssetsType.CustomCoinSprite, out var customCoin))
            {
                rewardIcon = customCoin;
            }

            if (EnterEmailPanel.GetPanelType(m) == EnterEmailType.IngameReward &&
                m.campaign.TryGetAsset<Sprite>(AssetsType.IngameRewardSprite, out var inGameRewardSprite))
            {
                rewardIcon = inGameRewardSprite;
            }

            if (EnterEmailPanel.GetPanelType(m) == EnterEmailType.ProductReward &&
                m.campaign.TryGetAsset<Sprite>(AssetsType.RewardSprite, out var productRewardSprite))
            {
                rewardIcon = productRewardSprite;
            }

            if (EnterEmailPanel.GetPanelType(m) == EnterEmailType.SelectionReward &&
                m.campaign.TryGetAsset<Sprite>(AssetsType.UnknownRewardSprite, out var selectionRewardSprite))
            {
                rewardIcon = selectionRewardSprite;
            }

            return rewardIcon;
        }

        internal Mission GetFirstUnactiveMission()
        {
            foreach (var m in missions)
            {
                if (!m.isServerCampaignActive)
                    return m;
            }

            return null;
        }

        [Serializable]
        internal class ServerMissionsHelper
        {
            public List<ServerDefinedMissions> missions = new List<ServerDefinedMissions>();
            
            internal List<MissionDescription> CreateMissionDescriptions(List<MissionDescription> originalList, SettingsDictionary<string, string> serverSettings)
            {
                List<MissionDescription> m = new List<MissionDescription>();

                //Array.ForEach(missions, (ServerDefinedMissions _m) =>
                foreach(var _m in missions)
                {

                    MissionType serverMissionType = _m.GetMissionType();

                    if (serverMissionType == MissionType.Undefined)
                        continue;

                    float rewardAmount = _m.GetRewardAmount() / 100.0f;
                    RewardType currency = _m.GetRewardType();

                    MonetizrManager.GameReward gr = MonetizrManager.GetGameReward(currency);

                    //no such reward
                    if (gr == null)
                    {
                        if (serverSettings.GetBoolParam("use_default_reward", true))
                        {
                            currency = RewardType.Coins;
                            gr = MonetizrManager.GetGameReward(currency);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    //award is too much
                    if (rewardAmount > 100.0f)
                        continue;

                    ulong rewardAmount2 = (ulong)Math.Ceiling(gr.maximumAmount * rewardAmount);
                    //}

                    //activateAfter = _m.GetActivateRange();

                    //string surveyUrl = serverSettings.GetParam(_m.survey);

                    Log.PrintV($"CreateMissionDescriptions:max:{gr.maximumAmount}:real:{rewardAmount2}:percent:{rewardAmount}");

                    m.Add(new MissionDescription
                    {
                        missionType = _m.GetMissionType(),
                        reward = rewardAmount2,
                        rewardCurrency = currency,
                        activateAfter = _m.GetActivateRange(),
                        surveyUrl = serverSettings.GetParam(_m.survey),
                        surveyId = string.IsNullOrEmpty(_m.surveyUnity) ? _m.survey : _m.surveyUnity,
                        hasUnitySurvey = !string.IsNullOrEmpty(_m.surveyUnity),
                        rewardPercent = rewardAmount,
                        id = _m.getId(),
                        alwaysHiddenInRC = _m.IsAlwaysHiddenInRC(),
                        autoStartAfter = _m.GetAutoStartId(),
                        rewardImage = _m.reward_image,
                        activateConditions = _m.activate_conditions,
                        openRtbRequestForProgrammatic = _m.ortb_request
                    }); ;

                }

                return m;
            }

            public static ServerMissionsHelper CreateFromJson(string json)
            {
                var result = new ServerMissionsHelper();

                if (!Utils.ValidateJson(json))
                    return result;

                //JsonUtility doesn't work with escaped jsons
                json = Utils.UnescapeJson(json);

                if(json.Contains("\'"))
                    json = json.Replace('\'', '\"');

                try
                {
                    result = JsonUtility.FromJson<ServerMissionsHelper>(json);
                }
                catch (Exception)
                {
                    throw;
                }

                return result;
            }
        }

        [Serializable]
        public class ServerDefinedMissions
        {
            //MissionType
            public string type;

            //0...100 integer
            public string percent_amount;

            //RewardType
            public string currency;

            //Range N-M or single number N
            public string activate_after;

            //Survey link
            public string survey;

            //Survey link
            public string surveyUnity;

            //Server id
            public string id;

            //Always hidden in reward center
            public string hidden_in_rc;

            //Auto start after completing 
            public string auto_start_after;

            public string reward_image;

            public string activate_conditions;
            
            public string ortb_request;

            public int GetAutoStartId()
            {
                int res = -1;

                if (auto_start_after == null)
                    return res;

                if (int.TryParse(auto_start_after, out res))
                    return res;

                return res;
            }

            public bool IsAlwaysHiddenInRC()
            {
                bool res = false;

                if (bool.TryParse(hidden_in_rc, out res))
                    return res;

                return res;
            }

            public int getId()
            {
                int res = -1;

                if (int.TryParse(id, out res))
                    return res;

                return -1;
            }

            public List<int> GetActivateRange()
            {
                List<int> result = new List<int>();

                if (activate_after == null)
                    return result;

                string[] p = activate_after.Split(';');

                Array.ForEach(p, (string s) =>
                {
                    int res = 0;
                    if (int.TryParse(s, out res))
                    {
                        result.Add(res);
                    }
                });

                return result;
            }


            public RewardType GetRewardType()
            {
                RewardType rt;

                if (System.Enum.TryParse<RewardType>(currency, out rt))
                    return rt;

                return RewardType.Coins;
            }

            public float GetRewardAmount()
            {
                float reward = 0;

                if (float.TryParse(percent_amount, out reward))
                    return Mathf.Clamp(reward, 0, 100);

                return 0;
            }

            public MissionType GetMissionType()
            {
                MissionType mt;

                if (System.Enum.TryParse<MissionType>(type, out mt))
                    return mt;

                return MissionType.Undefined;
            }
        }


        internal void CreateMissionsFromCampaign(ServerCampaign campaign)
        {
            var predefinedSponsoredMissions = MonetizrManager.Instance.sponsoredMissions;

            if (campaign == null && predefinedSponsoredMissions == null)
                return;

            string serverMissionsJson = campaign.serverSettings.GetParam("custom_missions");

            Log.PrintV($"Predefined missions from settings: {serverMissionsJson}");

            ServerMissionsHelper ic = null;

            try
            {
                ic = ServerMissionsHelper.CreateFromJson(serverMissionsJson);
            }
            catch (Exception e)
            {
                Log.PrintError($"Exception in CreateMissionsFromCampaign with json {serverMissionsJson}\n{e}");
            }

            if (ic == null)
                return;

            predefinedSponsoredMissions = ic.CreateMissionDescriptions(predefinedSponsoredMissions, campaign.serverSettings);

            //if (prefefinedSponsoredMissions.Count > 1)
            //    prefefinedSponsoredMissions = prefefinedSponsoredMissions.GetRange(serverDefinedMission, 1);

            Log.Print($"Found {predefinedSponsoredMissions.Count} predefined missions in campaign");

            /*serializedMissions.Load();

            //check if campaign is alive for current mission
            foreach (var m in missions)
            {
                m.isServerCampaignActive = MonetizrManager.Instance.HasCampaign(m.campaignId);

                //remove if it will not be in the predefined list
                m.isToBeRemoved = true;
            }*/


            for (int i = 0; i < predefinedSponsoredMissions.Count; i++)
            {
                MissionDescription missionDescription = predefinedSponsoredMissions[i];

                Mission m = FindMissionInCache(i, missionDescription.missionType, campaign.id,
                    missionDescription.reward);

                if (m == null)
                {
                    //
                    m = prepareNewMission(i, campaign, missionDescription);

                    if (m != null)
                    {
                        serializedMissions.Add(m);
                    }
                    else
                    {
                        Log.PrintError($"Can't create campaign with type {missionDescription.missionType}");
                    }
                }
                else
                {
                    Log.PrintV($"Found mission {campaign.id}:{i} in local data");
                }

                if (m == null)
                    continue;

                m.campaign = campaign;
                m.sdkVersion = MonetizrManager.SDKVersion;
                m.rewardAssetName = missionDescription.rewardImage;

                if (!missionDescription.hasUnitySurvey)
                    m.surveyUrl = missionDescription.surveyUrl;
                else
                    m.surveyUrl = m.campaign.GetAsset<string>(AssetsType.SurveyURLString);

                m.hasUnitySurvey = missionDescription.hasUnitySurvey;
                m.surveyId = missionDescription.surveyId;
                m.isServerCampaignActive = true;
                m.isToBeRemoved = false;
                m.campaignServerSettings = m.campaign.serverSettings;
                
                
                m.amountOfRVOffersShown = m.campaignServerSettings.GetIntParam("amount_of_rv_offers", -1);
                //m.amountOfNotificationsShown = m.campaignServerSettings.GetIntParam("amount_of_notifications", -1);
                m.amountOfNotificationsSkipped =
                    m.campaignServerSettings.GetIntParam("startup_skipped_notifications", int.MaxValue - 1);
                ; // int.MaxValue - 1; //first notification is always visible
                m.isVideoShown = false;
                m.isDisabled = true; //disable everything by default, activate them in UpdateMissionsActivity
                m.activateAfter = missionDescription.activateAfter;

                m.brandName = m.campaign.GetAsset<string>(AssetsType.BrandTitleString);

                m.isDeactivatedByCondition = false;

                if (!string.IsNullOrEmpty(missionDescription.activateConditions))
                {
                    m.conditions = Utils.ParseConditionsString(missionDescription.activateConditions);

                    if (!campaign.IsConditionsTrue(m.conditions))
                    {
                        m.isDisabled = true;
                        m.isDeactivatedByCondition = true;
                    }
                }

                m.state = m.isDisabled ? MissionUIState.Hidden : MissionUIState.Visible;

                bool showNotClaimedDisabled =
                    m.campaignServerSettings.GetBoolParam("RewardCenter.show_disabled_missions", true);

                if (showNotClaimedDisabled && m.isDisabled && !m.isDeactivatedByCondition)
                    m.state = MissionUIState.Visible;

            }

            Log.Print($"Loaded {missions.Count} missions from the campaign");

            UpdateMissionsActivity(null);
            
            //serializedMissions.SaveAll();
        }

        internal void LoadSerializedMissions()
        {
            serializedMissions.Load();

            //check if campaign is alive for current mission
            foreach (var m in missions)
            {
                m.isServerCampaignActive = MonetizrManager.Instance.HasCampaign(m.campaignId);

                //remove if it will not be in the predefined list
                m.isToBeRemoved = true;
            }
        }

        internal void SaveAndRemoveUnused()
        {
            //remove if mission in save, but not in the predefined list
            missions.RemoveAll(m => m.isToBeRemoved);

            serializedMissions.SaveAll();
        }

        internal Mission GetMission(string campaignId)
        {
            return missions.Find((Mission m) => m.campaignId == campaignId);
        }

        internal bool IsActiveByTime(Mission m)
        {
            bool r = DateTime.Now >= m.activateTime && DateTime.Now <= m.deactivateTime;
            return r;
        }

        internal List<Mission> GetMissionsForRewardCenter(bool includeDisabled)
        {
            return missions.FindAll((Mission m) =>
            {

                bool disabled = m.isDisabled;

                if (includeDisabled)
                    disabled = false;

                return m.isSponsored &&
                        m.isClaimed != ClaimState.Claimed &&
                        //!m.isDeactivatedByCondition &&
                        !disabled &&
                        IsActiveByTime(m) &&
                        m.isServerCampaignActive &&
                        m.autoStartAfter == -1;

            });
        }

        internal List<Mission> GetMissionsForRewardCenter(ServerCampaign campaign, bool includeDisabled = false)
        {
            if (campaign == null)
                return null;
            
            var res = GetMissionsForRewardCenter(includeDisabled);

            return res?.FindAll((Mission m) => m.campaignId == campaign.id);
        }

        internal int GetActiveMissionsNum(ServerCampaign campaign)
        {
            var res = GetMissionsForRewardCenter(campaign);
       
            return res?.Count ?? 0;
        }

        internal int GetActiveMissionsNum()
        {
            //var mList = missions.FindAll((Mission m) => { return m.isClaimed != ClaimState.Claimed; });

            return GetMissionsForRewardCenter(false).Count;
        }

        //check activateAfter ranges for all missions and activate them if missions in range already active 
        internal bool UpdateMissionsActivity(Mission finishedMission)
        {
            bool isUpdateNeeded = finishedMission != null;

            if (finishedMission != null)
                Log.PrintV($"-----UpdateMissionsActivity for {finishedMission.serverId}");

            foreach (var m in missions)
            {
                if (m == finishedMission)
                    continue;

                if (!m.isDisabled)
                    continue;

                if (m.isClaimed == ClaimState.Claimed)
                    continue;

                bool hasActivateAfter = m.activateAfter.Count > 0;

                Log.PrintV($"-----Updating activity for {m.serverId} has {hasActivateAfter} {m.isDeactivatedByCondition}");

                //check if mission self referenced in activate after
                if (hasActivateAfter && m.activateAfter.FindIndex(_id => _id == m.serverId) > 0)
                {
                    Log.PrintWarning($"Mission id {m.serverId} activate after itself!");
                    hasActivateAfter = false;
                }

                if (m.isDeactivatedByCondition)
                {
                    if (!m.campaign.IsConditionsTrue(m.conditions))
                        continue;

                    isUpdateNeeded = true;
                    m.isDisabled = false;
                    m.state = MissionUIState.ToBeShown;
                    m.isDeactivatedByCondition = false;
                    continue;
                }

                //no activate_after here
                if (!hasActivateAfter)
                {
                    if (m.isDisabled)
                        isUpdateNeeded = true;

                    m.isDisabled = false;
                    m.state = MissionUIState.ToBeShown;
                    continue;
                }

               


                bool shouldBeDisabled = false;

                foreach (var id in m.activateAfter)
                {
                    Mission _m = missions.Find(x => x.serverId == id);

                    if (_m != null && _m.isClaimed != ClaimState.Claimed)
                    {
                        shouldBeDisabled = true;

                        Log.PrintV($"------Mission {id} disabled because {_m.serverId} is not active");
                    }
                }

                //activate if all in range claimed
                if (!shouldBeDisabled)
                {
                    if (m.isDisabled)
                        isUpdateNeeded = true;

                    m.isDisabled = shouldBeDisabled;
                    m.state = MissionUIState.ToBeShown;
                }
            }

            if (isUpdateNeeded)
                serializedMissions.SaveAll();

            return isUpdateNeeded;
        }
    }
}