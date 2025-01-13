using System.Threading.Tasks;
using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Core;
using Monetizr.SDK.Missions;
using NUnit.Framework;
using UnityEngine;

namespace Monetizr.Tests
{
    [TestFixture]
    public class CampaignTests
    {
        private ServerCampaign _campaign;
        private Asset _testAsset;

        [SetUp]
        public void SetUp()
        {
            _campaign = new ServerCampaign();
            _testAsset = new Asset { type = "icon", url = "https://example.com/image.png", title = "Test Asset" };
            _campaign.assets.Add(_testAsset);
        }

        /*
        [Test, Order(1)]
        public void HasAssetInList_ReturnsTrue_WhenAssetIsPresent()
        {
            Assert.IsTrue(_campaign.HasAssetInList("icon"));
        }

        [Test, Order(2)]
        public void HasAssetInList_ReturnsFalse_WhenAssetIsNotPresent()
        {
            Assert.IsFalse(_campaign.HasAssetInList("non_existent_asset"));
        }
        */

        [Test, Order(3)]
        public void TryGetAssetInList_ReturnsTrue_AndAssignsAsset_WhenAssetIsPresent()
        {
            Asset asset;
            var result = _campaign.TryGetAssetInList("icon", out asset);

            Assert.IsTrue(result);
            Assert.AreEqual(_testAsset, asset);
        }

        [Test, Order(4)]
        public void TryGetAssetInList_ReturnsFalse_WhenAssetIsNotPresent()
        {
            Asset asset;
            var result = _campaign.TryGetAssetInList("non_existent_asset", out asset);

            Assert.IsFalse(result);
            Assert.IsNull(asset);
        }

        /*
        [Test, Order(5)]
        public void RemoveAssetsByTypeFromList_RemovesAssetCorrectly()
        {
            _campaign.RemoveAssetsByTypeFromList("icon");
            Assert.IsFalse(_campaign.HasAssetInList("icon"));
        }
        */

    }
}
