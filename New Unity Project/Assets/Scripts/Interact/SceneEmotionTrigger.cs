using System.Collections;
using UnityEngine;
using Game.Core.Interaction;
using Game.Core.Emotion;

namespace Game.Core.Interaction
{
    [RequireComponent(typeof(Collider2D))]
    public class SceneEmotionTrigger : MonoBehaviour, IInteractable
    {
        [Header("交互")]
        [Tooltip("交互提示文本")]
        [SerializeField] private string promptText;

        [Header("特效")]
        [Tooltip("中心向外扩散的特效对象（通常是 Particle System）")]
        [SerializeField] private GameObject burstEffect;

        [Header("情绪条显示")]
        [Tooltip("情绪条根节点，默认隐藏，触发后显示")]
        [SerializeField] private GameObject emotionBarRoot;

        [Header("情绪参数")]
        [Tooltip("触发后增加多少情绪值")]
        [SerializeField] private float emotionGain = 25f;

        [Tooltip("特效播放时长")]
        [SerializeField] private float burstDuration = 0.8f;

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
            // 1. 显示特效：从中心向外扩散
            if (burstEffect != null)
            {
                burstEffect.SetActive(true);

                var particle = burstEffect.GetComponent<ParticleSystem>();
                if (particle != null)
                    particle.Play();
            }

            // 2. 显示情绪条并立即更新情绪
            if (emotionBarRoot != null)
                emotionBarRoot.SetActive(true);

            if (EmotionBar.Instance != null)
            {
                EmotionBar.Instance.gameObject.SetActive(true);
                EmotionBar.Instance.ResetEmotion();
                EmotionBar.Instance.AddEmotion(emotionGain);
            }

            // 3. 过一段时间后让特效消失
            yield return new WaitForSeconds(burstDuration);

            if (burstEffect != null)
                burstEffect.SetActive(false);
        }
    }
}
