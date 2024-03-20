using UnityEngine;

namespace Monetizr.SDK.GIF
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
}