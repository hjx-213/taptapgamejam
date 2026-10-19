using UnityEngine;
using UnityEngine.UI;
using Game.Core.Interaction;

/// <summary>Opens an existing scene UI panel from a world-space clickable object.</summary>
[RequireComponent(typeof(Collider2D))]
public sealed class CloseupPanelInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptText = "查看";
    [SerializeField] private RectTransform closeupPanel;

    private static CloseupPanelInteractable openInstance;
    private Button closeButton;

    public static bool IsAnyOpen =>
        openInstance != null && openInstance.closeupPanel != null &&
        openInstance.closeupPanel.gameObject.activeInHierarchy;

    public string PromptText => promptText;
    public bool CanInteract => closeupPanel != null && !closeupPanel.gameObject.activeSelf;

    private void Awake()
    {
        if (closeupPanel == null)
        {
            Debug.LogError($"[{nameof(CloseupPanelInteractable)}] 未指定特写面板。", this);
            return;
        }

        Transform closeButtonTransform = closeupPanel.Find("CloseButton");
        if (closeButtonTransform != null)
        {
            closeButton = closeButtonTransform.GetComponent<Button>();
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseCloseup);
        }

        closeupPanel.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseCloseup);

        if (openInstance == this)
            openInstance = null;
    }

    public void Interact(GameObject player) => OpenCloseup();

    /// <summary>Can also be assigned to a Button's OnClick event.</summary>
    public void OpenCloseup()
    {
        if (closeupPanel == null)
            return;

        if (openInstance != null && openInstance != this)
            openInstance.CloseCloseup();

        openInstance = this;
        closeupPanel.gameObject.SetActive(true);
        // Keep persistent HUD controls (backpack and journal) above the full-screen
        // close-up raycast mask so they remain visible and clickable.
        closeupPanel.SetAsFirstSibling();
    }

    /// <summary>Can be assigned to a close button's OnClick event.</summary>
    public void CloseCloseup()
    {
        if (closeupPanel != null)
            closeupPanel.gameObject.SetActive(false);

        if (openInstance == this)
            openInstance = null;
    }

    private void Update()
    {
        if (IsAnyOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseCloseup();
    }
}
