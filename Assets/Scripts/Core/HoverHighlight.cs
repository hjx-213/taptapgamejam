using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Core.Interaction
{
    /// <summary>
    /// 鼠标悬停高亮反馈（2D 版本）。
    /// 走 Unity EventSystem，所以场景里必须有：
    ///   1. EventSystem 对象（UI Canvas 创建时自动加）
    ///   2. Main Camera 上挂 Physics 2D Raycaster
    ///
    /// 鼠标移到物体上自动高亮，移开还原。
    /// 这就是 S 级"物品反馈"的视觉部分
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HoverHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("高亮颜色")]
        [SerializeField] private Color _highlightColor = new Color(1f, 0.95f, 0.6f, 1f);
        [Range(1f, 2f)]
        [SerializeField] private float _highlightIntensity = 1.3f;

        private SpriteRenderer _renderer;
        private Color _originalColor;
        private bool _isHovered;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer != null) _originalColor = _renderer.color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            ApplyHighlight(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            ApplyHighlight(false);
        }

        private void ApplyHighlight(bool on)
        {
            if (_renderer == null) return;
            if (on)
            {
                // 简单实现：颜色混合（亮度 + 高光色）
                _renderer.color = _originalColor * _highlightIntensity + _highlightColor * 0.2f;
            }
            else
            {
                _renderer.color = _originalColor;
            }
        }

        private void OnDisable()
        {
            if (_isHovered)
            {
                _isHovered = false;
                ApplyHighlight(false);
            }
        }
    }
}