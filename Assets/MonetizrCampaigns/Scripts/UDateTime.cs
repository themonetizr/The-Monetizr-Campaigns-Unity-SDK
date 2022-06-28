using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Monetizr.Campaigns
{
    [System.Serializable]
    public class UDateTime : ISerializationCallbackReceiver
    {
        [HideInInspector] public DateTime dateTime;
        [HideInInspector] [SerializeField] private string _dateTime;

        public static implicit operator DateTime(UDateTime udt)
        {
            return (udt.dateTime);
        }

        public static implicit operator UDateTime(DateTime dt)
        {
            return new UDateTime() { dateTime = dt };
        }
               
        public void OnAfterDeserialize()
        {
            DateTime.TryParse(_dateTime, out dateTime);
        }

        public void OnBeforeSerialize()
        {
            _dateTime = dateTime.ToString();
        }

        public UDateTime AddSeconds(double value)
        {
            return new UDateTime { dateTime = this.dateTime.AddSeconds(value) };
        }
    }
}