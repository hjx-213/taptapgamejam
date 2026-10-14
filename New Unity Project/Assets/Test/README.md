# 主系统模块冒烟测试说明

本文件夹是给队友快速验证 5 个主系统模块用的临时测试。

> ⚠️ 这些脚本和场景**不是游戏内容**，验证完可自行删除（或保留作回归测试）。

---

## 测什么

| 模块 | 测试脚本 | 验证点 |
|---|---|---|
| EmotionBar（情绪条） | `EmotionTest.cs` | 加情绪后数值变化 + 颜色渐变（冷→暖→红） |
| DraggableItem（场景拖拽） | `DragTest.cs` | 鼠标拖动物体、松手检测 DropZone、消耗/归位 |
| DropZone（拖拽目标） | `DropTest.cs` | 匹配 itemId 后接收事件 |
| 背包 UI（拾取/使用/提示） | `TestSlotClick.cs` | 拾取→入格、悬停→提示、点击→消失 |

> SceneTransitionManager（黑屏淡入淡出）之前已单独用 `TestButton.cs` + `SceneA`/`SceneB` 测过，这里不再重复。

---

## 3. 背包 UI 测试（拾取 / 使用 / 提示）

### 搭建

1. **UI Canvas** 已在场景里（前面测情绪条时已建），继续用同一个
2. 准备 ItemDatabase / ItemData：
   - Project 面板右键 → **Create → Game → Item Database**，命名 `ItemDatabase.asset`，拖到场景里 `GameManager` 物体上的 `Inventory` 组件的 `Database` 字段
   - 右键 → **Create → Game → Item**，命名 `Item_Key.asset`，填入：
     - `itemId` = `key_main`
     - `itemName` = `生锈的钥匙`
     - `description` = `一把很旧的钥匙，似乎能打开某扇门`
     - `icon` = 任意 Sprite（拖一张内置 UISprite 或别的图）
   - 打开 `ItemDatabase.asset`，`Items` 列表里 Add Element → 拖入 `Item_Key.asset`
3. 场景里建一个**空物体**作为"可拾取道具"（2D Object → Sprites → Square），改名 `Pickup_Key`
   - ⚠️ **先加 Box Collider 2D**（Add Component → Box Collider 2D）
   - 然后加 **ItemPickup** 组件（Add Component → 搜 Item Pickup）
   - ItemPickup 的 `Item Id` = `key_main`

> ⚠️ **重要提醒**：ItemPickup / ItemUsage / DraggableItem / DropZone 都标了 `[RequireComponent(typeof(Collider2D))]`。Square 模板不一定带 Collider2D，必须**先加 Collider2D 再加这些脚本**，否则会弹 "Adding component failed. Add required component ... first" 错误。
4. **建背包 UI 槽位**：
   - Canvas 下 → 右键 → **UI → Image**（会自动变 Button，但后面改成 Image 即可）
   - 改名 `Slot_1`，调整 RectTransform：**Pos Y = 80，宽 80，高 80**（底部居中）
   - 挂 **InventorySlot** 组件
   - 选 `Slot_1` → 右键 → **UI → Image**（子物体 `Icon`）→ 拖入到 InventorySlot 的 `Icon Image` 字段
   - 复制 4 次（`Slot_2` ~ `Slot_5`），横向排列
5. **建 Tooltip**：
   - Canvas 下 → 右键 → **UI → Image**（半透明黑底，命名 `TooltipPanel`）
   - Panel 下加 **UI → Text**（道具名，白色）→ 拖入到 TooltipUI 的 `Name Text`
   - 再加 **UI → Text**（描述，灰色）→ 拖入到 `Description Text`
   - 选 Panel → 挂 **TooltipUI** 组件
   - 选 Panel 物体，Inspector 取消勾选（起始隐藏）
6. **建 InventoryUI 控制器**：
   - Canvas 下 → 右键 → **Create Empty**（命名 `InventoryUI`）
   - 挂 **InventoryUI** 组件
   - `Slots` 列表里把 5 个 Slot 都拖入
7. **测试点击脚本**（验证"使用后消失"）：
   - 每个 `Slot_N` 上挂 **TestSlotClick** 组件，默认 `Click To Remove = true`

### 运行与预期

1. Play ▶
2. 点击场景里的 `Pickup_Key`：
   - Console: `[Inventory] ✨ 获得道具: 生锈的钥匙`
   - 屏幕：`Slot_1` 出现钥匙图标
3. 鼠标停在 `Slot_1` 上：
   - 槽位轻微变亮
   - 出现 Tooltip 框："生锈的钥匙 / 一把很旧的钥匙，似乎能打开某扇门"
4. 点击 `Slot_1`：
   - Console: `[TestSlotClick] 移除 生锈的钥匙`
   - `Slot_1` 重新变空，Tooltip 消失
5. （进阶）拖拽生成测试：和 DraggableItem 测试结合，让 DropZone 在 `OnItemDropped` 事件里调 `Inventory.Instance.Add(和解之酒的数据)`，能看到新物品入格

