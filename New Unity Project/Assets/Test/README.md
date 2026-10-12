# 主系统模块冒烟测试说明

本文件夹是给队友快速验证 3 个主系统模块用的临时测试。

> ⚠️ 这些脚本和场景**不是游戏内容**，验证完可自行删除（或保留作回归测试）。

---

## 测什么

| 模块 | 测试脚本 | 验证点 |
|---|---|---|
| EmotionBar（情绪条） | `EmotionTest.cs` | 加情绪后数值变化 + 颜色渐变（冷→暖→红） |
| DraggableItem（场景拖拽） | `DragTest.cs` | 鼠标拖动物体、松手检测 DropZone、消耗/归位 |
| DropZone（拖拽目标） | `DropTest.cs` | 匹配 itemId 后接收事件 |

> SceneTransitionManager（黑屏淡入淡出）之前已单独用 `TestButton.cs` + `SceneA`/`SceneB` 测过，这里不再重复。

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
