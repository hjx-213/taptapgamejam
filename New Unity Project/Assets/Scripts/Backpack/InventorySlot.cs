
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Items;
using Game.Items.DragDrop;

namespace Game.UI
{
    /// <summary>
    /// 背包槽位：显示道具、悬停提示、点击使用，并支持拖拽到场景目标。
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class InventorySlot : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [Header("引用")]
        [SerializeField] private Image _iconImage;

        [Header("反馈")]
        [SerializeField] private Color _hoverColor =
            new Color(0.85f, 0.85f, 0.85f, 1f);

        [Header("拖拽规则")]
        [SerializeField] private DragDropRuleManager _ruleManager;

        [Header("事件")]
        public SlotClickEvent OnClicked;

        [System.Serializable]
        public class SlotClickEvent :
            UnityEngine.Events.UnityEvent<ItemData> { }

        public ItemData CurrentItem { get; private set; }
        public bool IsEmpty => CurrentItem == null;

        private Image _background;
        private Color _originalColor;
        private Camera _mainCamera;

        private bool _isDragging;
        private Vector3 _iconOriginalPosition;
        private string _draggedItemId;

        private void Awake()
        {
            _background = GetComponent<Image>();
            _originalColor = _background.color;
            _mainCamera = Camera.main;

            if (_iconImage == null)
            {
                foreach (var img in GetComponentsInChildren<Image>(true))
                {
                    if (img.gameObject != gameObject &&
                        img.gameObject.name.ToLower().Contains("icon"))
                    {
                        _iconImage = img;
                        break;
                    }
                }
            }

            Clear();
        }

        public void SetItem(ItemData item)
        {
            CurrentItem = item;

            if (_iconImage != null)
            {
                _iconImage.sprite = item.icon;
                _iconImage.enabled = item.icon != null;
            }
        }

        public void Clear()
        {
            CurrentItem = null;

            if (_iconImage != null)
            {
                _iconImage.sprite = null;
                _iconImage.enabled = false;
            }
        }

        // ---------- 悬停与点击：保留原有功能 ----------

        public void OnPointerEnter(PointerEventData eventData)
        {
            _background.color = _hoverColor;

            if (!IsEmpty && TooltipUI.Instance != null)
                TooltipUI.Instance.Show(CurrentItem);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _background.color = _originalColor;

            if (TooltipUI.Instance != null)
                TooltipUI.Instance.Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging || IsEmpty)
                return;

            OnClicked?.Invoke(CurrentItem);
        }

        // ---------- 背包道具拖拽 ----------

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsEmpty || _iconImage == null)
                return;

            _isDragging = true;
            _draggedItemId = CurrentItem.itemId;
            _iconOriginalPosition = _iconImage.rectTransform.position;

            // 避免拖拽过程中 Tooltip 挡住画面。
            if (TooltipUI.Instance != null)
                TooltipUI.Instance.Hide();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || _iconImage == null)
                return;

            // 只移动图标，不移动整个背包槽位。
            _iconImage.rectTransform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;

            _isDragging = false;

            // 无论成功与否，图标都回到槽位原处。
            if (_iconImage != null)
                _iconImage.rectTransform.position = _iconOriginalPosition;

            string itemId = _draggedItemId;
            _draggedItemId = null;

            if (string.IsNullOrEmpty(itemId))
                return;

            if (_mainCamera == null)
                _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                Debug.LogWarning("[InventorySlot] 找不到主相机，无法检测拖拽目标。");
                return;
            }

            // 将鼠标位置转换为世界坐标，检测下方的 2D 碰撞体。
            float depth = Mathf.Abs(_mainCamera.transform.position.z);

            Vector3 screenPoint = new Vector3(
                eventData.position.x,
                eventData.position.y,
                depth
            );

            Vector2 worldPoint = _mainCamera.ScreenToWorldPoint(screenPoint);
            Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint);

            foreach (var hit in hits)
            {
                if (hit == null)
                    continue;

                var zone = hit.GetComponent<DropZone>();

                if (zone == null)
                    zone = hit.GetComponentInParent<DropZone>();

                if (zone == null || !zone.AcceptsItem(itemId))
                    continue;

                // 确认背包中仍有这个道具。
                if (Inventory.Instance == null ||
                    !Inventory.Instance.Has(itemId))
                {
                    return;
                }

                // 通知规则管理器：这是背包物品拖入目标。
                if (_ruleManager != null)
                {
                    _ruleManager.RegisterSuccessfulDrop(
                        itemId,
                        zone.TargetId,
                        true
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "[InventorySlot] 尚未绑定 DragDropRuleManager。",
                        this
                    );
                }

                // 成功放入目标后消耗背包物品。
                Inventory.Instance.Remove(itemId);

                Debug.Log(
                    $"[InventorySlot] 道具 {itemId} 已拖入目标 {zone.TargetId}。"
                );

                return;
            }

            // 没有成功放入目标：不移除道具。
        }
    }
}
