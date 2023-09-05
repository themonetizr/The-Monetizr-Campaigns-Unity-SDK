using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{
    internal class MonetizrSurveyQuestionRoot : MonoBehaviour
    {
        public RectTransform rectTransform;
        public Text question;
        public string id;
        public ToggleGroup toggleGroup;
        public VerticalLayoutGroup verticalLayout;
        public RectTransform gridLayoutRoot;
    }

}
