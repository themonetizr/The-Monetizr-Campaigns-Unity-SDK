using System;
using System.Collections.Generic;
using UnityEngine;

namespace Monetizr.SDK
{
    [Serializable]
    internal class MissionsCollection : BaseCollection
    {
        [SerializeField] internal List<Mission> missions = new List<Mission>();

        internal override void Clear()
        {
            missions.Clear();
        }
    };

}