
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ThreeDISevenZeroR.UnityGifDecoder;
using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.Campaigns
{
    public class GifTexture
    {
        // Texture
        public Texture2D m_texture2d;
        // Delay time until the next texture.
        public float m_delaySec;

        public GifTexture(Texture2D texture2d, float delaySec)
        {
            m_texture2d = texture2d;
            m_delaySec = delaySec;
        }
    }

    [RequireComponent(typeof(RawImage))]
    public class GifImage : MonoBehaviour
    {
        public enum State
        {
            None,
            Loading,
            Ready,
            Playing,
            Pause,
        }

        public RawImage m_rawImage;
        private List<GifTexture> m_gifTextureList;
        private float m_delayTime;
        private int m_gifTextureIndex;
        private int m_nowLoopCount;
        private State state;
        private int loopCount;
                
        private void OnDestroy()
        {
            Clear();
        }

        private void Update()
        {
            switch (state)
            {
                case State.None:
                    break;

                case State.Loading:
                    break;

                case State.Ready:
                    break;

                case State.Playing:
                    if (m_rawImage == null || m_gifTextureList == null || m_gifTextureList.Count <= 0)
                    {
                        return;
                    }
                    if (m_delayTime > Time.time)
                    {
                        return;
                    }
                    // Change texture
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

                case State.Pause:
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
                Log.PrintError("URL is empty.");
                yield break;
            }

            if (state == State.Playing)
            {
                state = State.Ready;
                Play();
                yield break;
            }

            if (state == State.Loading)
            {
                Log.PrintWarning("Already loading.");
                yield break;
            }

            byte[] bytes = File.ReadAllBytes(url);

            if (bytes == null)
            {
                Log.PrintError("File load error.\n");
                state = State.None;
                yield break;
            }

            Clear();
            state = State.Loading;

            m_gifTextureList = new List<GifTexture>();

            using (var gifStream = new GifStream(bytes))
            {
                while (gifStream.HasMoreData)
                {
                    switch (gifStream.CurrentToken)
                    {
                        case GifStream.Token.Image:
                            var image = gifStream.ReadImage();
                            // do something with image

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
                            // log this comment
                            break;

                        default:
                            gifStream.SkipToken();
                            // this token has no use for you, skip it
                            break;
                    }
                }
            }

            loopCount = -1;
            state = State.Ready;

            if (autoPlay)
            {
                Play();
            }

        }

        private void Clear()
        {
            if (m_rawImage != null)
            {
                //m_rawImage.texture = null;
            }

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

            state = State.None;
        }

        internal void Play()
        {
            if (state != State.Ready)
            {
                Log.PrintWarning("State is not READY.");
                return;
            }
            if (m_rawImage == null || m_gifTextureList == null || m_gifTextureList.Count <= 0)
            {
                Log.PrintError($"Raw Image {m_rawImage} or GIF Texture list {m_gifTextureList} is null or empty {m_gifTextureList.Count}.");
                return;
            }
            state = State.Playing;
            m_rawImage.texture = m_gifTextureList[0].m_texture2d;
            m_delayTime = Time.time + m_gifTextureList[0].m_delaySec;
            m_gifTextureIndex = 0;
            m_nowLoopCount = 0;
        }

        internal void Stop()
        {
            if (state != State.Playing && state != State.Pause)
            {
                Log.PrintWarning("State is not Playing and Pause.");
                return;
            }
            state = State.Ready;
        }

        internal void Pause()
        {
            if (state != State.Playing)
            {
                Log.PrintWarning("State is not Playing.");
                return;
            }
            state = State.Pause;
        }

        internal void Resume()
        {
            if (state != State.Pause)
            {
                Log.PrintWarning("State is not Pause.");
                return;
            }
            state = State.Playing;
        }
    }
}