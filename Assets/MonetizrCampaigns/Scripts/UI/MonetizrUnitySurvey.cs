using Monetizr.SDK.Analytics;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.SDK.UI
{
    internal partial class MonetizrUnitySurvey : PanelController
    {
        public Button closeButton;
        public Image logo;
        public ScrollRect scroll;
        public RectTransform scrollRect;
        public MonetizrSurveyQuestionRoot monetizrQuestionRoot;
        public MonetizrSurveyQuestionRoot monetizrQuestionRootLandscape;
        public MonetizrSurveyQuestionRoot monetizrImageQuestionRoot;
        public RectTransform contentRoot;
        public MonetizrSurveyAnswer answerRadioButtonPrefab;
        public MonetizrSurveyAnswer answerEditablePrefab;
        public MonetizrSurveyAnswer answerCheckButtonPrefab;
        public MonetizrSurveyAnswer answerImageButtonPrefab;
        public Image rewardImage;
        public Button backButton;
        public Button nextButton;
        public Text nextButtonText;
        public Text progressText;
        public Image progressImage;
        public Animator crossButtonAnimator;

        private Question currentQuestion = null;
        private Question startQuestion = null;
        private Question endQuestion = null;
        private Question nextQuestion = null;
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

            internal int activeQuestionsAmount = 0;
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

        void SetProgress(float a)
        {
            
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

            MonetizrLogger.Print($"{m.surveyId}");

            if (!MonetizrUtils.ValidateJson(surveysContent))
            {
                MonetizrLogger.PrintError($"Json isn't properly formatted.");
                MonetizrLogger.PrintWarning($"{surveysContent}");
            }

            surveys = JsonUtility.FromJson<Surveys>(surveysContent);

            if (surveys.surveys.Count == 1)
                currentSurvey = surveys.surveys[0];
            else
                currentSurvey = surveys.surveys.Find(s => s.settings.id == m.surveyId);

            if (currentSurvey == null)
            {
                MonetizrLogger.PrintError($"{m.surveyId} not found in surveys!");
                _OnSkipButton();
                return false;
            }

            float width = 0;
            float height = -620;
            int id = -1;
            var campaignSettings = MonetizrInstance.Instance.localSettings.GetSetting(m.campaign.id).settings;

            currentSurvey.activeQuestionsAmount = 0;

            currentSurvey.questions.ForEach(q =>
            {
                q.answers.ForEach(a =>
                {
                    a.question = q;
                    a.survey = currentSurvey;

                    if (!string.IsNullOrEmpty(a.image))
                    {
                        q.hasImages = true;
                    }

                    if (a.requiredAnswer)
                    {
                        q.isQuiz = true;
                    }

                });
            });

            Question lastQuestion = null;

            currentSurvey.questions.ForEach(q =>
           {
               id++;
               
               if (q.IsQuestionAlreadyAnswered(campaignSettings))
               {
                   return;
               }
               
               q.questionNumber = ++currentSurvey.activeQuestionsAmount;

               if (startQuestion == null) startQuestion = q;

               q.previousQuestion = lastQuestion;

               if (lastQuestion != null) lastQuestion.nextQuestion = q;

               var qObj = GameObject.Instantiate<GameObject>(MobileUtils.IsInLandscapeMode() ?
                   monetizrQuestionRootLandscape.gameObject :
                   monetizrQuestionRoot.gameObject, contentRoot);

               var questionRoot = qObj.GetComponent<MonetizrSurveyQuestionRoot>();

               questionRoot.question.text = $"{PanelTextItem.ReplacePredefinedItemsInText(m, q.text)}";
               questionRoot.id = q.id;
               q.questionRoot = questionRoot;

               q.enumType = q.ParseType(q.type);

               if (q.randomOrder)
               {
                   ShuffleAnswersList(q);
               }

               q.questionRoot.verticalLayout.childAlignment = TextAnchor.MiddleCenter;

               if (id == 0 && q.answers.Count == 0)
               {
                   q.questionRoot.question.verticalOverflow = VerticalWrapMode.Overflow;
               }

               questionRoot.imageGridLayoutRoot.gameObject.SetActive(false);

               if (q.hasImages)
               {
                   q.questionRoot.verticalLayout.childAlignment = TextAnchor.UpperCenter;

                   questionRoot.gridLayoutRoot = questionRoot.imageGridLayoutRoot;
                   questionRoot.imageGridLayoutRoot.gameObject.SetActive(true);
               }

               if (MobileUtils.IsInLandscapeMode())
               {
                   q.questionRoot.verticalLayout.childAlignment = TextAnchor.UpperCenter;

                   if (id == 0 && q.answers.Count == 0)
                       q.questionRoot.question.alignment = TextAnchor.UpperLeft;
               }

               int answerNum = 0;

               q.answers.ForEach(a =>
               {
                   a.question = q;
                   a.survey = currentSurvey;

                   answerNum++;
                   if (answerNum >= 7)
                   {
                       a.disabled = true;
                       return;
                   }

                   GameObject aObj = null;
                   MonetizrSurveyAnswer answerRoot = null;

                   if (q.answers.Count > 1 && q.enumType == Type.Editable)
                       q.enumType = Type.One;

                   if (questionRoot.gridLayoutRoot == null)
                       questionRoot.gridLayoutRoot = questionRoot.rectTransform;

                   if (q.hasImages)
                   {
                       aObj = GameObject.Instantiate<GameObject>(answerImageButtonPrefab.gameObject,
                           questionRoot.gridLayoutRoot);
                   }
                   else
                   {
                       if (q.enumType == Type.Editable)
                           aObj = GameObject.Instantiate<GameObject>(answerEditablePrefab.gameObject,
                               questionRoot.rectTransform);
                       else if (q.enumType == Type.Multiple)
                           aObj = GameObject.Instantiate<GameObject>(answerCheckButtonPrefab.gameObject,
                               questionRoot.gridLayoutRoot);
                       else
                           aObj = GameObject.Instantiate<GameObject>(answerRadioButtonPrefab.gameObject,
                               questionRoot.gridLayoutRoot);
                   }

                   answerRoot = aObj.GetComponent<MonetizrSurveyAnswer>();

                   answerRoot.answer.text = PanelTextItem.ReplacePredefinedItemsInText(m, a.text);
                   answerRoot.id = a.id;

                   if (q.hasImages)
                   {
                       if (m.campaign.TryGetSpriteAsset(a.image, out var s))
                       {
                           answerRoot.image.sprite = s;
                       }
                   }

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
                   a.disabled = false;

                   if (q.enumType == Type.One)
                   {
                       answerRoot.toggle.group = questionRoot.toggleGroup;
                   }
               });

               if (MobileUtils.IsInLandscapeMode())
                   height += 620;
               else
                   width += 1000;

               lastQuestion = q;
           });

            currentQuestion = startQuestion;
            endQuestion = lastQuestion;

            contentRoot.sizeDelta = new Vector2(width, height);

            state = State.Idle;

            float step = 1.0f / (currentSurvey.activeQuestionsAmount);

            progressImage.fillAmount = step;

            rewardImage.enabled = !currentSurvey.settings.hideReward;

            if (!currentSurvey.settings.hideReward)
            {
                rewardImage.sprite = MissionsManager.GetMissionRewardImage(m); ;
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
            {
                if (!pressedAnswer.question.isQuiz)
                {
                    pressedAnswer.answerRoot.background.sprite =
                        pressedAnswer.answerRoot.toggle.isOn
                            ? pressedAnswer.answerRoot.greenBorder
                            : pressedAnswer.answerRoot.grayBorder;
                }
                else
                {
                    bool isOn = pressedAnswer.answerRoot.toggle.isOn;

                    if(pressedAnswer.requiredAnswer)
                        pressedAnswer.answerRoot.background.sprite =
                         isOn ? pressedAnswer.answerRoot.greenBorder : pressedAnswer.answerRoot.grayBorder;
                    else
                        pressedAnswer.answerRoot.background.sprite =
                            isOn ? pressedAnswer.answerRoot.redBorder : pressedAnswer.answerRoot.grayBorder;

                    pressedAnswer.answerRoot.markImage.sprite =
                        pressedAnswer.requiredAnswer && isOn ? pressedAnswer.answerRoot.greenRoundMark : pressedAnswer.answerRoot.redSquareMark;

                    pressedAnswer.answerRoot.markImage.enabled = isOn;
                }


            }

            if (qType == Type.One && CanMoveForward(pressedAnswer.question) && !pressedAnswer.question.isQuiz)
                OnNextButton();
            else
                UpdateButtons();
        }

        public void OnBackButton()
        {
            if (state == State.Moving)
                return;

            if (currentQuestion.previousQuestion == null)
                return;

            nextQuestion = currentQuestion.previousQuestion;

            state = State.Moving;

            progress = 0.0f;

        }

        public void OnNextButton()
        {
            if (state == State.Moving) return;

            if (currentQuestion.nextQuestion == null)
            {
                Complete();
                return;
            }

            nextQuestion = currentQuestion.nextQuestion;

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

            question.answers.ForEach(a =>
            {
                if (!question.isQuiz)
                    return;

                if (!a.disabled && a.answerRoot.toggle.isOn == false && a.requiredAnswer == true)
                {
                    result = false;
                    return;
                }

                if (!a.disabled && a.answerRoot.toggle.isOn == true && a.requiredAnswer == false)
                {
                    result = false;
                    return;
                }
            });

            return result;
        }

        public void UpdateButtons()
        {
            nextButtonText.text = currentQuestion.nextQuestion == null ? submitText : nextText;
            backButton.interactable = currentQuestion.previousQuestion != null;

            nextButton.interactable = CanMoveForward(currentQuestion);

            progressText.text = $"{currentQuestion.questionNumber} / {currentSurvey.activeQuestionsAmount}";
        }

        internal float scrollNormalizedPosition
        {
            get => MobileUtils.IsInLandscapeMode() ? 1.0f - scroll.verticalNormalizedPosition : scroll.horizontalNormalizedPosition;

            set
            {
                if (MobileUtils.IsInLandscapeMode())
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

            float p1 = (float)(currentQuestion.questionNumber - 1) / (currentSurvey.activeQuestionsAmount - 1);
            float p2 = (float)(nextQuestion.questionNumber - 1) / (currentSurvey.activeQuestionsAmount - 1);

            scrollNormalizedPosition = Mathf.Lerp(p1, p2, Tween(progress));

            float step = 1.0f / (currentSurvey.activeQuestionsAmount);

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
            MonetizrInstance.Instance.ShowMessage((bool _isSkipped) =>
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
            var campaignSettings = MonetizrInstance.Instance.localSettings.GetSetting(campaign.id).settings;

            currentSurvey.questions.ForEach(q =>
            {
                q.answers.ForEach(a =>
                {
                    var variableName = a.GetVariableName();

                    Dictionary<string, string> p = new Dictionary<string, string>();

                    var savedResponse = campaignSettings.GetParam(variableName);

                    if (q.oneTimeQuestion && savedResponse != null)
                    {
                        a.response = savedResponse;
                    }

                    if (string.IsNullOrEmpty(a.response))
                        return;

                    p.Add("survey_id", currentSurvey.settings.id);
                    p.Add("question_id", q.id);
                    p.Add("answer_id", a.id);
                    p.Add("answer_response", a.response);
                    p.Add("answer_text", a.text);
                    p.Add("question_text", q.text);
                    MonetizrInstance.Instance.Analytics._TrackEvent("Survey answer", campaign, false, p);

                    campaignSettings[variableName] = a.response;
                });
            });


            MonetizrInstance.Instance.localSettings.SaveData();
        }

        internal override void FinalizePanel(PanelId id)
        {

        }

    }

}