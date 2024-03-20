using System;
using System.Collections.Generic;

namespace Monetizr.Campaigns
{
    internal partial class MonetizrUnitySurvey
    {
        [Serializable]
        internal class Question
        {
            public string id;
            public string text;
            public string type;
            public string picture;
            public bool randomOrder;
            public Type enumType;
            public bool oneTimeQuestion;

            public List<Answer> answers = new List<Answer>();

            [NonSerialized] internal MonetizrSurveyQuestionRoot questionRoot;
            public Question previousQuestion;
            public Question nextQuestion;
            public int questionNumber;
            public bool hasImages = false;
            public bool isQuiz;

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

            internal bool IsQuestionAlreadyAnswered(SerializableDictionary<string, string> campaignSettings)
            {
                if (!oneTimeQuestion)
                    return false;

                foreach (var a in answers)
                {
                    var savedResponse = campaignSettings.GetParam(a.GetVariableName());

                    if (savedResponse != null)
                        return true;
                }

                return false;
            }

        }
    }

}