using UnityEngine;

namespace Monetizr.SDK.Utils
{
    internal abstract class Singleton<T> : MonoBehaviour where T : Component
    {

        private static T instance;

        public static T GetInstance()
        {
            return instance;
        }

        public static T Initialize(string name)
        {
            if (instance)
                return instance;

            var go = new GameObject(name);

            instance = go.AddComponent<T>();

            return instance;
        }
    }

}
