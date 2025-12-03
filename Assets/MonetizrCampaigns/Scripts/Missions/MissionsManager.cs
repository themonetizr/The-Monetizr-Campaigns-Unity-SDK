using Monetizr.SDK.Analytics;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.UI;
using Monetizr.SDK.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using EventType = Monetizr.SDK.Core.EventType;

namespace Monetizr.SDK.Missions
{
    internal class MissionsManager
    {
        internal List<Mission> missions => serializedMissions.GetMissions();
        private MissionsSerializeManager serializedMissions = new MissionsSerializeManager();

        internal void CleanRewardsClaims()
        {
            serializedMissions.Reset();
        }

        internal void SaveAll()
        {
            serializedMissions.SaveAll();
        }

        internal void CleanUp()
        {
            CleanRewardsClaims();
        }

        internal void CleanUserDefinedMissions()
        {
            missions.RemoveAll((e) => { return e.isSponsored == false; });
        }

        internal Mission FindMissionInCache(int id, MissionType mt, string ch, ulong reward)
        {
            foreach (var m in missions)
            {
                if (m.type == mt &&
                    m.campaignId == ch &&
                    m.id == id &&
                    m.apiKey == MonetizrInstance.Instance.GetCurrentAPIkey() &&
                    m.reward == reward)
                    return m;
            }

            return null;
        }

        internal bool CheckFullCampaignClaim(Mission _m)
        {
            foreach (var m in missions)
            {
                if (m.campaignId == _m.campaignId && m.isClaimed != ClaimState.Claimed) return false;
            }

            return true;
        }

        internal Mission PrepareVideoMission (MissionType mt, ServerCampaign campaign)
        {
            bool hasSomething = campaign.HasAsset(AssetsType.Html5PathString) || campaign.HasAsset(AssetsType.VideoFilePathString);

            if (!hasSomething && (campaign.campaignType == CampaignType.MonetizrBackend))
            {
                MonetizrLogger.PrintError($"Campaign {campaign.id} has no video asset: hasPath = " + hasSomething + " / isProgrammatic: " + !campaign.serverSettings.GetBoolParam("programmatic", false));
                return null;
            }
            
            return new Mission()
            {
                type = MissionType.VideoReward,
                progress = 1.0f,
                isDisabled = false,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
                hasVideo = true,
                isRewardIngame = true,
            };
        }

        internal Mission PrepareDoubleMission (MissionType mt, ServerCampaign campaign)
        {
            RewardType rt = RewardType.Coins;

            return new Mission()
            {
                startMoney = MonetizrManager.gameRewards[rt].GetCurrencyFunc(),
                type = MissionType.MutiplyReward,
                progress = 0.0f,
                isDisabled = false,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
                hasVideo = false,
                isRewardIngame = true,
            };
        }

        internal Mission PrepareSurveyMission (MissionType mt, ServerCampaign campaign)
        {
            string url = campaign.GetAsset<string>(AssetsType.SurveyURLString);

            return new Mission()
            {
                type = mt,
                isDisabled = false,
                surveyUrl = url,
                progress = 0.0f,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
                hasVideo = false,
                isRewardIngame = true,
            };
        }

        Mission PrepareVideoGiveawayMission(MissionType mt, ServerCampaign campaign)
        {
            bool hasHtml = campaign.HasAsset(AssetsType.Html5PathString);
            bool hasVideo = campaign.HasAsset(AssetsType.VideoFilePathString);
            bool video = !(campaign.serverSettings.GetParam("email_giveaway_mission_without_video") == "true");
            if (video && !hasHtml && !hasVideo) return null;
            RewardType rt = RewardType.Coins;

            return new Mission()
            {
                startMoney = MonetizrManager.gameRewards[rt].GetCurrencyFunc(),
                type = mt,
                progress = 1.0f,
                isDisabled = false,
                activateTime = DateTime.MinValue,
                deactivateTime = DateTime.MaxValue,
                hasVideo = video,
                isRewardIngame = false,
            };
        }

        internal Mission PrepareMission(MissionType mt, ServerCampaign campaign, bool hasVideo, bool isRewardInGame)
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

