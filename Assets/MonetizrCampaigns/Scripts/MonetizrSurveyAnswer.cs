using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{
    internal class MonetizrSurveyAnswer : MonoBehaviour
    {
        public string id;
        public Text answer;
        public Toggle toggle;
        public Text enteredAnswer;
        public InputField inputField;
        public Image background;
        public Image image;
        public Image markImage;

        public Sprite grayBorder;
        public Sprite redBorder;
        public Sprite greenBorder;

        public Sprite greenRoundMark;
        public Sprite redSquareMark;
    }
}
    