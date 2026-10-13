using UnityEngine;

public class GalleryManager : MonoBehaviour
{
    [Header("结局数据")]
    public EndingData[] endings;

    [Header("按钮")]
    public GameObject endingButtonPrefab;

    [Header("按钮生成位置")]
    public Transform buttonParent;

    void Start()
    {
        GenerateEndingButtons();
    }

    void GenerateEndingButtons()
    {
        GalleryCGViewer cgViewer =
            FindObjectOfType<GalleryCGViewer>(true);

        foreach (EndingData ending in endings)
        {
            GameObject button =
                Instantiate(
                    endingButtonPrefab,
                    buttonParent
                );

            EndingButton endingButton =
                button.GetComponent<EndingButton>();

            endingButton.endingData = ending;
            endingButton.cgViewer = cgViewer;
        }
    }

    public void RefreshEndingButtons()
    {
        EndingButton[] buttons = buttonParent.GetComponentsInChildren<EndingButton>(true);

        foreach (EndingButton button in buttons)
        {
            button.RefreshDisplay();
        }
    }
}