        internal Mission PrepareNewMission(int id, ServerCampaign campaign, MissionDescription md)
        {
            MissionType mt = md.missionType;
            Mission m = null;

            switch (mt)
            {
                case MissionType.MutiplyReward: m = PrepareDoubleMission(mt, campaign); break;
                case MissionType.VideoReward: m = PrepareVideoMission(mt, campaign); break;
                case MissionType.SurveyReward: m = PrepareSurveyMission(mt, campaign); break;
                case MissionType.VideoWithEmailGiveaway: m = PrepareVideoGiveawayMission(mt, campaign); break;
                case MissionType.MinigameReward:
                case MissionType.MemoryMinigameReward:
                case MissionType.ActionReward:
                case MissionType.CodeReward: m = PrepareMission(mt, campaign, false, true); break;
            }

            if (m == null)
            {
                MonetizrLogger.PrintError("Mission Type Undefined.");
                return null;
            }

            m.rewardType = md.rewardCurrency;
            m.startMoney = MonetizrManager.gameRewards[m.rewardType].GetCurrencyFunc();
            m.reward = md.reward;
            m.state = MissionUIState.Visible;
            m.id = id;
            m.isSponsored = true;
            m.isClaimed = ClaimState.NotClaimed;
            m.campaignId = campaign.id;
            m.apiKey = MonetizrInstance.Instance.GetCurrentAPIkey();
            m.sdkVersion = MonetizrSettings.SDKVersion;
            if (!md.hasUnitySurvey) m.surveyUrl = md.surveyUrl;
            m.surveyId = md.surveyId;
            m.serverId = md.id;
            m.rewardPercent = md.rewardPercent;
            m.autoStartAfter = md.autoStartAfter;
            m.alwaysHiddenInRC = md.alwaysHiddenInRC;
            m.openRtbRequestForProgrammatic = md.openRtbRequestForProgrammatic;
            m.hasCongrats = md.hasCongrats;
            return m;
        }

        internal Action ClaimAction(Mission m, Action<bool> __onComplete, Action updateUIDelegate)
        {
            var onComplete = __onComplete;
            var nextMission = missions.Find(_m => m.serverId == _m.autoStartAfter);

            if (nextMission != null && nextMission != m)
            {
                onComplete = (bool skipped) =>
                {
                    if (!skipped) ClaimAction(nextMission, __onComplete, updateUIDelegate).Invoke();
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
                MonetizrInstance.Instance.OnClaimRewardComplete(m, false, onComplete, updateUIDelegate);
            };
        }

        internal Action VideoClaimAction(Mission m, Action<bool> onComplete, Action updateUIDelegate)
        {
            Action<bool> onVideoComplete = (bool isSkipped) =>
            {
                MonetizrInstance.Instance.OnClaimRewardComplete(m, isSkipped, onComplete, updateUIDelegate);
            };

            return () =>
            {
                OnVideoPlayPress(m, onVideoComplete);
            };
        }

        internal Action MinigameClaimAction(Mission m, Action<bool> onComplete, Action updateUIDelegate)
        {
            Action<bool> onMinigameComplete = (bool isSkipped) =>
            {
                MonetizrInstance.Instance.OnClaimRewardComplete(m, isSkipped, onComplete, updateUIDelegate);
            };

            return () =>
            {
                MonetizrInstance.Instance.ShowMinigame(onMinigameComplete, m);
            };
        }

        internal Action ActionRewardClaimAction(Mission m, Action<bool> onComplete, Action updateUIDelegate)
        {
            Action<bool> onActionComplete = (bool isSkipped) =>
            {
                MonetizrInstance.Instance.OnClaimRewardComplete(m, isSkipped, onComplete, updateUIDelegate);
            };

            return () =>
            {
                MonetizrInstance.Instance.ShowActionView(onActionComplete, m);
            };
        }

        internal Action CodeRewardClaimAction(Mission m, Action<bool> onComplete, Action updateUIDelegate)
        {
            Action<bool> onCodeComplete = (bool isSkipped) =>
            {
                MonetizrInstance.Instance.OnClaimRewardComplete(m, isSkipped, onComplete, updateUIDelegate);
            };

            return () =>
            {
                MonetizrInstance.Instance.ShowCodeView(onCodeComplete, m);
            };
        }

        internal Action SurveyClaimAction(Mission m, Action<bool> onComplete, Action updateUIDelegate)
        {
            Action<bool> onSurveyComplete = (bool isSkipped) =>
            {
                MonetizrInstance.Instance.OnClaimRewardComplete(m, isSkipped, onComplete, updateUIDelegate);
            };

            return () =>
            {
                if (!m.hasUnitySurvey)
#if UNITY_EDITOR_WIN
                {
                    MonetizrLogger.Print($"Completing survey {m.surveyUrl} on windows");
                    onSurveyComplete.Invoke(false);
                }
#else
                    MonetizrInstance.Instance.ShowSurvey(onSurveyComplete, m);
#endif
                else
                    MonetizrInstance.Instance.ShowUnitySurvey(onSurveyComplete, m);
            };
        }

        internal Action GetEmailGiveawayClaimAction(Mission m, Action<bool> onComplete, Action updateUIDelegate)
        {
            MonetizrInstance.Instance.temporaryRewardTypeSelection = RewardSelectionType.Product;

            bool needToPlayVideo = m.hasVideo;

#if UNITY_EDITOR_WIN
            needToPlayVideo = false;
#endif

            if (m.isVideoShown) needToPlayVideo = false;

            Action<bool> onVideoComplete = (bool isVideoSkipped) =>
            {
                if (MonetizrInstance.Instance.claimForSkippedCampaigns) isVideoSkipped = false;

                if (isVideoSkipped)
                {
                    onComplete?.Invoke(isVideoSkipped);
                    return;
                }

                if (m.campaignServerSettings.GetParam("watch_video_only_once") == "true") m.isVideoShown = true;

                MonetizrInstance.Instance.ShowEnterEmailPanel(
                    (bool isMailSkipped) =>
                    {
                        if (isMailSkipped)
                        {
                            MonetizrMobileAnalytics.TrackEvent(m, m.adPlacement, EventType.ButtonPressSkip);
                            onComplete?.Invoke(isMailSkipped);
                            return;
                        }

                        MonetizrInstance.Instance.WaitForEndRequestAndNotify(onComplete, m, updateUIDelegate);
                    },
                    m,
                    PanelId.GiveawayEmailEnterNotification);

            };

            return () =>
            {
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
            MonetizrInstance.Instance.ShowHTML5((bool isSkipped) => { onComplete(isSkipped); }, m);
        }

        internal void UpdateMissionsRewards(RewardType rt, GameReward reward)
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
                if (!m.isServerCampaignActive) return m;
            }
            return null;
        }

