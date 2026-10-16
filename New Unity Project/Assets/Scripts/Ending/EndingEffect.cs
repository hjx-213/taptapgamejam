using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EndingEffect : MonoBehaviour
{
    [Header("黑幕遮罩（从中心向四周扩散）")]
    public Image fadePanel;

    [Header("结局显示内容")]
    public float fadeDuration = 5f;
    public GameObject endingTitle;
    public GameObject endingName;
    public GameObject restartButton;
    public GameObject mainMenuButton;
    public UnityEngine.UI.Image endingCG;

    private Material _centerRevealMaterial;
    private static readonly int CenterId = Shader.PropertyToID("_Center");
    private static readonly int RadiusId = Shader.PropertyToID("_Radius");
    private static readonly int SoftnessId = Shader.PropertyToID("_Softness");

    private void Awake()
    {
        if (fadePanel != null)
        {
            _centerRevealMaterial = new Material(Shader.Find("UI/CenterReveal"));
            fadePanel.material = _centerRevealMaterial;
            fadePanel.color = Color.black;
            fadePanel.raycastTarget = true;
            fadePanel.enabled = true;

            _centerRevealMaterial.SetVector(CenterId, new Vector4(0.5f, 0.5f, 0f, 0f));
            _centerRevealMaterial.SetFloat(SoftnessId, 0.18f);
            _centerRevealMaterial.SetFloat(RadiusId, 1.8f);
        }
    }

    public void SetEndingCG(Sprite cg)
    {
        endingCG.sprite = cg;
    }

    private IEnumerator AnimateMask(float fromRadius, float toRadius)
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);
            float radius = Mathf.Lerp(fromRadius, toRadius, t);

            if (_centerRevealMaterial != null)
                _centerRevealMaterial.SetFloat(RadiusId, radius);

            yield return null;
        }

        if (_centerRevealMaterial != null)
            _centerRevealMaterial.SetFloat(RadiusId, toRadius);
    }

    public IEnumerator FadeIn()
    {
        if (fadePanel == null)
        {
            Debug.LogWarning("[EndingEffect] 未绑定 fadePanel，跳过黑幕扩散效果。");
            yield break;
        }

        if (_centerRevealMaterial == null)
            Awake();

        fadePanel.raycastTarget = true;

        endingTitle.SetActive(false);
        endingName.SetActive(false);
        restartButton.SetActive(false);
        mainMenuButton.SetActive(false);

        // 先让黑幕完全覆盖屏幕，再从中间向四周扩散，显出 CG。
        _centerRevealMaterial.SetFloat(RadiusId, 1.8f);
        yield return AnimateMask(1.8f, 0f);

        yield return new WaitForSeconds(2f);

        // 重新变黑：从中心向四周收拢。
        yield return AnimateMask(0f, 1.8f);

        fadePanel.raycastTarget = false;

        endingTitle.SetActive(true);
        endingName.SetActive(true);
        restartButton.SetActive(true);
        mainMenuButton.SetActive(true);
    }
}