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
            UIController.SetColorForElement(textElement, m.additionalParams.dictionary, "text_color");
            UIController.SetColorForElement(textElement, m.additionalParams.dictionary, $"{parentId.ToString()}.{textContent}_color");

            var param = $"{parentId.ToString()}.{textContent}";

            if (MonetizrManager.temporaryRewardTypeSelection == MonetizrManager.RewardSelectionType.Ingame)
            {
                var param2 = $"{parentId.ToString()}.{textContent}2";

                if (m.additionalParams.dictionary.ContainsKey(param2))
                {
                    param = param2;
                }
            }

            if (m.additionalParams.dictionary.ContainsKey(param))
            {
                string t = m.additionalParams.GetParam(param);

                string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

                t = t.Replace("%ingame_reward%", $"{m.reward} {rewardTitle}");
                t = t.Replace("<br/>", "\n");

                textElement.text = t;
            }
        }

        
    }

}
