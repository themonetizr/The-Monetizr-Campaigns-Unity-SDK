﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Monetizr.Campaigns
{

    internal static class AssetsHelper
    {
        /// <summary>
        /// Downloads a 2D asset and returns it as a Sprite in <paramref name="onAssetDownloaded"/>.
        /// </summary>
        public static IEnumerator Download2DAsset(Challenge.Asset asset, Action<Challenge.Asset, Sprite> onAssetDownloaded, Action onDownloadFailed = null)
        {
            UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(asset.url);

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                Log.PrintError(uwr.error);
                onDownloadFailed?.Invoke();
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
            onAssetDownloaded.Invoke(asset, Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero));
        }

        /// <summary>
        /// Downloads any type of asset and returns its data as an array of bytes in <paramref name="onAssetDownloaded"/>
        /// </summary>
        public static IEnumerator DownloadAssetData(Challenge.Asset asset, Action<Challenge.Asset, byte[]> onAssetDownloaded, Action onDownloadFailed = null)
        {
            UnityWebRequest uwr = UnityWebRequest.Get(asset.url);

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                Log.PrintError(uwr.error);
                onDownloadFailed?.Invoke();
                yield break;
            }

            onAssetDownloaded.Invoke(asset, uwr.downloadHandler.data);
        }
    }

}