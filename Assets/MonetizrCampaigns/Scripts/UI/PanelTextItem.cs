using Monetizr.SDK.Core;
using Monetizr.SDK.Missions;
using Monetizr.SDK.Utils;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.SDK.UI
{
    internal class PanelTextItem : MonoBehaviour
    {
        public Text textElement;
        public string textContent;
        public Graphic buttonGrafic;

        public void InitializeByParent(PanelId parentId, Mission m)
        {
            if (m == null) return;

            if (buttonGrafic == null && textElement == null) return;
            
            var param = $"{parentId}.{textContent}";
            var param2 = $"{param}2";
            var paramWithType = $"{parentId}.{m.type}.{textContent}";
            var paramWithTypeAndId = $"{parentId}.{m.type}.{m.serverId}.{textContent}";

            string[] colorVars = { "button_bg_color", $"{param}_bg_color", $"{paramWithType}_bg_color", $"{paramWithTypeAndId}_bg_color" };

            foreach (var c in colorVars)
            {
                UIController.SetColorForElement(buttonGrafic, m.campaignServerSettings, c);
            }

            if (textElement == null) return;

            string[] textVars = { "text_color", $"{param}_color", $"{paramWithType}_color", $"{paramWithTypeAndId}_color" };

            foreach (var t in textVars)
            {
                UIController.SetColorForElement(textElement, m.campaignServerSettings, t);
            }
            
            if (MonetizrManager.temporaryRewardTypeSelection == MonetizrManager.RewardSelectionType.Ingame &&
                m.campaignServerSettings.ContainsKey(param2))
            {
                param = param2;
            }

            string []paramStrings = {param, paramWithType, paramWithTypeAndId};

            foreach (var s in paramStrings)
            {
                if (m.campaignServerSettings.ContainsKey(s))
                {
                    UpdateRewardText(m, s);
                }
            };
            
        }

        internal static string ReplacePredefinedItemsInText(Mission m, in string str)
        {
            string t = str;

            string rewardTitle = MonetizrManager.gameRewards[m.rewardType].title;

            t = new StringBuilder(t)
                .Replace("%ingame_reward%", $"{MonetizrUtils.ScoresToString(m.reward)} {rewardTitle}")
                .Replace("%reward_amount%", $"{MonetizrUtils.ScoresToString(m.reward)}")
                .Replace("%reward_title%", $"{rewardTitle}")
                .Replace("<br/>", "\n")
                .ToString();

            return t;
        }

        private void UpdateRewardText(Mission m, string param)
        {
            string t = m.campaignServerSettings.GetParam(param);

            textElement.text = PanelTextItem.ReplacePredefinedItemsInText(m, t);
        }

    }

}
