using Monetizr.SDK.Core;
using Monetizr.SDK.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Monetizr.Tests 
{
    public static class TestManager
    {
        public static float sdkInitializationDelay = 2f;

        private static string apiKey = "t_rsNjLXzbaWkJrXdvUVEc4IW2zppWyevl9j_S5Valo";
        private static string bundleID = "com.monetizr.sample";

        public static void Setup ()
        {
            GameObject camera = new GameObject("Default Camera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(camera, SceneManager.GetActiveScene());
            camera.transform.SetAsFirstSibling();
            PlayerPrefs.SetString("campaigns", "");
            PlayerPrefs.SetString("missions", "");
            Sprite mockImage = TestUtils.CreateMockSprite();
            Time.timeScale = 5;
            MonetizrManager.bundleId = bundleID;
            MonetizrManager.SetAdvertisingIds("", false);
            MonetizrManager.SetGameCoinAsset(RewardType.Coins, mockImage, "Coins", () => { return 0; }, (ulong reward) => { }, 100);
            MonetizrManager.SetTeaserPosition(MonetizrUtils.IsInLandscapeMode() ? new Vector2(700, 300) : new Vector2(-230, -765));
            MonetizrManager.InitializeForTests(apiKey, null, () => { }, null, null);
        }
    }
}