using Monetizr.SDK.Core;
using Monetizr.SDK.New;
using Monetizr.SDK.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Monetizr.Tests 
{
    public static class TestManager
    {
        public static float sdkInitializationDelay = 2f;

        public static void Setup ()
        {
            GameObject camera = new GameObject("Default Camera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(camera, SceneManager.GetActiveScene());
            camera.transform.SetAsFirstSibling();
            PlayerPrefs.SetString("campaigns", "");
            PlayerPrefs.SetString("missions", "");
            Sprite mockImage = TestUtils.CreateMockSprite();
            Time.timeScale = 5;
            MonetizrSettingsMenu.LoadTestSettings();
            MonetizrManager.SetAdvertisingIds("", false);
            MonetizrManager.SetGameCoinAsset(RewardType.Coins, mockImage, "Coins", () => { return 0; }, (ulong reward) => { }, 100);
            MonetizrManager.SetTeaserPosition(New_MobileUtils.IsInLandscapeMode() ? new Vector2(700, 300) : new Vector2(-230, -765));
            MonetizrManager.InitializeForTests(null, () => { }, null, null);
        }
    }
}