        internal void CreateMissionsFromCampaign (ServerCampaign campaign)
        {
            if (campaign == null) return;
            string serverMissionsJson = campaign.serverSettings.GetParam("custom_missions");
            MonetizrLogger.Print($"Predefined missions from settings: {serverMissionsJson}");
            ServerMissionsHelper ic = null;
            List<MissionDescription> predefinedSponsoredMissions = new List<MissionDescription>();

            try
            {
                ic = ServerMissionsHelper.CreateFromJson(serverMissionsJson);
            }
            catch (Exception e)
            {
                MonetizrLogger.PrintError($"Exception in CreateMissionsFromCampaign with json {serverMissionsJson}\n{e}");
            }

            if (ic == null) return;

            predefinedSponsoredMissions = ic.CreateMissionDescriptions(predefinedSponsoredMissions, campaign.serverSettings);
            MonetizrLogger.Print($"Found {predefinedSponsoredMissions.Count} predefined missions in campaign");

            for (int i = 0; i < predefinedSponsoredMissions.Count; i++)
            {
                MissionDescription missionDescription = predefinedSponsoredMissions[i];
                Mission m = FindMissionInCache(i, missionDescription.missionType, campaign.id, missionDescription.reward);

                if (m == null)
                {
                    m = PrepareNewMission(i, campaign, missionDescription);

                    if (m != null)
                    {
                        serializedMissions.Add(m);
                    }
                    else
                    {
                        MonetizrLogger.PrintError($"Can't create campaign with type {missionDescription.missionType}");
                    }
                }
                else
                {
                    MonetizrLogger.Print($"Found mission {campaign.id}:{i} in local data");
                }

                if (m == null) continue;

                m.campaign = campaign;
                m.sdkVersion = MonetizrSettings.SDKVersion;
                m.rewardAssetName = missionDescription.rewardImage;

                if (!missionDescription.hasUnitySurvey)
                {
                    m.surveyUrl = missionDescription.surveyUrl;
                }
                else
                {
                    m.surveyUrl = m.campaign.GetAsset<string>(AssetsType.SurveyURLString);
                }

                m.hasUnitySurvey = missionDescription.hasUnitySurvey;
                m.surveyId = missionDescription.surveyId;
                m.isServerCampaignActive = true;
                m.isToBeRemoved = false;
                m.campaignServerSettings = m.campaign.serverSettings;
                m.amountOfRVOffersShown = m.campaignServerSettings.GetIntParam("amount_of_rv_offers", -1);
                m.amountOfNotificationsSkipped = m.campaignServerSettings.GetIntParam("startup_skipped_notifications", int.MaxValue - 1);
                m.isVideoShown = false;
                m.isDisabled = true;
                m.activateAfter = missionDescription.activateAfter;
                m.brandName = m.campaign.GetAsset<string>(AssetsType.BrandTitleString);
                m.isDeactivatedByCondition = false;

                if (!string.IsNullOrEmpty(missionDescription.activateConditions))
                {
                    m.conditions = MonetizrUtils.ParseConditionsString(missionDescription.activateConditions);

                    if (!campaign.AreConditionsTrue(m.conditions))
                    {
                        m.isDisabled = true;
                        m.isDeactivatedByCondition = true;
                    }
                }

                m.state = m.isDisabled ? MissionUIState.Hidden : MissionUIState.Visible;
                bool showNotClaimedDisabled = m.campaignServerSettings.GetBoolParam("RewardCenter.show_disabled_missions", true);
                if (showNotClaimedDisabled && m.isDisabled && !m.isDeactivatedByCondition) m.state = MissionUIState.Visible;
            }

            MonetizrLogger.Print($"Loaded {missions.Count} missions from the campaign");
            UpdateMissionsActivity(null);
        }

