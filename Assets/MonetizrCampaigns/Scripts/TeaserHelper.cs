using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monetizr.Campaigns;

public class TeaserHelper : MonoBehaviour
{
    private void Start()
    {
        //MonetizrManager.SetTeaserRoot(GetComponentInParent<RectTransform>());
    }

    void OnEnable()
    {
        MonetizrManager.OnMainMenuShow();
    }

    void OnDisable()
    {
        //Debug.Log("on disable!");
        MonetizrManager.OnMainMenuHide();
    }
}
