using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CanvasReferenceScaler : MonoBehaviour
{
    private CanvasScaler cs = null;

    private void Start()
    {
        cs = gameObject.GetComponent<CanvasScaler>();
        var initialRefRes = cs.referenceResolution;

        float aspect = (float)Screen.height / (float)Screen.width;

        if (aspect >= 1.777)
        {
            gameObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(initialRefRes.x, initialRefRes.x * aspect);
        }
        else
        {
            gameObject.GetComponent<CanvasScaler>().matchWidthOrHeight = 1;
        }
    }

    public Vector2 GetScreenReferenceResolution()
    {
        return cs.referenceResolution;
    }

}
