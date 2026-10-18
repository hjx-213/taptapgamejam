using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Items;
using Game.Items.DragDrop;

namespace Game.UI
{
    // 背包槽位：显示道具、悬停提示，并支持拖拽到场景目标。
    [RequireComponent(typeof(Image))]
    public class InventorySlot : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
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

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsEmpty || _iconImage == null)
                return;

            _isDragging = true;
            _draggedItemId = CurrentItem.itemId;
            _iconOriginalPosition = _iconImage.rectTransform.position;

            if (TooltipUI.Instance != null)
                TooltipUI.Instance.Hide();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || _iconImage == null)
                return;

            _iconImage.rectTransform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;

            _isDragging = false;

            if (_iconImage != null)
                _iconImage.rectTransform.position = _iconOriginalPosition;

            string itemId = _draggedItemId;
            _draggedItemId = null;

            if (string.IsNullOrEmpty(itemId))
                return;

            // Screen-space puzzle panels are UI targets rather than world colliders.
            // Give them first chance to consume the inventory drag.
            if (EventSystem.current != null)
            {
                var pointer = new PointerEventData(EventSystem.current)
                {
                    position = eventData.position
                };
                var uiHits = new System.Collections.Generic.List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, uiHits);
                foreach (var uiHit in uiHits)
                {
                    var closeupTarget = uiHit.gameObject.GetComponentInParent<SinkCloseupDropTarget>();
                    if (closeupTarget != null)
                    {
                        closeupTarget.TryDrop(itemId);
                        return;
                    }
                }
            }

            if (_mainCamera == null)
                _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                Debug.LogWarning("[InventorySlot] 找不到主相机，无法检测拖拽目标。");
                return;
            }

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

                if (Inventory.Instance == null ||
                    !Inventory.Instance.Has(itemId))
                {
                    return;
                }

                // 目标区知道当前场景应该使用哪个规则管理器；背包是跨场景 UI，
                // 其预制体上的 _ruleManager 通常为空，所以先向 DropZone 转发。
                bool registeredByDropZone = zone.RegisterInventoryDrop(itemId);

                if (!registeredByDropZone && _ruleManager != null)
                {
                    _ruleManager.RegisterSuccessfulDrop(
                        itemId,
                        zone.TargetId,
                        true
                    );
                }
                else if (!registeredByDropZone)
                {
                    Debug.LogWarning(
                        "[InventorySlot] DropZone 和 InventorySlot 都没有绑定 DragDropRuleManager；物品仍会被消耗，但不会计入拖拽规则。",
                        this
                    );
                }

                bool itemRemoved = Inventory.Instance.Remove(itemId);

                if (itemRemoved)
                {
                    BackpackPanelToggle backpackToggle =
                        FindObjectOfType<BackpackPanelToggle>();

                    if (backpackToggle != null)
                        backpackToggle.Close();
                }

                Debug.Log($"[InventorySlot] 道具 {itemId} 已拖入目标 {zone.TargetId}。");
                return;
            }
        }
    }
}
