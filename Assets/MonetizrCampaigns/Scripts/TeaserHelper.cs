using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monetizr.Campaigns;

public class TeaserHelper : MonoBehaviour
{
    void OnEnable()
    {
        MonetizrManager.OnMainMenuShow();
        //MonetizrManager.ShowTinyMenuTeaser();
    }

    void OnDisable()
    {
        MonetizrManager.OnMainMenuHide();
        //MonetizrManager.HideTinyMenuTeaser();
    }
}
