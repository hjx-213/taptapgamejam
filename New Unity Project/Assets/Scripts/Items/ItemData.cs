using UnityEngine;

namespace Game.Items
{
    // 单个道具的数据定义
    // 在 Project 面板右键 → Create → Game → Item 创建，是一个 .asset 文件
    // 字段说明：
    //   - itemId：程序内部使用的唯一 ID，必须全局唯一
    //   - itemName：UI 显示名（如 "带血银角杯"）
    //   - icon：UI 背包格子里的图标
    [CreateAssetMenu(fileName = "Item_", menuName = "Game/Item", order = 0)]
    public class ItemData : ScriptableObject
    {
        [Tooltip("程序内部使用的唯一 ID，必须全局唯一")]
        public string itemId;

        [Tooltip("UI 显示名")]
        public string itemName = "未命名道具";

        [Tooltip("UI 显示描述")]
        [TextArea(2, 4)]
        public string description = "";

        [Tooltip("UI 背包格子图标")]
        public Sprite icon;
    }
}