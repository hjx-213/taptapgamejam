using UnityEngine;

[CreateAssetMenu(
    fileName = "Ending_",
    menuName = "Game/Ending Data"
)]
public class EndingData : ScriptableObject
{
    [Header("结局信息")]
    public string endingFlag;

    public string endingTitle;

    public string endingName;

    [Header("结局 CG")]
    public Sprite endingCG;
}