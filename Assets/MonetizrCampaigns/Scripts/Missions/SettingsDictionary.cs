using System;
using System.Collections.Generic;
using System.Linq;

namespace Monetizr.SDK.Missions
{
    public class SettingsDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public SettingsDictionary() { }

        public SettingsDictionary(Dictionary<TKey, TValue> d) : base(d) { }

        public TValue GetParam(TKey p, TValue def = default(TValue))
        {
            if (p == null)
                return def;

            return TryGetValue(p, out var value) ? value : def;
        }

        public bool GetBoolParam(TKey p, bool defaultParam)
        {
            if (p == null)
                return defaultParam;

            if (!TryGetValue(p, out var value))
            {
                return defaultParam;
            }

            return bool.TryParse(value.ToString(), out var result) ? result : defaultParam;
        }

        public List<float> GetRectParam(TKey p, List<float> defaultParam)
        {
            if (p == null)
                return defaultParam;

            if (!TryGetValue(p, out var value))
            {
                return defaultParam;
            }

            string val = value.ToString();

            var svals = val.Split(new char[] { ';', ',' });

            var v = new List<float>(0);

            Array.ForEach(svals, s =>
            {
                if (!float.TryParse(s, out var f))
                    return;

                v.Add(f);
            });

            return v.Count == 4 ? v : defaultParam;
        }

        public int GetIntParam(List<TKey> pl, int defaultParam = 0)
        {
            foreach (var p in pl)
            {
                if (ContainsKey(p))
                {
                    return GetIntParam(p, defaultParam);
                }
            }
            
            return defaultParam;
        }

        public int GetIntParam(TKey p, int defaultParam = 0)
        {
            if (p == null)
                return defaultParam;

            if (!TryGetValue(p, out var value))
            {
                return defaultParam;
            }

            return int.TryParse(value.ToString(), out var result) ? result : defaultParam;
        }

        public float GetFloatParam(TKey p, float defaultParam = 0.0f)
        {
            if (p == null)
                return defaultParam;

            if (!TryGetValue(p, out var value))
            {
                return defaultParam;
            }

            return float.TryParse(value.ToString(), out var result) ? result : defaultParam;
        }

    }

}