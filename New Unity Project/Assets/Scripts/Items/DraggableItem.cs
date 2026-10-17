using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Game.Items.DragDrop
{
    // 场景内可拖拽道具（不入背包，只在场景里拖）
    // 用法：
    //   - 挂在任何带 Box Collider 2D 的物体上
    //   - Inspector 设置 ItemId（道具标识符）
    //   - 鼠标按下开始拖拽，松开时检测下方 DropZone
    //   - 命中匹配的 DropZone 触发 OnDropped 事件；未命中自动归位
    // 注意：本组件依赖 EventSystem（场景里要有 EventSystem，相机要有 Physics 2D Raycaster）
    [RequireComponent(typeof(Collider2D))]
    public class DraggableItem : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("道具")]
        [Tooltip("道具 ID（用于匹配 DropZone 的 AcceptedItemIds）")]
        [SerializeField] private string _itemId;

        [Header("事件")]
        [Tooltip("拖到匹配的 DropZone 上时触发，参数是接收方 DropZone")]
        public UnityEvent<DropZone> OnDropped;

        [Tooltip("拖拽结束时没找到 DropZone 时触发（用于做吸附/晃动反馈）")]
        public UnityEvent OnDropCancelled;

        private Camera _mainCamera;
        private Vector3 _originalPosition;
        private Vector3 _dragOffset;
        private bool _isDragging;

        public string ItemId => _itemId;
        public bool IsDragging => _isDragging;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isDragging = true;
            _originalPosition = transform.position;
            _dragOffset = transform.position - ScreenToWorld(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            transform.position = ScreenToWorld(eventData.position) + _dragOffset;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isDragging) return;
            _isDragging = false;

            // 检测鼠标松开位置的 DropZone
            Vector2 worldPoint = ScreenToWorld(eventData.position);
            Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint);

            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                var zone = hit.GetComponent<DropZone>();
                if (zone != null && zone.AcceptsItem(_itemId))
                {
                    zone.OnItemDropped(this);
                    OnDropped?.Invoke(zone);

                    // 默认行为：拖拽成功后回到原位，不因消耗设置而消失
                    transform.position = _originalPosition;
                    return;
                }
            }

            // 没匹配到任何 DropZone：归位
            transform.position = _originalPosition;
            OnDropCancelled?.Invoke();
        }

        // 重置拖拽道具。会重新激活并归位
        public void ResetItem()
        {
            gameObject.SetActive(true);
            transform.position = _originalPosition;
            _isDragging = false;
        }

        private Vector3 ScreenToWorld(Vector2 screenPos)
        {
            if (_mainCamera == null) return Vector3.zero;
            float depth = Mathf.Abs(_mainCamera.transform.position.z);
            Vector3 sp = new Vector3(screenPos.x, screenPos.y, depth);
            return _mainCamera.ScreenToWorldPoint(sp);
        }
    }
}