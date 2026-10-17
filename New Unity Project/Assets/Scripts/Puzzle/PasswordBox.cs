using UnityEngine;
using UnityEngine.Events;
using Game.Puzzle;

namespace Game.Sample
{
    /// <summary>
    /// 示例谜题：密码箱。
    ///
    /// 工作流程：
    ///   1. 玩家点击密码箱 → OnInteract 触发（UIManager 弹出密码输入 UI）
    ///   2. UI 输入完成后回调 OnCodeEntered(code)
    ///   3. 如果 code == correctCode → Solve() 触发 → GameFlow 收到胜利信号
    /// </summary>
    public class PasswordBox : PuzzleBase
    {
        [Header("密码设置")]
        [Tooltip("正确密码")]
        [SerializeField] private string _correctCode = "1234";

        private string _inputCode = "";
        [SerializeField] private GameObject _errorText;
        [SerializeField] private GameObject _passwordPanel;

        [Header("UI 回调（给 UIManager 用）")]
        [Tooltip("UI 输入完成时调用，传入玩家输入的密码")]
        public UnityEvent<string> OnCodeSubmitted;

        private void OpenPasswordPanel()
        {
            if (_passwordPanel != null)
                _passwordPanel.SetActive(true);
        }

        private void ClosePasswordPanel()
        {
            if (_passwordPanel != null)
                _passwordPanel.SetActive(false);
        }

        protected override void OnPlayerInteract(GameObject player)
        {
            Debug.Log($"[{_puzzleName}] 等待玩家输入密码…");
            OpenPasswordPanel();
        }

        /// <summary>
        /// 由 UIManager 或其他系统调用：传入玩家输入的密码。
        /// </summary>
        public void OnCodeSubmit(string inputCode = null)
        {
            _inputCode = inputCode ?? _inputCode;

            if (string.IsNullOrEmpty(_inputCode))
            {
                Debug.Log($"[{_puzzleName}] 还未输入密码");
                return;
            }

            OnCodeSubmitted?.Invoke(_inputCode);

            if (_inputCode == _correctCode)
            {
                if (_errorText != null)
                    _errorText.SetActive(false);

                Solve();
                ClosePasswordPanel();
            }
            else
            {
                Debug.Log($"[{_puzzleName}] 密码错误：{_inputCode}");

                if (_errorText != null)
                    _errorText.SetActive(true);

                _inputCode = "";
            }
        }

        public override void Reset()
        {
            base.Reset();
            _inputCode = "";
        }
    }
}