using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.Core.Emotion
{
    /// <summary>
    /// 情绪条：单条数值 + 颜色变化（冷色→暖色→红色），无数值标注。
    ///
    /// 用法：
    ///   - 挂到 UI 节点（Image），引用一个 fillImage（Fill Type = Horizontal）
    ///   - 调用 EmotionBar.Instance.AddEmotion(amount) 增/减情绪
    ///   - 颜色自动在 冷-暖-热 之间
    ///   - 越过暖阈值 / 热阈值时分别触发事件
    /// </summary>
    public class EmotionBar : MonoBehaviour
    {
        public static EmotionBar Instance { get; private set; }

        [Header("UI")]
        [Tooltip("填充满的 Image（Fill Type = Horizontal）")]
        [SerializeField] private Image _fillImage;

        [Header("数值")]
        [Tooltip("情绪条最大数值")]
        [SerializeField] private float _maxValue = 100f;
        [SerializeField] private float _currentValue = 0f;

        [Header("颜色档")]
        [SerializeField] private Color _coldColor = new Color(0.4f, 0.6f, 1f);  // 冷（蓝）
        [SerializeField] private Color _warmColor = new Color(1f, 0.6f, 0.3f);  // 暖（橙）
        [SerializeField] private Color _hotColor = new Color(1f, 0.2f, 0.2f);   // 热（红）

        [Header("阈值（maxValue 比例 0~1）")]
        [Range(0, 1)] [SerializeField] private float _warmThreshold = 0.4f;
        [Range(0, 1)] [SerializeField] private float _hotThreshold = 0.8f;

        [Header("事件")]
        [Tooltip("越过暖阈值时触发一次")]
        public UnityEvent OnWarmThresholdReached;
        [Tooltip("越过热阈值时触发一次（情绪最高）")]
        public UnityEvent OnHotThresholdReached;

        private bool _warmTriggeredThisCycle;
        private bool _hotTriggeredThisCycle;

        public float CurrentValue => _currentValue;
        public float MaxValue => _maxValue;
        public float Progress => _maxValue > 0f ? _currentValue / _maxValue : 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject); // 情绪跨场景保留
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            UpdateVisuals();
        }

        /// <summary>增加 / 减少情绪（amount 可为负）</summary>
        public void AddEmotion(float amount)
        {
            _currentValue = Mathf.Clamp(_currentValue + amount, 0f, _maxValue);

            float p = Progress;
            if (!_warmTriggeredThisCycle && p >= _warmThreshold)
            {
                _warmTriggeredThisCycle = true;
                OnWarmThresholdReached?.Invoke();
            }
            if (!_hotTriggeredThisCycle && p >= _hotThreshold)
            {
                _hotTriggeredThisCycle = true;
                OnHotThresholdReached?.Invoke();
            }
        }

        /// <summary>重置情绪条（新一轮）</summary>
        public void ResetEmotion()
        {
            _currentValue = 0f;
            _warmTriggeredThisCycle = false;
            _hotTriggeredThisCycle = false;
        }

        private void UpdateVisuals()
        {
            if (_fillImage == null) return;
            float t = Progress;
            _fillImage.fillAmount = t;
            _fillImage.color = ComputeColor(t);
        }

        private Color ComputeColor(float t)
        {
            if (t <= _warmThreshold)
            {
                float k = _warmThreshold > 0f ? (t / _warmThreshold) : 1f;
                return Color.Lerp(_coldColor, _warmColor, k);
            }
            if (t <= _hotThreshold)
            {
                float range = _hotThreshold - _warmThreshold;
                float k = range > 0f ? (t - _warmThreshold) / range : 1f;
                return Color.Lerp(_warmColor, _hotColor, k);
            }
            return _hotColor;
        }
    }
}