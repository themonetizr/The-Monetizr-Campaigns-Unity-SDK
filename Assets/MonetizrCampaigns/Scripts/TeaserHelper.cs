using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monetizr.Campaigns;

public class TeaserHelper : MonoBehaviour
{
    void OnEnable()
    {
        MonetizrManager.ShowTinyMenuTeaser();
    }

    void OnDisable()
    {
        MonetizrManager.HideTinyMenuTeaser();
    }
}
