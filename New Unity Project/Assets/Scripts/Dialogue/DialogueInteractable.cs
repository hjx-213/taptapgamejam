using UnityEngine;
using Game.Core.Interaction;
using Game.Dialogue;

/// <summary>
/// 可复用的点击交互组件：显示调查文字，或直接开始指定主线剧情。
/// 需要与 SpriteRenderer/Collider2D 放在同一个物体上。
/// </summary>
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
    [SerializeField] private string promptText = "调查";
    [SerializeField] private bool canInteract = true;

    [Header("剧情解锁条件")]
    [SerializeField] private bool requireFlag = false;
    [SerializeField] private string requiredFlagName = "";
    [SerializeField] private bool requiredFlagValue = true;

    [Header("设置剧情标记")]
    [SerializeField] private bool setFlagOnInteract = false;
    [SerializeField] private string flagNameToSet = "";
    [SerializeField] private bool flagValueToSet = true;

    [Header("调查文字")]
    [TextArea(2, 5)]
    [SerializeField] private string infoText = "这里似乎有什么值得调查的。";
    [SerializeField] private string displayName = "调查";
    [SerializeField] private string position = "左";
    [SerializeField] private Sprite avatar;

    [Header("主线剧情（仅对应模式需要）")]
    [SerializeField] private int storyDialogueId = -1;

    public string PromptText => promptText;
    public bool CanInteract
    {
        get
        {
            if (!canInteract)
                return false;

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
        if (setFlagOnInteract)
        {
            if (FlagManager.Instance == null)
            {
                Debug.LogError("场景中没有找到 FlagManager。", this);
                return;
            }

            if (!string.IsNullOrWhiteSpace(flagNameToSet))
            {
                FlagManager.Instance.SetFlag(
                    flagNameToSet,
                    flagValueToSet
                );
            }
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
