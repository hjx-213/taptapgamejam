using UnityEngine;
using Game.Puzzle;

/// <summary>
/// 梳妆匣谜题自测脚本（临时，测完删）。
///
/// 用途：验证 PuzzleBase → PuzzleManager 的解谜链路。
/// 梳妆匣的真实流程是「点击→特写场景→点击飞鸟→Solve()」，
/// 本测试只验证最后一步：手动触发 Solve() 能不能让 PuzzleManager 收到。
///
/// 用法：
///   1. 打开 Assets/Test/SceneB
///   2. Hierarchy 右键 Create Empty → 命名 RuleTest_C
///   3. 给它挂本脚本
///   4. ▶ Play → 按 P 键
///   5. Console 应打印 2 行 [RuleTest_C] + 多行 [PuzzleManager] 日志
///
/// 验证完删掉本脚本即可。
/// </summary>
public class RuleTest_C : MonoBehaviour
{
    /// <summary>内嵌一个最小谜题，覆写交互逻辑。</summary>
    private class TestPuzzle : PuzzleBase
    {
        // 兜底：PuzzleBase.OnSolved 字段未初始化，这里手动 new 出来。
        // （这是一个待修的小 bug：PuzzleBase 应当把 OnSolved 初始化为 new UnityEvent()。）
        private void Awake()
        {
            if (OnSolved == null) OnSolved = new UnityEngine.Events.UnityEvent();
        }

        protected override void OnPlayerInteract(GameObject player)
        {
            // 真实梳妆匣：玩家点击飞鸟后调 Solve()
            // 本测试：直接 Solve()，验证 PuzzleManager 收到
            Debug.Log("[TestPuzzle] 玩家交互 → 调 Solve()");
            Solve();
        }
    }

    private TestPuzzle _puzzle;
    private PuzzleManager _puzzleManager;

    private void Start()
    {
        // ---- 1. 自建 PuzzleManager ----
        var pmGO = new GameObject("TestPuzzleManager");
        pmGO.SetActive(false);
        _puzzleManager = pmGO.AddComponent<PuzzleManager>();
        pmGO.SetActive(true);

        // ---- 2. 自建 TestPuzzle ----
        var pzGO = new GameObject("TestPuzzle");
        pzGO.SetActive(false);
        _puzzle = pzGO.AddComponent<TestPuzzle>();
        pzGO.SetActive(true);

        // ---- 3. 手动注册到 PuzzleManager ----
        // PuzzleBase.Start() 不会自动注册，需子类或外部显式调 RegisterPuzzle()
        _puzzleManager.RegisterPuzzle(_puzzle);

        Debug.Log(
            "[RuleTest_C] 准备完毕。按 P 键测试：触发 Solve()，看 PuzzleManager 收没收到");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && _puzzle != null)
        {
            Debug.Log("[RuleTest_C] P 触发：模拟玩家点击 → OnPlayerInteract");
            _puzzle.Interact(gameObject);
            // 验证状态
            Debug.Log(
                $"[RuleTest_C] 谜题 IsSolved: {_puzzle.IsSolved}");
        }
    }
}
