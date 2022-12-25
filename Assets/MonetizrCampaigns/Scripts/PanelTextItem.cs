using System.Collections;
using System.Collections.Generic;
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

        public void InitializeByParent(PanelId parentId, Mission m)
        {
            if (m == null)
                return;

            var param = $"{parentId.ToString()}.{textContent}";
            var param_with_type = $"{parentId.ToString()}.{m.type.ToString()}.{textContent}";
            var param_with_type_and_id = $"{parentId.ToString()}.{m.type.ToString()}.{m.serverId}.{textContent}";

            UIController.SetColorForElement(textElement, m.campaignServerSettings.dictionary, "text_color");

            UIController.SetColorForElement(textElement, m.campaignServerSettings.dictionary, $"{param}_color");
            UIController.SetColorForElement(textElement, m.campaignServerSettings.dictionary, $"{param_with_type}_color");
            UIController.SetColorForElement(textElement, m.campaignServerSettings.dictionary, $"{param_with_type_and_id}_color");

            
            if (MonetizrManager.temporaryRewardTypeSelection == MonetizrManager.RewardSelectionType.Ingame)
            {
                var param2 = $"{parentId.ToString()}.{textContent}2";

                if (m.campaignServerSettings.dictionary.ContainsKey(param2))
                {
                    param = param2;
                }
            }

            string []prm = {param, param_with_type, param_with_type_and_id};

            System.Array.ForEach(prm, s =>
            {
                if (m.campaignServerSettings.dictionary.ContainsKey(s))
                {
                    UpdateRewardText(m, s);
                }
            });
            
        }

        internal static string ReplacePredefinedItemsInText(Mission m, in string str)
        {
            string t = str;

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            t = t.Replace("%ingame_reward%", $"{MonetizrRewardedItem.ScoreShow(m.reward)} {rewardTitle}");
            t = t.Replace("%reward_amount%", $"{MonetizrRewardedItem.ScoreShow(m.reward)}");
            t = t.Replace("%reward_title%", $"{rewardTitle}");
            t = t.Replace("<br/>", "\n");

            return t;
        }

        private void UpdateRewardText(Mission m, string param_with_type_and_id)
        {
            string t = m.campaignServerSettings.GetParam(param_with_type_and_id);

            t = PanelTextItem.ReplacePredefinedItemsInText(m, t);

            textElement.text = t;
        }

    }

}
