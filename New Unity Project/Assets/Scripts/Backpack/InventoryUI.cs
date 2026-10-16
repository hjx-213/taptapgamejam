using System.Collections.Generic;
using UnityEngine;
using Game.Items;

namespace Game.UI
{
    // 背包 UI 控制器：监听 Inventory 事件，自动把新道具填入第一个空格位；
    // 道具被移除时找到对应槽位并清空。
    public class InventoryUI : MonoBehaviour
    {
        [Header("槽位")]
        [Tooltip("所有槽位（按顺序，第一个空格位优先填入）")]
        [SerializeField] private List<InventorySlot> _slots = new List<InventorySlot>();

        [Header("打印事件")]
        [Tooltip("启用了会在 Console 打印每次事件")]
        [SerializeField] private bool _verbose = true;

        public static InventoryUI Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // 注意：不调用 DontDestroyOnLoad——因为 InventoryUI 通常是 Canvas 子物体
            // 跨场景时 UI 会随场景重建，槽位会从 Inventory 重新同步
        }

        private void Start()
        {
            // 启动时如果有 Inventory，订阅事件并同步现有道具
            TryBindToInventory();
        }

        private void OnEnable()
        {
            // 重新激活时（比如手记→背包切换）从 Inventory 同步现有道具
            // 因为物品可能在 InventoryUI 禁用期间被加入
            if (Inventory.Instance != null)
            {
                if (!_isBound) TryBindToInventory();
                else RefreshFromInventory();
            }
        }

        private void Update()
        {
            // Inventory 可能在场景切换后才被加载，重试绑定
            if (Inventory.Instance != null && !_isBound)
            {
                TryBindToInventory();
            }
        }

        private bool _isBound;

        private void TryBindToInventory()
        {
            if (_isBound) return; // 守卫：防止 OnEnable + Start 重复订阅
            if (Inventory.Instance == null) return;
            Inventory.Instance.OnItemAdded.AddListener(HandleItemAdded);
            Inventory.Instance.OnItemRemoved.AddListener(HandleItemRemoved);
            _isBound = true;

            // 同步已有道具（场景切换后可能背包已持有东西）
            RefreshFromInventory();

            if (_verbose) Debug.Log($"[InventoryUI] 已绑定 Inventory，槽位 {_slots.Count} 个");
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_isBound && Inventory.Instance != null)
            {
                Inventory.Instance.OnItemAdded.RemoveListener(HandleItemAdded);
                Inventory.Instance.OnItemRemoved.RemoveListener(HandleItemRemoved);
            }
        }

        // ---------- 事件处理 ----------
        private void HandleItemAdded(ItemData item)
        {
            // 找第一个空格位
            var slot = _slots.Find(s => s != null && s.IsEmpty);
            if (slot == null)
            {
                if (_verbose) Debug.LogWarning($"[InventoryUI] 背包已满，无法加入 {item.itemName}");
                return;
            }

            slot.SetItem(item);
            if (_verbose) Debug.Log($"[InventoryUI] 槽位 {slot.name} 填入 {item.itemName}");
        }

        private void HandleItemRemoved(string itemId)
        {
            var slot = _slots.Find(s => s != null && !s.IsEmpty && s.CurrentItem.itemId == itemId);
            if (slot == null) return;

            if (_verbose) Debug.Log($"[InventoryUI] 槽位 {slot.name} 清空 {itemId}");
            slot.Clear();
        }

        // ---------- 公共方法 ----------
        // 显示背包 UI 根物体
        public void Show() { gameObject.SetActive(true); }

        // 隐藏背包 UI 根物体
        public void Hide() { gameObject.SetActive(false); }

        // 切换显示/隐藏
        public void Toggle() { gameObject.SetActive(!gameObject.activeSelf); }

        // 用 Inventory 当前内容刷新所有槽位（用于切换场景后同步）
        public void RefreshFromInventory()
        {
            if (Inventory.Instance == null) return;

            // 先清空所有
            foreach (var s in _slots) if (s != null) s.Clear();

            // 再按 AllOwnedIds 顺序填入
            foreach (var id in Inventory.Instance.AllOwnedIds)
            {
                var data = ItemDatabaseRegistry.Current?.Get(id);
                if (data != null) HandleItemAdded(data);
            }
        }
    }
}
