using UnityEngine;
using UnityEngine.Events;

namespace Game.Items.DragDrop
{
    // 拖拽接收目标。
    // 保留原有接收筛选和 OnItemReceived 事件，并通知规则管理器。
    [RequireComponent(typeof(Collider2D))]
    public class DropZone : MonoBehaviour
    {
        [Header("该物品 ID")]
        [Tooltip("规则中用于识别这个物品的唯一 ID")]
        [SerializeField] private string _targetId;

        public string TargetId => _targetId;

        [Header("可接收物品列表")]
        [Tooltip("可接收的物品 ID，留空 = 接收任何道具")]
        [SerializeField] private string[] _acceptedItemIds;

        [Header("规则管理器")]
        [SerializeField] private DragDropRuleManager _ruleManager;

        // 可由特写面板等交互控制。保留 Collider2D 供场景点击使用时，
        // 可以单独禁止拖拽投放到场景里的目标。
        private bool _dropEnabled = true;

        [Header("事件")]
        [Tooltip("有匹配的 DraggableItem 拖入时触发")]
        public UnityEvent<DraggableItem> OnItemReceived;

        public bool AcceptsItem(string itemId)
        {
            if (!_dropEnabled)
                return false;

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

        public void SetDropEnabled(bool enabled)
        {
            _dropEnabled = enabled;
        }

        // 背包槽位没有必要引用某个场景的规则管理器；优先由目标区
        // 将拖拽结果转发给它当前场景绑定的管理器。
        public bool RegisterInventoryDrop(string itemId)
        {
            if (_ruleManager == null)
                return false;

            _ruleManager.RegisterSuccessfulDrop(
                itemId,
                _targetId,
                true
            );
            return true;
        }

        // 由 DraggableItem 在道具成功拖入时调用。
        public void OnItemDropped(DraggableItem item)
        {
            if (item == null)
                return;

            // 保留原有事件，避免影响已有谜题逻辑。
            OnItemReceived?.Invoke(item);

            // 通知当前场景绑定的规则管理器：场景物品成功拖到了这个目标。
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
