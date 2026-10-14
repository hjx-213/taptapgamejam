using UnityEngine;
using System.Collections;

public class EndingEffect : MonoBehaviour
{
    public CanvasGroup fadePanel;

    public float fadeDuration = 5f;
    public GameObject endingTitle;
    public GameObject endingName;
    public GameObject restartButton;
    public GameObject mainMenuButton;
    public UnityEngine.UI.Image endingCG;

    public void SetEndingCG(Sprite cg)
    {
        endingCG.sprite = cg;
    }

    public IEnumerator FadeIn()
    {
        fadePanel.alpha = 1f;
        fadePanel.blocksRaycasts = true;

        // 一开始隐藏结局文字
        endingTitle.SetActive(false);
        endingName.SetActive(false);
        restartButton.SetActive(false);
        mainMenuButton.SetActive(false);
        float time = 0f;

        // 黑幕 → CG
        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float alpha = 1f - time / fadeDuration;

            fadePanel.alpha = alpha;

            yield return null;
        }

        // 确保完全显示 CG
        fadePanel.alpha = 0f;

        // CG 停留 2 秒
        yield return new WaitForSeconds(2f);

        // 开始重新变黑
        time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float alpha = time / fadeDuration;

            fadePanel.alpha = alpha;

            yield return null;
        }

        // 确保完全黑
        fadePanel.alpha = 1f;

        // 黑幕不再阻挡鼠标，让 RestartButton 可以点击
        fadePanel.blocksRaycasts = false;

        // 显示结局文字
        endingTitle.SetActive(true);
        endingName.SetActive(true);
        restartButton.SetActive(true);
        mainMenuButton.SetActive(true);
    }
}