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

            UIController.SetColorForElement(textElement, m.campaignServerSettings.dictionary, "text_color");
            UIController.SetColorForElement(textElement, m.campaignServerSettings.dictionary, $"{parentId.ToString()}.{textContent}_color");

            var param = $"{parentId.ToString()}.{textContent}";

            if (MonetizrManager.temporaryRewardTypeSelection == MonetizrManager.RewardSelectionType.Ingame)
            {
                var param2 = $"{parentId.ToString()}.{textContent}2";

                if (m.campaignServerSettings.dictionary.ContainsKey(param2))
                {
                    param = param2;
                }
            }

            if (m.campaignServerSettings.dictionary.ContainsKey(param))
            {
                string t = m.campaignServerSettings.GetParam(param);

                string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

                t = t.Replace("%ingame_reward%", $"{m.reward} {rewardTitle}");
                t = t.Replace("<br/>", "\n");

                textElement.text = t;
            }
        }

        
    }

}
