using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{

    internal class PanelTextItem : MonoBehaviour
    {
        [SerializeField]
        public Text textElement;

        [SerializeField]
        public string textContent;

        [SerializeField]
        public Graphic buttonGrafic;

        public void InitializeByParent(PanelId parentId, Mission m)
        {
            if (m == null)
                return;

            var param = $"{parentId.ToString()}.{textContent}";
            var paramWithType = $"{parentId.ToString()}.{m.type.ToString()}.{textContent}";
            var paramWithTypeAndId = $"{parentId.ToString()}.{m.type.ToString()}.{m.serverId}.{textContent}";

            UIController.SetColorForElement(textElement, m.campaignServerSettings, "text_color");

            UIController.SetColorForElement(textElement, m.campaignServerSettings, $"{param}_color");
            UIController.SetColorForElement(textElement, m.campaignServerSettings, $"{paramWithType}_color");
            UIController.SetColorForElement(textElement, m.campaignServerSettings, $"{paramWithTypeAndId}_color");

            UIController.SetColorForElement(buttonGrafic, m.campaignServerSettings, "button_bg_color");

            UIController.SetColorForElement(buttonGrafic, m.campaignServerSettings, $"{param}_bg_color");
            UIController.SetColorForElement(buttonGrafic, m.campaignServerSettings, $"{paramWithType}_bg_color");
            UIController.SetColorForElement(buttonGrafic, m.campaignServerSettings, $"{paramWithTypeAndId}_bg_color");

            if (MonetizrManager.temporaryRewardTypeSelection == MonetizrManager.RewardSelectionType.Ingame)
            {
                var param2 = $"{parentId.ToString()}.{textContent}2";

                if (m.campaignServerSettings.ContainsKey(param2))
                {
                    param = param2;
                }
            }

            string []prm = {param, paramWithType, paramWithTypeAndId};

            System.Array.ForEach(prm, s =>
            {
                if (m.campaignServerSettings.ContainsKey(s))
                {
                    UpdateRewardText(m, s);
                }
            });
            
        }

        internal static string ReplacePredefinedItemsInText(Mission m, in string str)
        {
            string t = str;

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            t = new StringBuilder(t)
                .Replace("%ingame_reward%", $"{Utils.ScoresToString(m.reward)} {rewardTitle}")
                .Replace("%reward_amount%", $"{Utils.ScoresToString(m.reward)}")
                .Replace("%reward_title%", $"{rewardTitle}")
                .Replace("<br/>", "\n")
                .ToString();

            return t;
        }

        private void UpdateRewardText(Mission m, string param_with_type_and_id)
        {
            string t = m.campaignServerSettings.GetParam(param_with_type_and_id);

            t = PanelTextItem.ReplacePredefinedItemsInText(m, t);

            if(textElement != null)
                textElement.text = t;
        }

    }

}
