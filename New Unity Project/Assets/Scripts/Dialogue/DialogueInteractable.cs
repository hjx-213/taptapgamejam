using UnityEngine;
using Game.Core.Interaction;
using Game.Dialogue;

// 可复用的点击交互组件：显示调查文字，或直接开始指定主线剧情。
// 需要与 SpriteRenderer + BoxCollider2D 放在同一个物体上。
public class DialogueInteractable : MonoBehaviour, IInteractable
{
    public enum InteractionType
    {
        TemporaryInfo,   // 显示调查文字；结束后返回探索
        InfoThenStory,   // 显示调查文字；结束后开始指定主线
        StartStory       // 点击后直接开始指定主线
    }

    [Header("交互设置")]
    [SerializeField] private InteractionType interactionType = InteractionType.TemporaryInfo;

    [Header("剧情解锁条件")]
    [SerializeField] private bool requireFlag = false;
    [SerializeField] private string requiredFlagName = "";
    [SerializeField] private bool requiredFlagValue = true;

    [Header("已调查/已触发标记")]
    [SerializeField] private string inspectionFlagName = "";
    [SerializeField] private bool inspectionFlagValue = true;

    [Header("点击显示的信息")]
    [TextArea(2, 5)]
    [SerializeField] private string infoText;
    
    private string displayName;
    private string position;
    private Sprite avatar;

    [Header("点击切入的剧情ID（InfoThenStory、StartStory模式必填）")]
    [SerializeField] private int storyDialogueId = -1;

    public string PromptText => "点击";

    public bool CanInteract
    {
        get
        {
            if (!requireFlag)
                return true;

            if (FlagManager.Instance == null)
                return false;

            return FlagManager.Instance.GetFlag(requiredFlagName)
                == requiredFlagValue;
        }
    }

    public void Interact(GameObject player)
    {
        if (!string.IsNullOrWhiteSpace(inspectionFlagName))
        {
            if (FlagManager.Instance == null)
            {
                Debug.LogError("场景中没有找到 FlagManager。", this);
                return;
            }

            FlagManager.Instance.SetFlag(
                inspectionFlagName,
                inspectionFlagValue
            );
        }

        DialogueManager manager = FindObjectOfType<DialogueManager>();
        if (manager == null)
        {
            Debug.LogError($"[{nameof(DialogueInteractable)}] 场景中没有找到 DialogueManager。", this);
            return;
        }

        switch (interactionType)
        {
            case InteractionType.TemporaryInfo:
                manager.ShowTemporaryInfo(infoText, displayName, position, avatar);
                break;

            case InteractionType.InfoThenStory:
                if (storyDialogueId < 0)
                {
                    Debug.LogError($"[{nameof(DialogueInteractable)}] {name} 需要填写有效的主线剧情 ID。", this);
                    return;
                }
                manager.ShowTemporaryInfo(infoText, displayName, position, avatar, storyDialogueId);
                break;

            case InteractionType.StartStory:
                if (storyDialogueId < 0)
                {
                    Debug.LogError($"[{nameof(DialogueInteractable)}] {name} 需要填写有效的主线剧情 ID。", this);
                    return;
                }
                manager.StartMainDialogue(storyDialogueId);
                break;
        }
    }
}
