using UnityEngine;
using Game.Items;
using Game.Items.DragDrop;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// 银角杯清洗规则自测脚本（临时，测完删）。
///
/// 用途：在 SceneB 这种干净场景里验证「DragDropRuleManager 的规则能否正确执行」。
/// 不依赖 2-3 的对话系统 / 拖拽链路 / 拾取逻辑。
///
/// 用法：
///   1. 打开 Assets/Test/SceneB
///   2. Hierarchy 右键 Create Empty → 命名 RuleTest_B
///   3. 给它挂本脚本
///   4. Inspector 里把 Assets/Data/Items/ItemDatabase.asset 拖到 Database 字段
///   5. ▶ Play → 按键盘 P 键
///   6. 看 Console：应打印 4 行 [拖拽规则] 日志 + 背包 Has(yinjiaobei)=True
///
/// 验证完删掉本脚本即可。
/// </summary>
public class RuleTest_B : MonoBehaviour
{
    [Tooltip("拖入 ItemDatabase.asset（Assets/Data/Items 下）")]
    [SerializeField] private ItemDatabase _database;

    private DragDropRuleManager _ruleManager;

    private void Start()
    {
        if (_database == null)
        {
            Debug.LogError(
                "[RuleTest_B] 未设置 ItemDatabase！请在 Inspector 拖入 ItemDatabase.asset");
            return;
        }

        // ---- 1. 自建 Inventory（带数据库）----
        // 先 inactive，这样 AddComponent 不会立即触发 Awake。
        var invGO = new GameObject("TestInventory");
        invGO.SetActive(false);
        var inventory = invGO.AddComponent<Inventory>();

        // Awake 之前先注入数据库（用反射写私有字段）
        var dbField = typeof(Inventory).GetField(
            "_database", BindingFlags.NonPublic | BindingFlags.Instance);
        if (dbField != null)
            dbField.SetValue(inventory, _database);

        // 激活 → 此时 Awake 跑，_database 已就位，无 Error
        invGO.SetActive(true);

        // 注册到全局表（ItemPickup/ItemUsage 会查这里）
        _database.Init();
        ItemDatabaseRegistry.SetDatabase(_database);

        // ---- 2. 自建 DragDropRuleManager + 规则 ----
        var ruleGO = new GameObject("TestRuleManager");
        ruleGO.SetActive(false);
        _ruleManager = ruleGO.AddComponent<DragDropRuleManager>();
        ruleGO.SetActive(true);

        var rule = new DragDropRuleManager.DragRule
        {
            ruleName = "清洗银角杯",
            requirements = new List<DragDropRuleManager.CountRequirement>
            {
                new DragDropRuleManager.CountRequirement
                {
                    sourceItemId = "daixueyinjiaobei",
                    targetId = "shuicao",
                    requiredCount = 1
                }
            },
            totalRequiredCount = 1,
            grantItem = true,
            rewardItemId = "yinjiaobei"
        };

        // rules 是私有字段，用反射写入
        var rulesField = typeof(DragDropRuleManager).GetField(
            "rules", BindingFlags.NonPublic | BindingFlags.Instance);
        if (rulesField != null)
            rulesField.SetValue(
                _ruleManager,
                new List<DragDropRuleManager.DragRule> { rule });

        Debug.Log(
            "[RuleTest_B] 准备完毕。按 P 键测试：拖带血银角杯到 shuicao → 发银角杯");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && _ruleManager != null)
        {
            Debug.Log("[RuleTest_B] P 触发：daixueyinjiaobei → shuicao");
            _ruleManager.RegisterSuccessfulDrop(
                "daixueyinjiaobei", "shuicao", true);

            if (Inventory.Instance != null)
            {
                Debug.Log(
                    $"[RuleTest_B] 背包是否含 yinjiaobei: " +
                    $"{Inventory.Instance.Has("yinjiaobei")}");
            }
            else
            {
                Debug.LogWarning("[RuleTest_B] Inventory.Instance 为空");
            }
        }
    }
}
