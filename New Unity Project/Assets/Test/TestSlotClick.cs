using UnityEngine;
using Game.Items;
using Game.UI;

namespace Game.Test
{
    /// <summary>
    /// 背包 UI 测试用：每个槽位挂上后，**点击槽位 = 移除该道具**（模拟"使用后消失"）。
    /// 仅用于测试。真实游戏中可能换成"选中后点世界物体使用"或"直接弹出使用菜单"。
    /// </summary>
    public class TestSlotClick : MonoBehaviour
    {
        [Tooltip("测试模式：true = 点击直接移除（验证消失），false = 只在 Console 打印")]
        [SerializeField] private bool _clickToRemove = true;

        private void Start()
        {
            var slot = GetComponent<InventorySlot>();
            if (slot != null)
            {
                slot.OnClicked.AddListener(item => OnSlotClicked(item));
            }
            else
            {
                Debug.LogError("[TestSlotClick] 必须挂在带 InventorySlot 的物体上");
            }
        }

        private void OnSlotClicked(ItemData item)
        {
            if (item == null) return;
            Debug.Log($"[TestSlotClick] 点击槽位: {item.itemName} ({item.itemId})");

            if (_clickToRemove && Inventory.Instance != null)
            {
                Inventory.Instance.Remove(item.itemId);
                Debug.Log($"[TestSlotClick] 移除 {item.itemName}（验证使用后消失效果）");
            }
        }
    }
}
