using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{
    internal enum ButtonType
    {

    }

    [RequireComponent(typeof(Button))]
    internal class ButtonController : MonoBehaviour
    {
        internal MonetizrRewardedItem clickReceiver;
        public ButtonType buttonType;
        Button button;
        public int id = 0;

        void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClicked);
        }

        void OnButtonClicked()
        {
            Log.Print($"Clicked: {buttonType} id: {id}");

            clickReceiver.ButtonPressed(this);
            //GameState.GetInstance().ButtonPress(buttonType,id);       
        }
    }

}