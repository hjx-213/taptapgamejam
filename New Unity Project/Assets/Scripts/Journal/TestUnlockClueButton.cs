
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

        JournalManager.Instance.UnlockClue("peidaiduandao");
        JournalManager.Instance.UnlockClue("xiedouxinwen");
        JournalManager.Instance.UnlockClue("shibananchen");
    }
}
