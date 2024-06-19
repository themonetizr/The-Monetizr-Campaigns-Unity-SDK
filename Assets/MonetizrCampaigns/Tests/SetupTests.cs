using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Monetizr.SDK.Core;

namespace Monetizr.Tests
{
    public class SetupTests
    {
        [Test]
        public void AccessSDKVersion ()
        {
            Assert.IsNotNull(MonetizrSDKConfiguration.SDKVersion);
        }

        [UnityTest]
        public IEnumerator UseBasicMonetizrManagerFunction ()
        {
            yield return new WaitForSeconds(2f);
            Assert.DoesNotThrow(() => MonetizrManager.SetTeaserPosition(new Vector2(0, 0)));
        }
    }
}