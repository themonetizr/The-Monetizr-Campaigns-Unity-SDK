using Monetizr.Campaigns;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkButton : MonoBehaviour
{
    public string id;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void OnClick()
    {
        MonetizrManager.ShowWebPage(null, new Mission { surveyUrl = id, additionalParams = new SerializableDictionary<string, string>() });

#if UNITY_EDITOR_WIN
        //Application.OpenURL(id);
#else
        
#endif
    }
}
