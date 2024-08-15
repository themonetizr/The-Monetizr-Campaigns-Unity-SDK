using UnityEngine;

namespace Monetizr.SDK.Networking
{
    public static class NetworkingUtils
    {
        public static string GetInternetConnectionType ()
        {
            switch (Application.internetReachability)
            {
                case NetworkReachability.NotReachable:
                    return "no_connection";
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    return "mobile";
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    return "lan";
                default:
                    return "unknown";
            }
        }
    }
}