using UnityEngine;
using Game.Core.Emotion;

namespace Game.Test
{
    /// <summary>
    /// 冒烟测试：EmotionBar 基础功能验证。
    /// 挂到任意物体，Play 后自动给情绪条加 _testValue，看是否变色（蓝→橙→红）。
    /// 测完可删除本脚本。
    /// </summary>
    public class EmotionTest : MonoBehaviour
    {
        [Tooltip("测试时添加的情绪值（0~100），80 = 接近红")]
        [SerializeField] private float _testValue = 80f;

        private void Start()
        {
            if (EmotionBar.Instance == null)
            {
                Debug.LogError("[EmotionTest] 找不到 EmotionBar.Instance！确认场景里有 EmotionBar 组件");
                return;
            }
            Debug.Log($"[EmotionTest] 测试：添加情绪 {_testValue}");
            EmotionBar.Instance.AddEmotion(_testValue);
            Debug.Log($"[EmotionTest] 当前进度 = {EmotionBar.Instance.Progress:P0}（1.0=满）");
        }
    }
}
