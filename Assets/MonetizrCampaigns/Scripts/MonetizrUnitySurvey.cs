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
        public MonetizrSurveyAnswer answerEditablePrefab;

        public Button backButton;
        public Button nextButton;

        public Text nextButtonText;

        public Text progressText;

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
            bool isFirstQuestionEmpty = false;

            currentSurvey.questions.ForEach(q =>
            {
                var qObj = GameObject.Instantiate<GameObject>(monetizrQuestionRoot.gameObject, contentRoot);

                var questionRoot = qObj.GetComponent<MonetizrSurveyQuestionRoot>();

                questionRoot.question.text = q.text;
                questionRoot.id = q.id;
                q.questionRoot = questionRoot;
                //width += questionRoot.rectTransform.sizeDelta.x;

                if (q.randomOrder)
                {
                    ShuffleAnswersList(q);
                }

                //no vertical truncate and upper left aligment
                if (id == 0 && q.answers.Count == 0)
                {
                    q.questionRoot.question.verticalOverflow = VerticalWrapMode.Overflow;
                    q.questionRoot.question.alignment = TextAnchor.UpperLeft;
                    isFirstQuestionEmpty = true;
                }

                q.answers.ForEach(a =>
                {
                    GameObject aObj = null;

                    if (q.answers.Count > 1 && q.type == "editable")
                        q.type = "one";

                    if (q.type == "editable")
                        aObj = GameObject.Instantiate<GameObject>(answerEditablePrefab.gameObject, questionRoot.rectTransform);
                    else
                        aObj = GameObject.Instantiate<GameObject>(answerRadioButtonPrefab.gameObject, questionRoot.rectTransform);

                    var answerRoot = aObj.GetComponent<MonetizrSurveyAnswer>();

                    answerRoot.answer.text = a.text;
                    answerRoot.id = a.id;

                    if (answerRoot.toggle != null)
                    {
                        answerRoot.toggle.isOn = false;
                        answerRoot.toggle.gameObject.name = $"Q:{q.id}:A:{a.id}";
                        answerRoot.toggle.onValueChanged.AddListener(delegate
                        {
                            OnAnswerButton(a);
                        });
                    }

                    a.answerRoot = answerRoot;
                    a.question = q;

                    

                    if (q.type == "one")
                    {
                        answerRoot.toggle.group = questionRoot.toggleGroup;
                    }
                });

                width += 740;
                id++;
            });

            contentRoot.sizeDelta = new Vector2(width,0);

            backButton.interactable = false;

            nextButton.interactable = isFirstQuestionEmpty;

            state = State.Idle;

            

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

        internal void OnAnswerButton(Answer pressedAnswer)
        {
            //Log.Print($"------>>>>>>>{pressedAnswer.answerRoot.toggle.isOn} {pressedAnswer.answerRoot.toggle.gameObject.name}");

            //nextButton.interactable = true;

            UpdateButtons();
        }

        public void OnBackButton()
        {
            if (state == State.Moving)
                return;

            nextQuestion = Mathf.Clamp(currentQuestion-1,0, currentSurvey.questions.Count);

            state = State.Moving;

            progress = 0.0f;

            backButton.interactable = false;
            nextButton.interactable = false;

        }

        public void OnNextButton()
        {
            if (state == State.Moving)
                return;
                        
            //submit
            if(currentQuestion == currentSurvey.questions.Count-1)
            {
                Complete();
                return;
            }


            nextQuestion = currentQuestion+1;
                        
            state = State.Moving;

            progress = 0.0f;

            //Log.Print("----------------ON NEXT");

            backButton.interactable = false;
            nextButton.interactable = false;

        }

        public void UpdateButtons()
        {
            //Log.Print($"----------------UpdateButtons {currentQuestion}");

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

            var question = currentSurvey.questions[currentQuestion];

            bool isSelected = false;

            if (question.type != "editable")
            {
                question.answers.ForEach(a =>
                {
                    //Log.Print($"------{a.answerRoot.toggle.isOn} {a.answerRoot.toggle.gameObject.name}");

                    if (a.answerRoot.toggle.isOn)
                    {
                        isSelected = true;
                    }
                });

                if (question.answers.Count == 0)
                    isSelected = true;
            }
            else
            {
                isSelected = true;
            }

            nextButton.interactable = isSelected;

            progressText.text = $"{currentQuestion + 1} / {currentSurvey.questions.Count}";
        }

        public void Update()
        {
           
            if (state != State.Moving)
                return;

            progress += Time.deltaTime/0.4f;

            //Debug.Log($"----------------PROGRESS {progress} {Time.deltaTime}");

            float p1 = (float)currentQuestion /(currentSurvey.questions.Count-1);
            float p2 = (float)nextQuestion / (currentSurvey.questions.Count-1);

            //Log.Print($"----------------PROGRESS {p1} {p2}");

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

        public void _OnSkipButton()
        {

            //MonetizrManager.Analytics.TrackEvent("Minigame pressed", currentMission);
            //MonetizrManager.ShowRewardCenter(null);
            MonetizrManager.Analytics.TrackEvent("Survey skipped", currentMission);

            MonetizrManager.CallUserDefinedEvent(currentMission.campaignId, NielsenDar.GetPlacementName(AdType.Survey), MonetizrManager.EventType.ButtonPressSkip);

            isSkipped = true;

            SetActive(false);
        }

        public void OnSkipButton()
        {
            MonetizrManager.ShowMessage((bool _isSkipped) =>
            {
                if (!_isSkipped)
                {
                    _OnSkipButton();
                }
                
            },
                    currentMission,
                    PanelId.SurveyCloseConfirmation);
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