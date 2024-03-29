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
        public Button closeButton;
        public Image logo;
        public ScrollRect scroll;
        public RectTransform scrollRect;
        public MonetizrSurveyQuestionRoot monetizrQuestionRoot;
        public MonetizrSurveyQuestionRoot monetizrQuestionRootLandscape;
        public RectTransform contentRoot;
        public MonetizrSurveyAnswer answerRadioButtonPrefab;
        public MonetizrSurveyAnswer answerEditablePrefab;
        public MonetizrSurveyAnswer answerCheckButtonPrefab;
        public Image rewardImage;

        public Button backButton;
        public Button nextButton;

        public Text nextButtonText;

        public Text progressText;

        public Image progressImage;

        public Animator crossButtonAnimator;

        private int currentQuestion = 0;
        private int nextQuestion = 1;

        //private AdPlacement adType;

        private Surveys surveys;
        private Survey currentSurvey;

        private string submitText;
        private string nextText;


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
            public bool showLogo;
            public bool hideReward;
            public bool editableFieldIsMandatory;
        }

        internal enum Type
        {
            One,
            Multiple,
            Editable,

        }

        [Serializable]
        internal class Question
        {
            public string id;
            public string text;
            public string type;
            public string picture;
            public bool randomOrder;
            public Type enumType;

            public List<Answer> answers = new List<Answer>();

            [NonSerialized] internal MonetizrSurveyQuestionRoot questionRoot;

            internal Type ParseType(string type)
            {
                if (type == null)
                    return Type.One;

                Dictionary<string, Type> types = new Dictionary<string, Type>()
                {
                    { "intro", Type.One },
                    { "one", Type.One },
                    { "multiple", Type.Multiple },
                    { "editable", Type.Editable },
                    { "sumbit", Type.One },
                };

                if (types.ContainsKey(type))
                    return types[type];

                return Type.One;
            }
        }

        [Serializable]
        internal class Answer
        {
            public string id;
            public string text;
            public bool disabled;

            [NonSerialized] internal MonetizrSurveyAnswer answerRoot;
            [NonSerialized] internal Question question;
            [NonSerialized] internal string response;
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

        internal override AdPlacement? GetAdPlacement()
        {
            return AdPlacement.Survey;
        }

        internal override void PreparePanel(PanelId id, Action<bool> onComplete, Mission m)
        {
            this._onComplete = onComplete;
            this.panelId = id;
            this.currentMission = m;

            if (!LoadSurvey(m))
                return;
            
            bool hasLogo = m.campaign.TryGetAsset(AssetsType.BrandRewardLogoSprite, out Sprite res);
            
            logo.sprite = res;
            logo.gameObject.SetActive(hasLogo && currentSurvey.settings.showLogo);
            
            int closeButtonDelay = m.campaignServerSettings.GetIntParam("SurveyUnityView.close_button_delay", 3);

            StartCoroutine(ShowCloseButton(closeButtonDelay));
        }

        IEnumerator ShowCloseButton(float time)
        {
            yield return new WaitForSeconds(time);

            crossButtonAnimator.enabled = true;
        }


        private bool LoadSurvey(Mission m)
        {
            nextText = m.campaignServerSettings.GetParam("SurveyUnityView.next_text", "Next");
            submitText = m.campaignServerSettings.GetParam("SurveyUnityView.submit_text", "Submit");

            var surveysContent = m.surveyUrl.Replace('\'', '\"');

            Log.PrintV($"{m.surveyId}");

            if (!Utils.ValidateJson(surveysContent))
            {
                Log.PrintError($"Json isn't properly formatted.");
                Log.PrintWarning($"{surveysContent}");
            }

            surveys = JsonUtility.FromJson<Surveys>(surveysContent);

            if (surveys.surveys.Count == 1)
                currentSurvey = surveys.surveys[0];
            else
                currentSurvey = surveys.surveys.Find(s => s.settings.id == m.surveyId);

            if (currentSurvey == null)
            {
                Log.PrintError($"{m.surveyId} not found in surveys!");
                _OnSkipButton();
                return false;
            }

            float width = 0;
            float height = -620;
            int id = 0;
            //bool isFirstQuestionEmpty = false;

            currentSurvey.questions.ForEach(q =>
            {
                var qObj = GameObject.Instantiate<GameObject>(Utils.IsInLandscapeMode() ? 
                    monetizrQuestionRootLandscape.gameObject :
                    monetizrQuestionRoot.gameObject, contentRoot);

                var questionRoot = qObj.GetComponent<MonetizrSurveyQuestionRoot>();

                questionRoot.question.text = $"{PanelTextItem.ReplacePredefinedItemsInText(m, q.text)}";
                questionRoot.id = q.id;
                q.questionRoot = questionRoot;
                //width += questionRoot.rectTransform.sizeDelta.x;

                q.enumType = q.ParseType(q.type);

                if (q.randomOrder)
                {
                    ShuffleAnswersList(q);
                }

                q.questionRoot.verticalLayout.childAlignment = TextAnchor.MiddleCenter;

                //no vertical truncate and upper left aligment
                if (id == 0 && q.answers.Count == 0)
                {
                    q.questionRoot.question.verticalOverflow = VerticalWrapMode.Overflow;

                    //if (Utils.IsInLandscapeMode())
                    //    q.questionRoot.verticalLayout.childAlignment = TextAnchor.UpperCenter;
                    //q.questionRoot.question.alignment = TextAnchor.UpperLeft;
                    //isFirstQuestionEmpty = true;
                }

                if (Utils.IsInLandscapeMode())
                {
                    q.questionRoot.verticalLayout.childAlignment = TextAnchor.UpperCenter;

                    if (id == 0 && q.answers.Count == 0)
                        q.questionRoot.question.alignment = TextAnchor.UpperLeft;
                }


                int answerNum = 0;

                q.answers.ForEach(a =>
                {
                    answerNum++;
                    if (answerNum >= 7)
                    {
                        a.disabled = true;
                        return;
                    }

                    GameObject aObj = null;

                    if (q.answers.Count > 1 && q.enumType == Type.Editable)
                        q.enumType = Type.One;

                    if (questionRoot.gridLayoutRoot == null)
                        questionRoot.gridLayoutRoot = questionRoot.rectTransform;

                    if (q.enumType == Type.Editable)
                        aObj = GameObject.Instantiate<GameObject>(answerEditablePrefab.gameObject, questionRoot.rectTransform);
                    else if(q.enumType == Type.Multiple)
                        aObj = GameObject.Instantiate<GameObject>(answerCheckButtonPrefab.gameObject, questionRoot.gridLayoutRoot);
                    else
                        aObj = GameObject.Instantiate<GameObject>(answerRadioButtonPrefab.gameObject, questionRoot.gridLayoutRoot);

                    var answerRoot = aObj.GetComponent<MonetizrSurveyAnswer>();

                    answerRoot.answer.text = PanelTextItem.ReplacePredefinedItemsInText(m, a.text);
                    answerRoot.id = a.id;


                    if (q.enumType != Type.Editable)
                    {
                        answerRoot.toggle.isOn = false;
                        answerRoot.toggle.gameObject.name = $"Q:{q.id}:A:{a.id}";
                        answerRoot.toggle.onValueChanged.AddListener(delegate
                        {
                            OnAnswerButton(a);
                        });
                    }
                    else
                    {
                        answerRoot.inputField.onEndEdit.AddListener(delegate
                        {
                            OnAnswerButton(a);
                        });

                        answerRoot.inputField.onValueChanged.AddListener(delegate
                        {
                            OnAnswerButton(a);
                        });
                    }

                    a.answerRoot = answerRoot;
                    a.question = q;
                    a.disabled = false;



                    if (q.enumType == Type.One)
                    {
                        answerRoot.toggle.group = questionRoot.toggleGroup;
                    }
                });

                if(Utils.IsInLandscapeMode())
                    height += 620;
                else
                    width += 1000;
                
                id++;
            });

            contentRoot.sizeDelta = new Vector2(width, height);

            //backButton.interactable = false;
            //nextButton.interactable = isFirstQuestionEmpty;

            state = State.Idle;

            float step = 1.0f / (currentSurvey.questions.Count);

            progressImage.fillAmount = step;


            //-----

            rewardImage.enabled = !currentSurvey.settings.hideReward;

            if (!currentSurvey.settings.hideReward)
            {
                rewardImage.sprite = MissionsManager.GetMissionRewardImage(m);;
            }

            UpdateButtons();

            return true;
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
            var qType = pressedAnswer.question.enumType;

            if (qType == Type.Editable)
                pressedAnswer.response = pressedAnswer.answerRoot.inputField.text;
            else if (pressedAnswer.answerRoot.toggle.isOn)
                pressedAnswer.response = "true";
            else
                pressedAnswer.response = "";

            if (qType != Type.Editable)
                pressedAnswer.answerRoot.greenBackground.enabled = pressedAnswer.answerRoot.toggle.isOn;

            if (qType == Type.One && CanMoveForward(pressedAnswer.question))
                OnNextButton();
            else
                UpdateButtons();
        }

        public void OnBackButton()
        {
            if (state == State.Moving)
                return;

            nextQuestion = Mathf.Clamp(currentQuestion - 1, 0, currentSurvey.questions.Count);

            state = State.Moving;

            progress = 0.0f;

        }

        public void OnNextButton()
        {
            if (state == State.Moving)
                return;

            //submit
            if (currentQuestion == currentSurvey.questions.Count - 1)
            {
                Complete();
                return;
            }

            var question = currentSurvey.questions[currentQuestion];



            nextQuestion = currentQuestion + 1;

            state = State.Moving;

            progress = 0.0f;
        }

        internal bool CanMoveForward(Question question)
        {
            bool result = false;

            if (question.enumType == Type.Editable)
            {
                if (currentSurvey.settings.editableFieldIsMandatory && question.answers.Count > 0)
                {
                    return question.answers[0].answerRoot.inputField.text.Length > 0;
                }
                else
                {
                    return true;
                }
            }

            if (question.answers.Count == 0)
                return true;

            question.answers.ForEach(a =>
            {
                if (!a.disabled && a.answerRoot.toggle.isOn)
                {
                    result = true;
                    return;
                }
            });

            return result;
        }

        public void UpdateButtons()
        {
            //almost finished - change next to submit

            nextButtonText.text = currentQuestion == currentSurvey.questions.Count - 1 ? submitText : nextText;

            backButton.interactable = currentQuestion != 0;

            var question = currentSurvey.questions[currentQuestion];
            

            nextButton.interactable = CanMoveForward(question);

            progressText.text = $"{currentQuestion + 1} / {currentSurvey.questions.Count}";
        }

        internal float scrollNormalizedPosition
        {
            get => Utils.IsInLandscapeMode() ? 1.0f - scroll.verticalNormalizedPosition : scroll.horizontalNormalizedPosition;

            set
            {
                if(Utils.IsInLandscapeMode())
                    scroll.verticalNormalizedPosition = 1.0f - value;
                else
                    scroll.horizontalNormalizedPosition = value;
            }
        }

        public void Update()
        {

            if (state != State.Moving)
                return;

            progress += Time.deltaTime / 0.4f;

            float p1 = (float)currentQuestion / (currentSurvey.questions.Count - 1);
            float p2 = (float)nextQuestion / (currentSurvey.questions.Count - 1);


            scrollNormalizedPosition = Mathf.Lerp(p1, p2, Tween(progress));

            float step = 1.0f / (currentSurvey.questions.Count);

            progressImage.fillAmount = Mathf.Lerp(step, 1.0f, scrollNormalizedPosition);

            if (progress > 1.0f)
            {
                progress = 0;
                scrollNormalizedPosition = p2;
                state = State.Idle;
                currentQuestion = nextQuestion;

                UpdateButtons();
            }
        }

        
        public void _OnSkipButton()
        {
            isSkipped = true;

            HideSelf();
        }

        public void OnSkipButton()
        {
            MonetizrManager.ShowMessage((bool _isSkipped) =>
            {
                if (!_isSkipped)
                {
                    _OnSkipButton();
                }

            }, currentMission, PanelId.SurveyCloseConfirmation);
        }

        private void HideSelf()
        {
            SetActive(false);

            //MonetizrManager.ShowRewardCenter(null);
        }

        internal void Complete()
        {
            isSkipped = false;

            HideSelf();

            SubmitResponses();
        }

        internal void SubmitResponses()
        {
            var campaign = currentMission.campaign;

            currentSurvey.questions.ForEach(q =>
            {
                q.answers.ForEach(a =>
                {
                    if (string.IsNullOrEmpty(a.response))
                        return;

                    Dictionary<string, string> p = new Dictionary<string, string>();

                    p.Add("survey_id", currentSurvey.settings.id);
                    p.Add("question_id", q.id);
                    p.Add("answer_id", a.id);
                    p.Add("answer_response", a.response);
                    p.Add("answer_text", a.text);
                    p.Add("question_text", q.text);
                    MonetizrManager.Analytics._TrackEvent("Survey answer", campaign, false, p);

                    var varName = $"{currentSurvey.settings.id}-{q.id}-{a.id}";
                    MonetizrManager.Instance.localSettings.GetSetting(campaign.id).settings[varName] = a.response;
                });
            });


            MonetizrManager.Instance.localSettings.SaveData();
        }


        internal override void FinalizePanel(PanelId id)
        {

        }
    }

}