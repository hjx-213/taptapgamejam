using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JournalClueView : MonoBehaviour
{
    [Header("线索数据")]
    public JournalEntryData entryData;

    [Header("显示内容")]
    public GameObject visualRoot;
    public TMP_Text titleText;
    public TMP_Text contentText;
    public Image clueImage;

    [Header("推理状态")]
    public GameObject selectedHighlight;

    public bool IsSelected { get; private set; }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;

        if (selectedHighlight != null)
            selectedHighlight.SetActive(selected);
    }

    public void Refresh(int currentPageGroup)
    {
        if (entryData == null)
        {
            Debug.LogError("[手记] 没有绑定线索数据：" + gameObject.name);
            SetVisible(false);
            return;
        }

        if (JournalManager.Instance == null)
        {
            Debug.LogError("[手记] 找不到 JournalManager！");
            SetVisible(false);
            return;
        }

        bool unlocked =
            JournalManager.Instance.IsClueUnlocked(entryData.clueId);

        bool onCurrentPage =
            entryData.pageGroup == currentPageGroup;

        SetVisible(unlocked && onCurrentPage);

        if (titleText != null)
            titleText.text = entryData.clueTitle;

        if (contentText != null)
            contentText.text = entryData.clueContent;

        if (clueImage != null)
        {
            clueImage.sprite = entryData.clueImage;
            clueImage.enabled = entryData.clueImage != null;
        }
    }

    private void SetVisible(bool visible)
    {
        if (visualRoot != null)
            visualRoot.SetActive(visible);

        // 未解锁或不在当前页时，禁用整张卡片的点击功能。
        Button button = GetComponent<Button>();
        if (button != null)
            button.interactable = visible;

        // 同时清除这张卡片可能残留的选中高亮。
        if (!visible)
            SetSelected(false);
    }
}
