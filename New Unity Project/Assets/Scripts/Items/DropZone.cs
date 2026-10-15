
using UnityEngine;
using UnityEngine.Events;

namespace Game.Items.DragDrop
{
    /// <summary>
    /// 拖拽接收目标。
    /// 保留原有接收筛选和 OnItemReceived 事件，并通知规则管理器。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DropZone : MonoBehaviour
    {
        [Header("目标 ID")]
        [Tooltip("规则中用于识别这个目标的唯一 ID")]
        [SerializeField] private string _targetId;

        public string TargetId => _targetId;

        [Header("接收道具 ID 列表")]
        [Tooltip("可接收的道具 ID，留空 = 接收任何道具")]
        [SerializeField] private string[] _acceptedItemIds;

        [Header("规则管理器")]
        [SerializeField] private DragDropRuleManager _ruleManager;

        [Header("事件")]
        [Tooltip("有匹配的 DraggableItem 拖入时触发")]
        public UnityEvent<DraggableItem> OnItemReceived;

        public bool AcceptsItem(string itemId)
        {
            if (_acceptedItemIds == null || _acceptedItemIds.Length == 0)
                return true;

            if (string.IsNullOrEmpty(itemId))
                return false;

            foreach (var id in _acceptedItemIds)
            {
                if (id == itemId)
                    return true;
            }

            return false;
        }

        /// <summary>由 DraggableItem 在道具成功拖入时调用。</summary>
        public void OnItemDropped(DraggableItem item)
        {
            if (item == null)
                return;

            // 保留原有事件，避免影响已有谜题逻辑。
            OnItemReceived?.Invoke(item);

            // 通知规则管理器：场景物品成功拖到了这个目标。
            if (_ruleManager != null)
            {
                _ruleManager.RegisterSuccessfulDrop(
                    item.ItemId,
                    _targetId,
                    false
                );
            }
            else
            {
                Debug.LogWarning(
                    $"DropZone「{name}」尚未绑定 DragDropRuleManager。",
                    this
                );
            }
        }
    }
}
