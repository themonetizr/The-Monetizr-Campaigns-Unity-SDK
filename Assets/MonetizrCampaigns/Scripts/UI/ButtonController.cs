using Monetizr.SDK.Debug;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.SDK.UI
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
            Log.PrintV($"Clicked: {buttonType} id: {id}");

            clickReceiver.ButtonPressed(this);
            //GameState.GetInstance().ButtonPress(buttonType,id);       
        }
    }

}