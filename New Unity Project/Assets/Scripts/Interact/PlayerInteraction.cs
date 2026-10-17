using UnityEngine;
using UnityEngine.EventSystems;
using Game.Core.Interaction;

public class PlayerInteraction : MonoBehaviour
{
    [Header("点击设置")]
    [SerializeField] private Camera mainCamera;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        // 对话或手记显示期间，不允许点击场景物品。
        // 对话推进/手记操作由对应 UI 自己处理。
        Game.Dialogue.DialogueManager dialogueManager =
            FindObjectOfType<Game.Dialogue.DialogueManager>();

        if (dialogueManager != null && dialogueManager.IsDialogueActive)
            return;

        JournalUI journalUI = FindObjectOfType<JournalUI>();
        if (journalUI != null && journalUI.IsOpen)
            return;

        if (mainCamera == null)
        {
            Debug.LogWarning("PlayerInteraction：没有找到主摄像机！");
            return;
        }

        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider == null)
        {
            return;
        }

        IInteractable interactable = hit.collider.GetComponent<IInteractable>();

        if (interactable == null)
        {
            return;
        }

        if (!interactable.CanInteract)
        {
            return;
        }

        interactable.Interact(gameObject);
    }
}