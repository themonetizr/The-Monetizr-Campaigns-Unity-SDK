using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{
    internal class MonetizrUnitySurvey : PanelController
    {        
        private Mission currentMission;

        public Button closeButton;
        public Image logo;

        AdType adType;

        Survey currentSurvey;

        [Serializable]
        class Surveys
        {
            public List<Survey> surveys = new List<Survey>();
        }

        [Serializable]
        class Survey
        {
            public Settings settings = new Settings();
            public List<Question> questions = new List<Question>();
        }

        [Serializable]
        class Settings
        {
            public string id;
        }

        [Serializable]
        class Question
        {
            public string id;
            public string text;
            public string type;
            public string picture;
            public bool randomOrder;

            public List<Answer> answers = new List<Answer>();
        }

        [Serializable]
        class Answer
        {
            public string id;
            public string text;
        }

        
        void Update()
        {
            

        }

        void SetProgress(float a)
        {
            //uvRect.y = 0.5f * (1.0f - Tween(a));
            //teaserImage.uvRect = uvRect;
        }

        float Tween(float k)
        {
            return 0.5f * (1f - Mathf.Cos(Mathf.PI * k));
        }

        public void OnButtonClick()
        {
            //MonetizrManager.Analytics.TrackEvent("Minigame pressed", currentMission);
            //MonetizrManager.ShowRewardCenter(null);
            MonetizrManager.Analytics.TrackEvent("Survey skipped", currentMission);

            MonetizrManager.CallUserDefinedEvent(currentMission.campaignId, NielsenDar.GetPlacementName(AdType.Survey), MonetizrManager.EventType.ButtonPressSkip);

            isSkipped = true;

            SetActive(false);
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            this.onComplete = onComplete;
            this.panelId = id;
            this.currentMission = m;

            LoadSurvey(m);

            

            logo.sprite = MonetizrManager.Instance.GetAsset<Sprite>(m.campaignId, AssetsType.BrandRewardLogoSprite); ;
            logo.gameObject.SetActive(logo.sprite != null);

            adType = AdType.Minigame;

            MonetizrManager.CallUserDefinedEvent(m.campaignId, NielsenDar.GetPlacementName(adType), MonetizrManager.EventType.Impression);

            MonetizrManager.Analytics.BeginShowAdAsset(adType, m);

            MonetizrManager.Analytics.TrackEvent("Survey started", currentMission);

            //MonetizrManager.Analytics.TrackEvent("Minigame shown", m);
        }

        //{'survey': [{'settings': {'id': 'FebrezePreSurvey1'}, 'questions': [{'id': '1', 'text': 'are you ok?', 'type': 'one', 'answers': [{'id': '1', 'text': 'yes'}, {'id': '2', 'text': 'no'}], 'picture': ''}, {'id': '2', 'text': 'are you ok?', 'type': 'multiple', 'answers': [{'id': '1', 'text': 'yes'}, {'id': '2', 'text': 'no'}], 'picture': ''}, {'id': '3', 'text': 'are you ok?', 'type': 'editable', 'answers': [{'id': '1', 'text': 'could be better'}], 'picture': ''}]}]}

        private void LoadSurvey(Mission m)
        {
            var surveysContent = m.surveyUrl.Replace('\'', '\"');

            Log.PrintWarning($"{m.surveyId}");
            Log.PrintWarning($"{surveysContent}");

            var surveys = JsonUtility.FromJson<Surveys>(surveysContent);

            currentSurvey = surveys.surveys.Find(s => s.settings.id == m.surveyId);

            if(currentSurvey == null)
            {
                Log.PrintWarning($"{m.surveyId} not found in surveys!");
                OnButtonClick();
            }

            print(currentSurvey.settings.id);
            print(currentSurvey.questions[0].text);
            print(currentSurvey.questions[0].id);
            print(currentSurvey.questions[0].answers[0].text);

        }

        internal void Complete()
        {
            isSkipped = false;

            SetActive(false);

            MonetizrManager.Analytics.TrackEvent("Survey completed", currentMission);

            MonetizrManager.CallUserDefinedEvent(currentMission.campaignId, NielsenDar.GetPlacementName(adType), MonetizrManager.EventType.ButtonPressOk);
            //MonetizrManager.ShowCongratsNotification(null, m);
        }


        internal override void FinalizePanel(PanelId id)
        {
            MonetizrManager.Analytics.EndShowAdAsset(adType);
        }
    }

}