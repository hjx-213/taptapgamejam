using UnityEngine;

public class TestButton : MonoBehaviour
{
    [Tooltip("要切换到的场景名")]
    [SerializeField] private string _targetSceneName = "SceneA";

    /// <summary>Button OnClick 调这个方法</summary>
    public void OnClick()
    {
        if (Game.Core.SceneTransition.SceneTransitionManager.Instance == null)
        {
            Debug.LogError("找不到 SceneTransitionManager！检查 GameManager 是否挂了它");
            return;
        }
        Game.Core.SceneTransition.SceneTransitionManager.Instance.FadeToScene(_targetSceneName);
    }
}
