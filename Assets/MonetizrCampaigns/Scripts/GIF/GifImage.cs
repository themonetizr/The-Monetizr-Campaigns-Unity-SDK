using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Monetizr.SDK.Debug;
using ThreeDISevenZeroR.UnityGifDecoder;

namespace Monetizr.SDK.GIF
{
    [RequireComponent(typeof(RawImage))]
    public class GifImage : MonoBehaviour
    {
        public RawImage m_rawImage;
        private List<GifTexture> m_gifTextureList;
        private float m_delayTime;
        private int m_gifTextureIndex;
        private int m_nowLoopCount;
        private GifState state;
        private int loopCount;
                
        private void OnDestroy()
        {
            Clear();
        }

        private void Update()
        {
            switch (state)
            {
                case GifState.None:
                    break;

                case GifState.Loading:
                    break;

                case GifState.Ready:
                    break;

                case GifState.Playing:
                    if (m_rawImage == null || m_gifTextureList == null || m_gifTextureList.Count <= 0)
                    {
                        return;
                    }
                    if (m_delayTime > Time.time)
                    {
                        return;
                    }

                    m_gifTextureIndex++;
                    if (m_gifTextureIndex >= m_gifTextureList.Count)
                    {
                        m_gifTextureIndex = 0;

                        if (loopCount > 0)
                        {
                            m_nowLoopCount++;
                            if (m_nowLoopCount >= loopCount)
                            {
                                Stop();
                                return;
                            }
                        }
                    }

                    m_rawImage.texture = m_gifTextureList[m_gifTextureIndex].m_texture2d;
                    m_delayTime = Time.time + m_gifTextureList[m_gifTextureIndex].m_delaySec;
                    break;

                case GifState.Pause:
                    break;

                default:
                    break;
            }
        }

        internal void SetGifFromUrl(string url, bool autoPlay = true)
        {
            if (!gameObject.activeSelf)
                return;

            StartCoroutine(SetGifFromUrlCoroutine(url, autoPlay));
        }
              
        private IEnumerator SetGifFromUrlCoroutine(string url, bool autoPlay = true)
        {
            if (string.IsNullOrEmpty(url))
            {
                MonetizrLog.PrintError("URL is empty.");
                yield break;
            }

            if (state == GifState.Playing)
            {
                state = GifState.Ready;
                Play();
                yield break;
            }

            if (state == GifState.Loading)
            {
                MonetizrLog.PrintWarning("Already loading.");
                yield break;
            }

            byte[] bytes = File.ReadAllBytes(url);

            if (bytes == null)
            {
                MonetizrLog.PrintError("File load error.\n");
                state = GifState.None;
                yield break;
            }

            Clear();
            state = GifState.Loading;

            m_gifTextureList = new List<GifTexture>();

            using (var gifStream = new GifStream(bytes))
            {
                while (gifStream.HasMoreData)
                {
                    switch (gifStream.CurrentToken)
                    {
                        case GifStream.Token.Image:
                            var image = gifStream.ReadImage();

                            var frame = new Texture2D(
                                gifStream.Header.width,
                                gifStream.Header.height,
                                TextureFormat.ARGB32, false);

                            frame.SetPixels32(image.colors);
                            frame.Apply();

                            m_gifTextureList.Add(new GifTexture(frame, image.DelaySeconds));

                            break;

                        case GifStream.Token.Comment:
                            var comment = gifStream.ReadComment();
                            break;

                        default:
                            gifStream.SkipToken();
                            break;
                    }
                }
            }

            loopCount = -1;
            state = GifState.Ready;

            if (autoPlay)
            {
                Play();
            }

        }

        private void Clear()
        {
            if (m_gifTextureList != null)
            {
                for (int i = 0; i < m_gifTextureList.Count; i++)
                {
                    if (m_gifTextureList[i] != null)
                    {
                        if (m_gifTextureList[i].m_texture2d != null)
                        {
                            Destroy(m_gifTextureList[i].m_texture2d);
                            m_gifTextureList[i].m_texture2d = null;
                        }
                        m_gifTextureList[i] = null;
                    }
                }
                m_gifTextureList.Clear();
                m_gifTextureList = null;
            }

            state = GifState.None;
        }

        internal void Play()
        {
            if (state != GifState.Ready)
            {
                MonetizrLog.PrintWarning("State is not READY.");
                return;
            }
            if (m_rawImage == null || m_gifTextureList == null || m_gifTextureList.Count <= 0)
            {
                MonetizrLog.PrintError($"Raw Image {m_rawImage} or GIF Texture list {m_gifTextureList} is null or empty {m_gifTextureList.Count}.");
                return;
            }
            state = GifState.Playing;
            m_rawImage.texture = m_gifTextureList[0].m_texture2d;
            m_delayTime = Time.time + m_gifTextureList[0].m_delaySec;
            m_gifTextureIndex = 0;
            m_nowLoopCount = 0;
        }

        internal void Stop()
        {
            if (state != GifState.Playing && state != GifState.Pause)
            {
                MonetizrLog.PrintWarning("State is not Playing and Pause.");
                return;
            }
            state = GifState.Ready;
        }

        internal void Pause()
        {
            if (state != GifState.Playing)
            {
                MonetizrLog.PrintWarning("State is not Playing.");
                return;
            }
            state = GifState.Pause;
        }

        internal void Resume()
        {
            if (state != GifState.Pause)
            {
                MonetizrLog.PrintWarning("State is not Pause.");
                return;
            }
            state = GifState.Playing;
        }

    }

}