using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

namespace Game.UI
{
    /// <summary>
    /// 情绪过载全屏变色：颜色从中心出现，向外扩散覆盖屏幕，再淡出回到正常。
    ///
    /// 用法：
    ///   1. 准备一个柔边圆形 sprite（白色中心 → 透明边缘），用任意外部图片编辑工具
    ///      或从 Unity 自带 UI Sprite（圆形变体）制作。
    ///   2. Canvas 下创建空物体 EmotionOverlay，挂本组件。
    ///   3. Inspector 拖入带柔边圆的 Image（建议 RectTransform 居中、stretch 即可）。
    ///   4. 触发：调用 ShowOverlay() / ShowOverlay(color)，或由 EmotionBar 的
    ///      OnHotThresholdReached 事件挂接 ShowOverlay() 方法。
    ///   5. （可选）给 EmotionBar.cs 的 OnHotThresholdReached 接 ShowOverlay()，
    ///      阈值跨越时自动触发。
    /// </summary>
    public class EmotionOverlay : MonoBehaviour
    {
        [Header("UI 引用")]
        [Tooltip("柔边圆形 Image（白色中心 + 透明边缘）。" +
                 "会从中心缩放扩散到全屏。")]
        [SerializeField] private Image _overlayImage;

        [Header("默认颜色")]
        [Tooltip("未指定颜色时使用此颜色（情绪过载用红）")]
        [SerializeField] private Color _defaultColor =
            new Color(1f, 0.2f, 0.2f, 0.7f);

        [Header("动画时间（秒）")]
        [Tooltip("从中心扩散到全屏的秒数")]
        [Min(0.05f)] [SerializeField] private float _spreadDuration = 1.5f;

        [Tooltip("完全覆盖后的停留秒数")]
        [Min(0f)] [SerializeField] private float _holdDuration = 2f;

        [Tooltip("淡出回正常的秒数")]
        [Min(0.05f)] [SerializeField] private float _fadeOutDuration = 1.5f;

        [Header("缩放")]
        [Tooltip("起始缩放（中心小点）")]
        [Min(0.001f)] [SerializeField] private float _startScale = 0.05f;

        [Tooltip("结束缩放（建议比屏幕大 30%，确保覆盖全屏）")]
        [Min(1f)] [SerializeField] private float _endScale = 1.3f;

        [Header("动画曲线")]
        [Tooltip("扩散曲线：0 慢中快尾，1 线性，2 缓入缓出")]
        [Range(0, 2)] [SerializeField] private int _spreadEase = 2;

        [Header("事件")]
        [Tooltip("动画开始时触发（可挂 AudioManager 播'涌现'音效）")]
        public UnityEvent OnOverlayStart;

        [Tooltip("完全覆盖时触发（可挂 DialogueManager 弹对话）")]
        public UnityEvent OnOverlayFull;

        [Tooltip("淡出结束时触发")]
        public UnityEvent OnOverlayEnd;

        private Coroutine _animRoutine;
        private Vector3 _baseScale;
        private bool _wasOriginalActive;

        private void Awake()
        {
            if (_overlayImage == null)
                _overlayImage = GetComponent<Image>();

            if (_overlayImage == null)
            {
                Debug.LogError(
                    "[EmotionOverlay] 缺少 Image 组件，无法工作。", this);
                enabled = false;
                return;
            }

            // 如果没有指定 sprite，可选地运行时自动生成一个柔边圆形（白→透明）。
            // — 现阶段默认不启用。 兔司 将提供正式柔边圆 sprite 后，本注释块可删。
            //
            // === 需要 fallback 时解开下面的注释 ===
            // if (_overlayImage.sprite == null)
            // {
            //     _overlayImage.sprite = CreateRadialSprite(256);
            //     Debug.Log("[EmotionOverlay] 自动生成柔边圆形 sprite", this);
            // }

            // 记录原始缩放（用户可在 Inspector 调 RectTransform）
            _baseScale = _overlayImage.rectTransform.localScale;
            _wasOriginalActive = _overlayImage.enabled;

            // 默认隐藏（只禁用 Image 组件，不禁用 GameObject，避免子物体 Button 被隐藏）
            _overlayImage.enabled = false;
        }

        /// <summary>用默认颜色显示一次完整动画</summary>
        public void ShowOverlay()
        {
            ShowOverlay(_defaultColor);
        }

