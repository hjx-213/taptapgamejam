using UnityEngine;
using UnityEngine.Events;

namespace Game.Core
{
    // 游戏整体流程主控：管理 Playing/Won/Lost 状态切换。
    // 所有"游戏胜利后该干嘛"的逻辑（弹胜利 UI、播放胜利 BGM）
    // 都通过订阅 OnStateChanged 事件来响应，不需要直接引用 GameFlow。
    [System.Serializable]
    public class GameStateEvent : UnityEvent<GameState> { }

    public class GameFlow : MonoBehaviour
    {
        public static GameFlow Instance { get; private set; }

        [Tooltip("游戏开始时的初始状态")]
        [SerializeField] private GameState _initialState = GameState.Playing;

        // 当前游戏状态
        public GameState State { get; private set; }

        // 状态变化时触发（参数：旧→新）
        public GameStateEvent OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            State = _initialState;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // 由 PuzzleManager 在所有谜题解完后调用。
        public void OnAllPuzzlesSolved()
        {
            if (State != GameState.Playing) return;
            SetState(GameState.Won);
            Debug.Log("[GameFlow] 🎉 YOU WIN!");
        }

        // 外部强制切换状态（如未来要实现"游戏失败"）。
        public void SetState(GameState newState)
        {
            if (State == newState) return;
            GameState old = State;
            State = newState;
            Debug.Log($"[GameFlow] State: {old} -> {newState}");
            OnStateChanged?.Invoke(newState);
        }
    }
}