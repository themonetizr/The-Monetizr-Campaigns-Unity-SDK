using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Monetizr.SDK.Core;
using Monetizr.SDK.Analytics;

namespace Monetizr.Tests
{
    public class SetupTests
    {
        [SetUp]
        public void Setup ()
        {
            TestManager.Setup();
        }

        [Test, Order(1)]
        public void AccessSDKVersion ()
        {
            Assert.IsNotNull(MonetizrSDKConfiguration.SDKVersion);
        }

        [UnityTest, Order(2)]
        public IEnumerator HaveAdvertisingIDsBeenSet()
        {
            yield return new WaitForSeconds(TestManager.sdkInitializationDelay);

            Assert.IsTrue(MonetizrMobileAnalytics.isAdvertisingIDDefined);
            Assert.IsNotNull(MonetizrMobileAnalytics.advertisingID);
            Assert.IsNotNull(MonetizrMobileAnalytics.limitAdvertising);
        }

        [UnityTest, Order(3)]
        public IEnumerator HasAtLeastOneGameRewardBeenSetup()
        {
            yield return new WaitForSeconds(TestManager.sdkInitializationDelay);
            Assert.NotNull(MonetizrManager.gameRewards);
            Assert.GreaterOrEqual(MonetizrManager.gameRewards.Count, 1);
        }

        [UnityTest, Order(4)]
        public IEnumerator AccessMonetizrManagerGameobject ()
        {
            yield return new WaitForSeconds(TestManager.sdkInitializationDelay);
            Assert.NotNull(GameObject.Find("MonetizrManager"));
        }

    }
}