using Monetizr.SDK.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Monetizr.SDK.Video
{

    internal class MonetizrVideoPlayer : MonoBehaviour
    {
        public VideoPlayer videoPlayer;
        Action<bool> onComplete;
        private bool isSkipped = false;

        public void Play(string videoPath, Action<bool> onComplete)
        {
            this.onComplete = onComplete;
            var videoPlayer = GetComponent<VideoPlayer>();

            videoPlayer.playOnAwake = false;
            //videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            //videoPlayer.targetCameraAlpha = 0.5F;
            videoPlayer.url = videoPath;
            videoPlayer.frame = 100;
            videoPlayer.isLooping = false;

            videoPlayer.loopPointReached += EndReached;

            videoPlayer.Play();

            Log.Print($"{videoPlayer.width} {videoPlayer.height}");

            MonetizrManager.Instance.SoundSwitch(false);

            //MonetizrManager.analytics.BeginShowAdAsset(AdType.Video,null);
        }

        void EndReached(VideoPlayer vp)
        {
            //MonetizrManager.analytics.EndShowAdAsset(AdType.Video,null);

            MonetizrManager.Instance.SoundSwitch(true);

            onComplete.Invoke(isSkipped);
        }

        public void OnSkip()
        {
            Log.Print("OnSkip!");

            isSkipped = true;

            EndReached(videoPlayer);
        }

    }

}
