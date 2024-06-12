using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace Monetizr.Tests
{
    public class TestManager
    {
        [Test]
        public void SimpleCommonTest ()
        {
            throw new System.Exception();
        }

        [UnityTest]
        public IEnumerator SimpleUnityTest ()
        {
            yield return null;
        }

    }
}