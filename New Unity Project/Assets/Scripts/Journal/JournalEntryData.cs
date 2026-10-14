
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewJournalEntry",
    menuName = "Game/Journal Entry"
)]
public class JournalEntryData : ScriptableObject
{
    [Header("线索基本信息")]
    public string clueId;
    public string clueTitle;

    [TextArea(3, 8)]
    public string clueContent;

    [Header("线索图片")]
    public Sprite clueImage;

    [Header("所在页组")]
    [Min(0)]
    public int pageGroup = 0;
}
