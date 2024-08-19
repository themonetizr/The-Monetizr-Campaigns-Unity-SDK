using Monetizr.SDK.Campaigns;
using Monetizr.SDK.Missions;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace Monetizr.Tests
{
    public class MissionTests
    {
        [Test]
        public void CreateFromJson_ValidJsonWithEscapedCharacters_ReturnsHelperWithMissions()
        {
            string validJson = "{ \"missions\": [ { \"reward\": 100, \"survey\": \"test\" } ] }";
            var result = ServerMissionsHelper.CreateFromJson(validJson);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.missions.Count);
        }

    }

}