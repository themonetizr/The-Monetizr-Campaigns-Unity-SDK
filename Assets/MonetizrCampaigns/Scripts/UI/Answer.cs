using System;

namespace Monetizr.SDK
{
    internal partial class MonetizrUnitySurvey
    {
        [Serializable]
        internal class Answer
        {
            public string id;
            public string text;
            public bool disabled;
            public bool requiredAnswer = false;
            public string image;

            [NonSerialized] internal MonetizrSurveyAnswer answerRoot;
            [NonSerialized] internal Question question;
            [NonSerialized] internal string response;
            [NonSerialized] internal Survey survey;

            public string GetVariableName()
            {
                return question.oneTimeQuestion ? $"{question.id}-{id}" : $"{survey.settings.id}-{question.id}-{id}";
            }
        }
    }

}