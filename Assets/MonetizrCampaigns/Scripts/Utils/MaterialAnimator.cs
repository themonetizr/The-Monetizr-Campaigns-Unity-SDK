using UnityEngine;
using UnityEngine.UI;

namespace Monetizr.SDK.Utils
{
    public class MaterialAnimator : MonoBehaviour
    {
        public float animationTime;
        public Image image;
        public float scrollSpeed = 0.5f;

        private readonly Vector2 _startOffset = new Vector2(1, 0);
        private readonly Vector2 _endOffset = new Vector2(-1, 0);
        private float _t = 0;
        private bool _isShown = false;

        public void StartAnimation()
        {
            _t = 0;
            _isShown = false;
        }

        private void Update()
        {
            if (_isShown)
                return;

            _t += Time.deltaTime * scrollSpeed;

            if (_t > 1.0f)
            {
                _isShown = true;
                _t = 0;
            }

            var v = Vector2.Lerp(_startOffset, _endOffset, _t);

            image.material.SetTextureOffset("_DetailTex", v);
        }

    }

}