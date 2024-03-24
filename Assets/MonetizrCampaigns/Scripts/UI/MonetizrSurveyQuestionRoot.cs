using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.SDK.UI
{
    internal class MonetizrSurveyQuestionRoot : MonoBehaviour
    {
        public RectTransform rectTransform;
        public Text question;
        public string id;
        public ToggleGroup toggleGroup;
        public VerticalLayoutGroup verticalLayout;
        public GridLayoutGroup gridLayout;
        public RectTransform gridLayoutRoot;
        public RectTransform imageGridLayoutRoot;
    }

}
