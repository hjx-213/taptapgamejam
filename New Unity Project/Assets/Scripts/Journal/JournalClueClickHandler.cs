using UnityEngine;
using UnityEngine.UI;

public class JournalClueClickHandler : MonoBehaviour
{
    public JournalClueView clueView;
    public JournalUI journalUI;
    public Button button;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnClickClue);
    }

    private void OnClickClue()
    {
        if (journalUI == null || clueView == null)
            return;

        journalUI.ToggleClueSelection(clueView);
    }
}