        internal void LoadSerializedMissions()
        {
            serializedMissions.Load();

            foreach (var m in missions)
            {
                m.isServerCampaignActive = MonetizrInstance.Instance.HasCampaign(m.campaignId);
                m.isToBeRemoved = true;
            }
        }

        internal void SaveAndRemoveUnused()
        {
            missions.RemoveAll(m => m.isToBeRemoved);
            serializedMissions.SaveAll();
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
                if (includeDisabled) disabled = false;
                return m.isSponsored &&
                        m.isClaimed != ClaimState.Claimed &&
                        !disabled &&
                        IsActiveByTime(m) &&
                        m.isServerCampaignActive &&
                        m.autoStartAfter == -1;
            });
        }

        internal List<Mission> GetAllMissions ()
        {
            return missions.FindAll((Mission m) =>
            {
                return m.isSponsored &&
                        m.type != MissionType.ActionReward &&
                        IsActiveByTime(m) &&
                        m.isServerCampaignActive &&
                        m.autoStartAfter == -1;
            });
        }

        internal List<Mission> GetMissionsForRewardCenter(ServerCampaign campaign, bool includeDisabled = false)
        {
            if (campaign == null) return null;
            var res = GetMissionsForRewardCenter(includeDisabled);
            return res?.FindAll((Mission m) => m.campaignId == campaign.id);
        }

        internal List<Mission> GetAllMissions (ServerCampaign campaign)
        {
            if (campaign == null) return null;
            var res = GetAllMissions();
            return res?.FindAll((Mission m) => m.campaignId == campaign.id);
        }

        internal int GetActiveMissionsNum(ServerCampaign campaign)
        {
            var res = GetMissionsForRewardCenter(campaign);
            return res?.Count ?? 0;
        }

        internal int GetActiveMissionsNum()
        {
            return GetMissionsForRewardCenter(false).Count;
        }

        internal bool UpdateMissionsActivity(Mission finishedMission)
        {
            bool isUpdateNeeded = finishedMission != null;

            if (finishedMission != null) MonetizrLogger.Print($"-----UpdateMissionsActivity for {finishedMission.serverId}");

            foreach (var m in missions)
            {
                if (m == finishedMission) continue;
                if (!m.isDisabled) continue;
                if (m.isClaimed == ClaimState.Claimed) continue;

                bool hasActivateAfter = m.activateAfter.Count > 0;

                MonetizrLogger.Print($"-----Updating activity for {m.serverId} has {hasActivateAfter} {m.isDeactivatedByCondition}");

                if (hasActivateAfter && m.activateAfter.FindIndex(_id => _id == m.serverId) > 0)
                {
                    MonetizrLogger.PrintWarning($"Mission id {m.serverId} activate after itself!");
                    hasActivateAfter = false;
                }

                if (m.isDeactivatedByCondition)
                {
                    if (!m.campaign.AreConditionsTrue(m.conditions)) continue;

                    isUpdateNeeded = true;
                    m.isDisabled = false;
                    m.state = MissionUIState.ToBeShown;
                    m.isDeactivatedByCondition = false;
                    continue;
                }

                if (!hasActivateAfter)
                {
                    if (m.isDisabled) isUpdateNeeded = true;

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
                        MonetizrLogger.Print($"------Mission {id} disabled because {_m.serverId} is not active");
                    }
                }

                if (!shouldBeDisabled)
                {
                    if (m.isDisabled) isUpdateNeeded = true;
                    m.isDisabled = shouldBeDisabled;
                    m.state = MissionUIState.ToBeShown;
                }
            }

            if (isUpdateNeeded) serializedMissions.SaveAll();

            return isUpdateNeeded;
        }

    }

}