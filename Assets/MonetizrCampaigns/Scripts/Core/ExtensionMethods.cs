//undefine this to test slow internet
//#define TEST_SLOW_LATENCY

//if we define this - video and survey campaigns will work
//#define USING_WEBVIEW

using UnityEngine;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace Monetizr.SDK.Core
{
    internal static class ExtensionMethods
    {
        public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += obj => { tcs.SetResult(null); };
            return ((Task)tcs.Task).GetAwaiter();
        }
    }

}