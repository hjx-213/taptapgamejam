
using TMPro;
using UnityEngine;

public class JournalUI : MonoBehaviour
{
    [Header("手记界面")]
    public GameObject journalPanel;
    public GameObject darkOverlay;

    [Header("线索卡片")]
    public JournalClueView[] clueViews;

    [Header("翻页按钮")]
    public GameObject previousButton;
    public GameObject nextButton;

    [Header("页数设置")]
    [Min(1)]
    public int totalPageGroups = 3;

    [Header("组合结果提示")]
    public TMP_Text combinationResultText;

    private GameObject reasoningPanel;
    private TMP_Text reasoningMessageText;

    private JournalClueView firstSelectedClue;
    private JournalClueView secondSelectedClue;
    private bool isReasoningMode = false;

    // 当前页组，从 0 开始
    private int currentPageGroup = 0;

    public void OpenJournal()
    {
        currentPageGroup = 0;

        if (darkOverlay != null)
            darkOverlay.SetActive(true);

        if (journalPanel != null)
            journalPanel.SetActive(true);

        if (combinationResultText != null)
            combinationResultText.text = "";

        UpdatePageDisplay();
    }

    public void CloseJournal()
    {
        if (journalPanel != null)
            journalPanel.SetActive(false);

        if (darkOverlay != null)
            darkOverlay.SetActive(false);
    }

    public void NextPage()
    {
        if (currentPageGroup < totalPageGroups - 1)
        {
            currentPageGroup++;
            UpdatePageDisplay();
        }
    }

    public void PreviousPage()
    {
        if (currentPageGroup > 0)
        {
            currentPageGroup--;
            UpdatePageDisplay();
        }
    }

    private void UpdatePageDisplay()
    {
        int leftPageNumber = currentPageGroup * 2 + 1;
        int rightPageNumber = leftPageNumber + 1;

        if (previousButton != null)
            previousButton.SetActive(currentPageGroup > 0);

        if (nextButton != null)
            nextButton.SetActive(
                currentPageGroup < totalPageGroups - 1
            );
        
        if (clueViews != null)
        {
            foreach (JournalClueView clueView in clueViews)
            {
                if (clueView != null)
                    clueView.Refresh(currentPageGroup);
            }
        }
    }

    public void ToggleClueSelection(JournalClueView clue)
    {
        Debug.Log("[手记推理] ToggleClueSelection 已调用：" + clue.name);
        if (clue == null || !isReasoningMode)
            return;

        if (firstSelectedClue == clue)
        {
            firstSelectedClue.SetSelected(false);
            firstSelectedClue = secondSelectedClue;
            secondSelectedClue = null;
        }
        else if (secondSelectedClue == clue)
        {
            secondSelectedClue.SetSelected(false);
            secondSelectedClue = null;
        }
        else if (firstSelectedClue == null)
        {
            firstSelectedClue = clue;
            clue.SetSelected(true);
        }
        else if (secondSelectedClue == null)
        {
            secondSelectedClue = clue;
            clue.SetSelected(true);
        }
        else
        {
            if (reasoningMessageText != null)
                reasoningMessageText.text = "最多选择两条线索。";
        }

        if (reasoningMessageText != null &&
            firstSelectedClue != null &&
            secondSelectedClue != null)
        {
            reasoningMessageText.text = "已选择两条线索，点击推理进行组合。";
        }
    }

    public void ToggleReasoningPanel()
    {
        isReasoningMode = !isReasoningMode;

        if (!isReasoningMode)
        {
            ClearSelectedClues();
        }
        else if (reasoningMessageText != null)
        {
            reasoningMessageText.text = "请选择两条线索进行组合。";
        }

        if (reasoningPanel != null)
            reasoningPanel.SetActive(false);

        Debug.Log(isReasoningMode
            ? "已进入推理模式"
            : "已退出推理模式");
    }

    public void CombineSelectedClues()
    {
        if (!isReasoningMode)
        {
            Debug.Log("请先进入推理模式。");
            return;
        }

        if (firstSelectedClue == null || secondSelectedClue == null)
        {
            Debug.Log("请先选择两条线索。");
            return;
        }

        if (firstSelectedClue.entryData == null ||
            secondSelectedClue.entryData == null)
        {
            Debug.LogError("所选线索缺少数据绑定！");
            ClearSelectedClues();
            return;
        }

        string clueA = firstSelectedClue.entryData.clueId;
        string clueB = secondSelectedClue.entryData.clueId;

        bool correctCombination =
            (clueA == "clue_brother_letter" &&
            clueB == "clue_test_photo") ||
            (clueA == "clue_test_photo" &&
            clueB == "clue_brother_letter");

        if (correctCombination)
        {
            if (JournalManager.Instance == null)
            {
                Debug.LogError("找不到 JournalManager！");
                ClearSelectedClues();
                return;
            }

            JournalManager.Instance.UnlockClue("clue_date_connection");

            if (combinationResultText != null)
                combinationResultText.text = "发现了信件与照片之间的联系！";

            Debug.Log("推理成功：解锁 clue_date_connection。");

            // 刷新所有线索，让新线索立即显示。
            if (clueViews != null)
            {
                foreach (JournalClueView view in clueViews)
                {
                    if (view != null)
                        view.Refresh(currentPageGroup);
                }
            }
        }
        else
        {
            if (combinationResultText != null)
                combinationResultText.text = "这两条线索似乎没有直接联系。";

            Debug.Log("推理失败：这两条线索无法组合。");
        }

        ClearSelectedClues();
    }

    private void ClearSelectedClues()
    {
        if (firstSelectedClue != null)
            firstSelectedClue.SetSelected(false);

        if (secondSelectedClue != null)
            secondSelectedClue.SetSelected(false);

        firstSelectedClue = null;
        secondSelectedClue = null;

        if (reasoningMessageText != null)
            reasoningMessageText.text = "";
    }
}
