using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace Monetizr.Tests
{
    public class NetworkingTests
    {
        [UnityTest]
        public IEnumerator SimpleAPICall ()
        {
            UnityWebRequest request = UnityWebRequest.Get("https://official-joke-api.appspot.com/random_joke");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Console.WriteLine(responseText);
                Assert.NotNull(responseText);
            }
            else
            {
                Assert.Fail();
            }
        }

    }
}