---

## 常见问题

| 现象 | 原因 | 解法 |
|---|---|---|
| EmotionBar 无 Image Type 选项 | Source Image 是 None | 先给 Source Image 塞个 Sprite |
| 拖不动方块 | Camera 没 Physics2DRaycaster | Main Camera 加该组件 |
| 拖入不触发 | B 的 Collider 没勾 Is Trigger | 勾上 |
| 拖入不触发 | Accepted Item Ids 不匹配 | A/B 都用 `stone_main` |
| 方块被背景挡住 | URP 模板背景 SortingOrder 高 | A/B 的 Order in Layer 改 100 |
| 拖了没反应 | 在 Scene 视图拖而非 Game 视图 | 必须在 Game 视图里完成拖放 |
| DontDestroyOnLoad 警告 | EmotionBar 挂在 Canvas 子级 | 移到根级空物体 |
| 拾取后槽位不显示 | InventoryUI 没拖 Slot 进去 | 拖 5 个 Slot 到 Slots 列表 |
| Tooltip 不显示 | TooltipPanel 起始 SetActive(false) 未生效 | 检查 Panel 物体起始未勾选 |
| Tooltip 一直显示 | Mouse Exit 没触发 | 检查 Slot 上没其他东西拦截事件 |
| Add Component 弹 "required component" 错 | Square 不一定带 Collider2D | 先加 Box Collider 2D 再加脚本 |

---

---

## 搭建测试场景（5 分钟）

### 1. EmotionBar 测试

1. Hierarchy → **UI → Image**（自动建 Canvas + EventSystem）
2. 选中 Image → **Image 组件**：
   - **Source Image**：点右边小圆圈选 `UISprite`（或任意 Sprite，否则 Image Type 不出现）
   - **Image Type** = `Filled`，**Fill Method** = `Horizontal`
3. 把 Image 拖到一个**根级空物体**（不要挂在 Canvas 子级下，否则 DontDestroyOnLoad 警告）
4. 根物体挂 `EmotionBar` 组件 → Fill Image 拖入刚才的 Image
5. 挂 `EmotionTest` 组件（默认加 50，可在 Inspector 改 Test Value）
6. Play ▶ → 情绪条填充到 50% 并显示**橙色**

### 2. DraggableItem + DropZone 测试

1. Hierarchy → **2D Object → Sprites → Square**（方块 A）
2. A 上加 **Box Collider 2D** + `DraggableItem` + `DragTest`，**Item Id** = `stone_main`
3. 再建一个 Square（方块 B）
4. B 上加 **Box Collider 2D**（勾 **Is Trigger**）+ `DropZone` + `DropTest`，**Accepted Item Ids** = `stone_main`
5. **Main Camera** → 加 **Physics2DRaycaster**（拖拽依赖这个，否则拖不动）
6. 如果方块被背景挡住：选中 A/B → Sprite Renderer → **Order in Layer** 改成 `100`

---

## 运行与预期

### EmotionBar
- Play → Console 打印：
  ```
  [EmotionTest] 测试：添加情绪 50
  [EmotionTest] 当前进度 = 50%（1.0=满）
  ```
- 屏幕上情绪条填充到一半，颜色偏橙。

### 拖拽（关键：在 **Game 视图**里拖，不是 Scene 视图！）

- 不勾 Consume On Drop → 拖 A 到 B 上松开 → **A 弹回原位**
  ```
  [DragTest] ✅ 拖拽成功，落到 DropZone: ...
  [DropTest] ✅ 收到道具：stone_main（来自物体 ...）
  ```
- 勾 Consume On Drop → 拖 A 到 B 上松开 → **A 消失**

---

## 常见问题

| 现象 | 原因 | 解法 |
|---|---|---|
| EmotionBar 无 Image Type 选项 | Source Image 是 None | 先给 Source Image 塞个 Sprite |
| 拖不动方块 | Camera 没 Physics2DRaycaster | Main Camera 加该组件 |
| 拖入不触发 | B 的 Collider 没勾 Is Trigger | 勾上 |
| 拖入不触发 | Accepted Item Ids 不匹配 | A/B 都用 `stone_main` |
| 方块被背景挡住 | URP 模板背景 SortingOrder 高 | A/B 的 Order in Layer 改 100 |
| 拖了没反应 | 在 Scene 视图拖而非 Game 视图 | 必须在 Game 视图里完成拖放 |
| DontDestroyOnLoad 警告 | EmotionBar 挂在 Canvas 子级 | 移到根级空物体 |

---

## 备注

- `DraggableItem` 的 `_consumeOnDrop`：true = 消耗品（用完消失，调酒药材/纸条约章），false = 工具（用完归位，钥匙/撬棍）。
- 调酒谜题会用到 DraggableItem（大麦酒/清水 → 陶土壶）+ DropZone；情绪场景会用到 EmotionBar。
- 使用后可删本文件夹。
