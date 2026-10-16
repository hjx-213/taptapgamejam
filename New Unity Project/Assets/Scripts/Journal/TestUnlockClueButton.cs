
using UnityEngine;

public class TestUnlockClueButton : MonoBehaviour
{
    public void UnlockTestClue()
    {
        if (JournalManager.Instance == null)
        {
            Debug.LogError("找不到 JournalManager！请检查 MainMenu 中的 GlobalManager。");
            return;
        }

        JournalManager.Instance.UnlockClue("suosilan");
        JournalManager.Instance.UnlockClue("shenminvzi");
    }
}
