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

            if (m.additionalParams.dictionary.ContainsKey(param))
                textElement.text = m.additionalParams.GetParam(param);
        }

        
    }

}
