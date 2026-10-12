using UnityEngine;
using UnityEngine.Events;

namespace Game.Items.DragDrop
{
    /// <summary>
    /// 拖拽接收目标（如陶土壶、药锅、解谜目标点等）。
    ///
    /// 用法：
    ///   - 挂在目标物体上（带 Collider2D，建议 IsTrigger=true）
    ///   - Inspector 设置 AcceptedItemIds（接收的道具 ID 列表）
    ///     * 留空 = 接收任何道具
    ///   - 监听 OnItemReceived 事件（DraggableItem 拖入时触发，参数是 DraggableItem 本身）
    ///
    /// 配方检查逻辑（如调酒"两份大麦酒 + 一份清水"）由谜题脚本负责，本类只负责"接收"。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DropZone : MonoBehaviour
    {
        [Header("接收道具 ID 列表")]
        [Tooltip("可接收的道具 ID，留空 = 接收任何道具")]
        [SerializeField] private string[] _acceptedItemIds;

        [Header("事件")]
        [Tooltip("有匹配的 DraggableItem 拖入时触发")]
        public UnityEvent<DraggableItem> OnItemReceived;

        /// <summary>判断是否接收某个 itemId</summary>
        public bool AcceptsItem(string itemId)
        {
            if (_acceptedItemIds == null || _acceptedItemIds.Length == 0)
                return true; // 空数组 = 接收所有

            if (string.IsNullOrEmpty(itemId)) return false;

            foreach (var id in _acceptedItemIds)
            {
                if (id == itemId) return true;
            }
            return false;
        }

        /// <summary>由 DraggableItem 拖入时调用，触发事件</summary>
        public void OnItemDropped(DraggableItem item)
        {
            OnItemReceived?.Invoke(item);
        }
    }
}