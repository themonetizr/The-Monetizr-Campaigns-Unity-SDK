using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

        public Button backButton;
        public Button nextButton;

        public Text nextButtonText;

        private int currentQuestion = 0;
        private int nextQuestion = 1;

        private AdType adType;

        private Surveys surveys;
        private Survey currentSurvey;

        private enum State
        {
            Idle,
            Moving
        }

        private State state = State.Idle;
        private float progress = 0.0f;


        [Serializable]
        internal class Surveys
        {
            public List<Survey> surveys = new List<Survey>();
        }

        [Serializable]
        internal class Survey
        {
            public Settings settings = new Settings();
            public List<Question> questions = new List<Question>();
        }

        [Serializable]
        internal class Settings
        {
            public string id;
        }

        [Serializable]
        internal class Question
        {
            public string id;
            public string text;
            public string type;
            public string picture;
            public bool randomOrder;

            public List<Answer> answers = new List<Answer>();
            
            [NonSerialized] internal MonetizrSurveyQuestionRoot questionRoot;
        }

        [Serializable]
        internal class Answer
        {
            public string id;
            public string text;

            [NonSerialized] internal MonetizrSurveyAnswer answerRoot;
            [NonSerialized] internal Question question;
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
             
        private void LoadSurvey(Mission m)
        {
            var surveysContent = m.surveyUrl.Replace('\'', '\"');

            Log.PrintWarning($"{m.surveyId}");
            Log.PrintWarning($"{surveysContent}");

            surveys = JsonUtility.FromJson<Surveys>(surveysContent);

            if (surveys.surveys.Count == 1)
                currentSurvey = surveys.surveys[0];
            else
                currentSurvey = surveys.surveys.Find(s => s.settings.id == m.surveyId);

            if(currentSurvey == null)
            {
                Log.PrintWarning($"{m.surveyId} not found in surveys!");
                OnSkipButton();
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
                q.questionRoot = questionRoot;
                //width += questionRoot.rectTransform.sizeDelta.x;

                if (q.randomOrder)
                {
                    ShuffleAnswersList(q);
                }

                q.answers.ForEach(a =>
                {
                    var aObj = GameObject.Instantiate<GameObject>(answerRadioButtonPrefab.gameObject, questionRoot.rectTransform);

                    var answerRoot = aObj.GetComponent<MonetizrSurveyAnswer>();

                    answerRoot.answer.text = a.text;
                    answerRoot.id = a.id;
                    answerRoot.toggle.isOn = false;
                    
                    a.answerRoot = answerRoot;
                    a.question = q;

                    answerRoot.toggle.onValueChanged.AddListener(delegate {
                        OnAnswerButton(a);
                    });

                    if (q.type == "one")
                    {
                        answerRoot.toggle.group = questionRoot.toggleGroup;
                    }
                });

                width += 700;
            });

            contentRoot.sizeDelta = new Vector2(width,0);

            backButton.interactable = false;
            nextButton.interactable = false;

            state = State.Idle;

            print(currentSurvey.settings.id);
            print(currentSurvey.questions[0].text);
            print(currentSurvey.questions[0].id);
            print(currentSurvey.questions[0].answers[0].text);

        }

        private static void ShuffleAnswersList(Question q)
        {
            for (int i = 0; i < q.answers.Count; i++)
            {
                var temp = q.answers[i];
                int randomIndex = UnityEngine.Random.Range(i, q.answers.Count);
                q.answers[i] = q.answers[randomIndex];
                q.answers[randomIndex] = temp;
            }
        }

        internal void OnAnswerButton(Answer a)
        {
            nextButton.interactable = true;
        }

        public void OnBackButton()
        {
            if (state == State.Moving)
                return;

            nextQuestion = Mathf.Clamp(currentQuestion-1,0, currentSurvey.questions.Count);

            state = State.Moving;

            progress = 0.0f;

            
        }

        public void OnNextButton()
        {
            if (state == State.Moving)
                return;
                        
            //submit
            if(currentQuestion == currentSurvey.questions.Count-1)
            {

                return;
            }


            nextQuestion = currentQuestion+1;

            state = State.Moving;

            progress = 0.0f;

            Log.Print("----------------ON NEXT");
            //scroll.horizontalNormalizedPosition = 0.2f;
        }

        public void UpdateButtons()
        {
            Log.Print($"----------------UpdateButtons {currentQuestion}");

            //almost finished - change next to submit
            if (currentQuestion == currentSurvey.questions.Count-1)
            {
                nextButtonText.text = "Submit";
            }
            else
            {
                nextButtonText.text = "Next";
            }

           
            backButton.interactable = currentQuestion != 0;
            nextButton.interactable = false;


        }

        public void Update()
        {
            if (state != State.Moving)
                return;

            progress += Time.deltaTime/1.0f;

            //Debug.Log($"----------------PROGRESS {progress} {Time.deltaTime}");

            float p1 = (float)currentQuestion / (float)currentSurvey.questions.Count;
            float p2 = (float)nextQuestion / (float)currentSurvey.questions.Count;

            scroll.horizontalNormalizedPosition = Mathf.Lerp(p1,p2,Tween(progress));

            if(progress > 1.0f)
            {
                progress = 0;
                scroll.horizontalNormalizedPosition = p2;
                state = State.Idle;
                currentQuestion = nextQuestion;

                UpdateButtons();
            }
        }

        public void OnSkipButton()
        {

            //MonetizrManager.Analytics.TrackEvent("Minigame pressed", currentMission);
            //MonetizrManager.ShowRewardCenter(null);
            MonetizrManager.Analytics.TrackEvent("Survey skipped", currentMission);

            MonetizrManager.CallUserDefinedEvent(currentMission.campaignId, NielsenDar.GetPlacementName(AdType.Survey), MonetizrManager.EventType.ButtonPressSkip);

            isSkipped = true;

            SetActive(false);
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