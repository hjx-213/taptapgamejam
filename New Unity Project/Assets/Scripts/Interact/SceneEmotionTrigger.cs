using System.Collections;
using UnityEngine;
using Game.Core.Interaction;
using Game.Core.Emotion;
using Game.UI;

namespace Game.Core.Interaction
{
    [RequireComponent(typeof(Collider2D))]
    public class SceneEmotionTrigger : MonoBehaviour, IInteractable
    {
        [Header("交互")]
        [Tooltip("交互提示文本")]
        [SerializeField] private string promptText;

        [Header("情绪条显示")]
        [Tooltip("情绪条根节点，默认隐藏，触发后显示")]
        [SerializeField] private GameObject emotionBarRoot;

        [Header("情绪参数")]
        [Tooltip("触发后增加多少情绪值")]
        [SerializeField] private float emotionGain = 25f;

        [Tooltip("本次情绪条填充颜色，可为每个触发物单独设置")]
        [SerializeField] private Color emotionBarColor = new Color(1f, 0.35f, 0.35f, 1f);

        [Tooltip("情绪条显示多久后自动隐藏（秒）")]
        [Min(0f)] [SerializeField] private float emotionBarDisplayDuration = 3f;

        [Tooltip("是否只允许触发一次")]
        [SerializeField] private bool oneTimeOnly = true;

        private bool _triggered;

        public string PromptText => promptText;
        public bool CanInteract => !(_triggered && oneTimeOnly);

        public void Interact(GameObject player)
        {
            if (_triggered && oneTimeOnly)
                return;

            _triggered = true;
            StartCoroutine(TriggerEmotionSequence());
        }

        private IEnumerator TriggerEmotionSequence()
        {
            // 1. 显示全屏情绪颜色，从中心向外扩散后淡出
            EmotionOverlay overlay = FindObjectOfType<EmotionOverlay>();
            if (overlay != null)
                overlay.ShowOverlay(emotionBarColor);

            // 等待中心扩散遮罩和镜头反馈结束，再显示情绪条。
            if (overlay != null)
            {
                while (overlay.IsPlaying)
                    yield return null;
            }

            // 2. 显示情绪条并立即更新情绪
            if (emotionBarRoot != null)
                emotionBarRoot.SetActive(true);

            if (EmotionBar.Instance != null)
            {
                EmotionBar.Instance.gameObject.SetActive(true);
                EmotionBar.Instance.AddEmotion(emotionGain, emotionBarColor);
            }

            // 3. 展示一段时间后自动收起情绪条
            if (emotionBarDisplayDuration > 0f)
                yield return new WaitForSecondsRealtime(emotionBarDisplayDuration);

            if (emotionBarRoot != null)
                emotionBarRoot.SetActive(false);
        }
    }
}
