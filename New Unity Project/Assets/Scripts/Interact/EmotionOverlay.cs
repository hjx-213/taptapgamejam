using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

namespace Game.UI
{
    /// <summary>用中心圆形遮罩播放情绪颜色扩散，并淡出回到场景。</summary>
    public class EmotionOverlay : MonoBehaviour
    {
        [Header("UI 引用")]
        [SerializeField] private Image _overlayImage;

        [Header("动画时间（秒）")]
        [Min(0.05f)] [SerializeField] private float _spreadDuration = 1.5f;
        [Min(0f)] [SerializeField] private float _holdDuration = 0.4f;
        [Min(0.05f)] [SerializeField] private float _fadeOutDuration = 1.2f;

        [Header("受伤反馈")]
        [SerializeField] private bool _enableCameraShake = true;
        [Min(0f)] [SerializeField] private float _shakeDuration = 0.35f;
        [Min(0f)] [SerializeField] private float _shakeMagnitude = 0.12f;
        [Min(1f)] [SerializeField] private float _shakeFrequency = 28f;

        [Header("动画事件")]
        public UnityEvent OnOverlayStart;
        public UnityEvent OnOverlayFull;
        public UnityEvent OnOverlayEnd;

        private Material _material;
        private Coroutine _animRoutine;
        private Coroutine _shakeRoutine;
        private Transform _shakenCamera;
        private Vector3 _cameraBasePosition;
        private float _fullRadius = 1f;
        private static readonly int RadiusId = Shader.PropertyToID("_Radius");
        private static readonly int AspectId = Shader.PropertyToID("_Aspect");
        private static readonly int SoftnessId = Shader.PropertyToID("_Softness");
        private static readonly int MaskColorId = Shader.PropertyToID("_MaskColor");

        private void Awake()
        {
            if (_overlayImage == null)
                _overlayImage = GetComponent<Image>();

            if (_overlayImage == null)
            {
                Debug.LogError("[EmotionOverlay] 缺少 Image 组件，无法工作。", this);
                enabled = false;
                return;
            }

            Shader shader = Shader.Find("UI/CenterReveal");
            if (shader == null)
            {
                Debug.LogError("[EmotionOverlay] 找不到 UI/CenterReveal Shader。", this);
                enabled = false;
                return;
            }

            _material = new Material(shader);
            _overlayImage.material = _material;
            _overlayImage.color = Color.white;
            _overlayImage.raycastTarget = false;
            _overlayImage.enabled = false;
            _material.SetFloat(SoftnessId, 0.08f);
            UpdateAspect();
        }

        public void ShowOverlay(Color color)
        {
            if (_overlayImage == null || _material == null)
                return;

            if (_animRoutine != null)
                StopCoroutine(_animRoutine);

            UpdateAspect();
            _animRoutine = StartCoroutine(OverlayRoutine(color));
        }

        public void ClearOverlay()
        {
            if (_overlayImage == null || _material == null)
                return;

            if (_animRoutine != null)
                StopCoroutine(_animRoutine);

            _animRoutine = StartCoroutine(FadeOutOnly());
        }

        public bool IsPlaying => _animRoutine != null;

        private IEnumerator OverlayRoutine(Color color)
        {
            _overlayImage.enabled = true;
            _material.SetColor(MaskColorId, color);
            _material.SetFloat(RadiusId, 0f);
            OnOverlayStart?.Invoke();
            StartCameraShake();

            yield return AnimateRadius(0f, _fullRadius, _spreadDuration);
            OnOverlayFull?.Invoke();

            if (_holdDuration > 0f)
                yield return new WaitForSecondsRealtime(_holdDuration);

            yield return FadeOut();
            _overlayImage.enabled = false;
            OnOverlayEnd?.Invoke();
            _animRoutine = null;
        }

        private IEnumerator AnimateRadius(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);
                _material.SetFloat(RadiusId, Mathf.Lerp(from, to, t));
                yield return null;
            }

            _material.SetFloat(RadiusId, to);
        }

        private IEnumerator FadeOut()
        {
            Color color = _material.GetColor(MaskColorId);
            float startAlpha = color.a;
            float elapsed = 0f;

            while (elapsed < _fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                color.a = Mathf.Lerp(startAlpha, 0f, Mathf.Clamp01(elapsed / _fadeOutDuration));
                _material.SetColor(MaskColorId, color);
                yield return null;
            }

            color.a = 0f;
            _material.SetColor(MaskColorId, color);
        }

        private IEnumerator FadeOutOnly()
        {
            yield return FadeOut();
            _overlayImage.enabled = false;
            OnOverlayEnd?.Invoke();
            _animRoutine = null;
        }

        private void StartCameraShake()
        {
            if (!_enableCameraShake || _shakeDuration <= 0f || _shakeMagnitude <= 0f)
                return;

            Camera camera = Camera.main;
            if (camera == null)
                return;

            if (_shakeRoutine != null)
            {
                StopCoroutine(_shakeRoutine);
                if (_shakenCamera != null)
                    _shakenCamera.localPosition = _cameraBasePosition;
            }

            _shakenCamera = camera.transform;
            _cameraBasePosition = _shakenCamera.localPosition;
            _shakeRoutine = StartCoroutine(ShakeCameraRoutine());
        }

        private IEnumerator ShakeCameraRoutine()
        {
            float elapsed = 0f;
            float seedX = Random.Range(0f, 1000f);
            float seedY = Random.Range(0f, 1000f);

            while (elapsed < _shakeDuration && _shakenCamera != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float strength = 1f - Mathf.Clamp01(elapsed / _shakeDuration);
                float sampleX = Mathf.PerlinNoise(seedX, elapsed * _shakeFrequency) * 2f - 1f;
                float sampleY = Mathf.PerlinNoise(seedY, elapsed * _shakeFrequency) * 2f - 1f;
                Vector2 offset = new Vector2(sampleX, sampleY) * (_shakeMagnitude * strength);
                _shakenCamera.localPosition = _cameraBasePosition + new Vector3(offset.x, offset.y, 0f);
                yield return null;
            }

            if (_shakenCamera != null)
                _shakenCamera.localPosition = _cameraBasePosition;

            _shakeRoutine = null;
            _shakenCamera = null;
        }

        private void UpdateAspect()
        {
            if (_material == null || _overlayImage == null)
                return;

            Rect rect = _overlayImage.rectTransform.rect;
            float aspect = rect.height > 0f ? rect.width / rect.height : 1f;
            _fullRadius = Mathf.Sqrt(0.25f * aspect * aspect + 0.25f) + 0.08f;
            _material.SetFloat(AspectId, aspect);
        }

        private void OnDestroy()
        {
            if (_material != null)
                Destroy(_material);
        }
    }
}
