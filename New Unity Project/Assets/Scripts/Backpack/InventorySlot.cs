using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Items;

namespace Game.UI
{
    /// <summary>
    /// 背包单个槽位：显示道具图标，悬停显示 Tooltip，点击触发"使用"。
    ///
    /// 搭建方式（每个槽位）：
    ///   1. UI Button（Image 在背景 + 子 Image 显示图标）
    ///   2. 在背景 Image 挂本组件
    ///   3. Inspector 拖入 Icon Image（子物体）
    ///
    /// 数据来源：InventoryUI 调用 SetItem / Clear。
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class InventorySlot : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("引用")]
        [Tooltip("显示道具图标的子 Image（默认隐藏，无道具时不显示）")]
        [SerializeField] private Image _iconImage;

        [Header("反馈")]
        [Tooltip("悬停时高亮颜色")]
        [SerializeField] private Color _hoverColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        [Header("事件")]
        [Tooltip("点击槽位时触发（参数：ItemData）。可挂事件使用道具、装备等。")]
        public SlotClickEvent OnClicked;

        [System.Serializable]
        public class SlotClickEvent : UnityEngine.Events.UnityEvent<ItemData> { }

        public ItemData CurrentItem { get; private set; }
        public bool IsEmpty => CurrentItem == null;

        private Image _background;
        private Color _originalColor;

        private void Awake()
        {
            _background = GetComponent<Image>();
            _originalColor = _background.color;

            if (_iconImage == null)
            {
                // 自动找一个叫 Icon 的子 Image
                foreach (var img in GetComponentsInChildren<Image>(true))
                {
                    if (img.gameObject != gameObject && img.gameObject.name.ToLower().Contains("icon"))
                    {
                        _iconImage = img;
                        break;
                    }
                }
            }

            Clear();
        }

        /// <summary>设置槽位道具（由 InventoryUI 调用）</summary>
        public void SetItem(ItemData item)
        {
            CurrentItem = item;
            if (_iconImage != null)
            {
                _iconImage.sprite = item.icon;
                _iconImage.enabled = item.icon != null;
            }
        }

        /// <summary>清空槽位（由 InventoryUI 调用）</summary>
        public void Clear()
        {
            CurrentItem = null;
            if (_iconImage != null)
            {
                _iconImage.sprite = null;
                _iconImage.enabled = false;
            }
        }

        // ---------- 鼠标事件 ----------
        public void OnPointerEnter(PointerEventData eventData)
        {
            _background.color = _hoverColor;
            if (!IsEmpty && TooltipUI.Instance != null)
            {
                TooltipUI.Instance.Show(CurrentItem);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _background.color = _originalColor;
            if (TooltipUI.Instance != null)
            {
                TooltipUI.Instance.Hide();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsEmpty) return;
            OnClicked?.Invoke(CurrentItem);
        }
    }
}
