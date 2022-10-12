using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.Campaigns
{

    internal class MissionsManager
    {
        internal List<Mission> missions => serializedMissions.GetMissions();

        private CampaignsSerializeManager serializedMissions = new CampaignsSerializeManager();


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

        internal Mission getCampaignReadyForSurvey()
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
        }

        internal Mission FindMissionInCache(int id, MissionType mt, string ch)
        {
            foreach (var m in missions)
            {
                if (m.type == mt && m.campaignId == ch && m.id == id && m.apiKey == MonetizrManager.Instance.GetCurrentAPIkey())
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

        Mission prepareVideoMission(MissionType mt, string campaign, int reward)
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
                rewardType = RewardType.Coins,
                type = MissionType.VideoReward,
                reward = reward,
                progress = 1.0f,
                isDisabled = false,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
            };
        }

        Mission prepareTwitterMission(MissionType mt, string campaign, int reward)
        {
            return new Mission()
            {
                rewardType = RewardType.Coins,
                type = MissionType.TwitterReward,
                reward = reward,
                progress = 1.0f,
                isDisabled = false,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,

            };
        }

        Mission prepareDoubleMission(MissionType mt, string campaign, int reward)
        {
            RewardType rt = RewardType.Coins;

            return new Mission()
            {
                rewardType = rt,
                startMoney = MonetizrManager.gameRewards[rt].GetCurrencyFunc(),
                type = MissionType.MutiplyReward,
                reward = reward,
                progress = 0.0f,
                isDisabled = false,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
            };
        }

        Mission PrepareSurveyMission(MissionType mt, string campaign, int reward)
        {
            string url = MonetizrManager.Instance.GetAsset<string>(campaign, AssetsType.SurveyURLString);

            if (url == null || url.Length == 0)
                return null;

            return new Mission()
            {
                rewardType = RewardType.Coins,
                type = mt,
                reward = reward,
                isDisabled = false, //survey is disabled from start
                surveyUrl = url,
                //delaySurveyTimeSec = 30,//86400,
                progress = 0.0f,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
            };
        }

        Mission PrepareGiveawayMission(MissionType mt, string campaign, int reward)
        {
            //if no claimable reward in campaign - no give away missions
            var claimableReward = MonetizrManager.Instance.GetCampaign(campaign).rewards.Find((ServerCampaign.Reward obj) => { return obj.claimable == true; });

            if (claimableReward == null)
                return null;

            RewardType rt = RewardType.Coins;

            return new Mission()
            {
                rewardType = rt,
                startMoney = MonetizrManager.gameRewards[rt].GetCurrencyFunc(),
                type = mt,
                reward = reward,
                progress = 0.0f,
                isDisabled = false,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
            };
        }

        Mission prepareVideoGiveawayMission(MissionType mt, string campaign, int reward)
        {
            bool hasHtml = MonetizrManager.Instance.HasAsset(campaign, AssetsType.Html5PathString);
            bool hasVideo = MonetizrManager.Instance.HasAsset(campaign, AssetsType.VideoFilePathString);

            if (!hasHtml && !hasVideo)
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
                rewardType = rt,
                startMoney = MonetizrManager.gameRewards[rt].GetCurrencyFunc(),
                type = mt,
                reward = reward,
                progress = 1.0f,
                isDisabled = false,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
            };
        }

        Mission prepareMinigameMission(MissionType mt, string campaign, int reward)
        {            
            RewardType rt = RewardType.Coins;

            return new Mission()
            {
                rewardType = rt,
                startMoney = MonetizrManager.gameRewards[rt].GetCurrencyFunc(),
                type = mt,
                reward = reward,
                progress = 1.0f,
                isDisabled = false,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
            };
        }

        //TODO: make separate classes for each mission type
        Mission prepareNewMission(int id, MissionType mt, string campaign, int reward)
        {
            Mission m = null;

            switch (mt)
            {
                case MissionType.MutiplyReward: m = prepareDoubleMission(mt, campaign, reward); break;
                case MissionType.VideoReward: m = prepareVideoMission(mt, campaign, reward); break;
                case MissionType.SurveyReward: m = PrepareSurveyMission(mt, campaign, reward); break;
                case MissionType.TwitterReward: m = prepareTwitterMission(mt, campaign, reward); break;
                // case MissionType.GiveawayWithMail: m = prepareGiveawayMission(mt, campaign, reward); break;
                case MissionType.VideoWithEmailGiveaway: m = prepareVideoGiveawayMission(mt, campaign, reward); break;
                case MissionType.MinigameReward: m = prepareMinigameMission(mt, campaign, reward); break;
            }

            if (m == null)
                return null;
                        
            m.state = MissionUIState.Visible;
            m.id = id;
            m.isSponsored = true;
            m.isClaimed = ClaimState.NotClaimed;
            m.campaignId = campaign;
            m.apiKey = MonetizrManager.Instance.GetCurrentAPIkey();
            m.sdkVersion = MonetizrManager.SDKVersion;



            return m;
        }

        

        internal Action ClaimAction(Mission m, Action<bool> onComplete, Action updateUIDelegate)
        {
            switch (m.type)
            {
                case MissionType.SurveyReward: return SurveyClaimAction(m, onComplete, updateUIDelegate);
                case MissionType.MinigameReward: return MinigameClaimAction(m, onComplete, updateUIDelegate);
                case MissionType.VideoWithEmailGiveaway: return GetEmailGiveawayClaimAction(m, onComplete, updateUIDelegate);
            }

            return null;
        }

        internal Action MinigameClaimAction(Mission m, Action<bool> onComplete, Action updateUIDelegate)
        {
            Action<bool> onMinigameComplete = (bool isSkipped) =>
            {
                MonetizrManager.Instance.OnClaimRewardComplete(m, isSkipped, updateUIDelegate);
            };

            return () =>
            {
                //MonetizrManager.ShowSurvey(onSurveyComplete, m);

                MonetizrManager.ShowMinigame(onMinigameComplete, PanelId.MemoryGame, m);
                    
            };
        }

        internal Action SurveyClaimAction(Mission m, Action<bool> onComplete, Action updateUIDelegate)
        {
            Action<bool> onSurveyComplete = (bool isSkipped) =>
            {
                MonetizrManager.Instance.OnClaimRewardComplete(m, isSkipped, updateUIDelegate);
            };

#if UNITY_EDITOR_WIN
            return () => onSurveyComplete.Invoke(false);
#endif
                     

            return () =>
            {
                //MonetizrManager.ShowSurvey(onSurveyComplete, m);

                MonetizrManager.ShowNotification((bool _) => { MonetizrManager.ShowSurvey(onSurveyComplete, m); },
                           m,
                           PanelId.SurveyNotification);
            };
        }

        internal Action GetEmailGiveawayClaimAction(Mission m, Action<bool> onComplete, Action updateUIDelegate)
        {
            MonetizrManager.temporaryRewardTypeSelection = MonetizrManager.RewardSelectionType.Product;

            bool needToPlayVideo = !(m.additionalParams.GetParam("email_giveaway_mission_without_video") == "true");

#if UNITY_EDITOR_WIN
            needToPlayVideo = false;
#endif

            if(m.isVideoShown)
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

                if(m.additionalParams.GetParam("watch_video_only_once") == "true")
                    m.isVideoShown = true;

                MonetizrManager.ShowEnterEmailPanel(
                    (bool isMailSkipped) =>
                    {
                        if (isMailSkipped)
                        {
                            MonetizrManager.Analytics.TrackEvent("Email enter skipped", m);

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
            public mission[] missions;

            public List<MissionDescription> CreateMissionDescriptions(List<MissionDescription> originalList)
            {
                List<MissionDescription> m = new List<MissionDescription>();

                Array.ForEach(missions, (mission _m)=>{

                    MissionType serverMissionType = _m.GetMissionType();

                    if (serverMissionType == MissionType.Undefined)
                        return;
                    

                    MissionDescription original = originalList.Find((MissionDescription md) => { return md.missionType == serverMissionType; });

                                        

                    int rewardAmount = 1;
                    RewardType currency = RewardType.Coins;
                    RangeInt activateAfter = new RangeInt(0,-1);

                    if(original != null)
                    {
                        rewardAmount = original.reward;
                        currency = original.rewardCurrency;
                    }
                    else
                    {
                        rewardAmount = _m.GetRewardAmount();
                        
                        currency = _m.GetRewardType();
                                                

                        MonetizrManager.GameReward gr = MonetizrManager.GetGameReward(currency);

                        //no such reward
                        if (gr == null)
                            return;

                        //award is too much
                        if (rewardAmount > 100.0f)
                            return;

                        rewardAmount = (int)(gr.maximumAmount*(rewardAmount / 100.0f));
                    }

                    activateAfter = _m.GetActivateRange();

                    m.Add(new MissionDescription(_m.GetMissionType(), rewardAmount, currency, activateAfter));

                });

                return m;
            }
        }

        [Serializable]
        public class mission
        {
            public string type;
            public string percent_amount;
            public string currency;
            public string activate_after;

            public RangeInt GetActivateRange()
            {
                RangeInt defaultRange = new RangeInt(-1,0);

                if (activate_after == null)
                    return defaultRange;

                string[] p = activate_after.Split('-');

                int p1 = 0;
                int p2 = 0;

                if(p.Length > 0)
                    int.TryParse(p[0], out p1);

                if (p.Length > 1)
                    int.TryParse(p[1], out p2);

                if (p.Length == 0)
                    return defaultRange;

                if (p.Length == 1)
                    return new RangeInt(p1,0);

                if (p.Length == 2)
                    return new RangeInt(p1, p2-p1);

                return defaultRange;
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
                    return Mathf.Clamp(reward,0,100);

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

            if (campaigns.Count > 0)
            {
                ServerCampaign sc = MonetizrManager.Instance.GetCampaign(campaigns[0]);

                serverDefinedMission = sc.GetIntParam("server_defined_mission", 0);


                string serverMissionsJson = MonetizrManager.Instance.GetCampaign(campaigns[0]).GetParam("custom_missions");

                if (serverMissionsJson.Length > 0)
                {
                    ServerMissionsHelper ic = JsonUtility.FromJson<ServerMissionsHelper>(serverMissionsJson);

                    prefefinedSponsoredMissions = ic.CreateMissionDescriptions(prefefinedSponsoredMissions);
                }
            }

            //if (prefefinedSponsoredMissions.Count > 1)
            //    prefefinedSponsoredMissions = prefefinedSponsoredMissions.GetRange(serverDefinedMission, 1);


            serializedMissions.Load();

            //check if campaign is alive for current mission
            foreach(var m in missions)
            {
                m.isServerCampaignActive = MonetizrManager.Instance.HasCampaign(m.campaignId);
            }

            
            //search unbinded campaign
            for (int c = 0; c < campaigns.Count; c++)
            {
                string ch = campaigns[c];

                if(c >= MonetizrManager.maximumCampaignAmount)
                {
                    break;
                }

                //TODO: check if such mission type already existed for such campaign
                //if it exist - do not add it

                for (int i = 0; i < prefefinedSponsoredMissions.Count; i++)
                {
                    Mission m = FindMissionInCache(i, prefefinedSponsoredMissions[i].missionType, ch);

                    if (m == null)
                    {
                        m = prepareNewMission(i, 
                                prefefinedSponsoredMissions[i].missionType, 
                                ch, 
                                prefefinedSponsoredMissions[i].reward);

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
                        Log.Print($"Found campaign {ch} in local data");
                    }

                    if (m == null)
                        continue;

                    m.isServerCampaignActive = true;

                    
                    
                    m.state = m.isDisabled ? MissionUIState.Visible : MissionUIState.Hidden;

                    //rewrite these parameters here, because otherwise it will be saved in cache
                    m.additionalParams = new SerializableDictionary<string,string>(MonetizrManager.Instance.GetCampaign(ch).additional_params);
                    m.amountOfRVOffersShown = m.additionalParams.GetIntParam("amount_of_rv_offers", -1);
                    m.amountOfNotificationsShown = m.additionalParams.GetIntParam("amount_of_notifications", -1);
                    m.amountOfNotificationsSkipped = m.additionalParams.GetIntParam("startup_skipped_notifications", int.MaxValue - 1); ;// int.MaxValue - 1; //first notification is always visible
                    m.isVideoShown = false;
                    m.isDisabled = true; //disable everything by default, activate them in UpdateMissionsActivity
                    m.activateAfter = prefefinedSponsoredMissions[i].activateAfter;

                    InitializeNonSerializedFields(m);
                }

                

            }


            UpdateMissionsActivity(null);

            //TODO: remove outdated missions from local cache
            //???

            serializedMissions.SaveAll();
        }

        private void InitializeNonSerializedFields(Mission m)
        {
            m.brandLogo = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.BrandLogoSprite);
            m.brandRewardBanner = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.BrandRewardBannerSprite);
            m.brandBanner = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.BrandBannerSprite);
            m.brandName = MonetizrManager.Instance.GetAsset<string>(m.campaignId, AssetsType.BrandTitleString);
            m.surveyUrl = MonetizrManager.Instance.GetAsset<string>(m.campaignId, AssetsType.SurveyURLString);
        }

        //------------------------

        internal void AddMissionAndBindToCampaign(Mission sponsoredMission)
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

            if(surveyMission != null)
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
        }
              
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

        internal List<Mission> GetMissionsForRewardCenter()
        {
            return missions.FindAll((Mission m) => {

                return m.isSponsored &&
                        m.isClaimed != ClaimState.Claimed &&
                        !m.isDisabled &&
                        IsActiveByTime(m) &&
                        m.isServerCampaignActive;
                   
            });
        }

        //check activateAfter ranges for all missions and activate them if missions in range already active 
        internal bool UpdateMissionsActivity(Mission finishedMission)
        {
            bool isUpdateNeeded = false;

            foreach (var m in missions)
            {
                if (finishedMission != null && m != finishedMission)
                    continue;

                RangeInt r = m.activateAfter;

                //no activate_after here
                if (r.start == -1)
                {
                    if (m.isDisabled)
                        isUpdateNeeded = true;

                    m.isDisabled = false;
                    continue;
                }

                bool shouldBeDisabled = false;

                for(int i = r.start;; i++)
                {
                    if (i > r.start + r.length)
                        break;

                    if (i >= missions.Count)
                        break;

                    if (finishedMission != null && missions[i] == finishedMission)
                        continue;

                    if (missions[i].isClaimed != ClaimState.Claimed)
                        shouldBeDisabled = true;
                    
                }

                //activate if all in range claimed
                if (!shouldBeDisabled)
                {
                    if (m.isDisabled)
                        isUpdateNeeded = true;

                    m.isDisabled = shouldBeDisabled;
                }
            }

            return isUpdateNeeded;
        }
    }
}