        /// <summary>用指定颜色显示一次完整动画</summary>
        public void ShowOverlay(Color color)
        {
            if (_overlayImage == null) return;

            if (_animRoutine != null)
                StopCoroutine(_animRoutine);

            _animRoutine = StartCoroutine(OverlayRoutine(color));
        }

        /// <summary>立即停止动画并淡出</summary>
        public void ClearOverlay()
        {
            if (_overlayImage == null) return;

            if (_animRoutine != null)
            {
                StopCoroutine(_animRoutine);
                _animRoutine = null;
            }

            // 强制淡出
            StartCoroutine(FadeOutOnly());
        }

        public bool IsPlaying => _animRoutine != null;

        // ============ 动画协程 ============

        private IEnumerator OverlayRoutine(Color color)
        {
            _overlayImage.enabled = true;
            _overlayImage.color = color;
            _overlayImage.rectTransform.localScale =
                _baseScale * _startScale;

            // 阶段 0：事件
            OnOverlayStart?.Invoke();

            // 阶段 1：从中心扩散
            yield return StartCoroutine(SpreadAnimation());

            // 阶段 2：完全覆盖
            OnOverlayFull?.Invoke();

            // 阶段 3：停留
            if (_holdDuration > 0f)
                yield return new WaitForSecondsRealtime(_holdDuration);

            // 阶段 4：淡出
            yield return StartCoroutine(FadeOutAnimation());

            // 阶段 5：结束
            _overlayImage.enabled = _wasOriginalActive;
            OnOverlayEnd?.Invoke();
            _animRoutine = null;
        }

        private IEnumerator SpreadAnimation()
        {
            float t = 0f;

            while (t < _spreadDuration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / _spreadDuration);
                float eased = ApplyEase(p, _spreadEase);

                float scale = Mathf.Lerp(_startScale, _endScale, eased);
                _overlayImage.rectTransform.localScale =
                    _baseScale * scale;
                yield return null;
            }

            _overlayImage.rectTransform.localScale =
                _baseScale * _endScale;
        }

        private IEnumerator FadeOutAnimation()
        {
            float t = 0f;
            Color startColor = _overlayImage.color;

            while (t < _fadeOutDuration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / _fadeOutDuration);
                _overlayImage.color = Color.Lerp(
                    startColor,
                    new Color(startColor.r, startColor.g, startColor.b, 0f),
                    p
                );
                yield return null;
            }

            _overlayImage.color = new Color(
                startColor.r, startColor.g, startColor.b, 0f);
        }

        private IEnumerator FadeOutOnly()
        {
            yield return StartCoroutine(FadeOutAnimation());
            _overlayImage.enabled = _wasOriginalActive;
        }

        // ============ 工具 ============

        // === 备用：需要 fallback 时解开下面整块注释 ===
        // /// <summary>
        // /// 运行时生成柔边圆形 sprite（中心白色不透明 → 边缘透明）。
        // /// 用户无需准备外部图片。
        // /// </summary>
        // private static Sprite CreateRadialSprite(int size = 256)
        // {
        //     Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        //     Color[] pixels = new Color[size * size];
        //     Vector2 center = new Vector2((size - 1) / 2f, (size - 1) / 2f);
        //     float maxDist = size / 2f;
        //
        //     for (int y = 0; y < size; y++)
        //     {
        //         for (int x = 0; x < size; x++)
        //         {
        //             float dist = Vector2.Distance(new Vector2(x, y), center);
        //             float normalized = dist / maxDist; // 0 中心, 1 边缘
        //             float alpha = 1f - normalized;
        //             alpha = Mathf.Clamp01(alpha);
        //             alpha = alpha * alpha; // 边缘更柔
        //             pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
        //         }
        //     }
        //
        //     tex.SetPixels(pixels);
        //     tex.Apply();
        //     return Sprite.Create(
        //         tex,
        //         new Rect(0, 0, size, size),
        //         new Vector2(0.5f, 0.5f));
        // }

        private float ApplyEase(float p, int mode)
        {
            switch (mode)
            {
                case 0: // ease-in
                    return p * p;
                case 1: // linear
                    return p;
                case 2: // smoothstep
                default:
                    return p * p * (3f - 2f * p);
            }
        }

#if UNITY_EDITOR
        // 编辑器里：选中物体时实时显示扩散范围
        private void OnDrawGizmosSelected()
        {
            if (_overlayImage == null) return;

            Transform t = _overlayImage.transform;
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
            Gizmos.DrawWireSphere(t.position, _endScale);
        }
#endif
    }
}
