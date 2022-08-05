using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.Campaigns
{

    internal class MissionsManager
    {
        internal List<Mission> missions
        {
            get
            {
                return serializedMissions.GetMissions();
            }            
        }

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

            if(!hasVideo)
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

        Mission prepareSurveyMission(MissionType mt, string campaign, int reward)
        {
            string url = MonetizrManager.Instance.GetAsset<string>(campaign, AssetsType.SurveyURLString);

            if (url == null || url.Length == 0)
                return null;

            return new Mission()
            {
                rewardType = RewardType.Coins,
                type = mt,
                reward = reward,
                isDisabled = true, //survey is disabled from start
                surveyUrl = url,
                delaySurveyTimeSec = 30,//86400,
                progress = 1.0f,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
            };
        }

        Mission prepareGiveawayMission(MissionType mt, string campaign, int reward)
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
                case MissionType.SurveyReward: m = prepareSurveyMission(mt, campaign, reward); break;
                case MissionType.TwitterReward: m = prepareTwitterMission(mt, campaign, reward); break;
                case MissionType.GiveawayWithMail: m = prepareGiveawayMission(mt, campaign, reward); break;
                case MissionType.VideoWithEmailGiveaway: m = prepareVideoGiveawayMission(mt, campaign, reward); break;
            }

            if (m == null)
                return null;

            m.state = MissionUIState.Visible;
            m.id = id;
            m.isSponsored = true;
            m.isClaimed = ClaimState.NotClaimed;
            m.campaignId = campaign;
            m.apiKey = MonetizrManager.Instance.GetCurrentAPIkey();
            

            return m;
        }

        internal void AddMissionsToCampaigns()
        {
            //bind to server campagns
            var campaigns = MonetizrManager.Instance.GetAvailableCampaigns();

            //List<Tuple<int, MissionType, int>> tupleMiss = new List<Tuple<int, MissionType, int>>();

            var miss = MonetizrManager.Instance.sponsoredMissions;

            //int id = 0;
            //foreach (var m in MonetizrManager.Instance.sponsoredMissions)
            //{
            //    tupleMiss.Add(Tuple.Create(id, m.missionType, m.reward));
            //    id++;
            //}
              
            serializedMissions.Load();

            //check if campaign is alive for current mission
            foreach(var m in missions)
            {
                m.isServerCampaignActive = MonetizrManager.Instance.HasCampaign(m.campaignId);
            }

            
            //search unbinded campaign
            foreach (string ch in campaigns)
            {
                //TODO: check if such mission type already existed for such campaign
                //if it exist - do not add it

                for (int i = 0; i < miss.Count; i++)
                {
                    Mission m = FindMissionInCache(i, miss[i].missionType, ch);

                    if (m == null)
                    {
                        m = prepareNewMission(i, miss[i].missionType, ch, miss[i].reward);

                        if (m != null)
                        {
                            serializedMissions.Add(m);
                        }
                        else
                        {
                            Debug.LogError($"Can't create campaign with type {miss[i].missionType}");
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
                    //if (m != null)
                    //    missions.Add(m);

                    m.additionalParams = new SerializableDictionary<string,string>(MonetizrManager.Instance.GetCampaign(ch).additional_params);

                    InitializeNonSerializedFields(m);
                }

            }

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
    }
}