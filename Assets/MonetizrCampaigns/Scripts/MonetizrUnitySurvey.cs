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
        public ScrollRect scroll;
        public MonetizrSurveyQuestionRoot monetizrQuestionRoot;
        public RectTransform contentRoot;
        public MonetizrSurveyAnswer answerRadioButtonPrefab;

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


        //TODO list
        //- moving list back and forward
        //- create layout grid for position questions
        //
        //- create prefabs for questions
        //- create prefabs for different type of answers
        //- clone elements into question layout
        //
        private void LoadSurvey(Mission m)
        {
            var surveysContent = m.surveyUrl.Replace('\'', '\"');

            Log.PrintWarning($"{m.surveyId}");
            Log.PrintWarning($"{surveysContent}");

            var surveys = JsonUtility.FromJson<Surveys>(surveysContent);

            if (surveys.surveys.Count == 1)
                currentSurvey = surveys.surveys[0];
            else
                currentSurvey = surveys.surveys.Find(s => s.settings.id == m.surveyId);

            if(currentSurvey == null)
            {
                Log.PrintWarning($"{m.surveyId} not found in surveys!");
                OnButtonClick();
                return;
            }

            float width = 0;
            int id = 0;
            currentSurvey.questions.ForEach(q =>
            {
                var qObj = GameObject.Instantiate<GameObject>(monetizrQuestionRoot.gameObject, contentRoot);

                var questionRoot = qObj.GetComponent<MonetizrSurveyQuestionRoot>();

                questionRoot.question.text = currentSurvey.questions[0].text;
                questionRoot.id = currentSurvey.questions[0].id;
                //width += questionRoot.rectTransform.sizeDelta.x;

                q.answers.ForEach(a =>
                {
                    var aObj = GameObject.Instantiate<GameObject>(answerRadioButtonPrefab.gameObject, questionRoot.rectTransform);

                    var answerRoot = aObj.GetComponent<MonetizrSurveyAnswer>();

                    answerRoot.answer.text = a.text;
                    answerRoot.id = a.id;
                });

                width += 700;
            });

            contentRoot.sizeDelta = new Vector2(width,0);
            
                

            print(currentSurvey.settings.id);
            print(currentSurvey.questions[0].text);
            print(currentSurvey.questions[0].id);
            print(currentSurvey.questions[0].answers[0].text);

        }

        public void OnBackButton()
        {
            scroll.horizontalNormalizedPosition = 0.0f;
        }

        public void OnNextButton()
        {
            scroll.horizontalNormalizedPosition = 0.2f;
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