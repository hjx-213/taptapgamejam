using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class ImageStateTransition : MonoBehaviour
    {
        [Header("两种画面状态")]
        [SerializeField] private Image state1Image;
        [SerializeField] private Image state2Image;

        [Header("交叉淡入淡出时长（秒）")]
        [SerializeField, Min(0.01f)]
        private float duration = 0.8f;

        private Coroutine transitionCoroutine;
        private bool isState2;

        public bool IsState2 => isState2;
        public float Duration => duration;

        private void Awake()
        {
            ResetToState1();
        }

        // 将画面恢复为状态1
        public void ResetToState1()
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                transitionCoroutine = null;
            }

            isState2 = false;
            SetAlpha(state1Image, 1f);
            SetAlpha(state2Image, 0f);
        }

        // 在指定台词开始时调用
        public void BeginTransition()
        {
            BeginTransition(duration);
        }

        public void BeginTransition(float transitionDuration)
        {
            if (isState2)
                return;

            isState2 = true;

            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);

            duration = Mathf.Max(0.01f, transitionDuration);
            transitionCoroutine = StartCoroutine(CrossFade());
        }

        // 对话结束时调用，确保状态2完全显示
        public void CompleteTransition()
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                transitionCoroutine = null;
            }

            isState2 = true;
            SetAlpha(state1Image, 0f);
            SetAlpha(state2Image, 1f);
        }

        private IEnumerator CrossFade()
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                SetAlpha(state1Image, 1f - t);
                SetAlpha(state2Image, t);

                yield return null;
            }

            SetAlpha(state1Image, 0f);
            SetAlpha(state2Image, 1f);

            transitionCoroutine = null;
        }

        private void SetAlpha(Image image, float alpha)
        {
            if (image == null)
                return;

            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}
