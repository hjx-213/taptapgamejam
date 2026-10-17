using UnityEngine;
using Game.Puzzle;

/// <summary>
/// 梳妆匣谜题（MVP 版）。
///
/// 完整设计：点击梳妆匣 → 特写场景 → 点击飞鸟 → 暗格开 → 拾取道具
/// MVP 版（本次）：点击 → Solve() → PuzzleManager 收到
///   - 不做特写
///   - 不做飞鸟
///   - 不做暗格开
///   - 不发道具
///
/// 后续要做特写版时：
///   1. 在 OnPlayerInteract 里 SetActive(true) 一个全屏 UI 面板
///   2. 面板里有飞鸟按钮，点击后调 Solve()
///   3. 关闭面板返回场景
/// </summary>
public class DressingTablePuzzle : PuzzleBase
{
    [Header("MVP 调试")]
    [Tooltip("是否在 Solve 时打印日志")]
    [SerializeField] private bool _logOnSolve = true;

    protected override void OnPlayerInteract(GameObject player)
    {
        // MVP：直接调 Solve()，让 PuzzleManager 收到
        // 真实流程：先开特写 UI → 玩家点飞鸟 → 飞鸟回调里调 Solve()
        if (_logOnSolve)
            Debug.Log("[DressingTablePuzzle] 玩家点击 → Solve()");
        Solve();
    }
}
