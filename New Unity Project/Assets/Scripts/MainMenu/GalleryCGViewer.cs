using UnityEngine;
using UnityEngine.UI;

public class GalleryCGViewer : MonoBehaviour
{
    public GameObject cgPanel;
    public Image cgImage;

    public void ShowCG(Sprite cg)
    {
        cgImage.sprite = cg;
        cgPanel.SetActive(true);
    }

    public void CloseCG()
    {
        cgPanel.SetActive(false);
    }
}