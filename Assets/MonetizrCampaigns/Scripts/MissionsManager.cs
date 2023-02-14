using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Monetizr.Campaigns
{

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

       /*internal Mission getCampaignReadyForSurvey()
        {
            //Load();

            var claimed = missions.Find((Mission m) =>
            {
                if (m.isClaimed == ClaimState.NotClaimed)
                    return false;

                DateTime t = DateTime.Parse(m.claimedTime);
                var surveyTime = t.AddSeconds(m.delaySurveyTimeSec);
                bool timeHasCome = DateTime.Now > surveyTime;

                return m.surveyUrl.Length > 0 && timeHasCome && !m.surveyAlreadyShown;

            });

            if (claimed != null)
            {
                claimed.surveyAlreadyShown = true;
                //SaveAll();
            }

            return claimed;
        }*/

        //TODO: add currency
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

        //-------------------



        //using different approach here
        //instead of binding campaigns 
        //create set of missions for each campaign

        //TODO: 
        //- add global define for this approach
        //- add different types of missions with campaign
        //- restore progress
        //- claim full campaigns when all missions conntected to this campaign is over
        //- saving progress  

        Mission prepareVideoMission(MissionType mt, string campaign)
        {
            bool hasHtml = MonetizrManager.Instance.HasAsset(campaign, AssetsType.Html5PathString);
            bool hasVideo = MonetizrManager.Instance.HasAsset(campaign, AssetsType.VideoFilePathString);

            if (!hasVideo)
            {
                Debug.LogWarning($"campaign {campaign} has no video asset!");
            }

            if (!hasHtml && !hasVideo)
                return null;

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

        Mission prepareTwitterMission(MissionType mt, string campaign)
        {
            return new Mission()
            {
                //rewardType = RewardType.Coins,
                type = MissionType.TwitterReward,
                //reward = reward,
                progress = 1.0f,
                isDisabled = false,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
                hasVideo = false,
                isRewardIngame = true,
            };
        }

        Mission prepareDoubleMission(MissionType mt, string campaign)
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

        Mission PrepareSurveyMission(MissionType mt, string campaign)
        {
            string url = MonetizrManager.Instance.GetAsset<string>(campaign, AssetsType.SurveyURLString);

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

        Mission PrepareGiveawayMission(MissionType mt, string campaign)
        {
            //if no claimable reward in campaign - no give away missions
            var claimableReward = MonetizrManager.Instance.GetCampaign(campaign).rewards.Find((ServerCampaign.Reward obj) => { return obj.claimable == true; });

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

        Mission prepareVideoGiveawayMission(MissionType mt, string campaign)
        {
            bool hasHtml = MonetizrManager.Instance.HasAsset(campaign, AssetsType.Html5PathString);
            bool hasVideo = MonetizrManager.Instance.HasAsset(campaign, AssetsType.VideoFilePathString);

            bool video = !(MonetizrManager.Instance.GetCampaign(campaign).serverSettings.GetParam("email_giveaway_mission_without_video") == "true");

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

        Mission prepareMinigameMission(MissionType mt, string campaign)
        {
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
                hasVideo = false,
                isRewardIngame = true,
            };
        }

        //TODO: make separate classes for each mission type
        Mission prepareNewMission(int id, string campaign, MissionDescription md)
        {
            MissionType mt = md.missionType;
            Mission m = null;

            switch (mt)
            {
                case MissionType.MutiplyReward: m = prepareDoubleMission(mt, campaign); break;
                case MissionType.VideoReward: m = prepareVideoMission(mt, campaign); break;
                case MissionType.SurveyReward: m = PrepareSurveyMission(mt, campaign); break;
                case MissionType.TwitterReward: m = prepareTwitterMission(mt, campaign); break;
                // case MissionType.GiveawayWithMail: m = prepareGiveawayMission(mt, campaign, reward); break;
                case MissionType.VideoWithEmailGiveaway: m = prepareVideoGiveawayMission(mt, campaign); break;
                case MissionType.MinigameReward:
                case MissionType.MemoryMinigameReward: m = prepareMinigameMission(mt, campaign); break;

            }

            if (m == null)
                return null;

            m.rewardType = md.rewardCurrency;
            m.reward = md.reward;
            m.state = MissionUIState.Visible;
            m.id = id;
            m.isSponsored = true;
            m.isClaimed = ClaimState.NotClaimed;
            m.campaignId = campaign;
            m.apiKey = MonetizrManager.Instance.GetCurrentAPIkey();

            m.sdkVersion = MonetizrManager.SDKVersion;

            //if(string.IsNullOrEmpty(m.surveyUrl))

            if(!md.hasUnitySurvey)
                m.surveyUrl = md.surveyUrl;

            m.surveyId = md.surveyId;

            m.serverId = md.id;
            m.rewardPercent = md.rewardPercent;
            m.autoStartAfter = md.autoStartAfter;
            m.alwaysHiddenInRC = md.alwaysHiddenInRC;


            return m;
        }



        internal Action ClaimAction(Mission m, Action<bool> __onComplete, Action updateUIDelegate)
        {
            var onComplete = __onComplete;


            var nextMission = missions.Find(_m => m.serverId == _m.autoStartAfter);

            if (nextMission != null && nextMission != m)
            {
                Debug.Log($"======{nextMission.serverId}");

                onComplete = (bool skipped) =>
                {

                    Debug.Log($"======{nextMission.serverId} {skipped}");

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
                    Debug.Log($"Completing survey {m.surveyUrl} on windows");
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

            /*Action<bool> onComplete = (bool isSkipped) =>
            {
                //OnClaimRewardComplete(m, isSkipped, AddNewUIMissions);
                //MonetizrManager.Instance.OnClaimRewardComplete(m, isSkipped, updateUIDelegate);
            };*/

            //MonetizrManager.Analytics.TrackEvent("Claim pressed", m);

            Action<bool> onVideoComplete = (bool isVideoSkipped) =>
            {
                /*OnClaimRewardComplete(m, false);*/

                if (MonetizrManager.claimForSkippedCampaigns)
                    isVideoSkipped = false;

                if (isVideoSkipped)
                {
                    //MonetizrManager.Analytics.TrackEvent("Video skipped", m);

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
                            //MonetizrManager.Analytics.TrackEvent("Email enter skipped", m);

                            MonetizrManager.Analytics.TrackEvent(m, m.adPlacement, MonetizrManager.EventType.ButtonPressSkip);

                            onComplete?.Invoke(isMailSkipped);
                            return;
                        }

                        MonetizrManager.WaitForEndRequestAndNotify(onComplete, m);

                    },
                    m,
                    PanelId.GiveawayEmailEnterNotification);

            };

            //show video, then claim rewards if it's completed
            return () =>
            {
                //Debug.Log($"----- {needToPlayVideo}");

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
            //MonetizrManager.Analytics.TrackEvent("Watch video press", m);

#if UNITY_EDITOR_WIN
            onComplete?.Invoke(false);
            return;
#endif

            var htmlPath = MonetizrManager.Instance.GetAsset<string>(m.campaignId, AssetsType.Html5PathString);

            if (htmlPath != null)
            {
                MonetizrManager.ShowHTML5((bool isSkipped) => { onComplete(isSkipped); }, m);
            }
            else
            {
                var videoPath = MonetizrManager.Instance.GetAsset<string>(m.campaignId, AssetsType.VideoFilePathString);

                //MonetizrManager._PlayVideo(videoPath, (bool isSkipped) => { OnClaimRewardComplete(m, isSkipped); });

                MonetizrManager.ShowWebVideo((bool isSkipped) => { onComplete(isSkipped); }, m);
            }
        }

        internal void UpdateMissionsRewards(RewardType rt, MonetizrManager.GameReward reward)
        {
            foreach (var m in missions)
            {
                if (m.rewardType == rt)
                {
                    m.reward = (ulong)(reward.maximumAmount * (m.rewardPercent / 100.0f));
                }
            }

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
        public class ServerMissionsHelper
        {
            public ServerDefinedMissions[] missions;

            public List<MissionDescription> CreateMissionDescriptions(List<MissionDescription> originalList, SettingsDictionary<string, string> serverSettings)
            {
                List<MissionDescription> m = new List<MissionDescription>();

                Array.ForEach(missions, (ServerDefinedMissions _m) =>
                {

                    MissionType serverMissionType = _m.GetMissionType();

                    if (serverMissionType == MissionType.Undefined)
                        return;

                    //MissionDescription original = originalList.Find((MissionDescription md) => { return md.missionType == serverMissionType; });

                    //int rewardAmount = 1;
                    //RewardType currency = RewardType.Coins;
                    //List<int> activateAfter = new List<int>();

                    /*if(original != null)
                    {
                        rewardAmount = original.reward;
                        currency = original.rewardCurrency;
                    }
                    else
                    {*/
                    int rewardAmount = _m.GetRewardAmount();
                    RewardType currency = _m.GetRewardType();

                    MonetizrManager.GameReward gr = MonetizrManager.GetGameReward(currency);

                    //no such reward
                    if (gr == null)
                        return;

                    //award is too much
                    if (rewardAmount > 100.0f)
                        return;

                    ulong rewardAmount2 = (ulong)(gr.maximumAmount * (rewardAmount / 100.0f));
                    //}

                    //activateAfter = _m.GetActivateRange();

                    //string surveyUrl = serverSettings.GetParam(_m.survey);

                   // Debug.Log($"----------------- {_m.survey} : {_m.surveyUnity}");

                    m.Add(new MissionDescription
                    {
                        missionType = _m.GetMissionType(),
                        reward = rewardAmount2,
                        rewardCurrency = _m.GetRewardType(),
                        activateAfter = _m.GetActivateRange(),
                        surveyUrl = serverSettings.GetParam(_m.survey),
                        surveyId = string.IsNullOrEmpty(_m.surveyUnity) ? _m.survey : _m.surveyUnity,
                        hasUnitySurvey = !string.IsNullOrEmpty(_m.surveyUnity),
                        rewardPercent = rewardAmount,
                        id = _m.getId(),
                        alwaysHiddenInRC = _m.IsAlwaysHiddenInRC(),
                        autoStartAfter = _m.GetAutoStartId()
                    }); ;

                });

                return m;
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

            public int GetRewardAmount()
            {
                int reward = 0;

                if (int.TryParse(percent_amount, out reward))
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


        internal void AddMissionsToCampaigns()
        {


            //bind to server campagns
            var campaigns = MonetizrManager.Instance.GetAvailableCampaigns();

            var prefefinedSponsoredMissions = MonetizrManager.Instance.sponsoredMissions;

            int serverDefinedMission = 0;


            Debug.LogWarning($"AddMissionsToCampaigns count: {campaigns.Count}");

            if (campaigns.Count > 0)
            {
                ServerCampaign sc = MonetizrManager.Instance.GetCampaign(campaigns[0]);

                serverDefinedMission = sc.serverSettings.GetIntParam("server_defined_mission", 0);

                

                string serverMissionsJson = MonetizrManager.Instance.GetCampaign(campaigns[0]).serverSettings.GetParam("custom_missions");
                                
                Debug.LogWarning($"Predefined missions from settings: {serverMissionsJson}");

                if (serverMissionsJson?.Length > 0)
                {
                    serverMissionsJson = serverMissionsJson.Replace('\'', '\"');

                    ServerMissionsHelper ic = null;

                    try
                    {
                        ic = JsonUtility.FromJson<ServerMissionsHelper>(serverMissionsJson);

                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Problem {e.ToString()} with json {serverMissionsJson}");
                    }

                    if (ic != null)
                    {
                        prefefinedSponsoredMissions = ic.CreateMissionDescriptions(prefefinedSponsoredMissions, sc.serverSettings);
                    }
                }
            }

            //if (prefefinedSponsoredMissions.Count > 1)
            //    prefefinedSponsoredMissions = prefefinedSponsoredMissions.GetRange(serverDefinedMission, 1);

            Debug.Log($"Predefined missions has {prefefinedSponsoredMissions.Count} values");

            serializedMissions.Load();

            //check if campaign is alive for current mission
            foreach (var m in missions)
            {
                m.isServerCampaignActive = MonetizrManager.Instance.HasCampaign(m.campaignId);

                //remove if it will not be in the predefined list
                m.isToBeRemoved = true;
            }


            //search unbinded campaign
            for (int c = 0; c < campaigns.Count; c++)
            {
                string ch = campaigns[c];
                ServerCampaign serverCampaign = MonetizrManager.Instance.GetCampaign(ch);

                if (c >= MonetizrManager.maximumCampaignAmount)
                {
                    break;
                }

                //TODO: check if such mission type already existed for such campaign
                //if it exist - do not add it

                //TODO: use prefefinedSponsoredMissions for all cases
                for (int i = 0; i < prefefinedSponsoredMissions.Count; i++)
                {
                    MissionDescription md = prefefinedSponsoredMissions[i];

                    Mission m = FindMissionInCache(i, md.missionType, ch, prefefinedSponsoredMissions[i].reward);

                    if (m == null)
                    {
                        //
                        m = prepareNewMission(i, ch, prefefinedSponsoredMissions[i]);

                        if (m != null)
                        {
                            serializedMissions.Add(m);
                        }
                        else
                        {
                            Debug.LogError($"Can't create campaign with type {prefefinedSponsoredMissions[i].missionType}");
                        }
                    }
                    else
                    {
                        Log.Print($"Found mission {ch}:{i} in local data");
                    }

                    if (m == null)
                        continue;

                    m.sdkVersion = MonetizrManager.SDKVersion;

                    //if (string.IsNullOrEmpty(m.surveyUrl))

                    if(!md.hasUnitySurvey)
                        m.surveyUrl = md.surveyUrl;
                    else
                        m.surveyUrl = MonetizrManager.Instance.GetAsset<string>(m.campaignId, AssetsType.SurveyURLString);

                    m.hasUnitySurvey = md.hasUnitySurvey;

                    m.surveyId = md.surveyId;
                                       
                    m.isServerCampaignActive = true;


                    m.isToBeRemoved = false;
                    m.campaignServerSettings = MonetizrManager.Instance.GetCampaign(ch).serverSettings;

                    bool showNotClaimedDisabled = m.campaignServerSettings.GetBoolParam("RewardCenter.show_disabled_missions", true);

                    m.state = m.isDisabled ? MissionUIState.Visible : MissionUIState.Hidden;

                    if (showNotClaimedDisabled)
                        m.state = MissionUIState.Visible;

                    //rewrite these parameters here, because otherwise it will be saved in cache
                    //m.additionalParams = new SerializableDictionary<string,string>(MonetizrManager.Instance.GetCampaign(ch).additional_params);


                    m.amountOfRVOffersShown = m.campaignServerSettings.GetIntParam("amount_of_rv_offers", -1);
                    //m.amountOfNotificationsShown = m.campaignServerSettings.GetIntParam("amount_of_notifications", -1);
                    m.amountOfNotificationsSkipped = m.campaignServerSettings.GetIntParam("startup_skipped_notifications", int.MaxValue - 1); ;// int.MaxValue - 1; //first notification is always visible
                    m.isVideoShown = false;
                    m.isDisabled = true; //disable everything by default, activate them in UpdateMissionsActivity
                    m.activateAfter = prefefinedSponsoredMissions[i].activateAfter;

                    m.brandName = MonetizrManager.Instance.GetAsset<string>(m.campaignId, AssetsType.BrandTitleString);

                    m.campaign = serverCampaign;
                    //InitializeNonSerializedFields(m);
                }



            }

            //remove if mission in save, but not in the predefined list
            missions.RemoveAll(m => m.isToBeRemoved);

            Debug.Log($"Total amount of missions {missions.Count}");

            UpdateMissionsActivity(null);

            //TODO: remove outdated missions from local cache
            //???

            serializedMissions.SaveAll();
        }



       /* private void InitializeNonSerializedFields(Mission m)
        {
            m.brandLogo = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.BrandLogoSprite);
            m.brandRewardBanner = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.BrandRewardBannerSprite);
            m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.BrandBannerSprite);
            m.brandName = MonetizrManager.Instance.GetAsset<string>(m.campaignId, AssetsType.BrandTitleString);
            //m.surveyUrl = MonetizrManager.Instance.GetAsset<string>(m.campaignId, AssetsType.SurveyURLString);
        }*/

        //------------------------

        /*internal void AddMissionAndBindToCampaign(Mission sponsoredMission)
        {
            //bind to server campagns
            var challenges = MonetizrManager.Instance.GetAvailableCampaigns();

            if (challenges.Count == 0)
                return;

            //check already binded campaigns
            HashSet<string> bindedCampaigns = new HashSet<string>();
            missions.ForEach((Mission _m) => { if (_m.campaignId != null) bindedCampaigns.Add(_m.campaignId); });

            var activeChallenge = MonetizrManager.Instance.GetActiveCampaign();

            //bind to active challenge first
            challenges.Remove(activeChallenge);
            challenges.Insert(0, activeChallenge);


            //search unbinded campaign
            foreach (string ch in challenges)
            {
                if (bindedCampaigns.Contains(ch))
                    continue;

                sponsoredMission.campaignId = ch;

                if (MonetizrManager.Instance.HasAsset(ch, AssetsType.SurveyURLString))
                    sponsoredMission.surveyUrl = MonetizrManager.Instance.GetAsset<string>(ch, AssetsType.SurveyURLString);

                break;
            }

            //no campaings for binding missions
            if (sponsoredMission.campaignId != null)
            {
                Log.Print($"Bind campaign {sponsoredMission.campaignId} to mission {missions.Count}");

                missions.Add(sponsoredMission);
            }
        }

        internal bool TryToActivateSurvey(Mission m)
        {
            var surveyMission = missions.Find((Mission _m) => { return _m.type == MissionType.SurveyReward && _m.campaignId == m.campaignId && _m.isDisabled; });

            if (surveyMission != null)
            {
                Log.Print("Survey activated!");

                //surveyMission.isDisabled = false;

                surveyMission.state = MissionUIState.ToBeShown;

                surveyMission.activateTime = DateTime.Now.AddSeconds(surveyMission.delaySurveyTimeSec);
                surveyMission.deactivateTime = surveyMission.activateTime.AddSeconds(surveyMission.delaySurveyTimeSec);

                //TODO: Save
                //SaveReward(m);

                return true;
            }

            return false;
        }*/

        internal Mission GetMission(string campaignId)
        {
            return missions.Find((Mission m) => { return m.campaignId == campaignId; });
        }

        internal bool IsActiveByTime(Mission m)
        {
            bool r = DateTime.Now >= m.activateTime && DateTime.Now <= m.deactivateTime;
            return r;
        }

        internal Mission FindActiveSurveyMission()
        {
            return missions.Find((Mission m) =>
            {
                return m.type == MissionType.SurveyReward && m.isClaimed == ClaimState.NotClaimed && !m.isDisabled && IsActiveByTime(m);
            });
        }

        internal Mission FindMissionForStartNotify()
        {
            return missions.Find((Mission m) =>
            {
                return m.type != MissionType.SurveyReward &&
                        m.isClaimed == ClaimState.NotClaimed &&
                        !m.isDisabled &&
                        IsActiveByTime(m)
                        && m.isServerCampaignActive;

            });
        }

        internal List<Mission> GetMissionsForRewardCenter(bool includeDisabled = false)
        {           
            return missions.FindAll((Mission m) =>
            {

                bool disabled = m.isDisabled;

                if (includeDisabled)
                    disabled = false;

                return m.isSponsored &&
                        m.isClaimed != ClaimState.Claimed &&
                        !disabled &&
                        IsActiveByTime(m) &&
                        m.isServerCampaignActive &&
                        m.autoStartAfter == -1;

            });
        }

        internal int GetActiveMissionsNum()
        {
            //var mList = missions.FindAll((Mission m) => { return m.isClaimed != ClaimState.Claimed; });

            return GetMissionsForRewardCenter().Count;
        }

        //check activateAfter ranges for all missions and activate them if missions in range already active 
        internal bool UpdateMissionsActivity(Mission finishedMission)
        {
            bool isUpdateNeeded = false;

            foreach (var m in missions)
            {
                if (m == finishedMission)
                    continue;

                if (!m.isDisabled)
                    continue;

                if (m.isClaimed == ClaimState.Claimed)
                    continue;

                bool hasActivateAfter = m.activateAfter.Count > 0;

                //check if mission self referenced in activate after
                if (hasActivateAfter && m.activateAfter.FindIndex(_id => _id == m.serverId) > 0)
                {
                    Debug.LogWarning($"Mission id {m.serverId} activate after itself!");
                    hasActivateAfter = false;
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
                    }
                }


                /*for(int i = r.start;; i++)
                {
                    if (i > r.start + r.length)
                        break;

                    if (i >= missions.Count)
                        break;

                    if (missions[i] == finishedMission)
                        continue;

                    if (missions[i].isClaimed != ClaimState.Claimed)
                        shouldBeDisabled = true;
                    
                }*/

                //activate if all in range claimed
                if (!shouldBeDisabled)
                {
                    if (m.isDisabled)
                        isUpdateNeeded = true;

                    m.isDisabled = shouldBeDisabled;
                    m.state = MissionUIState.ToBeShown;
                }
            }

            if(isUpdateNeeded)
                serializedMissions.SaveAll();

            return isUpdateNeeded;
        }
    }
}