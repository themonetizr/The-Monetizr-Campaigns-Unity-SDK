using System;
using UnityEngine;
using UnityEngine.Video;
using Monetizr.SDK.Core;
using Monetizr.SDK.Debug;

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
            videoPlayer.url = videoPath;
            videoPlayer.frame = 100;
            videoPlayer.isLooping = false;
            videoPlayer.loopPointReached += EndReached;
            videoPlayer.Play();

            Log.Print($"{videoPlayer.width} {videoPlayer.height}");

            MonetizrManager.Instance.SoundSwitch(false);
        }

        private void EndReached (VideoPlayer vp)
        {
            MonetizrManager.Instance.SoundSwitch(true);
            onComplete.Invoke(isSkipped);
        }

        public void OnSkip ()
        {
            Log.Print("OnSkip!");

            isSkipped = true;

            EndReached(videoPlayer);
        }

    }

}
