using UnityEngine;

namespace Monetizr.SDK.GIF
{
    public class GifTexture
    {
        public Texture2D m_texture2d;
        public float m_delaySec;

        public GifTexture(Texture2D texture2d, float delaySec)
        {
            m_texture2d = texture2d;
            m_delaySec = delaySec;
        }

    }

}